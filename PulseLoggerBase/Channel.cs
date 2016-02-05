using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Base
{
	// (0.0.1)
	#region Channelクラス
	public class Channel
	{
		// パルスロガーの特定チャンネルのカウントから論理的なデータ値への変換を司ります．


		// (例) 
		// パルスロガーのCH1は，1カウントがブロック1＋ブロック2の10kWhに相当する．
		// CH2は，1カウントがブロック1の10kWhに相当する．
		// ということは，ブロック1の電力量は，CH2のカウント数の10倍であり，
		// ブロック2の電力量は，(CH1のカウント数－CH2のカウント数)の10倍である． 
		// これを，パルスロガーのチャンネルから見ると，
		// CH1の1カウントは，ブロック1には寄与せず，ブロック2には10kWhの寄与を及ぼす．
		// CH2の1カウントは，ブロック1に10kWhの寄与を及ぼし，ブロック2には-10kWhの寄与を及ぼす．

		// それらのチャンネルを実装するコードは，次のようになる．

		// var gains1 = new Dictionary<int, double>();
		// gains1[2] = 10;
		// var ch1 = new Channel{ Gains = gains1 };
		// var gains2 = new Dictionary<int, double>();
		// gains2[1] = 10;
		// gains2[2] = -10;
		// var ch2 = new Channel{ Gains = gains2 };


		/// <summary>
		/// カウントをデータに変換する際の倍率を取得／設定します．
		/// </summary>
		public Dictionary<int, double> Gains { get; set; }

		/// <summary>
		/// カウントをデータに変換します．
		/// </summary>
		/// <param name="count"></param>
		/// <returns></returns>
		public Dictionary<int, double> CountToActualData(int count)
		{
			var actualData = new Dictionary<int, double>();
			foreach (var gain in Gains)
			{
				actualData[gain.Key] = count * gain.Value;
			}
			return actualData;
		}
	}
	#endregion
}
