using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using ElectricPowerData;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Data;

	namespace EvaluateAlerts
	{

		class Program
		{

			static Properties.Settings MySettings
			{
				get
				{
					return EvaluateAlerts.Properties.Settings.Default;
				}
			}

			static void Main(string[] args)
			{
				//AlertData data = new AlertData(MySettings.DatabaseFile);
				//ConsumptionData c_data = new ConsumptionData(MySettings.DatabaseFile);	// ←dataと分ける意味あるの？

				//Console.WriteLine("CurrentRank : {0}", data.GetCurrentRank());

				var judge = new AlertJudgement(MySettings.DatabaseFile);
				judge.OutputAtomFeed(MySettings.AtomFeedDestination, 20);
				
				
				
	/*			
				foreach (var alert in data.GetDeclaredData(new DateTime(2013, 11, 1), new DateTime(2014, 2, 27)))
				{
					Console.Write("{0}  {1}   ", alert.Key, alert.Value);
					// 電力使用量データを取得する．
					var cons = c_data.Around1hour(alert.Key);
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
*/
				Console.ReadKey();
			}
		}
	}
}