using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{

	namespace Data
	{

		// (1.1.4)
		#region ConsumptionRecorderクラス
		public class ConsumptionRecorder : ConsumptionData
		{
			// (1.5.0) channels引数を追加。
			public ConsumptionRecorder(string fileName, int[] channels) : base(fileName, channels)
			{ }

			// (1.1.4.1)int=>doubleなIDictionaryにも対応．
			#region *データをDBに追加(InsertData)

			public void InsertData(DateTime time, IDictionary<int, double> data)
			{
				var insert_queries = data.Select(
					ch_data => string.Format("INSERT INTO consumptions_10min VALUES({0}, {1}, {2})",
										TimeConverter.TimeToInt(time), ch_data.Key, Math.Truncate(ch_data.Value))
				);
				InsertData(insert_queries);
			}

			public void InsertData(DateTime time, IDictionary<int, int> data)
			{
				var insert_queries = data.Select(
					ch_data => string.Format("INSERT INTO consumptions_10min VALUES({0}, {1}, {2})",
										TimeConverter.TimeToInt(time), ch_data.Key, ch_data.Value)
				);
				InsertData(insert_queries);
			}

			// (1.1.4.2)コミットの位置を修正．
			void InsertData(IEnumerable<string> queries)
			{
				using (var connection = new SQLiteConnection(this.ConnectionString))
				{
					connection.Open();
					// トランザクションを生成して...
					using (var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
					{
						try
						{
							using (SQLiteCommand command = connection.CreateCommand())
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

					//connection.Close();
				}
			}

			#endregion

		}
		#endregion

	}

}