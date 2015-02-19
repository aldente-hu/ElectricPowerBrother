using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Data
{
	public class AlertData : ConsumptionData
	{
		// ひどいモデルだなぁ．
		public AlertData(string fileName) : base(fileName) { }

		public IDictionary<DateTime, int> GetDeclaredData(DateTime from, DateTime to)
		{
			// 高い警報レベルが宣言されたデータのみを取り出します．
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


	}


}
