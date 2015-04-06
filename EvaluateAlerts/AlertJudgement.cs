using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Linq;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Data;
	using Helpers;

	namespace EvaluateAlerts
	{

		public class AlertJudgement : IUpdatingPlugin
		{
			IDictionary<int, AlertLevel> levels;

			readonly AlertData data;
			readonly ConsumptionData c_data;

			// (1.1.0)レベルの決め打ちを解消．Configureメソッドから(のみ)設定できます．
			#region *コンストラクタ(AlertJudgement)
			public AlertJudgement(string databaseFile) : this()
			{
				data = new AlertData(databaseFile);
				c_data = new ConsumptionData(databaseFile);	// ←dataと分ける意味あるの？
			}

			AlertJudgement()
			{
				levels = new Dictionary<int, AlertLevel>();

				// 決め打ちでレベルを用意してみる．
				/*
				this.DefineLevel(new AlertLevel
				{
					Rank = 16,
					Name = "警報",
					InBorder = (data) => { return data.Take(2).Min() >= 102; },
					OutBorder = (data) => { return data.Take(2).Max() < 96; }
				});
				this.DefineLevel(new AlertLevel
				{
					Rank = 4,
					Name = "注意報",
					InBorder = (data) => { return data.Take(2).Min() >= 97; },
					OutBorder = (data) => { return data.Take(2).Max() < 92; }
				});
				*/
			}
			#endregion


			public void DefineLevel(AlertLevel level)
			{
				if (level.Rank > 0)
				{
					levels.Add(level.Rank, level);
				}
			}


			// とりあえず引数なしで．
			public void Judge()
			{
				// time時点で最新のデータをn件取得する．
				var recent_data = c_data.GetRecentData(3).OrderByDescending(d => d.Key);
				var current_rank = data.GetCurrentRank();

				int new_rank = Check(current_rank, recent_data.Select(d => d.Value).ToArray());
				if (new_rank != current_rank)
				{
					// 記録する．
					data.InsertData(new AlertElement { 
						DataTime=recent_data.First().Key, DeclaredAt = DateTime.Now, Rank = new_rank });
					this.Inserted(this, EventArgs.Empty);
					// ログを出力してもいいかも！
					//Console.WriteLine("New rank : {0}, previous rank : {1}", new_rank, current_rank);
				}
			}

			public event EventHandler Inserted = delegate { };

			int Check(int current_rank, int[] recent_data)
			{

				// 現在より上のランクのレベルに達したか？
				foreach (var level in levels.OrderBy(l => -l.Key).Where((l) => l.Key > current_rank))
				{
					// Inborderをチェック．
					if (level.Value.InBorder(recent_data))
					{
						return level.Key;
					}
				}
				// 現在より下のランクのレベルに達したか？
				foreach (var level in levels.OrderBy(l => l.Key).Where((l) => l.Key < current_rank))
				{
					// OutBorderをチェック．
					if (level.Value.OutBorder(recent_data))
					{
						return level.Key;
					}
				}
				return current_rank;
			}


			public string FeedTitle { get; set; }
			public string FeedAuthor { get; set; }
			public string FeedID { get; set; }
			public string FeedSelfLink { get; set; }


			// (1.1.1)Titleなどをプロパティ化．
			// (1.0.1)
			#region *AlertをAtomFeedで通知(OutputAtomFeed)
			public void OutputAtomFeed(string destination, int n = 6)
			{
				var feed = new AtomFeed
				{
					ID = this.FeedID,
					Title = this.FeedTitle,
					Author = this.FeedAuthor,
					UpdatedAt = DateTime.Now,
					SelfLink = this.FeedSelfLink,
				};

				// 6件取得．
				var recent_data = data.GetRecentData(n);
				var times = recent_data.Keys.OrderByDescending(k => k).ToArray();

				for (int i = 0; i < times.Length - 1; i++)
				{
					var current = recent_data[times[i]];
					var current_time = current.DataTime.ToString("HH時mm分");
					var current_long_time = current.DataTime.ToString("M月d日HH時mm分");
					AtomEntry entry = new AtomEntry
					{
						ID = this.FeedID + SQLiteData.Convert.TimeToInt(current.DeclaredAt),
						PublishedAt = current.DeclaredAt
					};

					if (recent_data[times[i]].Rank > recent_data[times[i+1]].Rank)
					{
						// レベルが上昇．
						var name = levels[current.Rank].Name;
						entry.Title = string.Format("{0}発令({1})", name, current_time);
						entry.Content = string.Format("{1}に，{0}が発令されました．", name, current_long_time);

						feed.Entries.Add(entry);
					}
					else if (recent_data[times[i]].Rank < recent_data[times[i + 1]].Rank)
					{
						// レベルが下降．
						var name = levels[recent_data[times[i+1]].Rank].Name;
						entry.Title = string.Format("{0}解除({1})", name, current_time);
						entry.Content = string.Format("{1}に，{0}が解除されました．", name, current_long_time);
						if (current.Rank > 0)
						{
							entry.Content += string.Format(" 現在，{0}が発令されています．", levels[current.Rank].Name);
						}

						feed.Entries.Add(entry);
					}
				}

				XmlWriterSettings settings = new XmlWriterSettings
				{
					Indent = true,
					NewLineHandling = NewLineHandling.Replace,
					NewLineChars = "\n"
				};

				using (XmlWriter writer = XmlWriter.Create(destination, settings))
				{
					feed.OutputDocument().WriteTo(writer);
				}

			}
			#endregion



			#region (1.2.0)メール送信を実装．(とりあえず POP before SMTP 認証のみ．)
			private void SendMail(bool separate_destinations)
			{
				System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
				message.From = new System.Net.Mail.MailAddress(this.MailFrom);

				// messageを作成する．
				// TODO: Bodyのブラッシュアップが必要！
				var recent_data = data.GetRecentData(2);
				var times = recent_data.Keys.OrderByDescending(k => k).ToArray();

				var current = recent_data[times[0]];
				var current_time = current.DataTime.ToString("HH時mm分");
				var current_long_time = current.DataTime.ToString("M月d日HH時mm分");

				string body = string.Format("{0} 発表\n\n", current_long_time);

				if (recent_data[times[0]].Rank > recent_data[times[1]].Rank)
				{
					// レベルが上昇．
					var name = levels[current.Rank].Name;
					message.Subject = string.Format("{0}発令({1})", name, current_time);
					body += string.Format("{0}が発令されました．\n\n", name);

				}
				else if (recent_data[times[0]].Rank < recent_data[times[1]].Rank)
				{
					// レベルが下降．
					var name = levels[recent_data[times[1]].Rank].Name;
					message.Subject = string.Format("{0}解除({1})", name, current_time);
					message.Body = string.Format("{1}に，{0}が解除されました．", name, current_long_time);
					if (current.Rank > 0)
					{
						message.Body += string.Format("\n現在，{0}が発令されています．", levels[current.Rank].Name);
					}

				}
				body += this.MailSignature;


				// 送信する．
				if (separate_destinations)
				{
					foreach (var to in MailDestinations)
					{
						message.To.Clear();
						message.To.Add(to);
						this.MailSender.Post(message);
					}
				}
				else
				{
					foreach (var to in MailDestinations)
					{
						message.To.Add(to);
					}
					this.MailSender.Post(message);
				}

			}
			#endregion


			#region (1.1.0)プラグイン化

			public void Update()
			{
				DateTime latestDataTime = c_data.GetLatestDataTime();
				if (latestDataTime > _current)
				{
					Judge();
					_current = latestDataTime;
				}
			}
			DateTime _current;

			public void Configure(System.Xml.Linq.XElement config)
			{
				foreach (var element in config.Elements())
				{
					switch(element.Name.LocalName)
					{
						case "Levels":
							// レベルの定義を別のプラグインで行おうとすると，さらなるリフレクションが必要になるが...
							// AlertJudgementとレベル定義を一体化してしまった方がよい？
							// ↑とすると，ここで読み込むべき設定はほとんどない？パラメータをいじる程度か．
							foreach (var level_element in element.Elements("Level"))
							{
								this.DefineLevel(new AlertLevel(level_element));
							}
							break;
						case "Atom":
							foreach (var attribute in element.Attributes())
							{
								switch (attribute.Name.LocalName)
								{
									case "Destination":
										this.Inserted += (sender, e) => { this.OutputAtomFeed(attribute.Value); };
										break;
									case "Id":
										this.FeedID = attribute.Value;
										break;
									case "Author":
										this.FeedAuthor = attribute.Value;
										break;
									case "Title":
										this.FeedTitle = attribute.Value;
										break;
									case "SelfLink":
										this.FeedSelfLink = attribute.Value;
										break;
								}
							}
							break;
						// メール送信は後で設定？
						// <Mail SmtpServer="localhost" From="jappajil@...." ... >
						//   <To Address="kitsune@...." />
						//   <To Address="usagi@...." />
						//   <To Address="tanuki@...." />
						//   <Signature><![CDATA[～～～～～～]]>
						//   </Signature>
						// </Mail>
						case "Mail":
							int? port = (int?)element.Attribute("Port");
							MailSender = new Jappajil(element.Attribute("SmtpServer").Value, port ?? 25);

							MailSender.POPServer = (string)element.Attribute("PopServer");
							MailSender.UserName = (string)element.Attribute("UserName");
							MailSender.Password = (string)element.Attribute("Password");

							this.Inserted += (sender, e) => { this.SendMail(true);	/* 引数は決め打ち． */ };

							this.MailFrom = element.Attribute("From").Value;

							foreach (var child_element in element.Elements())
							{
								switch (child_element.Name.LocalName)
								{
									case "To":
										MailDestinations.Add(child_element.Attribute("Address").Value);
										break;
									case "Signature":
										this.MailSignature = child_element.Value;
										break;
								}
							}
							break;
					}
				}
			}

			#endregion

			public Jappajil MailSender
			{ get; set; }

			public string MailFrom { get; set; }
			/// <summary>
			/// メール本文の末尾につく署名です．
			/// </summary>
			public string MailSignature { get; set; }
			//public string PopServer { get; set; }

			#region *MailDestinationsプロパティ
			public IList<string> MailDestinations { get { return _mailDestinations; } }
			IList<string> _mailDestinations = new List<string>();
			#endregion

		}


		#region AlertLevelクラス
		public class AlertLevel
		{
			public int Rank { get; set; }
			public string Name { get; set; }

			// (1.1.0)
			#region *コンストラクタ(AlertLevel)
			public AlertLevel() { }
 
			public AlertLevel(XElement element)
			{

				this.Rank = (int)element.Attribute("Rank");
				this.Name = (string)element.Attribute("Name");


				this.InBorder = GenerateBorder(element.Element("In"));
				this.OutBorder = GenerateBorder(element.Element("Out"));
			}

			static Predicate<int[]> GenerateBorder(XElement element)
			{
				// ボーダーと一致する場合は，厳しい方に倒す．
				double border = (double)element.Attribute("Border");
				int span = (int)element.Attribute("Span");
				switch (element.Name.LocalName)
				{
					case "In":
						switch ((string)element.Attribute("Method"))
						{
							case "Minimum":
								return data => { return data.Take(span).Min() >= border; };
							case "Maximum":
								return data => { return data.Take(span).Max() >= border; };
							default:	// especially "Average":
								return data => { return data.Take(span).Average() >= border; };
						}
					case "Out":
						switch ((string)element.Attribute("Method"))
						{
							case "Minimum":
								return data => { return data.Take(span).Min() < border; };
							case "Maximum":
								return data => { return data.Take(span).Max() < border; };
							default:	// especially "Average":
								return data => { return data.Take(span).Average() < border; };
						}
					default:
						throw new ArgumentException();
				}

			}
			#endregion

			/// <summary>
			/// 発令条件を満たしたときにtrueを返します．
			/// </summary>
			public Predicate<int[]> InBorder { get; set; }

			/// <summary>
			/// 解除条件を満たしたときにtrueを返します．
			/// </summary>
			public Predicate<int[]> OutBorder { get; set; }
		}
		#endregion

	}
}