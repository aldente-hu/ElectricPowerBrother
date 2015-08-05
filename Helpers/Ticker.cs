using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Helpers
{

	// タイマを内蔵していて，指定された動作を繰り返し実行します．
	// 前回実行した時刻(あるいは前回処理したデータの時刻)を記憶しているようですが，どう使うのでしょうか？？？
	// ↑いまは使っていないっぽいorz


	#region Tickerクラス
	public class Ticker
	{

		#region プロパティ

		public Timer MyTimer { get; set; }

		public TimerCallback Callback { get; set; }

		// コールバックに与える引数．
		public object CallbackArgument { get; set; }

		#endregion

		#region *コンストラクタ(Ticker)

		public Ticker() { }

		public Ticker(Func<DateTime, DateTime> updateFunction) {
			this.Callback = (state) =>
			{
				NewestData = updateFunction.Invoke(NewestData);
			};
		}
		DateTime NewestData { get; set; }
		#endregion

		#region *開始(StartTimer)
		public void StartTimer(int dueTime, int period)
		{
			// コンストラクタでcallbackが設定されるはずなので，ここで確認はしない！
			MyTimer = new System.Threading.Timer(Callback, CallbackArgument, dueTime, period);
		}
		#endregion


	}
	#endregion


}
