using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;
using System.Xml;
using System.Xml.Linq;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Data
{
	public class AlertData : SQLiteData
	{
		// テーブル名: alerts
		//   region: 1(理工学部)，他の値はテスト用．
		//   data_time: 発令のもととなったデータの時刻．
		//   declared_at: 実際に発令された時刻．
		//   rank: 警報ランク


		// ひどいモデルだなぁ．
		public AlertData(string fileName) : base(fileName) { }

		public int GetCurrentRank()
		{
			using (var connection = new SQLiteConnection(this.ConnectionString))
			{
				connection.Open();
				using (SQLiteCommand command = connection.CreateCommand())
				{
					// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
					command.CommandText = "select rank from alerts where region = 1 order by data_time desc limit 1";
					using (var reader = command.ExecuteReader())
					{
						if (reader.Read())
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


		// 高い警報レベルが宣言されたデータのみを取り出します．
		public IDictionary<DateTime, int> GetDeclaredData(DateTime from, DateTime to)
		{
			var warnings = new Dictionary<DateTime, int>();

			using (var connection = new SQLiteConnection(this.ConnectionString))
			{
				connection.Open();
				using (SQLiteCommand command = connection.CreateCommand())
				{
					// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
					command.CommandText
						= "select data_time, rank from alerts where region = 1 and data_time > @from and data_time < @to order by data_time";
					command.Parameters.Add(new SQLiteParameter("@from",Convert.TimeToInt(from)));
					command.Parameters.Add(new SQLiteParameter("@to", Convert.TimeToInt(to)));
					using (var reader = command.ExecuteReader())
					{
						int current = 0;
						while (reader.Read())
						{
							DateTime time = Convert.IntToTime(System.Convert.ToInt32(reader["data_time"]));
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

		// 最近n件のレコードを取得します．
		public IDictionary<DateTime, AlertElement> GetRecentData(int n)
		{
			var alerts = new Dictionary<DateTime, AlertElement>();

			using (var connection = new SQLiteConnection(this.ConnectionString))
			{
				connection.Open();
				using (SQLiteCommand command = connection.CreateCommand())
				{
					// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
					command.CommandText
						= "select declared_at, data_time, rank from alerts where region = 1 order by data_time desc limit @1";
					command.Parameters.Add(new SQLiteParameter("@1", n));
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							DateTime time = Convert.IntToTime(System.Convert.ToInt32(reader["declared_at"]));
							DateTime data_time = Convert.IntToTime(System.Convert.ToInt32(reader["data_time"]));
							int rank = System.Convert.ToInt32(reader["rank"]);

							alerts.Add(time, new AlertElement { DataTime = data_time, DeclaredAt = time, Rank = rank });
						}
					}
				}
			}
			return alerts;

		}

		// (1.1.2.0)
		#region *データを追加(InsertData)
		/// <summary>
		/// データを追加します．
		/// </summary>
		/// <param name="data"></param>
		/// <param name="region"></param>
		public void InsertData(AlertElement data, int region = 1)
		{
			using (var connection = new SQLiteConnection(this.ConnectionString))
			{
				connection.Open();
				using (SQLiteCommand command = connection.CreateCommand())
				{
					// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
					command.CommandText = "INSERT INTO alerts VALUES(@region, @datatime, @declared, @rank)";
					command.Parameters.Add(new SQLiteParameter("@region", region));
					command.Parameters.Add(new SQLiteParameter("@datatime", Convert.TimeToInt(data.DataTime)));
					command.Parameters.Add(new SQLiteParameter("@declared", Convert.TimeToInt(data.DeclaredAt)));
					command.Parameters.Add(new SQLiteParameter("@rank", data.Rank));

					command.ExecuteNonQuery();
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
