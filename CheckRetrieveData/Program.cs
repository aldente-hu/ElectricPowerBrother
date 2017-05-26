using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.IO;
using System.Xml;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{

	using PulseLoggers;
	using RetrieveData;
	using Data;

	class Program
	{

		// (1.1.7)static変数にする．
		static Environment environment;

	
		// (1.1.6.2)switchってcharも使えるんだっけ．
		// (1.1.6)DB書き込みテストコマンドを追加．
		static void Main(string[] args)
		{

			System.Threading.Timer timer = null;

			//var environment = new Environment();
			using (XmlReader reader = XmlReader.Create(
				new FileStream(CheckRetrieveData.Properties.Settings.Default.ConfigurationFile, FileMode.Open, FileAccess.Read),
				new XmlReaderSettings()))
			{
				var doc = System.Xml.Linq.XDocument.Load(reader);
				environment = new Environment(
														CheckRetrieveData.Properties.Settings.Default.DatabaseFileName, 
														doc.Root.Element("Loggers"));
			}
			Console.WriteLine("Hello.");
			while (true)
			{
				var key = Console.ReadKey();
				switch(key.KeyChar)
				{ case 'r':
						environment.Run(false);
						break;
					case 's':
						environment.Run(true);
						break;
					case 'l':
						Console.WriteLine("Loggers : {0}", environment.LoggersCount);
						break;
					case 't':
						var db = new ConsumptionRecorder(CheckRetrieveData.Properties.Settings.Default.DatabaseFileName);
						Dictionary<int, double> test_data = new Dictionary<int, double>();
						test_data.Add(11, 28.0);
						test_data.Add(12, 37.0);
						test_data.Add(13, 5.0);
						test_data.Add(14, 7.0);
						test_data.Add(15, 18.0);
						db.InsertData(DateTime.Now, test_data);
						break;
					case 'a':
						if (timer == null)
						{
							timer = new System.Threading.Timer(environment.TimerCallback, null, 0, 80 * 1000);
						}
						else
						{
							Console.WriteLine("Timer has already started running.");
						}
						break;
					default:
						if (timer != null)
						{
							timer.Dispose();
						}
						Console.WriteLine("Bye.");
						return;
				}
			}

		}


		public static void OutputData(TimeSeriesDataDouble data)
		{
			Console.WriteLine(data.Time);

			foreach (var count in data.Data)
			{
				Console.Write("{0} -> {1}  ", count.Key, count.Value);
			}
			Console.WriteLine();
		}

		public static void OutputData(IEnumerable<TimeSeriesData<double>> dataSeries)
		{
			foreach (var data in dataSeries)
			{
				Console.WriteLine(data.Time);
				foreach (var count in data.Data)
				{
					Console.WriteLine("{0} --- {1}", count.Key, count.Value);
				}
			}
		}

	}
}
