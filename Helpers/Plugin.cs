using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Helpers
{

	// (1.1.6)便宜的にここでIPluginインターフェイスを定義しておく．
	public interface IPlugin : IPluginBase
	{
		//void Invoke(DateTime date, params string[] options);
		DateTime Update(DateTime latestData);
	}

	public interface IPluginBase
	{
		void Configure(System.Xml.Linq.XElement config);
	}


	// (1.1.10)データを更新するプラグインとデータを使って何かするプラグインを分けた方がいいと思う．
	public interface IUpdatingPlugin : IPluginBase
	{
		/// <summary>
		/// 新しいデータをとってきて，DBに記録します．
		/// </summary>
		void Update();
	}

}
