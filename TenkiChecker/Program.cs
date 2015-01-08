using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace TenkiChecker
{

	class Program
	{
		static System.Globalization.CultureInfo JpCulture = new System.Globalization.CultureInfo("ja-JP");

		static Properties.Settings MySettings
		{
			get
			{
				return TenkiChecker.Properties.Settings.Default;
			}
		}

		static TemperatureData TemperatureDB;
		static TemperatureCsvGenerator CsvGenerator;
		static System.Threading.Timer ticker1;
		static System.Threading.Timer ticker2;
		static System.Threading.Timer ticker3;

		static void Main(string[] args)
		{
			TemperatureDB = new TemperatureData(MySettings.DatabaseFile);
			CsvGenerator = new TemperatureCsvGenerator(MySettings.DatabaseFile);
			Console.WriteLine("Press the Enter key to end program.");
			ticker1 = new System.Threading.Timer(GetTemperature, null, 27 * 1000, 270 * 1000);
			ticker2 = new System.Threading.Timer(OutputTemperatureXml, null, 65 * 1000, 270 * 1000);
			ticker3 = new System.Threading.Timer(OutputTodayTemperatureCsv, null, 55 * 1000, 270 * 1000);

			Console.ReadKey();
		}


		static void GetTemperature(object state)
		{
			DateTime current = TemperatureDB.GetLatestTime();
			Console.WriteLine("===={0}", current);
			GetAmedasData(current);
		}

		static void OutputTemperatureXml(object state)
		{
			DateTime current = TemperatureDB.GetLatestTime();
			TemperatureDB.OutputOneDayXml(current, MySettings.XmlDestination);
		}

		static void OutputTodayTemperatureCsv(object state)
		{
			DateTime current = TemperatureDB.GetLatestTime();
			CsvGenerator.OutputTodayCsv(current, MySettings.TodayCsvDestination);
		}



		// after以後のデータを取得します。
		static async void GetAmedasData(DateTime after)
		{
			// http://www.tenki.jp/amedas/2/5/31461.html からデータを取得する．

			var source = @"http://www.tenki.jp/amedas/2/5/31461.html";

			System.Net.WebClient client = new WebClient();

			try
			{
				using (System.IO.Stream stream = await client.OpenReadTaskAsync(source))
				{
					using (System.IO.StreamReader reader = new System.IO.StreamReader(stream, new UTF8Encoding(false)))
					{
						bool expecting_date = false;
						bool data10min_active = false;
						bool on_table = false;

						DateTime? expecting_temperature_at = null;	// 次の行で、この時刻の気温を拾えるはず！
						DateTime? source_time = null;	// 最新観測データの時刻。
						DateTime? previous_data_time = null;	// 直前に取得したデータの時刻(最初のデータを取得するまでは，source_timeと同じ)．

						while (true)
						{
							string line = reader.ReadLine();
							//Console.WriteLine(line);
							if (line == null)
							{
								break;
							}

							if (data10min_active)
							{
								if (on_table)
								{
									// ★とりあえず出力する．
									if (!line.Contains("th>"))
									{
										Console.WriteLine(line);
									}

									if (expecting_temperature_at.HasValue)
									{
										// 気温取得！
										decimal? temperature = ReadTemperature(line);
										if (temperature.HasValue)
										{
											Console.WriteLine("Time: {0}, Temperature: {1}", expecting_temperature_at.Value, temperature.Value);
											TemperatureDB.InsertTemperature(expecting_temperature_at.Value, temperature.Value);
											previous_data_time = expecting_temperature_at;
										}
										expecting_temperature_at = null;
									}
									else if (line.Contains("</table>"))
									{
										on_table = false;
										// ここでreturnしてしまってもいいのでは？
										return;
									}
									else
									{
										// ☆データの日時の取得を試みる．
										expecting_temperature_at = ReadDateTime(line, previous_data_time.Value);
										if (expecting_temperature_at.HasValue && expecting_temperature_at.Value <= after)
										{
											return;
										}
									}
								}
								// not on_table
								else if (expecting_date)
								{
									string j_datetime_pattern = @"(\d{4}年\d\d月\d\d日)\s*(\d\d)時(\d\d)分";	// 0:00は"24時00分"と表示されるので注意が必要？
									var m = Regex.Match(line, j_datetime_pattern);
									if (m.Success)
									{
										// source_time = DateTime.Parse(m.Value, JpCulture);
										source_time = DateTime.Parse(m.Groups[1].Value, JpCulture).AddHours(Convert.ToDouble(m.Groups[2].Value)).AddMinutes(Convert.ToDouble(m.Groups[3].Value));
										// 欲しいデータの時刻より新しくなければここでreturnする．
										if (source_time <= after)
										{
											return;
										}
										previous_data_time = source_time.Value;
										expecting_date = false;
									}
								}

								else if (line.Contains("<table"))
								{
									on_table = true;
									Console.WriteLine("We've landed on the table.");
								}
							}
							else if (line.Contains("10分観測値"))
							{
								Console.WriteLine(line);
								// 10min.
								data10min_active = true;
								expecting_date = true;
								continue;
							}

						}
					}

				}
			}
			catch(System.Net.WebException ex)
			{
 				// WebExceptionはtemporaryなネットワーク障害に起因することが考えられるので，
				// いったん握りつぶしておく．
				Console.WriteLine(ex.Message);
			}
			

			/*
						<div class="contentsBox">

					<div class="titleBorder">
							<div class="titleBgLong">
									<h3>10分観測値</h3>
									<div class="dateRight">2014年12月17日 08時30分観測</div>
							</div>
					</div>

					<table class="amedas_table_entries" border="0" cellspacing="0" cellpadding="0">
							<tr>
									<th colspan="2">日時</th>
									<th>気温(℃)</th>
									<th>降水量(mm)</th>
									<th>風向(16方位)</th>
									<th>風速(m/s)</th>
									<th>日照時間(分)</th>
									<th>積雪深(cm)</th>
							</tr>        <tr><td class="bold" rowspan="6">17日</td>            <td>08:30</td>
									<td>-2.5</td>
									<td>0.0</td>
									<td>南西</td>
									<td>2.0</td>
									<td>0</td>
									<td><span class="grey">---</span></td>
							</tr>                <tr>            <td>08:20</td>
									<td>-2.7</td>
									<td>0.0</td>
									<td>西南西</td>
									<td>3.6</td>
									<td>0</td>
									<td><span class="grey">---</span></td>
							</tr>                <tr>            <td>08:10</td>
									<td>-2.6</td>
									<td>0.5</td>
									<td>南西</td>
									<td>4.4</td>
									<td>0</td>
									<td><span class="grey">---</span></td>
							</tr>                <tr>            <td>08:00</td>
									<td>-2.4</td>
									<td>0.0</td>
									<td>南西</td>
									<td>4.1</td>
									<td>0</td>
									<td>33</td>
							</tr>                <tr>            <td>07:50</td>
									<td>-2.5</td>
									<td>0.0</td>
									<td>西南西</td>
									<td>4.5</td>
									<td>0</td>
									<td><span class="grey">---</span></td>
							</tr>                <tr>            <td>07:40</td>
									<td>-2.5</td>
									<td>0.0</td>
									<td>西</td>
									<td>4.4</td>
									<td>0</td>
									<td><span class="grey">---</span></td>
							</tr>                    </table>

			</div><!-- /.contentsBox -->
			*/

		}


		// 気温データのある行から気温を取り出します．
		static decimal? ReadTemperature(string line)
		{
			// line should be like "    <td>-2.5</td>"
			string pattern = @"<td>(-?\d+\.\d)</td>";
			var m = Regex.Match(line, pattern);
			if (m.Success)
			{
				return decimal.Parse(m.Groups[1].Value);
			}
			return null;
		}

		// データ行の時刻を取り出します．
		static DateTime? ReadDateTime(string line, DateTime previousTime)
		{
			var pattern = @"<tr>(?:<td.*>(\d{1,2})日</td>)?\s*<td>(\d\d)\:(\d\d)</td>";
			//string input1 = "							</tr>        <tr><td class=\"bold\" rowspan=\"6\">17日</td>            <td>08:30</td>";
			//string input2 = "							</tr>                <tr>            <td>08:20</td>";
			var m = Regex.Match(line, pattern);
			if (m.Success)
			{
				Console.WriteLine(m.Value);
				Console.WriteLine("{0} - {1} : {2}", m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value);
				// input1, input2の順．
				//Console.WriteLine(m.Groups[1].Value);	// 17, null？
				//Console.WriteLine(m.Groups[2].Value); // 08, 08
				//Console.WriteLine(m.Groups[3].Value); // 30, 20


				DateTime myTime = previousTime;
				int day;
				if (int.TryParse(m.Groups[1].Value, out day))
				{
					while (myTime.Day != day)
					{
						myTime = myTime.AddDays(-1);
					}
					return new DateTime(myTime.Year, myTime.Month, myTime.Day).AddHours(int.Parse(m.Groups[2].Value)).AddMinutes(int.Parse(m.Groups[3].Value));
				}
				else
				{
					// コードがちょっと冗長かな？
					myTime = new DateTime(previousTime.Year, previousTime.Month, previousTime.Day).AddHours(int.Parse(m.Groups[2].Value)).AddMinutes(int.Parse(m.Groups[3].Value));
					while (myTime > previousTime)
					{
						myTime = myTime.AddDays(-1);
					}
					return myTime;
				}

			}
			else
			{
				return null;
			}
		}


	}
}
