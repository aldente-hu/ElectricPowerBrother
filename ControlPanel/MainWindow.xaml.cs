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
			public MainWindow()
			{
				InitializeComponent();
			}

			public static string DatabaseFile = @"B:\ep.sqlite3";

			public void OutputCsv()
			{
				var generator = new Legacy.DailyCsvGenerator(DatabaseFile);
				generator.CsvRoot = @"B:\detail\";
				generator.CsvEncoding = Encoding.GetEncoding("Shift_JIS");

				if (csv_calender.SelectedDate.HasValue)
				{
					generator.OutputOneDay(csv_calender.SelectedDate.Value);
			}
			}

			private void Button_Click(object sender, RoutedEventArgs e)
			{
				OutputCsv();
			}

			private void HourlyButton_Click(object sender, RoutedEventArgs e)
			{
				var generator = new Legacy.DailyHourlyCsvGenerator(DatabaseFile);
				generator.CsvRoot = @"B:\data\";
				generator.CsvEncoding = Encoding.GetEncoding("Shift_JIS");

				if (csv_calender.SelectedDate.HasValue)
				{
					generator.OutputOneDay(csv_calender.SelectedDate.Value);
				}
			}

			private void PltButton_Click(object sender, RoutedEventArgs e)
			{
				if (csv_calender.SelectedDate.HasValue)
				{
					OutputLegacyChartPlt(csv_calender.SelectedDate.Value);
			}
			}

			public void OutputLegacyChartPlt(DateTime date)
			{
				var generator = new Legacy.MonthlyChart(DatabaseFile);
				generator.Width = 800;
				generator.Height = 360;
				generator.SeriesNo = 2;
				generator.Maximum = 800;
				generator.Minimum = 0;
				generator.SeriesName = "riko";
				generator.SourceRootPath = @"B:\data\";
				generator.ChartDestination = @"B:\data\Y2015_08\riko.png";
				//generator.CurrentDate = DateTime.Today;
				generator.OtameshiPlt(DateTime.Today, @"B:\otameshi.plt");
			}


		}
	}
}