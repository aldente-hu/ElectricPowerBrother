using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml.Linq;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.TenkiChecker
{
	using Data;
	using Helpers;

	class Program
	{

		static Properties.Settings MySettings
		{
			get
			{
				return TenkiChecker.Properties.Settings.Default;
			}
		}


		// VistaではこれをMainメソッドのローカル変数にすると，タイマが途中(1～2回WebClientをOpenした後？)で止まってしまった．
		// Win7ではそのような問題はなかった．
		static Ticker ticker01;


		static void Main(string[] args)
		{
			Console.WriteLine("Press the Enter key to end program.");


			var amedasWatcher = new AmedasTemperatureWatcher(MySettings.DatabaseFile);

			//ticker01 = new Ticker { Callback = amedasWatcher.GetNewData };
			//ticker01 = new Ticker(amedasWatcher.GetNewData);
			//ticker01.StartTimer(14 * 1000, 270 * 1000);


			var xmlGenerator = new TemperatureXmlGenerator(MySettings.DatabaseFile);
			xmlGenerator.UpdateAction = (current) =>
			{
				xmlGenerator.OutputOneDayXml(current, MySettings.XmlDestination);
				xmlGenerator.Output24HoursXml(current, MySettings.LatestXmlDestination);
			};

			var ticker02 = new Ticker(xmlGenerator.Update);
			ticker02.StartTimer(34 * 1000, 90 * 1000);



			var csvGenerator = new TemperatureCsvGenerator(MySettings.DatabaseFile);
			csvGenerator.CommentOutHeader = false;
			csvGenerator.UseDateOnHeader = true;
			csvGenerator.UpdateAction = (current) =>
			{
				csvGenerator.OutputTodayCsv(current, MySettings.TodayCsvDestination);
			};

			var ticker03 = new Ticker(csvGenerator.Update);
			ticker03.StartTimer(44 * 1000, 90 * 1000);



			Console.ReadKey();
		}






	}
}
