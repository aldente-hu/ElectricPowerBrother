using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Helpers
{

	// (1.1.6)便宜的にここでIPluginインターフェイスを定義しておく．

	#region IPluginインターフェイス
	/// <summary>
	/// 更新されたデータをもとに「何かする」プラグインです．
	/// </summary>
	public interface IPlugin : IPluginBase
	{
		/// <summary>
		/// 更新されたデータをもとに行う処理を記述します．
		/// 処理済みデータの最新時刻を返します？
		/// </summary>
		/// <param name="latestData"></param>
		/// <returns></returns>
		DateTime Update(DateTime latestData);
	}
	#endregion

	// (1.1.10)データを更新するプラグインとデータを使って何かするプラグインを分けた方がいいと思う．
	#region IUpdatingPluginインターフェイス
	public interface IUpdatingPlugin : IPluginBase
	{
		/// <summary>
		/// 新しいデータをとってきて，DBに記録します．
		/// </summary>
		void Update();
	}
	#endregion



	#region IPluginBaseインターフェイス
	public interface IPluginBase
	{
		void Configure(System.Xml.Linq.XElement config);
	}
	#endregion


	namespace New
	{
		#region IPluginインターフェイス
		/// <summary>
		/// 更新されたデータをもとに「何かする」プラグインです．
		/// </summary>
		public interface IPlugin : IPluginBase
		{
			/// <summary>
			/// 更新されたデータをもとに行う処理を記述します．
			/// 処理済みデータの最新時刻を返します？
			/// </summary>
			/// <param name="latestData"></param>
			/// <returns></returns>
			Task<DateTime> UpdateAsync(DateTime latestData);
		}
		#endregion

	}

}
