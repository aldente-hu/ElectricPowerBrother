using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.RetrieveData
{
	using Data;
	using PulseLoggers;

	// (0.3.1)RetrieveDataに移動．クラスの名前がヘンだけどまあいいか．
	#region Environmentクラス
	public class Environment
	{
		List<IPulseLogger> loggers = new List<IPulseLogger>();
		//DateTime last_data_time = DateTime.Today.AddHours(-2);

		// (1.1.4)
		public int LoggersCount
		{
			get
			{
				return this.loggers.Count;
			}
		}
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
		readonly ConsumptionRecorder db;

		#region *コンストラクタ(Environment)
		public Environment(string databaseFile, System.Xml.Linq.XElement loggers)
		{
			db = new ConsumptionRecorder(databaseFile);

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
		#endregion


		// (1.1.5)データをDBに保存するかどうかを指定する引数を追加．
		// (1.1.4.2)tryの範囲を縮小．
		// (1.1.3.3)例外処理を追加．
		#region *トリガ動作を実行(Run)
		public void Run(bool saving = false)
		{
			DateTime next_data_time = db.GetLatestDataTime().AddMinutes(10);
			Console.WriteLine("Here we go! : {0}", next_data_time);

			IEnumerable<IDictionary<DateTime, TimeSeriesDataDouble>> results;
			// 並列動作でデータを取って来て欲しい．
			Task<IDictionary<DateTime, TimeSeriesDataDouble>>[] tasks
				= loggers.Select(logger => logger.GetDataAfterTask(next_data_time)).ToArray();
			try
			{
				foreach (var task in tasks)
				{
					task.Start();
				}
				if (!Task.WaitAll(tasks, 40 * 1000))
				{
					// これでtasksの各要素は破棄されるのだろうか？
					return;
				}
			}
			catch (AggregateException ex)
			{
				foreach (Exception inner in ex.InnerExceptions)
				{
					Console.WriteLine(inner.Message);
				}
				Console.WriteLine("Something is wrong.");
				return;
			}
			results = tasks.Select(t => t.Result);

			while (results.All(result => { return result.Keys.Contains(next_data_time); }))
			{
				// 総和を求める．
				var sum = new TimeSeriesDataDouble { Time = next_data_time };
				foreach (var result in results)
				{
					sum += result[next_data_time];
				}

				// 表示用データの生成．
				string data_string = string.Empty;
				foreach (var data in sum.Data)
				{
					data_string += string.Format("{0} -> {1}  ", data.Key, data.Value);
				}

				// DBにデータを保存する．
				if (saving)
				{
					db.InsertData(next_data_time, sum.Data);
					this.Tweet(this, new TweetEventArgs { Message = string.Format(
						"{0}のデータを記録しました． {1}", next_data_time.ToString("dd日 HH:mm"), data_string) });
				}
				else
				{
					Console.WriteLine(data_string);
				}

				//last_data_time = next_data_time;
				next_data_time = next_data_time.AddMinutes(10);
			}
			Console.WriteLine("That's all.");
		}
		#endregion

		public event EventHandler<TweetEventArgs> Tweet = delegate { };

		public void TimerCallback(object state)
		{
			Run(true);
		}

		public void TestTweet()
		{
			this.Tweet(this, new TweetEventArgs
			{
				Message = "毎度有り難うございます．"
			});

		}

	}
	#endregion


	public class TweetEventArgs : EventArgs
	{
		public string Message {get; set;}
	}

}
