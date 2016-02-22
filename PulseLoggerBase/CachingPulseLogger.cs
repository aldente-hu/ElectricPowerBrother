using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Base
{
	// (0.0.1)作ってみたが...CachingじゃないPulseLoggerなんて想定していないのでは？
	#region [abstract]CachingPulseLoggerクラス
	public abstract class CachingPulseLogger : IPulseLogger, ISetUpWithXElement
	{
		// パルスロガーからカウント数データを取得したら，即座に実データに変換して，それを保持します．

		#region データキャッシュ関連

		protected SortedDictionary<DateTime, TimeSeriesDataDouble> cachedData = new SortedDictionary<DateTime, TimeSeriesDataDouble>();
		protected void RemoveOldData(DateTime time)
		{
			var old_keys = cachedData.Keys.Where(k => k < time).ToArray();
			foreach (var k in old_keys)
			{
				cachedData.Remove(k);
			}
		}

		#endregion

		// この部分をIPulseLoggerにしておくかどうかは検討が必要．
		#region IPulseLogger実装

		/// <summary>
		/// 指定時刻以降の，実データに変換した値取得します．
		/// </summary>
		/// <param name="time"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public IEnumerable<TimeSeriesDataDouble> GetActualDataAfter(DateTime time, int max = -1)
		{
			// この時点で，timeより古いデータを破棄する．
			RemoveOldData(time);

			if (RestBegin.HasValue)
			{
				if (time > RestBegin.Value)
				{
					// 休憩モード
					var rest_end = RestEnd ?? DateTime.Now;
					while (time <= RestEnd.Value)
					{
						cachedData.Add(time, ReturnZeroData(time));
						time = time.AddMinutes(10);
					}
				}
			}

			while (time < DateTime.Now)
			{
				// timeのデータを返す．

				// すでにstorageされていれば，それを返す．
				// そうでなければパルスロガーにアクセスする．


				TimeSeriesDataDouble data;
				if (cachedData.TryGetValue(time, out data))
				{
					yield return data;
					// ここでは破棄を行わない！(取得元でdataを使う準備ができているかどうかわからないから．)
					//storagedData.Remove(time);
					time = time.AddMinutes(10);
				}
				else
				{
					foreach (var new_data in RetrieveActualData(time))
					{
						//if (new_data.Time >= time)	// timeは変わらないのだから，これがfalseになることはありえないのでは？
						//{
						try
						{
							cachedData.Add(new_data.Time, new_data);
						}
						catch (AggregateException ex)
						{
							throw new AggregateException(
								string.Format("[I tried to inserting {0} but it always existed.]", new_data.Time) + ex.Message);
						}
						yield return new_data;
						//}
					}
					yield break;
				}
			}

		}

		#endregion

		#region ActualData関連

		public List<Channel> Channels { get { return _channels; } }
		List<Channel> _channels = new List<Channel>();

		// 2 => 35 みたいなデータだけ返せばよい．

		/// <summary>
		/// time以降のデータを取り出します．
		/// </summary>
		/// <param name="time"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public IEnumerable<TimeSeriesDataDouble> RetrieveActualData(DateTime time, int max = -1)
		{
			foreach (var data in RetrieveCountsAfter(time, max))
			{
				var actualData = new Dictionary<int, double>();
				foreach (var count in data.Data)
				{
					int channel = count.Key;

					try
					{
						foreach (var a_data in _channels[channel].CountToActualData(count.Value))
						{
							if (actualData.Keys.Contains(a_data.Key))
							{
								actualData[a_data.Key] += a_data.Value;
							}
							else
							{
								actualData[a_data.Key] = a_data.Value;
							}
						}
					}
					catch (ArgumentOutOfRangeException ex)
					{
						throw new ArgumentOutOfRangeException(
							string.Format("[I have {0} channels but was required {1}.]", channel, _channels.Count) + ex.Message);
					}
				}
				yield return new TimeSeriesDataDouble { Time = data.Time, Data = actualData };
			}

		}

		#endregion

		/// <summary>
		/// パルスロガーにアクセスしてデータを取り出します．
		/// </summary>
		/// <param name="time"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public abstract IEnumerable<TimeSeriesDataInt> RetrieveCountsAfter(DateTime time, int max = -1);

		#region 並列化関連
		public Task<IDictionary<DateTime, TimeSeriesDataDouble>> GetCountsAfterTask(DateTime time)
		{
			return new Task<IDictionary<DateTime, TimeSeriesDataDouble>>(() => { return GetActualDataAfter(time).ToDictionary(data => data.Time); });
		}
		#endregion


		#region ローカルファイルシステムからのデータ取得関連

		/// <summary>
		/// ローカルファイルシステムからのデータ取得が可能かどうかの値を取得します．
		/// 基本クラスではfalseを返します．実装先のパルスロガーでそれが可能であれば，オーバーライドしてtrueを返して下さい．
		/// </summary>
		public virtual bool CanGetCountsFromLocal
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// ★ローカルファイルからデータを取得する場合の，ルートパスを指定します．(やぶれかぶれな対応．)
		/// CanGetCountsFromLocalプロパティがfalseを返す場合，setterはNotSupportedExceptionをスローします．
		/// </summary>
		public string LocalRoot {
			get
			{
				return _localRoot;
			}
			set
			{
				if (this.CanGetCountsFromLocal)
				{
					_localRoot = value;
				}
				else
				{
					throw new NotSupportedException("このパルスロガーは，ローカルファイルシステムからのデータ取得に対応していません．");
				}

			}
		}
		string _localRoot = string.Empty;

		#endregion


		#region 休憩関連

		/// <summary>
		/// 測定休止が始まる時刻を取得／設定します．この時刻の次のデータからは取得時に0が返ります．
		/// 値がnullの場合，休憩が設定されていないものとみなします．
		/// </summary>
		protected DateTime? RestBegin { get; set; }
		/// <summary>
		/// 測定休止が終了する時刻を取得／設定します．
		/// RestBeginプロパティがnullの場合は無効です．値がnullの場合，永遠に続くものとみなします．
		/// </summary>
		protected DateTime? RestEnd { get; set; }

		/// <summary>
		/// 該当する全てのチャンネルが0であるデータを返します．
		/// 未計測データを処理する場合に使うことを想定しています．
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		protected TimeSeriesDataDouble ReturnZeroData(DateTime time)
		{
			var data = new Dictionary<int, double>();
			foreach (var ch in Channels.SelectMany(l_ch => l_ch.Gains.Keys))
			{
				data[ch] = 0;
			}
			return new TimeSeriesDataDouble { Time = time, Data = data };
		}
		#endregion

		#region 設定関連

		#region ISetUpWithXElement実装

		public virtual void SetUp(System.Xml.Linq.XElement config)
		{
			// config.Name.LocalNameをチェックしますか？

			// Chに関する情報をここに入れる？

			foreach (var element in config.Elements("Channel"))
			{
				var gains = new Dictionary<int, double>();
				foreach (var data in element.Elements("Data"))
				{
					gains.Add((int)data.Attribute("Ch"), ((double?)data.Attribute("Gain")) ?? 1.0);
				}
				this.Channels.Add(new Channel { Gains = gains });
			}

			// Rest関連

			var rest_element = config.Element("Rest");
			if (rest_element != null)
			{

			}

		}

		#endregion

		#endregion

	}
	#endregion
}
