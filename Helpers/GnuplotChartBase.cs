using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Helpers
{

	// 今のところstaticなクラスになっているけど，どうしますかねぇ？
	public class GnuplotChartBase
	{
		// これはstaticでいいよね？
		public static string GnuplotBinaryPath { get; set; }

		// 直接コマンドを送るとうまくいかないことがあったような気がするので，
		// いったんpltファイルを生成するようにしてみる．


		// GnuplotChartからのコピペ．
		public static void GenerateGraph(PltFileGeneratorBase pltGenerator)
		{
			string pltFile = Path.GetTempFileName();

			using (StreamWriter writer = new StreamWriter(pltFile, false, new UTF8Encoding(false)))
			{
				// 何らかの形でpltファイルを生成する．
				pltGenerator.Generate(writer);
				//OutputCommands(writer, rootPath, outputFileName);
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

		#region テスト用メソッド

		public static void DrawTestChart(string destination)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo(GnuplotBinaryPath);
			startInfo.UseShellExecute = false; // ↓を設定するためには，←の設定が必須！
			startInfo.RedirectStandardInput = true;	// これが必須！
			startInfo.RedirectStandardOutput = true;

			Process gnuplot = Process.Start(startInfo);
			using (StreamWriter file = new StreamWriter(destination, false, new UTF8Encoding(false)))
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

		#endregion

	}



	public abstract class PltFileGeneratorBase
	{
		public string RootPath { get; set; }
		public string ChartDestination { get; set; }

		public abstract void Generate(StreamWriter writer);
	}



}
