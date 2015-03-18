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
					command.CommandText = string.Format("select data_time, rank from alerts where region = 1 and data_time > {0} and data_time < {1} order by data_time",
						Convert.TimeToInt(from), Convert.TimeToInt(to));
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
					command.CommandText = string.Format(
						"select declared_at, data_time, rank from alerts where region = 1 order by data_time desc limit {0}", n);
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


	}

	public struct AlertElement
	{
		public DateTime DeclaredAt;
		public DateTime DataTime;
		public int Rank;
	}


}
