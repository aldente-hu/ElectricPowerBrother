using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{

	using Helpers;
	using Data.New;

	namespace New
	{
		// (1.2.0)
		#region CunsumptionAtomGeneratorクラス
		public class ConsumptionAtomGenerator : ConsumptionData, Helpers.New.IPlugin
		{

			#region *コンストラクタ(ConsumptionAtomGenerator) ; 実質的な実装はなし
			public ConsumptionAtomGenerator(Data.IConnectionProfile profile) : base(profile) { }
			#endregion

			public string Destination { get; set; }

			public string Author { get; set; }
			public string Title { get; set; }
			public string ID { get; set; }
			public string SelfLink { get; set; }
			public string AlternateLink { get; set; }

			/// <summary>
			/// この後に時刻(整数)をつけたものが各記事のIDになります．
			/// このプロパティを設定しないと，IDプロパティの値が使われます．
			/// </summary>
			#region *EntryIDBaseプロパティ
			public string EntryIDBase
			{
				get { return string.IsNullOrEmpty(_entryIDBase) ? ID : _entryIDBase; }
				set { this._entryIDBase = value; }
			}
			string _entryIDBase = string.Empty;
			#endregion

			#region *Atomフィードを出力(Output)
			/// <summary>
			/// 指定した時刻時点でのAtomフィードを出力します．
			/// </summary>
			/// <param name="latestDataTime"></param>
			public async Task OutputAsync(DateTime latestDataTime)
			{
				var time = latestDataTime;

				AtomFeed feed = new AtomFeed
				{
					Author = this.Author,
					Title = this.Title,
					ID = this.ID,
					SelfLink = this.SelfLink,
					AlternateLink = this.AlternateLink,
					UpdatedAt = DateTime.Now
				};

				for (int i = 0; i < 3; i++)
				{
					var consumptions = await GetConsumptionsOnAsync(time);
					try
					{
						// ☆string.Formatの文字列をリソースとして与えるのはどうだろう？

						// 総情センターの表示をいったん削除する．
						AtomEntry entry = new AtomEntry
						{
							Content = string.Format("{0}までの10分間電力消費量[kWh] 1号館 : {1} 2号館 : {2}",
								time.ToString("MM月dd日HH時mm分"), consumptions[1], consumptions[2]),
							ID = this.EntryIDBase + TimeConverter.TimeToInt(time),
							Title = string.Format("{0}の電力消費量 ({1},{2})",
								time.ToString("dd日HH:mm"), consumptions[1], consumptions[2]),
							PublishedAt = time
						};

						feed.Entries.Add(entry);
					}
					catch (KeyNotFoundException) { }  // 3チャンネル分のデータがとれなければすっ飛ばす．
					time = time.AddMinutes(-10);
				}

				if (feed.Entries.Count > 0)
				{
					// 出力
					// ここを非同期化したい。
					using (XmlWriter writer = XmlWriter.Create(this.Destination, new XmlWriterSettings { Indent = true }))
					{
						feed.OutputDocument().WriteTo(writer);
					}
				}
			}
			#endregion


			// これはどこで定義すべきか？

			#region *指定時刻のデータを取得(GetConsumptionsOn)
			/// <summary>
			/// 指定された時刻のch毎のデータを取得します．
			/// </summary>
			/// <param name="time"></param>
			/// <returns></returns>
			public async Task<IDictionary<int, int>> GetConsumptionsOnAsync(DateTime time)
			{
				Dictionary<int, int> consumptions = new Dictionary<int, int>();

				using (var connection = await profile.GetConnectionAsync())
				{
					using (var command = connection.CreateCommand())
					{
						// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
						command.CommandText = "select ch, consumption from consumptions_10min where e_time = @1";
						command.Parameters.Add(profile.CreateParameter("@1", TimeConverter.TimeToInt(time)));

						using (var reader = command.ExecuteReader())
						{
							while (await reader.ReadAsync())
							{
								int ch = System.Convert.ToInt32(reader["ch"]);
								int consumption = System.Convert.ToInt32(reader["consumption"]);
								consumptions.Add(ch, consumption);
							}
						}
					}
					return consumptions;
				}


			}
			#endregion


			#region (1.3.0)プラグイン化

			public void Configure(System.Xml.Linq.XElement config)
			{
				foreach (var attribute in config.Attributes())
				{
					switch (attribute.Name.LocalName)
					{
						case "Destination":
							this.Destination = attribute.Value;
							break;
						case "Title":
							this.Title = attribute.Value;
							break;
						case "Author":
							this.Author = attribute.Value;
							break;
						case "SelfLink":
							this.SelfLink = attribute.Value;
							break;
						case "AlternateLink":
							this.AlternateLink = attribute.Value;
							break;
						case "Id":
							this.ID = attribute.Value;
							break;
						case "EntryIdBase":
							this.EntryIDBase = attribute.Value;
							break;
					}
				}
				this.UpdateAction = async (date) => { await OutputAsync(date); };

			}

			#endregion
		}
		#endregion

	}

}
