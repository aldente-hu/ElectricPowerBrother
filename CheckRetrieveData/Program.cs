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

	public class Environment
	{
		List<IPulseLogger> loggers = new List<IPulseLogger>();
		DateTime last_data_time = DateTime.Today.AddHours(-2);

		/*
		public Environment()
		{
			// これらは，XMLから読み込んで設定することになる！
			var logger1 = new LR8400
				{
					Address = System.Net.IPAddress.Parse("192.168.4.7"),
				};
			logger1.Credential.UserName = CheckRetrieveData.Properties.Settings.Default.UserName;
			logger1.Credential.Password = CheckRetrieveData.Properties.Settings.Default.Password;

			var gain11 =  new Dictionary<int,double>();
			gain11[1] = 1;
			var gain12 =  new Dictionary<int,double>();
			gain12[1] = -1;
			gain12[3] = 1;
			logger1.Channels.Add(new Channel { Gains = gain11 });
			logger1.Channels.Add(new Channel { Gains = gain12 });

			loggers.Add(logger1);


			var logger2 = new Logger8420
				{
					Address = System.Net.IPAddress.Parse("192.168.4.4")
				};
			var gain21 = new Dictionary<int, double>();
			gain21[2] = 1;

			for (int i = 0; i < 4; i++)
			{
				logger2.Channels.Add(new Channel { Gains = gain21 });
			}
			loggers.Add(logger2);

		}
		*/

		public Environment(System.Xml.Linq.XElement loggers)
		{
			foreach (var elem_logger in loggers.Elements())
			{
				// リフレクションでクラスを探し当てる．

				// Dllはどうする？パルスロガーの機種ごとに分ける？
				// →それやると機種の数だけプロジェクトが必要になるよ？
				// →でもそんなにたくさんの種類はないんだし，いいんじゃない？

				// MainProcedureと同じような仕様のプラグインにしてみますか？

				// 名前からDLLを特定し，そこからtypeをgetしなければならない！

				var name = elem_logger.Name.LocalName;	// ex. "Hioki.LR8400"
				var dll = (string)elem_logger.Attribute("Dll");

				// ※dll名の規約はどうしますかねぇ？
				var asm = Assembly.LoadFrom(string.Format("plugins/{0}.dll", string.IsNullOrEmpty(dll) ? name : dll));
				var type_info = asm.GetType("HirosakiUniversity.Aldente.ElectricPowerBrother.PulseLoggers." + name);

				var logger = Activator.CreateInstance(type_info) as IPulseLogger;
				logger.Configure(elem_logger);
				this.loggers.Add(logger);
			}
		}


		public void Run()
		{
			DateTime next_data_time = last_data_time.AddMinutes(10);
			Console.WriteLine("Here we go! : {0}", next_data_time);

			// 並列動作でデータを取って来て欲しい．
			Task<IDictionary<DateTime, TimeSeriesDataDouble>>[] tasks
				= loggers.Select(logger => logger.GetDataAfterTask(next_data_time)).ToArray();

			foreach (var task in tasks)
			{
				task.Start();
			}
			Task.WaitAll(tasks);

			// IEnumerableよりDictionaryを返した方が便利ですかねぇ？
			var results = tasks.Select(t => t.Result);

			while (results.All(result => { return result.Keys.Contains(next_data_time); }))
			{
				// 総和を求める．
				var sum = new TimeSeriesDataDouble { Time = next_data_time };
				foreach (var result in results)
				{
					sum += result[next_data_time];
				}

				foreach (var data in sum.Data)
				{
					Console.Write("{0} -> {1}  ", data.Key, data.Value);
				}
				Console.WriteLine();
				last_data_time = next_data_time;
				next_data_time = next_data_time.AddMinutes(10);
			}
			Console.WriteLine("That's all.");
		}


	}

	class Program
	{



		static void Main(string[] args)
		{

			//List<IPulseLogger> loggers = new List<IPulseLogger>();


			// ロガーにアクセスしてデータをとる．これをparallelにできないか？

			//var data = loggers[0].GetCountsAfter(time);
			//var data2 = loggers[1].GetCountsAfter(time);


			/*
			switch (time.Second % 2)
			{
				case 0:
					OutputData(loggers[0].GetCountsAfter(time));
					break;
				case 1:

					Hioki8420 logger2 = new Hioki8420 { Address = System.Net.IPAddress.Parse("192.168.4.4") };
					OutputData(loggers[1].GetCountsAfter(time));
					break;
			}
			*/

			/*
			using (System.Threading.Timer timer = new System.Threading.Timer(
				(state) => { TimerTicked(null, EventArgs.Empty); }, null, 0, 20 * 1000))
			{

				Console.WriteLine("That's all.");
			*/

			Environment environment;
			//var environment = new Environment();
			using (XmlReader reader = XmlReader.Create(
				new FileStream(CheckRetrieveData.Properties.Settings.Default.ConfigurationFile, FileMode.Open, FileAccess.Read),
				new XmlReaderSettings()))
			{
				var doc = System.Xml.Linq.XDocument.Load(reader);
				environment = new Environment(doc.Root.Element("Loggers"));
			}
			while (true)
			{
				environment.Run();
				var key = Console.ReadKey();
				if (key.KeyChar != 'r')
				{
					break;
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
