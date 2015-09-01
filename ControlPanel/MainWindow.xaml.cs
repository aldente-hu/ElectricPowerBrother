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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Windows.Threading;
using System.Reflection;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using RetrieveData;
	using Helpers;

	namespace ControlPanel
	{
		/// <summary>
		/// MainWindow.xaml の相互作用ロジック
		/// </summary>
		public partial class MainWindow : Window
		{
			Environment environment;

			// (0.1.8)
			#region *DataCsvGeneratorプロパティ
			public Legacy.DailyCsvGenerator DataCsvGenerator
			{
				get
				{
					return this.dataCsvGenerator;
				}
			}
			Legacy.DailyHourlyCsvGenerator dataCsvGenerator;
			#endregion

			// (0.1.8)
			#region *DetailCsvGeneratorプロパティ
			public Legacy.DailyCsvGenerator DetailCsvGenerator
			{
				get
				{
					return this.detailCsvGenerator;
				}
			}
			Legacy.DailyCsvGenerator detailCsvGenerator;
			#endregion

			Legacy.MonthlyChart monthlyChartGenerator;
			Legacy.IndexPage indexPage;

			// (0.1.7)IndexPageを追加．
			// (0.1.6)csvの出力先などにアプリケーション設定を適用．

			public MainWindow()
			{
				InitializeComponent();

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
								asm = Assembly.LoadFrom(string.Format("plugins/{0}.dll", string.IsNullOrEmpty(dll) ? name : dll));
								type_info = asm.GetType(type_name);
							}

							foreach (var config in task.Elements("Config"))
							{
								// インスタンスを生成する．
								IPluginBase generator = Activator.CreateInstance(type_info, Properties.Settings.Default.DatabaseFile) as IPluginBase;

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
									if (generator.GetType() == typeof(Legacy.DailyHourlyCsvGenerator))
									{
										dataCsvGenerator = (Legacy.DailyHourlyCsvGenerator)generator;
									}
									else if (generator.GetType() == typeof(Legacy.DailyCsvGenerator))
									{
										detailCsvGenerator = (Legacy.DailyCsvGenerator)generator;
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

				monthlyChartGenerator = new Legacy.MonthlyChart(Properties.Settings.Default.DatabaseFile);
				monthlyChartGenerator.Width = 640;
				monthlyChartGenerator.Height = 480;
				monthlyChartGenerator.SeriesNo = 2;
				monthlyChartGenerator.Maximum = 800;
				monthlyChartGenerator.Minimum = 0;
				monthlyChartGenerator.SeriesName = "riko";
				monthlyChartGenerator.SourceRootPath = Properties.Settings.Default.DataRoot;
				monthlyChartGenerator.MonthlyTotalChannels = new int[] { 1, 2 };
				monthlyChartGenerator.BorderLine = 600;

				// (0.1.5.1)
				Helpers.Gnuplot.BinaryPath = Properties.Settings.Default.GnuplotBinaryPath;

				indexPage = new Legacy.IndexPage(Properties.Settings.Default.DatabaseFile);
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
						environment = new Environment(
															Properties.Settings.Default.DatabaseFile,
															doc.Root.Element("Loggers"));
						environment.Tweet += environment_Tweet;
					}
					timer.Tick += Buttonお試し_Click;
				}
				else
				{
					// ☆適宜disableする．
				}

			}

			//public static string DatabaseFile = @"B:\ep.sqlite3";


			public void CreateZip(DateTime month)
			{
				detailCsvGenerator.CreateArchive(month);
			}

			private void PltButton_Click(object sender, RoutedEventArgs e)
			{
				if (LegacyCalender.SelectedDate.HasValue)
				{
					OutputLegacyChartPlt(LegacyCalender.SelectedDate.Value.AddDays(1));
				}
			}

			// (0.1.5)
			private void PngButton_Click(object sender, RoutedEventArgs e)
			{
				if (LegacyCalender.SelectedDate.HasValue)
				{
					OutputLegacyChart(LegacyCalender.SelectedDate.Value.AddDays(1));
				}
			}


			private void CreateZipButton_Click(object sender, RoutedEventArgs e)
			{
				if (LegacyCalender.SelectedDate.HasValue)
				{
					CreateZip(LegacyCalender.SelectedDate.Value);
				}
			}

			void OutputLegacyChartPlt(DateTime date)
			{
				monthlyChartGenerator.OtameshiPlt(date, @"B:\otameshi.plt");
			}

			// (0.1.5)
			void OutputLegacyChart(DateTime date)
			{
				monthlyChartGenerator.DrawChartTest(date);
			}


			// (0.1.7)
			void OutputIndexPage()
			{
				indexPage.Update();
			}

			private void buttonIndex_Click(object sender, RoutedEventArgs e)
			{
				OutputIndexPage();
			}




			private void Buttonお試し_Click(object sender, EventArgs e)
			{
				//environment.TestTweet();

				//☆現在の実装では同期実行している．これを非同期実行にするべき！
				//environment.Run(true);
				Task task = new Task(() => environment.Run(true));
				task.Start();
				task.Wait();
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


			DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15), IsEnabled = false };
			private void RetrieveLoggerData_Executed(object sender, ExecutedRoutedEventArgs e)
			{
				timer.Start();
			}

			private void RetrieveLoggerData_CanExecute(object sender, CanExecuteRoutedEventArgs e)
			{
				e.CanExecute = true;
			}



			private void OneDayOutput_Executed(object sender, ExecutedRoutedEventArgs e)
			{
				if (LegacyCalender.SelectedDate.HasValue)
				{
					var generator = ((ContentControl)sender).DataContext as Legacy.DailyCsvGenerator;
					if (generator != null)
					{
						generator.OutputOneDay(LegacyCalender.SelectedDate.Value);
					}
				}
			}

			private void OutputAll_Executed(object sender, ExecutedRoutedEventArgs e)
			{
				if (LegacyCalender.SelectedDate.HasValue)
				{
					var generator = ((ContentControl)sender).DataContext as Legacy.DailyCsvGenerator;
					if (generator != null)
					{
						generator.UpdateFiles(LegacyCalender.SelectedDate.Value.AddDays(1));
					}
				}
			}

			private void CreateArchive_Executed(object sender, ExecutedRoutedEventArgs e)
			{
				if (LegacyCalender.SelectedDate.HasValue)
				{
					var generator = ((ContentControl)sender).DataContext as Legacy.DailyCsvGenerator;
					if (generator != null)
					{
						generator.CreateArchive(LegacyCalender.SelectedDate.Value);
					}
				}
			}

			private void DateSpecified_CanExecute(object sender, CanExecuteRoutedEventArgs e)
			{
				// senderはGroupBox(CommandBindingを設定した場所)で，e.SourceがButton！
				e.CanExecute = LegacyCalender.SelectedDate.HasValue && ((ContentControl)sender).DataContext is Legacy.DailyCsvGenerator;
			}

		}

		public static class Commands
		{
			/// <summary>
			/// データロガーのデータを取得します．
			/// </summary>
			public static RoutedCommand RetrieveLoggerDataCommand = new RoutedCommand();


			/// <summary>
			/// 1日分の出力を行います．パラメータで対象日を与えます．
			/// </summary>
			public static RoutedCommand OneDayOutputCommand = new RoutedCommand();

			/// <summary>
			/// 対象日までの一斉出力を行います．パラメータで対象日を与えます．
			/// </summary>
			public static RoutedCommand OutputAllCommand = new RoutedCommand();


			public static RoutedCommand CreateArchiveCommand = new RoutedCommand();
		}
	}



}