using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;
using System.Xml;
using ElectricPowerData;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	public class ConsumptionXmlGenerator : ConsumptionData
	{
		public ConsumptionXmlGenerator(string fileName) : base(fileName)
		{ }


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


		public void OutputDailyXml(string destination)
		{
			// 1.ドキュメントを生成．

			XDocument doc = new XDocument(new XElement("daily_consumptions"));

			var trinity = DefineTrinity(DateTime.Today);

			foreach (var series in trinity)
			{
				XElement elem = new XElement("daily", new XAttribute("date", series.Value.ToString("yyyy-MM-dd")));
				elem.SetAttributeValue("id", series.Key);

				foreach (var data in GetHourlyConsumptions(series.Value))
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
		public void OutputTrinityXml(DateTime time, string destination)
		{
			// 1.ドキュメントを生成．
			XDocument doc = new XDocument(new XElement("consumptions"));

			var trinity = DefineTrinity(time);
			foreach (var series in trinity)
			{
				XElement elem = new XElement("daily", new XAttribute("date", series.Value.ToString("yyyy-MM-dd")));
				elem.SetAttributeValue("id", series.Key);

				var series_current = series.Value;
				DateTime from = series_current - series_current.TimeOfDay;
				DateTime to = from.AddDays(1);
				Console.WriteLine("{0} - {1}", from, to);

				foreach (var data in GetDetailConsumptions(from, to))
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

		// 01/21/2015 by aldente
		/// <summary>
		/// 最新24時間分の電力消費量をxmlで出力します．
		/// 新しい方から順に出力します．
		/// </summary>
		/// <param name="destination"></param>
		public void Output24HoursXml(string destination)
		{
			DateTime latest = GetRikoLatestTime();

			XDocument doc = new XDocument(new XElement("consumptions"));
			var root = doc.Root;

			foreach (var data in GetDetailConsumptions(latest.AddDays(-1), latest).OrderByDescending(data => data.Key))
			{
				root.Add(
					new XElement("consumption", new XAttribute("e_time", data.Key.ToString()), data.Value)
				);
			}

			OutputXmlDocument(doc, destination);
		}


	}
}
