using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Data
{
	public class SQLiteData
	{
			// とりあえず1つのファイルに1つのインスタンスを対応させてみる．

		public string FileName
		{
			get { return _fileName; }
		}
		readonly string _fileName;

		/// <summary>
		/// データベースへの接続文字列を取得します．
		/// </summary>
		protected string ConnectionString
		{
			get
			{
				return string.Format("Data source={0}", FileName);
			}
		}

		public SQLiteData(string fileName)
		{
			this._fileName = fileName;
		}

		// System.Convertと紛らわしいですかねぇ？
		public static class Convert
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

	}
}
