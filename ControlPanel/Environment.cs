using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.ControlPanel
{
	using Data;
	//using PulseLoggers;
	using Base;

	// (0.2.0)ControlPanelに移動．
	// (0.3.1)RetrieveDataに移動．クラスの名前がヘンだけどまあいいか．
	#region Environmentクラス
	public class Environment
	{
		List<CachingPulseLogger> loggers = new List<CachingPulseLogger>();
		//DateTime last_data_time = DateTime.Today.AddHours(-2);

		public List<CachingPulseLogger>PulseLoggers
		{
			get
			{
				return loggers;
			}
		}

		// (1.1.4)
		#region *LoggersCountプロパティ
		public int LoggersCount
		{
			get
			{
				return this.loggers.Count;
			}
		}
		#endregion




		#region 使用例？

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

		#endregion

		readonly ConsumptionRecorder db;  // for RetrieveData

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

				var name = elem_logger.Name.LocalName;  // ex. "Hioki.LR8400"
				var dll = (string)elem_logger.Attribute("Dll");

				// ※dll名の規約はどうしますかねぇ？
				var asm = Assembly.LoadFrom(string.Format("plugins/{0}.dll", string.IsNullOrEmpty(dll) ? name : dll));
				var type_info = asm.GetType("HirosakiUniversity.Aldente.ElectricPowerBrother.PulseLoggers." + name);

				var logger = Activator.CreateInstance(type_info) as CachingPulseLogger;
				logger.SetUp(elem_logger);
				this.loggers.Add(logger);
			}
		}
		#endregion

		public DateTime GetNextDataTime()
		{
			return db.GetLatestDataTime().AddMinutes(10);
		}

		// (1.1.5)データをDBに保存するかどうかを指定する引数を追加．
		// (1.1.4.2)tryの範囲を縮小．
		// (1.1.3.3)例外処理を追加．
		#region *トリガ動作を実行(Run)
//		public void Run(bool saving = false, Task<IDictionary<DateTime, TimeSeriesDataDouble>>[] tasks = null)
		public void Run(bool saving = false)
		{
			DateTime next_data_time = db.GetLatestDataTime().AddMinutes(10);
			Console.WriteLine("Here we go! : {0}", next_data_time);

			//if (tasks == null)
			//{
			//	tasks = loggers.Select(logger => logger.GetCountsAfterTask(next_data_time)).ToArray();
			//}
			var tasks = loggers.Select(logger => logger.GetCountsAfterTask(next_data_time)).ToArray();

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
			var results = tasks.Select(t => t.Result);
			// ここで結果だけを取り出しているので，各要素がどのロガーから来たデータなのかは簡単には判別できない．

			ProcessData(results, next_data_time, saving);
			Console.WriteLine("That's all.");
		}
		#endregion


		// Runから分離．
		// 1つの時刻に対するデータを計算して，DBに記録する．
		public void ProcessData(IEnumerable<IDictionary<DateTime, TimeSeriesDataDouble>> results, DateTime next_data_time, bool saving = false)
		{
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
					this.Tweet(this, new TweetEventArgs
					{
						Message = string.Format(
						"{0}のデータを記録しました． {1}", next_data_time.ToString("dd日 HH:mm"), data_string)
					});
				}
				else
				{
					Console.WriteLine(data_string);
				}

				//last_data_time = next_data_time;
				next_data_time = next_data_time.AddMinutes(10);
			}

		}

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
		public string Message { get; set; }
	}

}
