using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Linq;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Data.MySQL;
	using Helpers;

	namespace EvaluateAlerts.MySQL
	{
		// (1.4.0)
		#region AlertJudgementクラス
		public class AlertJudgement : IUpdatingPlugin
		{
			IDictionary<int, AlertLevel> levels;

			readonly AlertData data;
			readonly ConsumptionData c_data;

			// (1.1.0)レベルの決め打ちを解消．Configureメソッドから(のみ)設定できます．
			#region *コンストラクタ(AlertJudgement)
			public AlertJudgement(ConnectionProfile profile) : this()
			{
				data = new AlertData(profile);
				c_data = new ConsumptionData(profile);	// ←dataと分ける意味あるの？
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
			#region *判定を行う(Judge)
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

			// (1.2.1.6)解除チェック周辺のアルゴリズムを修正．
			// 新しい警報レベルを返します．
			int Check(int current_rank, int[] recent_data)
			{
				// 発令(InBorder)については，高い方から順にチェックし，高い方を満たしていればそれより低いのはチェックしません．
				// 「高いレベルの発令条件を満たすならばそれより低いレベルは全て満たす」ように設定して下さい．
				// (プログラムではそのチェックを行わないので，設定する人の責任で行って下さい．)

				// 現在より上のランクのレベルに達したか？
				foreach (var level in levels.OrderBy(l => -l.Key).Where((l) => l.Key > current_rank))
				{
					// InBorderをチェック．
					if (level.Value.InBorder(recent_data))
					{
						return level.Key;
					}
				}

				// (1.2.1.7)さらに修正(「解除されたレベル」より1つ低いレベルを返さなければいけないのだった)．
				// (1.2.1.6)解除(OutBorder)については，低い方から順にチェックし，低い方を満たしていればそれより高いのはチェックしません．
				// 「低いレベルの解除条件を満たすならばそれより高いレベルは全て満たす」ように設定して下さい．
				// (プログラムではそのチェックを行わないので，設定する人の責任で行って下さい．)
				int new_rank = 0;

				// (1.2.1.6)さらに修正．
				// (1.2.1.2)修正．
				// 現在より下のランクのレベルに達したか？
				foreach (var level in levels.OrderBy(l => l.Key).Where((l) => l.Key <= current_rank))
				{
					// OutBorderをチェック．
					if (level.Value.OutBorder(recent_data))	// 解除基準を満たした．
					{
						return new_rank;
					}
					else
					{
						new_rank = level.Key;
					}
				}
				// (1.2.1.6)修正．
				// (1.2.1.4)修正．
				return current_rank;	// 現状維持．(レベル設定の変更がなければnew_rankと等しいはず．)
			}
			#endregion


			#region 通知関連

			#region フィード用プロパティ

			public string FeedTitle { get; set; }
			public string FeedAuthor { get; set; }
			public string FeedID { get; set; }
			public string FeedSelfLink { get; set; }

			#endregion

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
						ID = this.FeedID + Data.DataTicker.TimeConverter.TimeToInt(current.DeclaredAt),
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


			#region メール送信関連プロパティ

			public Jappajil MailSender
			{ get; set; }

			/// <summary>
			/// Sender(すなわちReturn-Path？)やReplyToに設定されるアドレスを取得／設定します．
			/// </summary>
			public string MailReplyTo { get; set; }

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

			#endregion

			// (1.2.1)メールにReplyToを設定するように修正．
			#region (1.2.0)メール送信を実装．(とりあえず POP before SMTP 認証のみ．)
			private void SendMail(bool separate_destinations)
			{
				System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
				message.From = new System.Net.Mail.MailAddress(this.MailFrom);
				if (!string.IsNullOrEmpty(this.MailReplyTo))
				{
					message.Sender = new System.Net.Mail.MailAddress(this.MailReplyTo);
					message.ReplyToList.Add(this.MailReplyTo);
				}

				// message本文を作成する．
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
					body += string.Format("{1}に，{0}が解除されました．", name, current_long_time);	// (1.2.1.5)修正．
					if (current.Rank > 0)
					{
						message.Body += string.Format("\n現在，{0}が発令されています．", levels[current.Rank].Name);
					}

				}
				body += this.MailSignature;

				message.Body = body;	// (1.2.1.1)本文をmessageに設定(それまでされていなかったorz)．

				// (1.2.1.3)デバッグ用のログ表示を追加．
				// 送信する．
				if (separate_destinations)
				{
					foreach (var to in MailDestinations)
					{
						message.To.Clear();
						message.To.Add(to);
						this.MailSender.Post(message);
						Console.WriteLine("{0} にメールを送信しました．", to);
					}
				}
				else
				{
					foreach (var to in MailDestinations)
					{
						message.To.Add(to);
					}
					this.MailSender.Post(message);
					Console.WriteLine("みんな({0})にメールを送信しました．", string.Join(",", MailDestinations));
				}

			}
			#endregion

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
							this.MailReplyTo = element.Attribute("ReplyTo").Value;

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


		}
		#endregion


	}
}