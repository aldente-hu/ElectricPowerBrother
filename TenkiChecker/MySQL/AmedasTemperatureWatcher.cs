using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.TenkiChecker.MySQL
{

	// ★★非MySQL版のコピペかよ！！！

	// (1.2.1) インターフェイスをIUpdatingPluginに変更．
	#region AmedasTemperatureWatcherクラス
	public class AmedasTemperatureWatcher : TemperatureData, Helpers.IUpdatingPlugin
	{
		// コンストラクタ以外はSQLite版と同じ。

		static System.Globalization.CultureInfo JpCulture = new System.Globalization.CultureInfo("ja-JP");

		#region *定番コンストラクタ(AmedasTemperatureWatcher)
		public AmedasTemperatureWatcher(ElectricPowerBrother.Data.MySQL.ConnectionProfile profile)
			: base(profile)
		{ }
		#endregion

		#region *新しいデータを取得(GetNewData)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="state">未使用です．</param>
		public void GetNewData(object state)
		{
			var time = GetLatestDataTime();
			Console.WriteLine("ハジメ " + time.ToLongTimeString());	// for debug
			GetAmedasData(time);
			Console.WriteLine("オワリ");	// for debug
			//return GetLatestDataTime();
		}
		#endregion

		#region *Amedasのデータを取得(GetAmedasData)
		/// <summary>
		/// after以後のデータを取得します(afterは含まない)。
		/// </summary>
		/// <param name="after"></param>
		public void GetAmedasData(DateTime after)
		{
			// 2017年10月20日夕方に，ページテンプレートがリニューアルされたっぽい．
			// http://www.tenki.jp/amedas/2/5/31461.html からデータを取得する．

			//var source = @"http://www.tenki.jp/amedas/2/5/31461.html";
			// ↑Urlプロパティで設定して下さい．

			System.Net.WebClient client = new WebClient();

			try
			{
				//using (System.IO.Stream stream = await client.OpenReadTaskAsync(source))
				using (System.IO.Stream stream = client.OpenRead(this.Url))
				{
					using (System.IO.StreamReader reader = new System.IO.StreamReader(stream, new UTF8Encoding(false)))
					{
						bool expecting_date = false;
						bool data10min_active = false;
						bool on_table = false;

						DateTime? expecting_temperature_at = null;  // 次の行で、この時刻の気温を拾えるはず！
						DateTime? source_time = null; // 最新観測データの時刻。
						DateTime? previous_data_time = null;  // 直前に取得したデータの時刻(最初のデータを取得するまでは，source_timeと同じ)．

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

									if (expecting_temperature_at.HasValue)
									{
										// [5]気温を取得！
										decimal? temperature = ReadTemperature(line);
										if (temperature.HasValue)
										{
											Console.WriteLine("Time: {0}, Temperature: {1}", expecting_temperature_at.Value, temperature.Value);
											InsertTemperature(expecting_temperature_at.Value, temperature.Value);
											previous_data_time = expecting_temperature_at;
										}
										expecting_temperature_at = null;
									}
									else if (line.Contains("</table>"))
									{
										on_table = false;
										// ここでreturnしてしまってもいいのでは？
										Console.WriteLine("リターン1");
										return;
									}
									else
									{
										// [4]データの日時を取得．(nullでなければ，その次の行に気温データがあるものと決め打ちしている．)
										expecting_temperature_at = ReadDateTime(line, previous_data_time.Value);
										if (expecting_temperature_at.HasValue && expecting_temperature_at.Value <= after)
										{
											Console.WriteLine("リターン2");
											return;
										}
									}
								}
								// not on_table
								else if (expecting_date)
								{
									string datetime_pattern = @"time datetime=""(\S+)""";
									var m = Regex.Match(line, datetime_pattern);
									if (m.Success)
									{
										// [2]観測時刻を発見！
										source_time = DateTime.Parse(m.Groups[1].Value);
										// 欲しいデータの時刻より新しくなければここでreturnする．
										if (source_time <= after)
										{
											Console.WriteLine("リターン3");
											return;
										}
										previous_data_time = source_time.Value;
										expecting_date = false;
									}
								}

								else if (line.Contains("<table"))
								{
									// [3]テーブル発見！
									on_table = true;
									Console.WriteLine("We've landed on the table.");
								}
							}
							else if (line.Contains("10分観測値"))
							{
								// [1] 10分観測値のセクションを発見！
								// 10min.
								data10min_active = true;
								expecting_date = true;
								continue;
							}

						}
					}

				}
			}
			catch (System.Net.WebException ex)
			{
				// WebExceptionはtemporaryなネットワーク障害に起因することが考えられるので，
				// いったん握りつぶしておく．
				Console.WriteLine(ex.Message);
			}
			Console.WriteLine("リターン4");

			/*
			<!-- 10分観測値 -->
			<section class="section-wrap">
				<h3>アメダス履歴<span>(10分観測値)</span><time datetime="2017-10-23T08:30:00+09:00" class="date-time">23日08:30観測</time></h3>
										<table class="common-list-entries amedas-table-entries">
					<tr>
						<th colspan="2">日時</th>
						<th>気温(℃)</th>
						<th>降水量(mm)</th>
						<th>風向(16方位)</th>
						<th>風速(m/s)</th>
						<th>日照時間(分)</th>
						<th>積雪深(cm)</th>
					</tr>    <tr>
							<td class="bold" rowspan="6">23日</td>      <td>08:30</td>
						<td>8.4</td>
						<td class="precip-rank-color-1">1.0</td>
						<td>北北東</td>
						<td>7.2</td>
						<td>0</td>
						<td><span class="grey">---</span></td>
					</tr>
							<tr>      <td>08:20</td>
						<td>8.6</td>
						<td class="precip-rank-color-1">0.5</td>
						<td>北北東</td>
						<td>5.5</td>
						<td>0</td>
						<td><span class="grey">---</span></td>
					</tr>
						<!-- 繰り返しを省略 -->
					<tr>      <td>07:40</td>
						<td>8.6</td>
						<td class="precip-rank-color-1">1.0</td>
						<td>北</td>
						<td>4.2</td>
						<td>0</td>
						<td><span class="grey">---</span></td>
					</tr>
							<tr>
						<th colspan="2">日時</th>
						<th>気温(℃)</th>
						<th>降水量(mm)</th>
						<th>風向(16方位)</th>
						<th>風速(m/s)</th>
						<th>日照時間(分)</th>
						<th>積雪深(cm)</th>
					</tr>
				</table>
			</section>
			<!-- /10分観測値 -->
			 */

		}
		#endregion

		#region データ取得用staticメソッド

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
			//string input1 = "    <td class="bold" rowspan="6">23日</td>      <td>09:40</td>";
			//string input2 = "    <tr>      <td>09:30</td>";
			// 1行目だけtrの開始タグの後に改行が入っているorz
			// →trは見ないことにする。時刻の列なのは、td要素の値に":"が含まれているかどうかで判別できる。
			var pattern = @"(?:<td.*>(\d{1,2})日</td>)?\s*<td>(\d\d)\:(\d\d)</td>";
			var m = Regex.Match(line, pattern);
			if (m.Success)
			{
				Console.WriteLine(m.Value);
				Console.WriteLine("{0} - {1} : {2}", m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value);
				// input1, input2の順．
				//Console.WriteLine(m.Groups[1].Value);	// 23, null？
				//Console.WriteLine(m.Groups[2].Value); // 09, 09
				//Console.WriteLine(m.Groups[3].Value); // 40, 30


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

		#endregion

		#region  (1.1.3)以下プラグイン用．

		/// <summary>
		/// その日のデータだけを出力するかどうかの値を取得／設定します．
		/// falseであれば，最近24時間分の値を出力します．
		/// </summary>
		public string Url { get; set; }

		public void Configure(System.Xml.Linq.XElement config)
		{
			// config.Name.LocalNameをチェックしますか？

			this.Url = config.Attribute("Url").Value;

			// UpdateActionを使わない！

		}

		public void Update()
		{
			GetNewData(null);
		}

		#endregion

	}
	#endregion

}
