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
			public ConsumptionRecorder(string fileName) : base(fileName)
			{ }

			public void InsertData(DateTime time, IDictionary<int, int> data)
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
								foreach (var ch_data in data)
								{
									command.CommandText = string.Format(
										"INSERT INTO consumptions_10min VALUES({0}, {1}, {2})", Convert.TimeToInt(time), ch_data.Key, ch_data.Value);
									command.ExecuteNonQuery();
									//command.ExecuteNonQueryAsync();

									// コミットする．
									transaction.Commit();
								}
							}
						}
						catch (Exception ex)
						{
							transaction.Rollback();
							throw ex;
						}
					}

					connection.Close();
				}

			}


		}
		#endregion

	}

}