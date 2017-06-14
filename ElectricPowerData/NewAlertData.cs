using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Common;

using System.Xml;
using System.Xml.Linq;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Data
{

	namespace New
	{
		public class AlertData : DataModelBase
		{
			// テーブル名: alerts
			//   region: 1(理工学部)，他の値はテスト用．
			//   data_time: 発令のもととなったデータの時刻．
			//   declared_at: 実際に発令された時刻．
			//   rank: 警報ランク


			// ひどいモデルだなぁ．
			#region *定番コンストラクタ(AlertData)
			public AlertData(IConnectionProfile profile) : base(profile)
			{
			}
			#endregion

			#region *現在のランクを取得(GetCurrentRankAsync)
			public async Task<int> GetCurrentRankAsync()
			{
				using (var connection = await profile.GetConnectionAsync())
				{
					using (var command = connection.CreateCommand())
					{
						// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
						command.CommandText = "select rank from alerts where region = 1 order by data_time desc limit 1";
						using (var reader = await command.ExecuteReaderAsync())
						{
							if (await reader.ReadAsync())
							{
								return System.Convert.ToInt32(reader["rank"]);
							}
							else
							{
								// とりあえず．
								return 0;
							}
						}
					}
				}
			}
			#endregion

			// 高い警報レベルが宣言("発令"(←→"解除"))されたデータのみを取り出します．
			#region *発令データを取得(GetDeclaredData)
			public async Task<IDictionary<DateTime, int>> GetDeclaredDataAsync(DateTime from, DateTime to)
			{
				var warnings = new Dictionary<DateTime, int>();

				using (var connection = await profile.GetConnectionAsync())
				{
					using (var command = connection.CreateCommand())
					{
						// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
						command.CommandText
							= "select data_time, rank from alerts where region = 1 and data_time > @from and data_time < @to order by data_time";
						command.Parameters.Add(profile.CreateParameter("@from", TimeConverter.TimeToInt(from)));
						command.Parameters.Add(profile.CreateParameter("@to", TimeConverter.TimeToInt(to)));
						using (var reader = await command.ExecuteReaderAsync())
						{
							int current = 0;
							while (await reader.ReadAsync())
							{
								DateTime time = TimeConverter.IntToTime(System.Convert.ToInt32(reader["data_time"]));
								int rank = System.Convert.ToInt32(reader["rank"]);
								if (rank > current)
								{
									//Console.WriteLine("{0} : {1}", date, total);
									warnings.Add(time, rank);
								}
								current = rank;
							}
						}
					}
				}
				return warnings;
			}
			#endregion

			// 最近n件のレコードを取得します．
			#region *最近のデータを取得(GetRecentData)
			public async Task<IDictionary<DateTime, AlertElement>> GetRecentDataAsync(int n)
			{
				var alerts = new Dictionary<DateTime, AlertElement>();

				using (var connection = await profile.GetConnectionAsync())
				{
					connection.Open();
					using (var command = connection.CreateCommand())
					{
						// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
						command.CommandText
							= "select declared_at, data_time, rank from alerts where region = 1 order by data_time desc limit @1";
						command.Parameters.Add(profile.CreateParameter("@1", n));
						using (var reader = await command.ExecuteReaderAsync())
						{
							while (await reader.ReadAsync())
							{
								DateTime time = TimeConverter.IntToTime(System.Convert.ToInt32(reader["declared_at"]));
								DateTime data_time = TimeConverter.IntToTime(System.Convert.ToInt32(reader["data_time"]));
								int rank = System.Convert.ToInt32(reader["rank"]);

								alerts.Add(time, new AlertElement { DataTime = data_time, DeclaredAt = time, Rank = rank });
							}
						}
					}
				}
				return alerts;

			}
			#endregion

			// (1.1.2.0)
			#region *データを追加(InsertData)
			/// <summary>
			/// データを追加します．
			/// </summary>
			/// <param name="data"></param>
			/// <param name="region"></param>
			public async Task InsertDataAsync(AlertElement data, int region = 1)
			{
				using (var connection = await profile.GetConnectionAsync())
				{
					connection.Open();
					using (var command = connection.CreateCommand())
					{
						// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
						command.CommandText = "INSERT INTO alerts VALUES(@region, @datatime, @declared, @rank)";
						command.Parameters.Add(profile.CreateParameter("@region", region));
						command.Parameters.Add(profile.CreateParameter("@datatime", TimeConverter.TimeToInt(data.DataTime)));
						command.Parameters.Add(profile.CreateParameter("@declared", TimeConverter.TimeToInt(data.DeclaredAt)));
						command.Parameters.Add(profile.CreateParameter("@rank", data.Rank));

						await command.ExecuteNonQueryAsync();
					}
				}
			}
			#endregion

		}

		// (1.1.1.2)
		#region AlertElement構造体
		public struct AlertElement
		{
			public DateTime DeclaredAt;
			public DateTime DataTime;
			public int Rank;
		}
		#endregion

	}
}
