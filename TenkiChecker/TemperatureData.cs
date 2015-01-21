using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Data.SQLite;
using ElectricPowerData;

using System.Xml.Linq;
using System.Xml;

namespace TenkiChecker
{

	public class TemperatureData
	 : SQLiteDateTimeConverting
	{

		public TemperatureData(string fileName)
			: base(fileName)
		{ }

		// temperatures
		//   - time integer
		//   - temperature integer
		// temperatureは摂氏温度の10倍の値を保持する．


		public DateTime GetLatestTime()
		{
			using (var connection = new SQLiteConnection(this.ConnectionString))
			{
				DateTime time;
				connection.Open();
				using (SQLiteCommand command = connection.CreateCommand())
				{
					command.CommandText = "select max(time) from temperatures";
					using (var reader = command.ExecuteReader())
					{
						reader.Read();
						time = IntToTime(Convert.ToInt32(reader[0]));
					}
				}
				connection.Close();
				return time;
			}

		}


		public IDictionary<DateTime, decimal> GetTemperatures(DateTime from, DateTime to)
		{
			var temperatures = new Dictionary<DateTime, decimal>();

			using (var connection = new SQLiteConnection(this.ConnectionString))
			{
				connection.Open();
				using (SQLiteCommand command = connection.CreateCommand())
				{
					command.CommandText = string.Format(
						"select time, temperature from temperatures where time >= {0} and time <= {1}", TimeToInt(from), TimeToInt(to));
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							temperatures.Add(IntToTime(Convert.ToInt32(reader[0])), IntToTemperature(Convert.ToInt32(reader[1])));
						}
					}
				}
				connection.Close();
				return temperatures;
			}

		}

		protected IDictionary<DateTime, decimal> GetOneDayTemperatures(DateTime date)
		{
			var from = date - date.TimeOfDay;
			var to = from.AddDays(1);
			return GetTemperatures(from, to);
		}

		public void InsertTemperature(DateTime time, decimal temperature)
		{
			using (var connection = new SQLiteConnection(this.ConnectionString))
			{
				connection.Open();
				using (SQLiteCommand command = connection.CreateCommand())
				{
					command.CommandText = string.Format(
						"INSERT INTO temperatures VALUES({0}, {1})", TimeToInt(time), TemperatureToInt(temperature));
					command.ExecuteNonQuery();
				}
				connection.Close();
			}

		}






		static int TemperatureToInt(decimal temperature)
		{
			return Convert.ToInt32(decimal.Ceiling(temperature * 10));
		}

		static decimal IntToTemperature(int data)
		{
			return data / 10.0M;
		}


		// 引数は，"-3.6"とか．
		//static int ConvertToInt(string temperature)
		//{
		//	return TemperatureToInt(decimal.Parse(temperature));
		//}

	}

}
