using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using SQLite;
using System.Data.SQLite;

namespace SQLiteNetTest
{
	class Program
	{
		static Properties.Settings MySettings
		{
			get
			{
				return SQLiteNetTest.Properties.Settings.Default;
			}
		}

		#region *[static]コンストラクタ
		static Program()
		{
			XmlGenerator = new ConsumptionXmlGenerator(MySettings.DatabaseFile);

			ChartGenerator = new GnuplotChart(MySettings.DatabaseFile);
			ChartGenerator.TemplatePath = MySettings.PltTemplatePath;
			ChartGenerator.OutputPath = MySettings.PltOutputPath;
			ChartGenerator.GnuplotBinaryPath = MySettings.GnuplotBinaryPath;

			// for test
			CsvGenerator = new ConsumptionCsvGenerator(MySettings.DatabaseFile);
		}
		#endregion
		static ConsumptionXmlGenerator XmlGenerator;
		static GnuplotChart ChartGenerator;
		static ConsumptionCsvGenerator CsvGenerator;

		// とりあえず2つ．
		// 1とか2とか付く変数は，各インスタンスで保持した方がいいのかもしれない．
		static DateTime NewestData1 = new DateTime(0);
		static DateTime NewestData2 = new DateTime(0);
		static DateTime NewestData3 = new DateTime(0);

		static void UpdateXmlFiles(object state)
		{
			var current = XmlGenerator.GetRikoLatestTime();
			if (current > NewestData1)
			{
				XmlGenerator.OutputDailyXml(MySettings.DailyXmlDestination);
				XmlGenerator.Output24HoursXml(current, MySettings.DetailXmlDestination);
				NewestData1 = current;
			}
		}

		static void UpdateSvgChart(object state)
		{
			var current = XmlGenerator.GetRikoLatestTime();
			if (current > NewestData2)
			{
				ChartGenerator.GenerateGraph(current);
				NewestData2 = current;
			}

		}

		static void UpdateTrinityCsvFile(object state)
		{
			var current = CsvGenerator.GetRikoLatestTime();
			if (current > NewestData3)
			{
				CsvGenerator.OutputTrinityCsv(current, MySettings.TrinityCsvDestination);
				NewestData3 = current;
			}
		}

		static System.Threading.Timer ticker1;
		static System.Threading.Timer ticker2;
		static System.Threading.Timer ticker3;

		static void Main(string[] args)
		{

			ticker1 = new System.Threading.Timer(UpdateXmlFiles, null, 0, 60 * 1000);
			ticker2 = new System.Threading.Timer(UpdateSvgChart, null, 14 * 1000, 60 * 1000);
			ticker3 = new System.Threading.Timer(UpdateTrinityCsvFile, null, 28 * 1000, 60 * 1000);

			Console.WriteLine("Press the Enter key to end program.");
			Console.ReadKey();
		}


	}
}
