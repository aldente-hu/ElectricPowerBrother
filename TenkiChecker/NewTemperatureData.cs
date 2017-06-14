using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Xml.Linq;
using System.Xml;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{

	using Data;

	namespace TenkiChecker.Data
	{

		namespace New
		{

			#region TemperatureDataクラス
			public class TemperatureData : DataTickerModel  /* , ITemperatureData */
			{
				#region *定番コンストラクタ(TemperatureData)
				public TemperatureData(IConnectionProfile profile)
					: base(profile)
				{ }
				#endregion

				// temperatures
				//   - time integer
				//   - temperature integer
				// temperatureは摂氏温度の10倍の値を保持する．

				#region データIOメソッド

				#region *最新データの時刻を取得(GetLatestDataTime)
				public override async Task<DateTime> GetLatestDataTimeAsync()
				{
					return await GetLatestTimeAsync();
				}

				// (1.3.0)表向きにはGetLatestDataTimeメソッドで代替．
				async Task<DateTime> GetLatestTimeAsync()
				{
					using (var connection = await profile.GetConnectionAsync())
					{
						DateTime time;
						using (var command = connection.CreateCommand())
						{
							command.CommandText = "select max(time) from temperatures";
							using (var reader = await command.ExecuteReaderAsync())
							{
								await reader.ReadAsync();
								time = TimeConverter.IntToTime(System.Convert.ToInt32(reader[0]));
							}
						}
						//connection.Close();
						return time;
					}

				}
				#endregion

				#region *指定した範囲の気温データを取得(GetTemperatures)
				public async Task<IDictionary<DateTime, decimal>> GetTemperaturesAsync(DateTime from, DateTime to)
				{
					var temperatures = new Dictionary<DateTime, decimal>();

					using (var connection = await profile.GetConnectionAsync())
					{
						using (var command = connection.CreateCommand())
						{
							command.CommandText = string.Format(
								"select time, temperature from temperatures where time >= {0} and time <= {1}", TimeConverter.TimeToInt(from), TimeConverter.TimeToInt(to));
							using (var reader = command.ExecuteReader())
							{
								while (reader.Read())
								{
									temperatures.Add(TimeConverter.IntToTime(System.Convert.ToInt32(reader[0])), IntToTemperature(System.Convert.ToInt32(reader[1])));
								}
							}
						}
						//connection.Close();
						return temperatures;
					}

				}
				#endregion

				#region *1日分の気温データを取得(GetOneDayTemperature)
				protected async Task<IDictionary<DateTime, decimal>> GetOneDayTemperaturesAsync(DateTime date)
				{
					var from = date - date.TimeOfDay;
					var to = from.AddDays(1);
					return await GetTemperaturesAsync(from, to);
				}
				#endregion

				// 02/20/2015 by aldente : 非同期にしてみた．
				#region *気温データを追加(InsertTemperature)
				public async Task InsertTemperatureAsync(DateTime time, decimal temperature)
				{
					using (var connection = await profile.GetConnectionAsync())
					{
						using (var command = connection.CreateCommand())
						{
							command.CommandText = string.Format(
								"INSERT INTO temperatures VALUES({0}, {1})", TimeConverter.TimeToInt(time), TemperatureToInt(temperature));
							//command.ExecuteNonQuery();
							await command.ExecuteNonQueryAsync();
						}
						//connection.Close();
					}

				}
				#endregion

				#endregion

				#region 気温変換用staticメソッド

				static int TemperatureToInt(decimal temperature)
				{
					return System.Convert.ToInt32(decimal.Ceiling(temperature * 10));
				}

				static decimal IntToTemperature(int data)
				{
					return data / 10.0M;
				}

				#endregion

			}
			#endregion

		}
	}

}
