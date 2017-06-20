using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Helpers
{

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


	namespace New
	{

		// 実装例はGnuplotTrinityChartクラス．
		#region PltFileGeneratorBaseクラス
		public abstract class PltFileGeneratorBase
		{
			// (1.3.11) ChartDestinationプロパティをGnuplotTrinityChartクラスに移動．

			public string RootPath { get; set; }


			// (1.2.0)引数にtimeを追加．timeを使わないならば，どんな値を与えても構いません．
			public abstract Task GenerateAsync(StreamWriter writer, DateTime time);
			public async Task Generateasync(StreamWriter writer)
			{
				await GenerateAsync(writer, DateTime.MinValue);
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

}
