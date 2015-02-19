using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.TenkiChecker
{
	using Data;


	// 01/21/2015 by aldente : ほとんどConsumptionXmlGeneratorのコピペ．
	public class TemperatureXmlGenerator : TemperatureData
	{
		public TemperatureXmlGenerator(string fileName) : base(fileName)
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


		// 01/21/2015 by aldente
		/// <summary>
		/// 最新24時間分の気温をxmlで出力します．
		/// 新しい方から順に出力します．
		/// </summary>
		/// <param name="destination"></param>
		public void Output24HoursXml(DateTime current, string destination)
		{
			XDocument doc = new XDocument(new XElement("temperatures"));
			var root = doc.Root;

			foreach (var data in GetTemperatures(current.AddDays(-1), current).OrderByDescending(data => data.Key))
			{
				root.Add(
					new XElement("temperature", new XAttribute("time", data.Key.ToString()), data.Value)
				);
			}

			OutputXmlDocument(doc, destination);
		}

		// 01/21/2015 by aldente : TemperatureDataからTemperatureXmlGeneratorに移動．
		public void OutputOneDayXml(DateTime date, string destination)
		{
			Console.WriteLine(date);
			// 1.ドキュメントを生成．
			XDocument doc = new XDocument(new XElement("amedas_hirosaki"));

			XElement elem = new XElement("daily", new XAttribute("date", date.ToString("yyyy-MM-dd")));
			//elem.SetAttributeValue("id", series.Key);

			var from = date - date.TimeOfDay;
			Console.WriteLine(from);

			foreach (var data in GetOneDayTemperatures(date))
			{
				// 面倒だから時刻はTotalHoursを実数でそのまま出してしまおうか．
				TimeSpan i_time = data.Key - from;
				elem.Add(
					new XElement("temperature", new XAttribute("hour", i_time.TotalHours.ToString("F3")), data.Value)
				);
			}
			doc.Root.Add(elem);


			// 2.XMLドキュメントを出力．
			OutputXmlDocument(doc, destination);

		}

	}


}
