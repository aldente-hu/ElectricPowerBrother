using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using RetrieveData;

	namespace PulseLoggers
	{

		#region IPulseLoggerインターフェイス
		public interface IPulseLogger
		{
			IEnumerable<TimeSeriesDataDouble> GetDataAfter(DateTime time, int max = -1);
			
			Task<IDictionary<DateTime, TimeSeriesDataDouble>> GetDataAfterTask(DateTime time);

			void Configure(System.Xml.Linq.XElement config);	// configは，その機種に対応したXElement．

		}
		#endregion

		public interface IPulseLoggerWithSubSource : IPulseLogger
		{
			IEnumerable<TimeSeriesDataDouble> GetDataFromSubSourceAfter(DateTime time, int max = -1);
			Task<IDictionary<DateTime, TimeSeriesDataDouble>> GetDataFromSubSourceAfterTask(DateTime time);
		}


		#region Channelクラス
		public class Channel
		{
			public Dictionary<int, double> Gains { get; set; }

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


		#region [abstract]StoragingPulseLoggerクラス
		public abstract class StoragingPulseLogger : IPulseLogger
		{
			protected SortedDictionary<DateTime, TimeSeriesDataDouble> storagedData = new SortedDictionary<DateTime, TimeSeriesDataDouble>();
			protected void RemoveOldData(DateTime time)
			{
				var old_keys = storagedData.Keys.Where(k => k < time).ToArray();
				foreach (var k in old_keys)
				{
					storagedData.Remove(k);
				}
			}

			public IEnumerable<TimeSeriesDataDouble> GetDataAfter(DateTime time, int max = -1)
			{
				// この時点で，timeより古いデータを破棄していいような気がする...ので削除する．
				RemoveOldData(time);

				while (time < DateTime.Now)
				{
					TimeSeriesDataDouble data;
					if (storagedData.TryGetValue(time, out data))
					{
						yield return data;
						// ここでは削除を行わない！(取得したデータを使う準備ができているかどうかわからないから．)
						//storagedData.Remove(time);
						time = time.AddMinutes(10);	// (0.2.1.2)この間違いが多すぎ！
					}
					else
					{
						foreach (var new_data in RetrieveActualData(time))
						{
							//if (new_data.Time >= time)	// timeは変わらないのだから，これがfalseになることはありえないのでは？
							//{
							try
							{
								storagedData.Add(new_data.Time, new_data);
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

			// ActualData関連

			public List<Channel> Channels { get { return _channels; } }
			List<Channel> _channels = new List<Channel>();

			// 2 => 35 みたいなデータだけ返せばよい．

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


			/// <summary>
			/// ロガーにアクセスしてデータを取り出します．
			/// </summary>
			/// <param name="time"></param>
			/// <param name="max"></param>
			/// <returns></returns>
			public abstract IEnumerable<TimeSeriesDataInt> RetrieveCountsAfter(DateTime time, int max = -1);


			// 並列化関連
			public Task<IDictionary<DateTime, TimeSeriesDataDouble>> GetDataAfterTask(DateTime time)
			{
				return new Task<IDictionary<DateTime, TimeSeriesDataDouble>>(() => { return GetDataAfter(time).ToDictionary(data => data.Time); });
			}

			#region 設定関連

			public virtual void Configure(System.Xml.Linq.XElement config)
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

			}

			#endregion

		}
		#endregion


		#region [abstract]StoragingPulseLoggerクラス
		public abstract class StoragingPulseLoggerWithSubSource : StoragingPulseLogger, IPulseLoggerWithSubSource
		{
			/// <summary>
			/// ロガーにアクセスしてデータを取り出します．
			/// </summary>
			/// <param name="time"></param>
			/// <param name="max"></param>
			/// <returns></returns>
			public abstract IEnumerable<TimeSeriesDataInt> RetrieveCountsAfter(DateTime time, bool fromSubSource, int max = -1);

			// ※これをコピペせずに済ませたい...
			public IEnumerable<TimeSeriesDataDouble> GetDataFromSubSourceAfter(DateTime time, int max = -1)
			{
				// この時点で，timeより古いデータを破棄していいような気がする...ので削除する．
				RemoveOldData(time);

				while (time < DateTime.Now)
				{
					TimeSeriesDataDouble data;
					if (storagedData.TryGetValue(time, out data))
					{
						yield return data;
						// ここでは削除を行わない！(取得したデータを使う準備ができているかどうかわからないから．)
						//storagedData.Remove(time);
						time = time.AddMinutes(10); // (0.2.1.2)この間違いが多すぎ！
					}
					else
					{
						foreach (var new_data in RetrieveActualData(time))
						{
							//if (new_data.Time >= time)	// timeは変わらないのだから，これがfalseになることはありえないのでは？
							//{
							try
							{
								storagedData.Add(new_data.Time, new_data);
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


			// 並列化関連
			public Task<IDictionary<DateTime, TimeSeriesDataDouble>> GetDataFromSubSourceAfterTask(DateTime time)
			{
				return new Task<IDictionary<DateTime, TimeSeriesDataDouble>>(() => { return GetDataAfter(time).ToDictionary(data => data.Time); });
			}
		}
		#endregion
	}
}