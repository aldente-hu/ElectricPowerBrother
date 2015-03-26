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

		#region *[static]MySettingsプロパティ
		static Properties.Settings MySettings
		{
			// (1.1.1.0)TrinityCsvDestinationに，(TrinityDataRootPathからの)相対パスあるいは絶対パスを入れられるようにした．
			get
			{
				return HirosakiUniversity.Aldente.ElectricPowerBrother.Properties.Settings.Default;
			}
		}
		#endregion

		#region *[static]コンストラクタ(Program)
		static Program()
		{
			GnuplotChartBase.GnuplotBinaryPath = MySettings.GnuplotBinaryPath;
		}
		#endregion


		// staticである必要はある？
		// ↑必要な場合があるみたい．TenkiCheckerのProgram.csを参照．
		static Ticker ticker01;
		static Ticker ticker02;
		static Ticker ticker03;
		static Ticker ticker04;
		static Ticker ticker05;	// (1.2.0)電力量のatom出力用．
		static Ticker ticker06;	// (1.2.1)Variableなcsvの出力用．

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
				var csvDestination = System.IO.Path.IsPathRooted(MySettings.TrinityCsvDestination) ?
					MySettings.TrinityCsvDestination :
					System.IO.Path.Combine(MySettings.TrinityDataRootPath, MySettings.TrinityCsvDestination);
				csvGenerator.OutputTrinityCsv(current, csvDestination);
			};

			ticker03 = new Ticker(csvGenerator.Update);
			ticker03.StartTimer(28 * 1000, 60 * 1000);

			
			var pltGenerator = new GnuplotTrinityChart
			{
				Width = 1000,
				Height = 600,
				FontSize = 18,
				RootPath = MySettings.TrinityDataRootPath,
				ChartDestination = MySettings.TrinitySvgOutputPath,
				TrinityCsvPath = MySettings.TrinityCsvDestination,
				TemperatureCsvPath = MySettings.TemperatureCsvPath
			};
			

			ticker04 = new Ticker();
			// Tickerの動作の設定方法は2通り．
			// 01～03のように引数で動作を与える方法と，
			// ↓のようにCallbackプロパティを直接設定する方法がある．
			// 両者の違いは何だっけ？最新データの時刻が変わった時にだけ動作するのが前者だったっけ？
			ticker04.Callback = (state) =>
			{
				GnuplotChartBase.GenerateGraph(pltGenerator);
			};
			ticker04.StartTimer(34 * 1000, 120 * 1000);


			ConsumptionAtomGenerator atomGenerator = new ConsumptionAtomGenerator(MySettings.DatabaseFile);
			atomGenerator.ID = @"http://den.st.hirosaki-u.ac.jp/consumptions/";
			atomGenerator.SelfLink = @"http://den.st.hirosaki-u.ac.jp/latest.atom";
			atomGenerator.Author = "電力量計測システム";
			atomGenerator.Title = "理工学部電力消費量";
			atomGenerator.AlternateLink = "http://den.st.hirosaki-u.ac.jp/";
			atomGenerator.Destination = MySettings.AtomDestination;
			atomGenerator.UpdateAction = (current) => {
				atomGenerator.Output(current);
			};
			ticker05 = new Ticker(atomGenerator.Update);
			ticker05.StartTimer(3 * 1000, 60 * 1000);

			ConsumptionVariableCsvGenerator vcsvGenerator = new ConsumptionVariableCsvGenerator(MySettings.DatabaseFile);
			vcsvGenerator.Destination = MySettings.VariableCsvDestination;
			vcsvGenerator.SpanHour = MySettings.VariableCsvSpanHour;
			vcsvGenerator.SplitByHour = MySettings.VariableCsvSplitByHour;
			vcsvGenerator.Riko2CorrectionFactor = 1;
			ticker06 = new Ticker(vcsvGenerator.OutputCsv);
			ticker06.StartTimer(8 * 1000, 60 * 1000);



			Console.WriteLine("Press the Enter key to end program.");
			Console.ReadKey();
		}


	}
}
