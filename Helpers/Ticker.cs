using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Helpers
{
	public class Ticker
	{

		Timer timer;

		//public TimerCallback Callback { get; protected set; }
		public TimerCallback Callback { get; set; }

		#region *コンストラクタ(Ticker)
		//public Ticker(TimerCallback callback)
		//{
		//	this.Callback = callback;
		//}

		public Ticker() { }

		public Ticker(Func<DateTime, DateTime> updateFunction) {
			this.Callback = (state) =>
			{
				NewestData = updateFunction.Invoke(NewestData);
			};
		}
		DateTime NewestData { get; set; }
		#endregion

		// コールバックに与える引数．
		public object CallbackArgument { get; set; }

		public void StartTimer(int dueTime, int period)
		{
			// コンストラクタでcallbackが設定されるはずなので，ここで確認はしない！
			timer = new System.Threading.Timer(Callback, CallbackArgument, dueTime, period);
		}


	}


}
