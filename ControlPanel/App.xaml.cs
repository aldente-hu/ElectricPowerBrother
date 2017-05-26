using HirosakiUniversity.Aldente.ElectricPowerBrother.ControlPanel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.ControlPanel
{
	/// <summary>
	/// App.xaml の相互作用ロジック
	/// </summary>
	public partial class App : Application
	{
		// (0.1.11.0)
		#region スタート時(Application_Startup)
		private void Application_Startup(object sender, StartupEventArgs e)
		{
			MainWindow window = new MainWindow();

			int n = e.Args.Length;
			for (int i = 0; i < n; i++)
			{
				switch (e.Args[0])
				{
					case "--auto":
						window.AutoStart = true;
						break;
				}
			}
			
			window.Show();
		}
		#endregion

	}
}