using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;
using System.Xml;


namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Data.New;
	
	namespace New
	{

		public class ConsumptionXmlGenerator : ConsumptionData
		{
			public ConsumptionXmlGenerator(Data.IConnectionProfile profile) : base(profile)
			{ }

			// これを非同期化したいが...
			protected void OutputXmlDocument(XDocument doc, string destination)
			{
				// ☆将来的にはこれをプロパティ化しましょう．
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


			public async Task OutputDailyXmlAsync(string destination)
			{
				// 1.ドキュメントを生成．

				XDocument doc = new XDocument(new XElement("daily_consumptions"));

				var trinity = await DefineTrinityAsync(DateTime.Today);

				foreach (var series in trinity)
				{
					XElement elem = new XElement("daily", new XAttribute("date", series.Value.ToString("yyyy-MM-dd")));
					elem.SetAttributeValue("id", series.Key);

					foreach (var data in await GetHourlyConsumptionsAsync(series.Value))
					{
						elem.Add(
							new XElement("hourly", new XAttribute("hour", data.Key), data.Value)
						);
					}
					doc.Root.Add(elem);
				}

				// 2.XMLドキュメントを出力．
				OutputXmlDocument(doc, destination);

			}

			// 01/19/2015 by aldente : メソッド名をOutput24HoursXmlからOutputTrinityXmlに変更．
			public async Task OutputTrinityXmlAsync(DateTime time, string destination)
			{
				// 1.ドキュメントを生成．
				XDocument doc = new XDocument(new XElement("consumptions"));

				var trinity = await DefineTrinityAsync(time);
				foreach (var series in trinity)
				{
					XElement elem = new XElement("daily", new XAttribute("date", series.Value.ToString("yyyy-MM-dd")));
					elem.SetAttributeValue("id", series.Key);

					var series_current = series.Value;
					DateTime from = series_current - series_current.TimeOfDay;
					DateTime to = from.AddDays(1);
					Console.WriteLine("{0} - {1}", from, to);

					foreach (var data in await GetDetailConsumptionsAsync(from, to))
					{
						// 面倒だから時刻はTotalHoursを実数でそのまま出してしまおうか．
						TimeSpan i_time = data.Key - from;
						elem.Add(
							new XElement("consumption", new XAttribute("hour", i_time.TotalHours.ToString("F3")), data.Value)
						);
					}
					doc.Root.Add(elem);

				}

				// 2.XMLドキュメントを出力．
				OutputXmlDocument(doc, destination);

			}

			// (1.1.2.2) 実装を修正．
			// 01/21/2015 by aldente
			/// <summary>
			/// 最新24時間分の電力消費量をxmlで出力します．
			/// 新しい方から順に出力します．
			/// </summary>
			/// <param name="destination"></param>
			public async Task Output24HoursXml(string destination)
			{
				DateTime latest = await GetLatestDataTimeAsync();

				XDocument doc = new XDocument(new XElement("consumptions"));
				var root = doc.Root;

				foreach (var data in (await GetDetailConsumptionsAsync(latest.AddDays(-1), latest)).OrderByDescending(data => data.Key))
				{
					root.Add(
						new XElement("consumption", new XAttribute("e_time", data.Key.ToString()), data.Value)
					);
				}

				OutputXmlDocument(doc, destination);
			}

		}

	}
}
