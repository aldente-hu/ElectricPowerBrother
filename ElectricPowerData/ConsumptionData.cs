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

		#region *コンストラクタ(ConsumptionData) ; 実質的実装はなし
		public ConsumptionData(string fileName)
			: base(fileName)
		{ }
		#endregion


		#region *最新データの時刻を取得(GetLatestDataTime)
		/// <summary>
		/// 最新データの時刻(ch1, ch2のデータが揃っている時刻)を取得します．
		/// </summary>
		/// <returns></returns>
		public override DateTime GetLatestDataTime()
		{
			using (var connection = new SQLiteConnection(this.ConnectionString))
			{
				connection.Open();
				// "select min(latest) from (select ch, max(e_time) as latest from consumptions_10min where ch in (1, 2) group by ch)"
				// で一発なのだが，サブクエリ部分の結果が返るのに3秒くらいかかったので，
				// ch間の比較はプログラム側で行うことにする(↓のクエリはほぼ一瞬で返る)．
				var latest1 = GetLatestTime(connection, 1);
				var latest2 = GetLatestTime(connection, 2);
				//connection.Close();

				return latest1 < latest2 ? latest1 : latest2;
			}

		}
		#endregion

		// privateでいいのでは？
		#region *指定したchの最新データの時刻を取得(GetLatestTime)
		/// <summary>
		/// 指定したチャンネルの最新データの時刻を取得します．
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="ch"></param>
		/// <returns></returns>
		protected DateTime GetLatestTime(SQLiteConnection connection, int ch)
		{
			using (SQLiteCommand command = connection.CreateCommand())
			{
				// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
				command.CommandText = "select max(e_time) from consumptions_10min where ch = @ch";
				command.Parameters.Add(new SQLiteParameter("@ch", ch));

				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					return Convert.IntToTime(System.Convert.ToInt32(reader[0]));
				}
			}
		}
		#endregion


		// (1.1.7)単独チャンネルにも対応．
		// (1.1.1.0)新しい方からn件のデータを取得します(ch1と2の和)．
		#region *最近のデータを取得(GetRecentData)
		/// <summary>
		/// 新しい方からn件のデータ(ch1とch2の和)を取得します．
		/// </summary>
		/// <param name="n">データを取得する数．</param>
		/// <returns></returns>
		public IDictionary<DateTime, int> GetRecentData(int n)
		{
			return GetRecentDataBase(n, "ch in (1, 2)");
		}

		public IDictionary<DateTime, int> GetRecentData(int n, int ch)
		{
			return GetRecentDataBase(n, string.Format("ch = {0}", ch));
		}

		IDictionary<DateTime, int> GetRecentDataBase(int n, string ch_condition)
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
						"select e_time, sum(consumption) as total from consumptions_10min where e_time <= @from and e_time > @to and {0} group by e_time",
						ch_condition);
					command.Parameters.Add(new SQLiteParameter("@from", Convert.TimeToInt(time)));
					command.Parameters.Add(new SQLiteParameter("@to", Convert.TimeToInt(time.AddMinutes(-10 * n))));

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
		#endregion

		// before以前3週間の隔日の合計を取得します．
		#region *日合計を取得(GetLatestDaily)
		/// <summary>
		/// 指定した日以前の3週間についての日合計を取得します．
		/// </summary>
		/// <param name="before">指定した日以前のデータを取得します．自身は含みません．</param>
		/// <returns></returns>
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
					command.CommandText = 
						"select date, sum(consumption) as total from jdhm_consumptions_10min where date < @date and date >= @date - 21 and ch in (1, 2) group by date";
					command.Parameters.Add(new SQLiteParameter("@date", Convert.DateToInt(before)));
					
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
		#endregion

		// dateの各hour毎の合計を取得します．
		#region *1時間ごとのデータを取得(GetHourlyConsumptions)
		/// <summary>
		/// 指定した日の1時間毎のデータを取得します(ch1とch2の合計)．
		/// </summary>
		/// <param name="date">データ取得の対象日．</param>
		/// <param name="completed_only">trueであれば，データが揃っている時間だけを返します．</param>
		/// <returns></returns>
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
					command.CommandText = 
						"select hour + 1 as e_hour, count(consumption) as cnt, sum(consumption) as total from jdhm_consumptions_10min where date = @date and ch in (1, 2) group by hour";
					command.Parameters.Add(new SQLiteParameter("@date", Convert.DateToInt(date)));

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
				//connection.Close();
			}
			return hourlyConsumptions;

		}
		#endregion

		// from自身は含まず，to自身は含みます．
		#region *消費量データを取得(GetDetailConsumptions)
		/// <summary>
		/// 指定した期間のデータ(ch1とch2の合計)を取得します．
		/// </summary>
		/// <param name="from">期間の始点です．この時刻のデータは含まれません．</param>
		/// <param name="to">期間の終点です．この時刻のデータが含まれます．</param>
		/// <returns></returns>
		public IDictionary<DateTime, int> GetDetailConsumptions(DateTime from, DateTime to)
		{
			var detailConsumptions = new Dictionary<DateTime, int>();

			using (var connection = new SQLiteConnection(ConnectionString))
			{
				connection.Open();
				using (SQLiteCommand command = connection.CreateCommand())
				{
					// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
					command.CommandText = 
						"select e_time, sum(consumption) as total from consumptions_10min where e_time > @1 and e_time <= @2 and ch in (1, 2) group by e_time";
					command.Parameters.Add(new SQLiteParameter("@1", Convert.TimeToInt(from)));
					command.Parameters.Add(new SQLiteParameter("@2", Convert.TimeToInt(to)));
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
				//connection.Close();
			}

			return detailConsumptions;
		}
		#endregion

		// (1.1.5) chを指定できるようにしました．
		// (1.1.3)
		/// <summary>
		/// e_timeがfromより後でtoまでのデータを全て返します．
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public IDictionary<DateTime, IDictionary<int, int>> GetParticularConsumptions(DateTime from, DateTime to, params int[] channels)
		{

			var data = new Dictionary<DateTime, IDictionary<int,int>>();

			using (var connection = new SQLiteConnection(ConnectionString))
			{
				connection.Open();
				using (SQLiteCommand command = connection.CreateCommand())
				{
					// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．

					// IN演算子でCommandParameterを使う方法がわからないので，string.Formatでごまかす．
					// intなので，SQLインジェクションの心配はないよね？
					command.CommandText =
						string.Format("select e_time, ch, consumption from consumptions_10min where e_time > @from and e_time <= @to and ch in ({0})", string.Join(",", channels));
					command.Parameters.Add(new SQLiteParameter("@from", Convert.TimeToInt(from)));
					command.Parameters.Add(new SQLiteParameter("@to", Convert.TimeToInt(to)));
					//command.Parameters.Add(new SQLiteParameter("@ch", channels));
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							DateTime e_time = Convert.IntToTime(System.Convert.ToInt32(reader["e_time"]));
							int ch = System.Convert.ToInt32(reader["ch"]);
							int consumption = System.Convert.ToInt32(reader["consumption"]);
							if (!data.Keys.Contains(e_time))
							{
								data.Add(e_time, new Dictionary<int, int>());
							}
							data[e_time].Add(ch, consumption);	// e_timeとchがともに重複することはないはずだが….
						}
					}

				}
				//connection.Close();
			}
			return data;

		}

		public IDictionary<DateTime, IDictionary<int, int>> GetParticularConsumptions(DateTime from, DateTime to)
		{
			// かつての実装．
			return GetParticularConsumptions(from, to, 1, 2);
		}

		// (1.1.6)
		public int GetMonthlyTotal(DateTime month, params int[] channels)
		{
			// monthが 08/31 なら，8月の合計，
			// monthが 09/01 00:00 なら8月の合計，
			// monthが 09/01 00:01 なら9月の合計を返す．

			month = month.AddSeconds(-1);
			var from = new DateTime(month.Year, month.Month, 1);
			var to = from.AddDays(31);
			to = to.AddDays(to.Day - 1);

			return GetTotal(from, to, channels);
		}

		// (1.1.6)
		public int GetTotal(DateTime from, DateTime to, params int[] channels)
		{
			using (var connection = new SQLiteConnection(ConnectionString))
			{
				connection.Open();
				using (SQLiteCommand command = connection.CreateCommand())
				{
					// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．

					// IN演算子でCommandParameterを使う方法がわからないので，string.Formatでごまかす．
					// intなので，SQLインジェクションの心配はないよね？
					command.CommandText =
						string.Format("select sum(consumption) as total from consumptions_10min where e_time > @from and e_time <= @to and ch in ({0})", string.Join(",", channels));
					command.Parameters.Add(new SQLiteParameter("@from", Convert.TimeToInt(from)));
					command.Parameters.Add(new SQLiteParameter("@to", Convert.TimeToInt(to)));
					//command.Parameters.Add(new SQLiteParameter("@ch", channels));
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							return System.Convert.ToInt32(reader["total"]);
						}
						// こんなことあるのか？
						throw new ApplicationException("データベースから合計値が得られませんでした．");
					}

				}
				//connection.Close();
			}

		}


		#region *trinityを決定(DefineTrinity)
		/// <summary>
		/// 指定した日時に対して，trinityを決定します．
		/// </summary>
		/// <param name="current">trinityを決定する対象の日時．</param>
		/// <returns></returns>
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
		#endregion

		// ※↓これ使ってるの？
		// timeの1時間前は含まない．
		#region *前後1時間のデータを取得(Around1hour)
		/// <summary>
		/// 指定した時刻の前後1時間のデータ(ch1とch2の合計)を，時刻順(古→新)の配列として返します．
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public int[] Around1hour(DateTime time)
		{
			var consumptions = new List<int>();

			using (var connection = new SQLiteConnection(ConnectionString))
			{
				connection.Open();
				using (SQLiteCommand command = connection.CreateCommand())
				{
					// ☆Commandの書き方は他にも用意されているのだろう(と信じたい)．
					command.CommandText = "select sum(consumption) as total from consumptions_10min where e_time > @from and e_time <= @to and ch in (1, 2) group by e_time order by e_time";
					command.Parameters.Add(new SQLiteParameter("@from", Convert.TimeToInt(time.AddHours(-1))));
					command.Parameters.Add(new SQLiteParameter("@to", Convert.TimeToInt(time.AddHours(1))));
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							consumptions.Add(System.Convert.ToInt32(reader[0]));
						}
					}
				}
				//connection.Close();
			}

			return consumptions.ToArray();

		}
		#endregion




	}


}
