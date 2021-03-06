﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.TenkiChecker.MySQL
{
	using ElectricPowerBrother.Data.MySQL;

	#region TemperatureXmlGeneratorクラス
	public class TemperatureXmlGenerator : TemperatureData, Helpers.IPlugin
	{

		// コンストラクタ以外はSQLite版と同じ。

		#region *定番コンストラクタ(TemperatureXmlGenerator)
		public TemperatureXmlGenerator(ConnectionProfile profile) : base(profile)
		{ }
		#endregion

		#region 出力メソッド

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

		#endregion

		#region  (1.1.2)以下プラグイン用．

		/// <summary>
		/// その日のデータだけを出力するかどうかの値を取得／設定します．
		/// falseであれば，最近24時間分の値を出力します．
		/// </summary>
		public bool OneDay { get; set; }

		public void Configure(System.Xml.Linq.XElement config)
		{
			// config.Name.LocalNameをチェックしますか？

			var one_day = (bool?)config.Attribute("OneDay");
			if (one_day.HasValue)
			{
				this.OneDay = one_day.Value;
			}

			this.UpdateAction = (current) =>
			{ this.Invoke(current, (string)config.Attribute("Destination")); };

			// あれ，Invokeはいらない？
			// むしろUpdateをプラグイン化する必要がある．

		}


		// プラグイン用に共通のメソッドを与える．
		public void Invoke(DateTime date, params string[] options)
		{
			// 最初のパラメータが出力先を与える．
			var destination = options[0];
			if (OneDay)
			{
				this.OutputOneDayXml(date, destination);
			}
			else
			{
				this.Output24HoursXml(date, destination);
			}
		}


		#endregion

	}
	#endregion

}
