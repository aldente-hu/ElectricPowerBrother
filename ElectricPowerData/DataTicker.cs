using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Data
{
	public class DataTicker
	{

		// SQLiteDataのコピペ。
		#region Ticker関連

		// ※abstract化した方がよい？
		public virtual DateTime GetLatestDataTime() { return new DateTime(0); }

		public Action<DateTime> UpdateAction { get; set; }

		public DateTime Update(DateTime latestData)
		{
			// ※GetLatestDataTimeが設定されていない場合のことはとりあえず考えない．
			var current = GetLatestDataTime();
			if (current > latestData)
			{
				UpdateAction.Invoke(current);

				// UpdateActionでは，こういう処理を行う．
				//OutputDailyXml(DailyXmlDestination);
				//OutputTrinityXml(current, DetailXmlDestination);
				//Output24HoursXml(LatestXmlDestination);
			}
			return current;
		}

		#endregion

		// System.Convertと紛らわしいですかねぇ？
		#region TimeConverterクラス
		public static class TimeConverter
		{
			// DateTime型か，DateTimeOffset型かは後々見直すことにする．

			static DateTime DateOrigin = new DateTime(1970, 1, 1);

			public static DateTime IntToDate(int date)
			{
				return DateOrigin.AddDays(date);
			}

			public static int DateToInt(DateTime date)
			{
				return date.Subtract(DateOrigin).Days;
			}

			public static DateTime IntToTime(int time)
			{
				return DateOrigin.AddSeconds(time + 32400);
			}

			public static int TimeToInt(DateTime time)
			{
				return System.Convert.ToInt32(time.Subtract(DateOrigin).TotalSeconds - 32400);
			}

		}
		#endregion

	}
}
