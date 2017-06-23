using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{

	namespace Data
	{
		namespace New
		{
			// (1.1.4)
			#region ConsumptionRecorderクラス
			public class ConsumptionRecorder : ConsumptionData
			{
				// (1.5.0) channels引数を追加。
				public ConsumptionRecorder(IConnectionProfile profile, int[] channels) : base(profile, channels)
				{ }

				// (1.1.4.1)int=>doubleなIDictionaryにも対応．
				#region *データをDBに追加(InsertData)

				public async Task InsertDataAsync(DateTime time, IDictionary<int, double> data)
				{
					var insert_queries = data.Select(
						ch_data => string.Format("INSERT INTO consumptions_10min VALUES({0}, {1}, {2})",
											TimeConverter.TimeToInt(time), ch_data.Key, Math.Truncate(ch_data.Value))
					);
					await InsertDataAsync(insert_queries);
				}

				public async Task InsertDataAsync(DateTime time, IDictionary<int, int> data)
				{
					var insert_queries = data.Select(
						ch_data => string.Format("INSERT INTO consumptions_10min VALUES({0}, {1}, {2})",
											TimeConverter.TimeToInt(time), ch_data.Key, ch_data.Value)
					);
					await InsertDataAsync(insert_queries);
				}

				// (1.1.4.2)コミットの位置を修正．
				async Task InsertDataAsync(IEnumerable<string> queries)
				{
					using (var connection = await profile.GetConnectionAsync())
					{
						// トランザクションを生成して...
						using (var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
						{
							try
							{
								using (var command = connection.CreateCommand())
								{
									foreach (var query in queries)
									{
										command.CommandText = query;
										command.ExecuteNonQuery();
									}
									// コミットする．
									transaction.Commit();
								}
							}
							catch (Exception)
							{
								transaction.Rollback();
								throw;
							}
						}
					}
				}

				#endregion

			}
			#endregion

		}
	}

}