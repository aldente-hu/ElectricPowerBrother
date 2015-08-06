using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Main
{
	using Helpers;

	#region PluginTickerクラス
	public class PluginTicker
	{
		public int Count { get; set; }
		public int Interval { get; set; }

		readonly IPluginBase _plugin;
		DateTime _latestDataTime;

		#region *コンストラクタ(PluginTicker)
		public PluginTicker(IPluginBase plugin)
		{
			this._plugin = plugin;
		}
		#endregion


		public void OnTick(object sender, EventArgs e)
		{
			++Count;
			if (Count >= Interval)
			{
				Count = 0;
				// 手抜き実装？
				if (_plugin is IUpdatingPlugin)
				{
					((IUpdatingPlugin)_plugin).Update();
				}
				else if (_plugin is IPlugin)
				{
					_latestDataTime = ((IPlugin)_plugin).Update(_latestDataTime);
				}
			}

		}

	}
	#endregion


}
