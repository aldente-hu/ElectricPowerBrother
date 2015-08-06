using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteNetTest
{
	public class SQLiteDateTimeConverting : SQLiteData
	{

		public SQLiteDateTimeConverting(string fileName) : base(fileName)
		{
		}

		// 以下はもう少し上の階層に記述してもよい．
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
			return Convert.ToInt32(time.Subtract(DateOrigin).TotalSeconds - 32400);
		}
	}
}
