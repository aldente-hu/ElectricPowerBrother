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

			public MainWindow()
			{
				InitializeComponent();

				dataCsvGenerator = new Legacy.DailyHourlyCsvGenerator(DatabaseFile);
				dataCsvGenerator.CsvRoot = @"B:\data\";
				dataCsvGenerator.CsvEncoding = Encoding.GetEncoding("Shift_JIS");
				dataCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "理工学部", Channels = new int[] { 1, 2 } });
				dataCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "なにこれ", Channels = new int[] { 3, 2 } });
				dataCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "1号館", Channels = new int[] { 1 } });
				dataCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "2号館", Channels = new int[] { 2 } });
				dataCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "総情センター", Channels = new int[] { 3 } });


				detailCsvGenerator = new Legacy.DailyCsvGenerator(DatabaseFile);
				detailCsvGenerator.CsvRoot = @"B:\detail\";
				detailCsvGenerator.CsvEncoding = Encoding.GetEncoding("Shift_JIS");
				detailCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "理工学部", Channels = new int[] { 1, 2 } });
				//dataCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "なにこれ", Channels = new int[] { 3, 2 } });
				detailCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "1号館", Channels = new int[] { 1 } });
				detailCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "2号館", Channels = new int[] { 2 } });
				detailCsvGenerator.Columns.Add(new Legacy.DailyCsvGenerator.CsvColumn { Name = "総情センター", Channels = new int[] { 3 } });

			}

			public static string DatabaseFile = @"B:\ep.sqlite3";

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
					OutputLegacyChartPlt(csv_calender.SelectedDate.Value);
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

			public void OutputLegacyChartPlt(DateTime date)
			{
				var consumption_table = new Data.ConsumptionData(DatabaseFile);

				var generator = new Legacy.MonthlyChart();
				generator.Width = 640;
				generator.Height = 480;
				generator.SeriesNo = 2;
				generator.Maximum = 800;
				generator.Minimum = 0;
				generator.SeriesName = "riko";
				generator.SourceRootPath = @"B:\data\";
				generator.ChartDestination = @"B:\data\Y2015_08\riko.png";
				//generator.CurrentDate = DateTime.Today;
				generator.MonthlyTotal = consumption_table.GetMonthlyTotal(date, 1, 2);	// チャンネルは決め打ち！
				generator.OtameshiPlt(date, @"B:\otameshi.plt");
			}


		}
	}
}