using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ElectricPowerData;

namespace EvaluateAlerts
{
	class Program
	{
		static void Main(string[] args)
		{
			string database = @"B:\ep.sqlite3";
			AlertData data = new AlertData(database);

			foreach (var alert in data.GetDeclaredData(new DateTime(2013, 11,1), new DateTime(2014, 2, 27)))
			{
				Console.Write("{0}  {1}   ", alert.Key, alert.Value);
				// 電力使用量データを取得する．
				var cons = data.Around1hour(alert.Key);
				// ひどいコードだｗ
				Console.WriteLine("{0},{1},{2},{3},{4},{5},{6}",
					cons[0] + cons[1] + cons[2] + cons[3] + cons[4] + cons[5],
					cons[6] + cons[1] + cons[2] + cons[3] + cons[4] + cons[5],
					cons[6] + cons[7] + cons[2] + cons[3] + cons[4] + cons[5],
					cons[6] + cons[7] + cons[8] + cons[3] + cons[4] + cons[5],
					cons[6] + cons[7] + cons[8] + cons[9] + cons[4] + cons[5],
					cons[6] + cons[7] + cons[8] + cons[9] + cons[10] + cons[5],
					cons[6] + cons[7] + cons[8] + cons[9] + cons[10] + cons[11]
					);
			}

			//Console.ReadKey();
		}
	}
}
