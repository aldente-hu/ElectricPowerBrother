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



namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	namespace ControlPanel
	{
		/// <summary>
		/// MainWindow.xaml の相互作用ロジック
		/// </summary>
		public partial class MainWindow : Window
		{
			Legacy.DailyHourlyCsvGenerator dataCsvGenerator;
			Legacy.DailyCsvGenerator detailCsvGenerator;
			Legacy.MonthlyChart monthlyChartGenerator;

			// (0.1.6)csvの出力先などにアプリケーション設定を適用．

			public MainWindow()
			{
				InitializeComponent();

				dataCsvGenerator = new Legacy.DailyHourlyCsvGenerator(Properties.Settings.Default.DatabaseFile);
				dataCsvGenerator.CsvRoot = Properties.Settings.Default.DataRoot;
				dataCsvGenerator.CsvEncoding = Encoding.GetEncoding("Shift_JIS");
				dataCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "理工学部", Channels = new int[] { 1, 2 } });
				//dataCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "なにこれ", Channels = new int[] { 3, 2 } });
				dataCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "1号館", Channels = new int[] { 1 } });
				dataCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "2号館", Channels = new int[] { 2 } });
				dataCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "総情センター", Channels = new int[] { 3 } });


				detailCsvGenerator = new Legacy.DailyCsvGenerator(Properties.Settings.Default.DatabaseFile);
				detailCsvGenerator.CsvRoot = Properties.Settings.Default.DetailRoot;
				detailCsvGenerator.CsvEncoding = Encoding.GetEncoding("Shift_JIS");
				detailCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "理工学部", Channels = new int[] { 1, 2 } });
				//dataCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "なにこれ", Channels = new int[] { 3, 2 } });
				detailCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "1号館", Channels = new int[] { 1 } });
				detailCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "2号館", Channels = new int[] { 2 } });
				detailCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "総情センター", Channels = new int[] { 3 } });

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
			}

			//public static string DatabaseFile = @"B:\ep.sqlite3";

			public void OutputCsv()
			{
				if (csv_calender.SelectedDate.HasValue)
				{
					detailCsvGenerator.OutputOneDay(csv_calender.SelectedDate.Value);
				}
			}

			public void OutputAllCsv()
			{
				detailCsvGenerator.UpdateFiles(DateTime.Now);
			}

			public void CreateZip(DateTime month)
			{
				detailCsvGenerator.CreateArchive(month);
			}

			private void Button_Click(object sender, RoutedEventArgs e)
			{
				OutputCsv();
			}

			private void HourlyButton_Click(object sender, RoutedEventArgs e)
			{

				if (csv_calender.SelectedDate.HasValue)
				{
					dataCsvGenerator.OutputOneDay(csv_calender.SelectedDate.Value);
				}
			}

			private void PltButton_Click(object sender, RoutedEventArgs e)
			{
				if (csv_calender.SelectedDate.HasValue)
				{
					OutputLegacyChartPlt(csv_calender.SelectedDate.Value.AddDays(1));
				}
			}

			// (0.1.5)
			private void PngButton_Click(object sender, RoutedEventArgs e)
			{
				if (csv_calender.SelectedDate.HasValue)
				{
					OutputLegacyChart(csv_calender.SelectedDate.Value.AddDays(1));
				}
			}

			private void CsvAllButton_Click(object sender, RoutedEventArgs e)
			{
				OutputAllCsv();
			}


			private void CreateZipButton_Click(object sender, RoutedEventArgs e)
			{
				if (csv_calender.SelectedDate.HasValue)
				{
					CreateZip(csv_calender.SelectedDate.Value);
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


		}
	}
}