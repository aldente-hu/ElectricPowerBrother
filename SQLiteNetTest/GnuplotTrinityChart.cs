using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.IO;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Helpers;

	/// <summary>
	/// かつてのGnuplotTrinityChartクラスに対応しています．
	/// </summary>
	public class GnuplotTrinityChart : PltFileGeneratorBase
	{
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

		/// <summary>
		/// Trinityデータのファイルパスを，絶対パスまたは(RootPathからの)相対パスで指定します．
		/// </summary>
		public string TrinityCsvPath { get; set; }

		/// <summary>
		/// 気温データのファイルパスを，絶対パスまたは(RootPathからの)相対パスで指定します．
		/// </summary>
		public string TemperatureCsvPath { get; set; }

		public override void Generate(StreamWriter writer)
		{
			// ※決め打ちだらけだけど，まあとりあえず．


			writer.WriteLine(string.Format("cd '{0}'", RootPath));
			writer.WriteLine(string.Format("set terminal svg enhanced size {0},{1} fsize {2}", this.Width, this.Height, this.FontSize));
			writer.WriteLine(string.Format("set output '{0}'", ChartDestination));	// ※とりあえず決め打ち．
			writer.WriteLine("set encoding utf8");
			writer.WriteLine("set title \"電力消費量 (理工学部＋α)\"");	// 全角の"＋"は使わない方がよい．

			// 各軸の設定
			writer.WriteLine("set xlabel '時刻 [Hour]'");
			writer.WriteLine("set xrange [ 0.0 : 24.0 ]");
			writer.WriteLine("set xtics border mirror norotate 0,1,24");
			writer.WriteLine("set ylabel '10分間電力消費量 [kWh]'");
			writer.WriteLine("set yrange [ 50.0 : 130.0 ]");
			writer.WriteLine("set ytics border 50,10,130");
			writer.WriteLine("set y2label '気温 [℃]'");
			writer.WriteLine("set y2range [ -8.0 : 8.0 ]");
			writer.WriteLine("set y2tics border -8,2,8");

			// レイアウトの設定
			writer.WriteLine("set grid y2tics");
			writer.WriteLine("set key left top autotitle columnheader");	// これを使うと文字エンコーディングがおかしくなる？

			// プロットするデータの設定
			writer.WriteLine("set datafile separator ','");
			writer.WriteLine(string.Format(
				"plot '{0}' using 1:2 w lines lw 3 lc rgbcolor '#FF0000', '{0}' using 1:3 w lines lc rgbcolor '#FF3399', '{0}' using 1:4 w lines lc rgbcolor '#FF99CC', '{1}' using 1:2 w lines lw 3 lc rgbcolor '#0000CC' axis x1y2",
				TrinityCsvPath,
				TemperatureCsvPath)
			);

			writer.WriteLine("set output");
			writer.WriteLine("exit");

		}



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



}
