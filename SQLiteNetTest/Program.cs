using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

using HirosakiUniversity.Aldente.ElectricPowerBrother.Helpers;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	class Program
	{
		static Properties.Settings MySettings
		{
			get
			{
				return HirosakiUniversity.Aldente.ElectricPowerBrother.Properties.Settings.Default;
			}
		}

		static Program()
		{
			GnuplotChartBase.GnuplotBinaryPath = MySettings.GnuplotBinaryPath;
		}


		// staticである必要はある？
		static Ticker ticker01;
		static Ticker ticker02;
		static Ticker ticker03;
		static Ticker ticker04;

		static void Main(string[] args)
		{
			/*
			 * こういうことをしても，出力されるのは常に同じ時刻！
			 */ /*
			DateTime myTime = DateTime.Now;
			Console.WriteLine(myTime.ToString());
			ticker1 = new System.Threading.Timer((state) =>
			{
				state = ((DateTime)state).AddSeconds(2);
				Console.WriteLine(state.ToString());
				myTime = (DateTime)state;
			}, myTime, 0, 7 * 100);
		
			*/


			
			// これはどこで作ってもいい．
			ConsumptionXmlGenerator xmlGenerator = new ConsumptionXmlGenerator(MySettings.DatabaseFile);
			xmlGenerator.UpdateAction = (current) =>
			{
				xmlGenerator.OutputDailyXml(MySettings.DailyXmlDestination);
				xmlGenerator.OutputTrinityXml(current, MySettings.DetailXmlDestination);
				xmlGenerator.Output24HoursXml(MySettings.LatestXmlDestination);
			};

			//ticker01.Callback = UpdateXmlFiles;
			ticker01 = new Ticker(xmlGenerator.Update);
			ticker01.StartTimer(0, 60 * 1000);


			GnuplotChart chartGenerator = new GnuplotChart(MySettings.DatabaseFile);
			chartGenerator.TemplatePath = MySettings.PltTemplatePath;
			chartGenerator.OutputPath = MySettings.PltOutputPath;
			chartGenerator.GnuplotBinaryPath = MySettings.GnuplotBinaryPath;
			chartGenerator.UpdateAction = (current) =>
			{
				chartGenerator.GenerateGraph(current);
			};

			ticker02 = new Ticker(chartGenerator.Update);
			ticker02.StartTimer(14 * 1000, 60 * 1000);


			var csvGenerator = new ConsumptionCsvGenerator(MySettings.DatabaseFile);
			csvGenerator.CommentOutHeader = false;
			csvGenerator.UpdateAction = (current) => {
				csvGenerator.OutputTrinityCsv(current, MySettings.TrinityCsvDestination);
			};

			ticker03 = new Ticker(csvGenerator.Update);
			ticker03.StartTimer(28 * 1000, 60 * 1000);

			
			var pltGenerator = new GnuplotTrinityChart
			{
				Width = 1000,
				Height = 600,
				FontSize = 18,
				RootPath = MySettings.TrinityDataRootPath,
				ChartDestination = MySettings.TrinitySvgOutputPath
			};
			
			ticker04 = new Ticker((state) =>
			{
				GnuplotChartBase.GenerateGraph(pltGenerator);
			});
			ticker04.StartTimer(34 * 1000, 60 * 1000);
			

			Console.WriteLine("Press the Enter key to end program.");
			Console.ReadKey();
		}


	}
}
