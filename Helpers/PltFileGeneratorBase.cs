using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Helpers
{
	// 実質staticクラスだけど，これでいいのか？
	// (継承もされていないし，インスタンスも作られていない．このままならヘルパにした方がいいかも？)
	/*
	#region GnuplotChartBaseクラス
	[Obsolete("Gnuplotクラスを使用して下さい．")]
	public class GnuplotChartBase
	{
		// これはstaticでいいよね？
		public static string GnuplotBinaryPath { get; set; }

		// 直接コマンドを送るとうまくいかないことがあったような気がするので，
		// いったんpltファイルを生成するようにしてみる．

		// (1.1.2.3)同期処理にしたので，変数を再びローカル変数に戻す．
		// (1.1.2.2)非同期処理に備えて，これらの変数(GenerateGraphのローカル変数であったもの)をstaticにしておく．
		//static Process process;
		//static string pltFile;

		// 02/23/2014 by aldente : (1.1.2.0)一時ファイルを削除する処理を追加．
		// GnuplotChartからのコピペ．
		public static void GenerateGraph(PltFileGeneratorBase pltGenerator)
		{
			var pltFile = Path.GetTempFileName();

			using (StreamWriter writer = new StreamWriter(pltFile, false, new UTF8Encoding(false)))
			{
				// 何らかの形でpltファイルを生成する．
				pltGenerator.Generate(writer);
				//OutputCommands(writer, rootPath, outputFileName);
			}

			if (!string.IsNullOrEmpty(GnuplotBinaryPath))
			{
				// 非同期で実行する．
				// ↑非同期実行では一時ファイルを削除できなかったので，やむをえず同期実行にしてみる．

				//if (process != null) { process.Dispose(); }
				var process = new Process();
				{
					process.StartInfo.FileName = GnuplotBinaryPath;
					process.StartInfo.Arguments = pltFile;
					process.StartInfo.CreateNoWindow = true;
					process.StartInfo.UseShellExecute = false;	// これを設定しないと，CreateNoWindowは無視される．
					// { // 非同期実行のコード
					//process.EnableRaisingEvents = true;	// (1.1.2.2)これを設定しないと，Exitedイベントが発生しない！
					//process.Exited += (sender, e) =>
					//{
					//	Console.WriteLine("We're deleting this file! : {0}", pltFile);	// for debug (1.1.2.1)
					//	File.Delete(pltFile);
					//};
					// }

					// { // 同期実行のコード
					process.Start();
					if (process.WaitForExit(60 * 1000))
					{
						Console.WriteLine("We're deleting this file! : {0}", pltFile);	// for debug (1.1.2.1)
						File.Delete(pltFile);
					}
					else
					{
						// タイムアウト
						Console.WriteLine("gnuplotのプロセスがタイムアウトしたよ！");
					}
					// }

					process.Dispose();
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
	#endregion
	*/

	// 実装例はGnuplotTrinityChartクラス．
	#region PltFileGeneratorBaseクラス
	public abstract class PltFileGeneratorBase
	{
		// (1.3.11) ChartDestinationプロパティをGnuplotTrinityChartクラスに移動．

		public string RootPath { get; set; }


		// (1.2.0)引数にtimeを追加．timeを使わないならば，どんな値を与えても構いません．
		public abstract void Generate(StreamWriter writer, DateTime time);
		public void Generate(StreamWriter writer)
		{
			Generate(writer, DateTime.MinValue);
		}

		// (1.1.3.0)
		/// <summary>
		/// 与えられた相対パスを，RootPathからの絶対パスに変換して返します．
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		protected string GetAbsolutePath(string path)
		{
			if (Path.IsPathRooted(path))
			{
				return path;
			}
			else
			{
				if (Path.IsPathRooted(RootPath))
				{
					return Path.Combine(RootPath, path);
				}
				else
				{
					throw new InvalidOperationException("RootPathプロパティにルートが含まれていません．");
				}
			}
		}
	}
	#endregion



}
