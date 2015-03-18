using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;


namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Data
{

	public class ConsumptionData : SQLiteData
	{

		public ConsumptionData(string fileName)
			: base(fileName)
		{ }


		public override DateTime GetLatestDataTime()
		{
			return GetRikoLatestTime();
			// いずれGetRikoLatestTimeの実装で置き換えよう，と考え中．
		}

		[Obsolete("GetLatestDataTimeメソッドを使用して下さい．")]
		public DateTime GetRikoLatestTime()
		{
			using (var connection = new SQLiteConnection(this.ConnectionString))
			{
				connection.Open();
				// "select min(latest) from (select ch, max(e_time) as latest from consumptions_10min where ch in (1, 2) group by ch)"
				// で一発なのだが，サブクエリ部分の結果が返るのに3秒くらいかかったので，
				// ch間の比較はプログラム側で行うことにする(↓のクエリはほぼ一瞬で返る)．
				var latest1 = GetLatestTime(connection, 1);
				var latest2 = GetLatestTime(connection, 2);
				connection.Close();

				return latest1 < latest2 ? latest1 : latest2;
			}

		}

		protected DateTime GetLatestTime(SQLiteConnection connection, int ch)
		{
			using (SQLiteCommand command = connection.CreateCommand())
			{
				// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
				command.CommandText = string.Format(
					"select max(e_time) from consumptions_10min where ch = {0}",
					ch);
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					return Convert.IntToTime(System.Convert.ToInt32(reader[0]));
				}
			}
		}

		// (1.1.1.0)新しい方からn件のデータを取得します(ch1と2の和)．
		public IDictionary<DateTime, int> GetRecentData(int n)
		{
			IDictionary<DateTime, int> data = new Dictionary<DateTime, int>();
			var time = GetLatestDataTime();

			using (var connection = new SQLiteConnection(this.ConnectionString))
			{
				connection.Open();
				using (SQLiteCommand command = connection.CreateCommand())
				{
					// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
					command.CommandText = string.Format(
						"select e_time, sum(consumption) as total from consumptions_10min where e_time <= {0} and e_time > {1} and ch in (1, 2) group by e_time",
						Convert.TimeToInt(time), Convert.TimeToInt(time.AddMinutes(-10 * n)));
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							DateTime data_time = Convert.IntToTime(System.Convert.ToInt32(reader["e_time"]));
							int total = System.Convert.ToInt32(reader["total"]);
							//Console.WriteLine("{0} : {1}", date, total);
							data.Add(data_time, total);
						}
					}
				}
			}
			return data;
		}

		// before以前3週間の隔日の合計を取得します．
		public IDictionary<DateTime, int> GetLatestDaily(DateTime before)
		{
			// キーがdate，値がtotalに対応．
			var dailyConsumptions = new Dictionary<DateTime, int>();

			//int today = DateToInt(DateTimeOffset.Now);
			//Console.WriteLine(today);

			using (var connection = new SQLiteConnection(this.ConnectionString))
			{
				connection.Open();
				using (SQLiteCommand command = connection.CreateCommand())
				{
					// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
					command.CommandText = string.Format(
						"select date, sum(consumption) as total from jdhm_consumptions_10min where date < {0} and date >= {0} - 21 and ch in (1, 2) group by date",
						Convert.DateToInt(before));
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							int date = System.Convert.ToInt32(reader["date"]);
							int total = System.Convert.ToInt32(reader["total"]);
							//Console.WriteLine("{0} : {1}", date, total);
							dailyConsumptions.Add(Convert.IntToDate(date), total);
						}
					}
				}
			}
			return dailyConsumptions;

		}


		// dateの各hour毎の合計を取得します．
		public IDictionary<int, int> GetHourlyConsumptions(DateTime date, bool completed_only = true)
		{
			// キーがhour，値がtotalに対応．
			var hourlyConsumptions = new Dictionary<int, int>();

			using (var connection = new SQLiteConnection(ConnectionString))
			{
				connection.Open();
				using (SQLiteCommand command = connection.CreateCommand())
				{
					// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
					command.CommandText = string.Format(
						"select hour + 1 as e_hour, count(consumption) as cnt, sum(consumption) as total from jdhm_consumptions_10min where date = {0} and ch in (1, 2) group by hour",
						Convert.DateToInt(date));
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							if (!completed_only || System.Convert.ToInt32(reader["cnt"]) == 12)
							{
								int hour = System.Convert.ToInt32(reader["e_hour"]);
								int total = System.Convert.ToInt32(reader["total"]);
								//Console.WriteLine("{0} : {1}", hour, total);
								hourlyConsumptions.Add(hour, total);
							}
						}
					}
				}
				connection.Close();
			}
			return hourlyConsumptions;

		}

		// from自身は含まず，to自身は含みます．
		public IDictionary<DateTime, int> GetDetailConsumptions(DateTime from, DateTime to)
		{
			var detailConsumptions = new Dictionary<DateTime, int>();

			using (var connection = new SQLiteConnection(ConnectionString))
			{
				connection.Open();
				using (SQLiteCommand command = connection.CreateCommand())
				{
					// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
					command.CommandText = string.Format(
						"select e_time, sum(consumption) as total from consumptions_10min where e_time > {0} and e_time <= {1} and ch in (1, 2) group by e_time",
						Convert.TimeToInt(from), Convert.TimeToInt(to));
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							DateTime e_time = Convert.IntToTime(System.Convert.ToInt32(reader["e_time"]));
							int total = System.Convert.ToInt32(reader["total"]);
							detailConsumptions.Add(e_time, total);
						}
					}
				}
				connection.Close();
			}

			return detailConsumptions;
		}



		// 現在の日時に対して，trinityを決定します．
		public IDictionary<string, DateTime> DefineTrinity(DateTime current)
		{
			var trinity = new Dictionary<string,DateTime>();

			TimeSpan timeOfDay = current.TimeOfDay;	// 時刻の情報だけ取り出している．
			DateTime today = current - timeOfDay;	// 日付の情報だけ取り出している．
			// とりあえず無条件で．
			//trinity["本日"] = today;
			trinity["本日"] = current;

			var consumptions = GetLatestDaily(today).OrderByDescending(p => p.Value);
			int size = consumptions.Count();
			trinity["最大"] = consumptions.First().Key + timeOfDay;
			trinity["標準"] = consumptions.Skip((size - 1) / 2).First().Key + timeOfDay;
			return trinity;
		}

		// timeの1時間前は含まない．
		public int[] Around1hour(DateTime time)
		{
			var consumptions = new List<int>();

			using (var connection = new SQLiteConnection(ConnectionString))
			{
				connection.Open();
				using (SQLiteCommand command = connection.CreateCommand())
				{
					// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
					command.CommandText = string.Format(
						"select sum(consumption) as total from consumptions_10min where e_time > {0} and e_time <= {1} and ch in (1, 2) group by e_time order by e_time",
						Convert.TimeToInt(time.AddHours(-1)), Convert.TimeToInt(time.AddHours(1)));
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							consumptions.Add(System.Convert.ToInt32(reader[0]));
						}
					}
				}
				connection.Close();
			}

			return consumptions.ToArray();

		}





	}


}
