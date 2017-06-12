using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Windows.Threading;
using System.Reflection;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Helpers;

	namespace ControlPanel
	{
		/// <summary>
		/// MainWindow.xaml の相互作用ロジック
		/// </summary>
		public partial class MainWindow : Window
		{
			IEnvironment environment;

			static Data.MySQL.ConnectionProfile MyConnectionProfile
			{
				get
				{
					return new Data.MySQL.ConnectionProfile
					{
						Server = Properties.Settings.Default.MyServer,
						UserName = Properties.Settings.Default.MyUserName,
						Password = Properties.Settings.Default.MyPassword,
						Database = Properties.Settings.Default.MyDatabase
					};
				}
			}

			// (0.1.7)IndexPageを追加．
			// (0.1.6)csvの出力先などにアプリケーション設定を適用．

			#region *コンストラクタ(MainWindow)
			public MainWindow()
			{
				InitializeComponent();

				InitializeLegacyOutputTab();

				// (0.1.8)Legacyタブの作業だけを行いたければ，DataLoggersConfig(ならびに関連のplugin)を用意する必要はない．
				if (File.Exists(Properties.Settings.Default.DataLoggersConfig))
				{

					using (XmlReader reader = XmlReader.Create(
						new FileStream(Properties.Settings.Default.DataLoggersConfig, FileMode.Open, FileAccess.Read),
						new XmlReaderSettings()))
					{
						var doc = XDocument.Load(reader);

						// 以下、readerは関係ない？
						if (string.IsNullOrEmpty(Properties.Settings.Default.MyServer))
						{
							// SQLiteを使用。
							environment = new Environment(Properties.Settings.Default.DatabaseFile,	doc.Root.Element("Loggers"));
						}
						else
						{
							// MySQLを使用。
							environment = new MySQL.Environment(MyConnectionProfile, doc.Root.Element("Loggers"));
						}
						environment.Tweet += environment_Tweet;
					}
					timer.Tick += LoggerTimer_Tick;
				}
				else
				{
					// ☆適宜disableする．
				}

			}
			#endregion


			// (0.1.11.0)
			#region *AutoStartプロパティ
			/// <summary>
			/// 起動時にロガーデータの取得を自動で開始するかどうかの値を取得／設定します。
			/// 起動後に設定しても何の効果もありません。
			/// </summary>
			public bool AutoStart { get; set; }

			#endregion

			// (0.2.0)やっぱりLoadedに戻す。
			// (0.1.11.1)LoadedからInitializedに変更(Loadedは複数回呼ばれるので)。
			// (0.1.11.0)ロガーデータの取得を自動で開始できるように改良。
			#region *ウィンドウ初期化時
			private void Window_Loaded(object sender, EventArgs e)
			{
				if (AutoStart)
				{
					Commands.RetrieveLoggerDataCommand.Execute(null, this);
					this.AutoStart = false;
				}
			}
			#endregion

			//public static string DatabaseFile = @"B:\ep.sqlite3";


			// (0.2.0)定期実行時は使わないように変更．
			private void Buttonお試し_Click(object sender, EventArgs e)
			{

				DateTime next_data_time = environment.GetNextDataTime();
				Console.WriteLine("Here we go! : {0}", next_data_time);


				if (!string.IsNullOrEmpty(textBoxRoot.Text))
				{
					// ローカルからのデータ取得を試みる．
					var logger = environment.PulseLoggers.FirstOrDefault(l => l.CanGetCountsFromLocal);
					if (logger != null)
					{
						logger.LocalRoot = textBoxRoot.Text;
						try
						{
							environment.Run(true);
						}
						finally
						{
							logger.LocalRoot = string.Empty;  // 元に戻す．
						}
						return;
					}
				}
				// それ以外の場合は普通に実行する．
				environment.Run(true);
			}

			// 別スレッドから実行される！
			void environment_Tweet(object sender, TweetEventArgs e)
			{
				Dispatcher.BeginInvoke(new Action<string>(AddMessage), e.Message);
			}

			void AddMessage(string message)
			{
				var now = DateTime.Now;
				textBlockInfo.Text = textBlockInfo.Text
					+ string.Format("{0} {1} : {2}\n",
					now.ToShortDateString(), now.ToLongTimeString(),
					message);
			}


			#region RetrieveLoggerData

			DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15), IsEnabled = false };
			private void RetrieveLoggerData_Executed(object sender, ExecutedRoutedEventArgs e)
			{
				timer.Start();
			}

			private void RetrieveLoggerData_CanExecute(object sender, CanExecuteRoutedEventArgs e)
			{
				e.CanExecute = true;
			}

			#endregion

			// (0.2.0)定期実行時のハンドラとして分離．
			private void LoggerTimer_Tick(object sender, EventArgs e)
			{
				Task task = new Task(() => environment.Run(true));
				task.Start();
				task.Wait();
			}



			#region [レガシー出力]タブ関連


			#region *"Legacy出力"タブを初期化(InitializeLegacyOutputTab)
			void InitializeLegacyOutputTab()
			{
				// (0.1.9)設定ファイルからgeneratorを生成してみる．
				using (XmlReader reader = XmlReader.Create(Properties.Settings.Default.OutputConfigFile))
				{
					XDocument doc = XDocument.Load(reader);
					var root = doc.Root;
					if (root.Name.LocalName == "ElectricPower")
					{
						// (0.2.2)static変数の設定を行う．
						foreach (var element in root.Element("Values").Elements())
						{
							foreach (var attribute in element.Attributes())
							{
								var assemblies = AppDomain.CurrentDomain.GetAssemblies();
								Type type = null;
								for (int i = 0; i < assemblies.Length; i++)
								{
									type = assemblies[i].GetType("HirosakiUniversity.Aldente.ElectricPowerBrother." + element.Name.LocalName, false);
									if (type != null)
									{
										//asm = assemblies[i];
										break;
									}
								}
								type.GetProperty(attribute.Name.LocalName).SetValue(null, attribute.Value);
							}
						}


						int n = 1;

						foreach (var task in root.Element("Tasks").Elements())
						{
							var name = task.Name.LocalName;
							var type_name = "HirosakiUniversity.Aldente.ElectricPowerBrother." + name;

							// ↓と思ったけど，Type.GetTypeで取得できるのは自身のアセンブリにある型だけなのか？
							// ローカルで認識できるものはローカルで取得する．
							//Type type_info = Type.GetType(type_name, false);
							// 名前からDLLを特定し，そこからtypeをgetしなければならない！
							var dll = (string)task.Attribute("Dll");
							var asm = Assembly.LoadFrom(string.Format("{0}.dll", string.IsNullOrEmpty(dll) ? name : dll));
							Type type_info = asm.GetType(type_name);
							if (type_info == null)
							{
								asm = Assembly.LoadFrom(string.Format("{0}.dll", string.IsNullOrEmpty(dll) ? name : dll));
								type_info = asm.GetType(type_name);
							}

							foreach (var config in task.Elements("Config"))
							{
								// インスタンスを生成する．
								IPluginBase generator;
								if (name.Contains("MySQL"))
								{
									generator = Activator.CreateInstance(type_info, MyConnectionProfile) as IPluginBase;
								}
								else
								{
									generator = Activator.CreateInstance(type_info, Properties.Settings.Default.DatabaseFile) as IPluginBase;
								}

								if (generator == null)
								{
									// エラー．
								}
								else
								{
									// 設定は，XMLの要素をインスタンスに渡して，中で行う．
									generator.Configure(config);

									// ここでInvokeするワケではない．Invoke時の挙動を設定するのである．
									// ↑Configureでそれも含めて行ってしまえるよね？
									// generator.Invoke();

									// Tickerをどこに置く？
									// ここで生成して，staticに置いておく？

									//var interval = (int?)config.Attribute("Interval");

									//var ticker = new PluginTicker(generator);
									//ticker.Interval = interval ?? 300;
									//ticker.Count = ticker.Interval - 3 * (n++);
									//TimerTicked += ticker.OnTick;
									//PluginTickers.Add(ticker);

									// これらは複数回出てこないよね？
									var interfaces = generator.GetType().GetInterfaces();
									if (interfaces.Contains(typeof(Legacy.IDailyHourlyCsvGenerator)))
									{
										dataCsvGenerator = (Legacy.IDailyHourlyCsvGenerator)generator;
									}
									else if (!interfaces.Contains(typeof(Legacy.IDailyHourlyCsvGenerator)) && interfaces.Contains(typeof(Legacy.IDailyCsvGenerator)))
									{
										detailCsvGenerator = (Legacy.IDailyCsvGenerator)generator;
									}
								}

							}
						}

					}
					else
					{
						// ルート要素が違う！
					}

				}

				monthlyChartGenerator = GenerateLegacyMonthlyChart("riko");
				monthlyChartGenerator1 = GenerateLegacyMonthlyChart("riko1");
				monthlyChartGenerator2 = GenerateLegacyMonthlyChart("riko2");

				// (0.1.5.1)
				Helpers.Gnuplot.BinaryPath = Properties.Settings.Default.GnuplotBinaryPath;

				if (string.IsNullOrEmpty(Properties.Settings.Default.MyServer))
				{
					indexPage = new Legacy.IndexPage(Properties.Settings.Default.DatabaseFile);
				}
				else
				{
					indexPage = new ElectricPowerBrother.MySQL.Legacy.IndexPage(MyConnectionProfile);
				}
				indexPage.Destination = Properties.Settings.Default.IndexPageDestination;
				indexPage.Template = Properties.Settings.Default.IndexPageTemplate;
				indexPage.CharacterEncoding = new UTF8Encoding(false);

				// とりあえずここでEnvorinmentを初期化する．(Appの方がいいのか？)
				
				//environment = new Environment()

				// (0.1.8)Legacyタブの作業だけを行いたければ，DataLoggersConfig(ならびに関連のplugin)を用意する必要はない．
				if (File.Exists(Properties.Settings.Default.DataLoggersConfig))
				{

					using (XmlReader reader = XmlReader.Create(
						new FileStream(Properties.Settings.Default.DataLoggersConfig, FileMode.Open, FileAccess.Read),
						new XmlReaderSettings()))
					{
						var doc = System.Xml.Linq.XDocument.Load(reader);
						if (string.IsNullOrEmpty(Properties.Settings.Default.MyServer))
						{
							environment = new Environment(
															Properties.Settings.Default.DatabaseFile,
															doc.Root.Element("Loggers"));
						}
						else
						{
							environment = new MySQL.Environment(MyConnectionProfile, doc.Root.Element("Loggers"));
						}
						environment.Tweet += environment_Tweet;
					}
					timer.Tick += Buttonお試し_Click;
				}
				else
				{
					// ☆適宜disableする．
				}

			
			}
			#endregion


			// (0.1.10)staticメソッドとして分離。
			#region *[static]LegacyMonthlyChartの設定を行う(GenerateLegacyMonthlyChart)
			static private Legacy.IMonthlyChart GenerateLegacyMonthlyChart(string seriesName)
			{
				int seriesNo;
				int maximum;
				int? borderLine = null;
				int[] channels;
				switch (seriesName)
				{
					case "riko":
						seriesNo = 2;
						maximum = 800;
						borderLine = 600;
						channels = new int[] { 1, 2 };
						break;
					case "riko1":
						seriesNo = 3;
						maximum = 500;
						borderLine = null;
						channels = new int[] { 1 };
						break;
					case "riko2":
						seriesNo = 4;
						maximum = 500;
						borderLine = null;
						channels = new int[] { 2 };
						break;
					default:
						throw new ArgumentException("未定義の系列名です。", "seriesName");
				}

				Legacy.IMonthlyChart chartGenerator;
				if (string.IsNullOrEmpty(Properties.Settings.Default.MyServer))
				{
					chartGenerator = new Legacy.MonthlyChart(Properties.Settings.Default.DatabaseFile)
					{
						Width = 640,
						Height = 480,
						SeriesNo = seriesNo,
						Maximum = maximum,
						Minimum = 0,
						SourceRootPath = Properties.Settings.Default.DataRoot,
						SeriesName = seriesName,
						MonthlyTotalChannels = channels
					};
				}
				else
				{
					chartGenerator = new ElectricPowerBrother.MySQL.Legacy.MonthlyChart(MyConnectionProfile)
					{
						Width = 640,
						Height = 480,
						SeriesNo = seriesNo,
						Maximum = maximum,
						Minimum = 0,
						SourceRootPath = Properties.Settings.Default.DataRoot,
						SeriesName = seriesName,
						MonthlyTotalChannels = channels
					};

				}

				if (borderLine.HasValue)
				{
					chartGenerator.BorderLine = borderLine;
				}
				return chartGenerator;
			}
			#endregion

			#region LegacyChart関連

			// (0.1.10)riko1用とriko2用を追加。
			Legacy.IMonthlyChart monthlyChartGenerator;
			Legacy.IMonthlyChart monthlyChartGenerator1;
			Legacy.IMonthlyChart monthlyChartGenerator2;
			Legacy.IIndexpage indexPage;

			// (0.1.10)各チャンネルに対応。
			private void PltButton_Click(object sender, RoutedEventArgs e)
			{
				if (LegacyCalender.SelectedDate.HasValue)
				{
					OutputLegacyChartPlt(((ComboBoxItem)comboBoxChartSeries.SelectedItem).Content.ToString(), LegacyCalender.SelectedDate.Value.AddDays(1));
				}
			}

			// (0.1.10)各チャンネルに対応。
			// (0.1.5)
			private void PngButton_Click(object sender, RoutedEventArgs e)
			{
				if (LegacyCalender.SelectedDate.HasValue)
				{
					OutputLegacyChart(((ComboBoxItem)comboBoxChartSeries.SelectedItem).Content.ToString(), LegacyCalender.SelectedDate.Value.AddDays(1));
				}
			}
			// (0.1.10)各チャンネルに対応。
			void OutputLegacyChartPlt(string seriesName, DateTime date)
			{
				switch (seriesName)
				{
					case "riko":
						monthlyChartGenerator.OtameshiPlt(date, @"B:\otameshi.plt");
						break;
					case "riko1":
						monthlyChartGenerator1.OtameshiPlt(date, @"B:\otameshi1.plt");
						break;
					case "riko2":
						monthlyChartGenerator2.OtameshiPlt(date, @"B:\otameshi2.plt");
						break;
				}
			}

			// (0.1.10)各チャンネルに対応。
			// (0.1.5)
			void OutputLegacyChart(string seriesName, DateTime date)
			{
				switch (seriesName)
				{
					case "riko":
						monthlyChartGenerator.DrawChartTest(date);
						break;
					case "riko1":
						monthlyChartGenerator1.DrawChartTest(date);
						break;
					case "riko2":
						monthlyChartGenerator2.DrawChartTest(date);
						break;
				}
			}

			#endregion

			#region indexページ出力関連

			private void buttonIndex_Click(object sender, RoutedEventArgs e)
			{
				OutputIndexPage();
			}

			// (0.1.7)
			void OutputIndexPage()
			{
				indexPage.Update();
			}

			#endregion


			/*
						private void CreateZipButton_Click(object sender, RoutedEventArgs e)
						{
							if (LegacyCalender.SelectedDate.HasValue)
							{
								CreateZip(LegacyCalender.SelectedDate.Value);
							}
						}

						public void CreateZip(DateTime month)
						{
							detailCsvGenerator.CreateArchive(month);
						}

			*/

			// (0.3.1) 型をインターフェイスに変更。
			// (0.1.8)
			#region *DataCsvGeneratorプロパティ
			public Legacy.IDailyCsvGenerator DataCsvGenerator
			{
				get
				{
					return this.dataCsvGenerator;
				}
			}
			Legacy.IDailyHourlyCsvGenerator dataCsvGenerator;
			#endregion

			// (0.3.1) 型をインターフェイスに変更。
			// (0.1.8)
			#region *DetailCsvGeneratorプロパティ
			public Legacy.IDailyCsvGenerator DetailCsvGenerator
			{
				get
				{
					return this.detailCsvGenerator;
				}
			}
			Legacy.IDailyCsvGenerator detailCsvGenerator;
			#endregion


			#region コマンドハンドラ

			// CSV出力関連．他はイベントハンドラを使っている．

			#region OneDayOutput

			private void OneDayOutput_Executed(object sender, ExecutedRoutedEventArgs e)
			{
				if (LegacyCalender.SelectedDate.HasValue)
				{
					var generator = ((ContentControl)sender).DataContext as Legacy.IDailyCsvGenerator;
					if (generator != null)
					{
						generator.OutputOneDay(LegacyCalender.SelectedDate.Value);
					}
				}
			}

			#endregion

			#region OutputAll

			private void OutputAll_Executed(object sender, ExecutedRoutedEventArgs e)
			{
				if (LegacyCalender.SelectedDate.HasValue)
				{
					var generator = ((ContentControl)sender).DataContext as Legacy.IDailyCsvGenerator;
					if (generator != null)
					{
						generator.UpdateFiles(LegacyCalender.SelectedDate.Value.AddDays(1));
					}
				}
			}

			#endregion

			#region CreateArchive

			private void CreateArchive_Executed(object sender, ExecutedRoutedEventArgs e)
			{
				if (LegacyCalender.SelectedDate.HasValue)
				{
					var generator = ((ContentControl)sender).DataContext as Legacy.IDailyCsvGenerator;
					if (generator != null)
					{
						generator.CreateArchive(LegacyCalender.SelectedDate.Value);
					}
				}
			}

			#endregion

			private void DateSpecified_CanExecute(object sender, CanExecuteRoutedEventArgs e)
			{
				// senderはGroupBox(CommandBindingを設定した場所)で，e.SourceがButton！
				e.CanExecute = LegacyCalender.SelectedDate.HasValue && ((ContentControl)sender).DataContext is Legacy.IDailyCsvGenerator;
			}

			#endregion

			#endregion

		}

	}



}