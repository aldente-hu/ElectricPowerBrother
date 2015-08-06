using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

using System.Xml.Linq;
using System.Xml;

namespace SQLiteNetTest
{

	public class ElectricPowerData
	{
		// とりあえず1つのファイルに1つのインスタンスを対応させてみる．

		public string FileName
		{
			get { return _fileName; }
		}
		readonly string _fileName;

		/// <summary>
		/// データベースへの接続文字列を取得します．
		/// </summary>
		protected string ConnectionString
		{
			get
			{
				return string.Format("Data source={0}", FileName);
			}
		}

		public ElectricPowerData(string fileName)
		{
			this._fileName = fileName;
		}



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
						DateToInt(before));
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							int date = Convert.ToInt32(reader["date"]);
							int total = Convert.ToInt32(reader["total"]);
							//Console.WriteLine("{0} : {1}", date, total);
							dailyConsumptions.Add(IntToDate(date), total);
						}
					}
				}
			}
			return dailyConsumptions;

		}



		public IDictionary<int, int> GetHourlyConsumptions(DateTime date)
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
						"select hour + 1 as e_hour, sum(consumption) as total from jdhm_consumptions_10min where date = {0} and ch in (1, 2) group by hour",
						DateToInt(date));
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							int hour = Convert.ToInt32(reader["e_hour"]);
							int total = Convert.ToInt32(reader["total"]);
							//Console.WriteLine("{0} : {1}", hour, total);
							hourlyConsumptions.Add(hour, total);
						}
					}
				}
			}
			return hourlyConsumptions;

		}





		public void OutputXml(string destination)
		{
			// 1.ドキュメントを生成．

			XDocument doc = new XDocument(new XElement("daily_consumptions"));

			DateTime today = DateTime.Today;
			var consumptions = GetLatestDaily(today).OrderByDescending(p => p.Value);
			int size = consumptions.Count();
			var max_date = consumptions.First().Key;
			var std_date = consumptions.Skip((size - 1) / 2).First().Key;

			DateTime[] days = new DateTime[] { today, max_date, std_date };
			for (int i = 0; i < 3; i++)
			{
				XElement elem = new XElement("daily", new XAttribute("date", days[i].ToString("yyyy-MM-dd")));
				foreach (var data in GetHourlyConsumptions(days[i]))
				{
					elem.Add(
						new XElement("hourly", new XAttribute("hour", data.Key), data.Value)
					);
				}
				doc.Root.Add(elem);
			}

			// 2.ドキュメントを出力．

			XmlWriterSettings settings =
				new XmlWriterSettings
				{
					NewLineHandling = NewLineHandling.Replace,
					Indent = true,
					IndentChars = "\t"
				};
			using (XmlWriter writer = XmlWriter.Create(destination, settings))
			{
				doc.WriteTo(writer);
			}


		}




		// 以下はもう少し上の階層に記述してもよい．
		// DateTime型か，DateTimeOffset型かは後々見直すことにする．

		static DateTime DateOrigin = new DateTime(1970, 1, 1);

		static DateTime IntToDate(int date)
		{
			return DateOrigin.AddDays(date);
		}

		static int DateToInt(DateTime date)
		{
			return date.Subtract(DateOrigin).Days;
		}

	}


}
