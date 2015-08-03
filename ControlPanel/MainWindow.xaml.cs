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

			public void OutputCsv()
			{
				var generator = new Legacy.DailyCsvGenerator(@"B:\ep.sqlite3");
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
				var generator = new Legacy.DailyHourlyCsvGenerator(@"B:\ep.sqlite3");
				generator.CsvRoot = @"B:\data\";
				generator.CsvEncoding = Encoding.GetEncoding("Shift_JIS");

				if (csv_calender.SelectedDate.HasValue)
				{
					generator.OutputOneDay(csv_calender.SelectedDate.Value);
				}
			}

		}
	}
}