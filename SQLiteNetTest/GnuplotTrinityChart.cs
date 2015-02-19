using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.IO;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{

	public class GnuplotTrinityChart
	{
		public string GnuplotBinaryPath { get; set; }

		/// <summary>
		/// 出力するグラフの横幅を取得／設定します．
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// 出力するグラフの高さを取得／設定します．
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// 出力するグラフのフォントサイズを取得／設定します．
		/// </summary>
		public int FontSize { get; set; }

		public GnuplotTrinityChart()
		{
			this.Width = 640;
			this.Height = 480;
			this.FontSize = 12;
		}


		// 未使用．
		/*
		public void DrawChart(string destination)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo(this.GnuplotBinaryPath);
			startInfo.UseShellExecute = false; // ↓を設定するためには，←の設定が必須！
			startInfo.RedirectStandardInput = true;	// これが必須！
			startInfo.RedirectStandardOutput = true;
			startInfo.CreateNoWindow = true;

			Process gnuplot = Process.Start(startInfo);
			using (StreamWriter file = new StreamWriter(destination, false, new UTF8Encoding(false)))
			{
				using (StreamReader output = gnuplot.StandardOutput)
				{
					using (StreamWriter input = gnuplot.StandardInput)
					{
						OutputCommands(input);
					}
					file.Write(output.ReadToEnd());
					gnuplot.WaitForExit();
				}
			}

		}
		*/

		public void OutputCommands(StreamWriter destination, string rootPath, string outputFileName)
		{
			destination.WriteLine(string.Format("cd '{0}'", rootPath));
			destination.WriteLine(string.Format("set terminal svg enhanced size {0},{1} fsize {2}", this.Width, this.Height, this.FontSize));
			destination.WriteLine(string.Format("set output '{0}'", outputFileName));	// ※とりあえず決め打ち．
			destination.WriteLine("set encoding utf8");	
			destination.WriteLine("set title \"電力消費量 (理工学部＋α)\"");	// 全角の"＋"は使わない方がよい．

			// 各軸の設定
			destination.WriteLine("set xlabel '時刻 [Hour]'");
			destination.WriteLine("set xrange [ 0.0 : 24.0 ]");
			destination.WriteLine("set xtics border mirror norotate 0,1,24");
			destination.WriteLine("set ylabel '10分間電力消費量 [kWh]'");
			destination.WriteLine("set yrange [ 50.0 : 130.0 ]");
			destination.WriteLine("set ytics border 50,10,130");
			destination.WriteLine("set y2label '気温 [℃]'");
			destination.WriteLine("set y2range [ -8.0 : 8.0 ]");
			destination.WriteLine("set y2tics border -8,2,8");

			// レイアウトの設定
			destination.WriteLine("set grid y2tics");
			destination.WriteLine("set key left top autotitle columnheader");	// これを使うと文字エンコーディングがおかしくなる？

			// ※決め打ちを解消しましょう！
			// プロットするデータの設定
			destination.WriteLine("set datafile separator ','");
			destination.WriteLine("plot 'public/himichu/trinity.csv' using 1:2 w lines lw 3 lc rgbcolor '#FF0000', 'public/himichu/trinity.csv' using 1:3 w lines lc rgbcolor '#FF3399', 'public/himichu/trinity.csv' using 1:4 w lines lc rgbcolor '#FF99CC', 'public/himichu/today_temperature.csv' using 1:2 w lines lw 3 lc rgbcolor '#0000CC' axis x1y2");

			destination.WriteLine("set output");
			destination.WriteLine("exit");

			/*
cd 'C:\Users\den\Documents\EP-5.5\ElectricPower'
set terminal svg enhanced size 900,600
set output 'public/himichu/today_test.svg'
#load 'var/common_j.plt'

set title '電力消費量 (理工学部＋α)'

set xlabel "時刻 [Hour]"
set xrange [ 0.00000 : 24.0000 ]
set xtics border mirror norotate 0.000,1,24.000
set ylabel "10分間電力消費量 [kWh]"
set yrange [ 50.000 : 130.000 ] noreverse nowriteback
set ytics border mirror norotate 50.000,10,130.000
set y2label "気温 [℃]"
set y2range [ -8.000 : 8.000 ] noreverse nowriteback
set y2tics border mirror norotate -8.000,2,8.000


set grid y2tics
set key left top

set datafile separator ','
plot 'public/himichu/trinity.csv' using 1:2 w lines lw 3 lc rgbcolor "#FF0000" title 'tanuki', 'public/himichu/trinity.csv' using 1:3 w lines lc rgbcolor "#FF3399" title 'kitsune', 'public/himichu/trinity.csv' using 1:4 w lines lc rgbcolor "#FF99CC" title 'usagi', 'public/himichu/today_temperature.csv' using 1:2 w lines lw 3 lc rgbcolor "#0000CC" axis x1y2 title '気温'


set output
			 */

		}

		// DrawChartの代わり？今はこちらを使用している．
		// GnuplotChartからのコピペ．
		public void GenerateGraph(string rootPath, string outputFileName)
		{
			string pltFile = Path.GetTempFileName();

			using (StreamWriter writer = new StreamWriter(pltFile, false, new UTF8Encoding(false)))
			{
				OutputCommands(writer, rootPath, outputFileName);
			}

			if (!string.IsNullOrEmpty(GnuplotBinaryPath))
			{
				// 非同期で実行する．
				using (var process = new Process())
				{
					process.StartInfo.FileName = GnuplotBinaryPath;
					process.StartInfo.Arguments = pltFile;
					process.StartInfo.CreateNoWindow = true;
					process.StartInfo.UseShellExecute = false;	// これを設定しないと，CreateNoWindowは無視される．
					process.Start();
				}
			}

		}


		// for test.
		public void DrawTestChart()
		{
			ProcessStartInfo startInfo = new ProcessStartInfo(this.GnuplotBinaryPath);
			startInfo.UseShellExecute = false; // ↓を設定するためには，←の設定が必須！
			startInfo.RedirectStandardInput = true;	// これが必須！
			startInfo.RedirectStandardOutput = true;

			Process gnuplot = Process.Start(startInfo);
			using (StreamWriter file = new StreamWriter(@"B:\tanuki.svg", false, new UTF8Encoding(false)))
			{
				using (StreamReader output = gnuplot.StandardOutput)
				{
					using (StreamWriter input = gnuplot.StandardInput)
					{
						input.WriteLine("set terminal svg enhanced size 800,600");
						//writer.WriteLine("set output 'B:\\test.svg'");
						input.WriteLine("test");
						input.WriteLine("exit");
					}
					file.Write(output.ReadToEnd());
					gnuplot.WaitForExit();
				}
			}

		}


	}

}
