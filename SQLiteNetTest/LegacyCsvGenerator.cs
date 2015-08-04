using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

using System.Xml.Linq;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Data;
	using Helpers;

	namespace Legacy
	{

		// 1時間ごと

		// # 07/28/2015
		// # 時刻,理工学部,1号館,2号館,総情センター
		// 01,361,171,190,14
		// 02,344,160,184,13


		// 1時間ごとと10分ごとのCSVを作成するクラスを分けた方がいいのでは？




		// (1.3.6)Legacy名前空間に落とし込む．
		#region DailyCsvGeneratorクラス
		public class DailyCsvGenerator : ConsumptionData, IPlugin
		{

			public DailyCsvGenerator(string databaseFile) : base(databaseFile)
			{ }

			/// <summary>
			/// データが1時間ごとかどうかを示す値を取得／設定します．
			/// </summary>
			public bool IsHourly { get; set; }

			private string TimeFormat
			{
				get { return this.IsHourly ? "HH" : "HH:mm"; }
			}

			public string CsvRoot
			{ get; set; }

			/// <summary>
			/// CSVファイルの文字エンコーディングを取得／設定します．デフォルトはEncoding.UTF8です．
			/// </summary>
			public Encoding CsvEncoding
			{
				get { return this._encoding; }
				set
				{
					this._encoding = value;
				}
			}
			Encoding _encoding = Encoding.UTF8;

			const int RECURSIVE_COUNT_LIMIT = 31;

			// UpdateActionに設定する？
			// 更新したファイルの最新データの時刻を返す．
			public DateTime UpdateFiles(DateTime latestData, int recursiveCount = 0)
			{
				// 07/31 02:00 -> 07/31 00:00
				// 07/31 00:00 -> 07/30 00:00
				DateTime date_origin = latestData.AddSeconds(-1).Date;
				DateTime date_end = date_origin.AddDays(1);

				// date_originの日のファイルが存在するか確認．
				string target = Path.Combine(CsvRoot, DailyCsvDestinationGenerator.Generate(date_origin));

				DateTime csv_last_data;
				StreamWriter writer = null;

				try
				{
					if (File.Exists(target))
					{
						// あれば，そのファイルを開く．
						csv_last_data = this.RetrieveLatestTime(target, date_origin);
						writer = new StreamWriter(File.Open(target, FileMode.Append, FileAccess.Write), this.CsvEncoding);
					}
					else
					{
						// なければ，その前日のファイルについて同様のことを行う．
						if (recursiveCount < RECURSIVE_COUNT_LIMIT)
						{
							csv_last_data = UpdateFiles(date_origin, recursiveCount + 1);
							if (date_origin != csv_last_data)
							{
								// 出力終了．
								return csv_last_data;
							}
						}

						// 腹をくくって(あるいは満を持して)出力開始．

						csv_last_data = date_origin;

						// 月別のディレクトリがなければ作る．
						var directory = Path.GetDirectoryName(target);
						if (!Directory.Exists(directory))
						{
							Directory.CreateDirectory(directory);
						}
						// ファイルを新規作成する．
						writer = new StreamWriter(File.Open(target, FileMode.CreateNew, FileAccess.Write), this.CsvEncoding);
						OutputHeader(writer, date_origin);

					}

					// fileを再使用することはできない！(writerをDisposeすると，fileもDisposeされる．)
				
					// ※chの決め打ちも解消しましょう．
					SortedSet<int> channels_set = new SortedSet<int>();
					var all_channels = 
						Columns.Select(col => col.Channels).Aggregate(
							(total_channels, channels) => total_channels.Union(channels).ToArray()
						);
					
					var data = this.GetParticularConsumptions(csv_last_data, date_end, all_channels);

					csv_last_data = Record(writer, csv_last_data, data);
				}

				finally
				{
					if (writer != null)
					{
						writer.Dispose();
					}
				}
				return csv_last_data;


				// 旧実装

				// 07/31 02:00 -> 07/31 00:00
				// 07/31 00:00 -> 07/30 00:00
/*				DateTime date_origin = (latestData.TimeOfDay == TimeSpan.Zero)
					? latestData.AddDays(-1) : latestData - latestData.TimeOfDay;

				// date_originの日のファイルが存在するか確認．
				string target = Path.Combine(CsvRoot, DailyCsvDestinationGenerator.Generate(date_origin));
				if (!File.Exists(target))
				{
					// なければ，その前日のファイルについて同様のことを行う．
					if (recursiveCount < RECURSIVE_COUNT_LIMIT)
					{
						if (date_origin == UpdateFiles(date_origin, recursiveCount + 1))
						{
							return;
						}
					}
					else
					{ return; }
				}

				// あれば，そのファイルを開く．
				DateTime csv_last_data;
				using (var file = File.Open(target, FileMode.Open, FileAccess.ReadWrite))
				{
					// ファイルに記録された最新の時刻を取得する．
					Match last_match = null;
					using (StreamReader reader = new StreamReader(file))
					{
						while(!reader.EndOfStream)
						{
							var line = reader.ReadLine();
							var time_pattern = new Regex(@"^(\d\d)(?:\:(\d\d))?,");
							var m = time_pattern.Match(line);
							if (m.Success)
							{
								last_match = m;
							}
						}
					}

					if (last_match == null)
					{
						throw new ApplicationException("データのないCSVファイルがあります！");
					}

					int hour = int.Parse(last_match.Groups[1].Value);
					int min = 0;
					if (last_match.Groups[2].Captures.Count > 0)
					{
						min = int.Parse(last_match.Groups[2].Captures[0].Value);
					}
					csv_last_data = date_origin.AddMinutes(hour * 60 + min);

					// ※chの決め打ちも解消しましょう．
					var data = this.GetParticularConsumptions(csv_last_data, latestData, 1,2,3);

					// ※とりあえずdetailから考える．

					// こういうときってfileのポインタはどこにあるの？末尾？
					using (StreamWriter writer = new StreamWriter(file))
					{
						foreach (var onetime in data.OrderBy(d => d.Key))
						{
							// onetime.Value : { 1 => 36, 2 => 39, 3 => 4 }
							OutputDataRow(writer, onetime.Key, onetime.Value);
						}
					}

				}
					

				// 記録すべきデータがあれば，書き込む．
 * 
 * */
			}


			// (1.3.6)とにかく1日分のデータを記録する．
			// 24時までコンプリートすればtrue，さもなければfalseを返す．
			public bool OutputOneDay(DateTime target_date)
			{
				// dateは時刻を度外視する．
				// 7/31 02:00 だろうと，7/31 00:00 だろうと，7/31のファイルを出力する．
				// つまり，8/01 00:00 までのデータを出力する．

				var date_end = target_date.Date.AddDays(1);

				// 前日までのデータが出力されていなくても気にしないことにする？



				return date_end == UpdateFiles(date_end, RECURSIVE_COUNT_LIMIT);

				// 旧実装


				// 07/31 02:00 -> 07/31 00:00
				// 07/31 00:00 -> 07/31 00:00
/*				DateTime date_origin = date - date.TimeOfDay;
				DateTime date_end = date_origin.AddDays(1);

				// date_originの日のファイルが存在するか確認．
				string target = Path.Combine(CsvRoot, DailyCsvDestinationGenerator.Generate(date));

				DateTime csv_last_data;
				StreamWriter writer = null;

				try
				{
					if (File.Exists(target))
					{
						// あれば，そのファイルを開く．
						csv_last_data = this.RetrieveLatestTime(target, date_origin);
						writer = new StreamWriter(File.Open(target, FileMode.Append, FileAccess.Write), this.CsvEncoding);
					}
					else
					{
						csv_last_data = date_origin;

						// 月別のディレクトリがなければ作る．
						var directory = Path.GetDirectoryName(target);
						if (!Directory.Exists(directory))
						{
							Directory.CreateDirectory(directory);
						}
						// ファイルを新規作成する．
						writer = new StreamWriter(File.Open(target, FileMode.CreateNew, FileAccess.Write), this.CsvEncoding);
						OutputHeader(writer, date_origin);

					}

					// fileを再使用することはできない！(writerをDisposeすると，fileもDisposeされる．)

					// ※chの決め打ちも解消しましょう．
					var data = this.GetParticularConsumptions(csv_last_data, date_end, 1, 2, 3);

					csv_last_data = Record(writer, csv_last_data, data);
				}

				finally
				{
					if (writer != null)
					{
						writer.Dispose();
					}
				}
  
				return csv_last_data == date_end;

 */ }

			protected virtual DateTime Record(StreamWriter writer, DateTime csv_last_data, IDictionary<DateTime, IDictionary<int, int>> data)
			{
				foreach (var onetime in data.OrderBy(d => d.Key))
				{
					// onetime.Value : { 1 => 36, 2 => 39, 3 => 4 }
					OutputDataRow(writer, onetime.Key, onetime.Value);
					if (csv_last_data < onetime.Key)
					{
						csv_last_data = onetime.Key;
					}
				}
				return csv_last_data;
			}

			// (1.3.8.2)データが無くても例外を出さないようにする．
			// (1.3.6)
			DateTime RetrieveLatestTime(string fileName, DateTime dateOrigin)
			{
				// ファイルに記録された最新の時刻を取得する．
				Match last_match = null;
				using (StreamReader reader = new StreamReader(File.Open(fileName, FileMode.Open, FileAccess.ReadWrite)))
				{
					while (!reader.EndOfStream)
					{
						var line = reader.ReadLine();
						var time_pattern = new Regex(@"^(\d\d)(?:\:(\d\d))?,");
						var m = time_pattern.Match(line);
						if (m.Success)
						{
							last_match = m;
						}
					}
				}

				if (last_match == null)
				{
					return dateOrigin;
					//throw new ApplicationException("データのないCSVファイルがあります！");
				}

				int hour = int.Parse(last_match.Groups[1].Value);
				int min = 0;
				if (last_match.Groups[2].Captures.Count > 0)
				{
					min = int.Parse(last_match.Groups[2].Captures[0].Value);
				}
				return dateOrigin.AddMinutes(hour * 60 + min);


			}


			// <LegacyCsvGenerator root="B:\data" encoding="Shift_JIS">
			//    <Column name="理工学部" ch="1+2" />
			//    <Column name="1号館" ch="1" />
			//    <Column name="2号館" ch="2" />
			//    <Column name="総情センター" ch="3" />
			// </LegacyCsvGenerator>

			// (1.3.9) 実装開始．
			public void Configure(System.Xml.Linq.XElement config)
			{
				this.CsvRoot = (string)config.Attribute("root");

				foreach (var attribute in config.Attributes())
				{
					switch(attribute.Name.LocalName)
					{
						case "encoding":
							this.CsvEncoding = Encoding.GetEncoding(attribute.Value);
							break;
					}
				}

				foreach (var element in config.Elements("Column"))
				{
					Columns.Add(CsvColumn.Generate(element));
				}

			}

			public IList<CsvColumn> Columns { get { return _columns; } }
			List<CsvColumn> _columns = new List<CsvColumn>();

			public class CsvColumn
			{
				public string Name { get; set; }
				public int[] Channels { get; set; }

				// <Column name="理工学部" ch="1+2" />

				public static CsvColumn Generate(XElement columnElement)
				{
					var col = new CsvColumn();
					col.Name = (string)columnElement.Attribute("name");
					col.Channels = columnElement.Attribute("ch").Value.Split('+', ',').Select(d => int.Parse(d)).ToArray();		
					return col;
				}

			}

			// (1.3.9) 2行目をコメントアウト．
			public void OutputHeader(StreamWriter writer, DateTime date)
			{
				// ※決め打ちバージョン．

				// ※↓これを動的に生成したい．
				writer.WriteLine("# " + date.ToString("MM/dd/yyyy"));
				string[] series = Columns.Select(c => c.Name).ToArray();
				//string[] series = new string[] { "理工学部", "1号館", "2号館", "総情センター" };
				writer.WriteLine("# 時刻," + string.Join(",", series));

			}


			public void OutputDataRow(StreamWriter writer, DateTime time, IDictionary<int, int> data)
			{
				// ※決め打ちバージョン．

				// ※↓これを動的に生成したい．
				int[] output_data = Columns.Select(col => col.Channels.Sum((ch) => data[ch])).ToArray();
				//int[] output_data = new int[] { data[1] + data[2], data[1], data[2], data[3]};

				var time_format = time.TimeOfDay == TimeSpan.Zero ? this.TimeFormat.Replace("HH", "24") : this.TimeFormat;
				writer.WriteLine("{0},{1}", time.ToString(time_format), string.Join(",", output_data));
			}

		}
		#endregion




		// とりあえずここに書く．

		// (1.3.7)
		#region DailyHourlyCsvGeenratorクラス
		public class DailyHourlyCsvGenerator : DailyCsvGenerator
		{
			public DailyHourlyCsvGenerator(string databaseFile) : base(databaseFile)
			{ }

			protected override DateTime Record(StreamWriter writer, DateTime csv_last_data, IDictionary<DateTime, IDictionary<int, int>> data)
			{
				while (true)
				{
					var next_hour = csv_last_data.AddHours(1);
					var next_hour_data = data.Where(d => (d.Key > csv_last_data && d.Key <= next_hour));
					if (next_hour_data.Count() == 6)
					{
						// { 1 => 38, 2 => 39, 3 =>  6 } と
						// { 1 => 30, 2 => 38, 3 =>  7 } から
						// { 1 => 68, 2 => 77, 3 => 13 } を得るにはどうすればいいんだろう？
						var hourly_total = next_hour_data.Select(d => d.Value).Aggregate((total, onetime) => Total(total, onetime));

						// onetime.Value : { 1 => 36, 2 => 39, 3 => 4 }
						OutputDataRow(writer, next_hour, hourly_total);
						csv_last_data = next_hour;
					}
					else
					{
						return csv_last_data;
					}
				}
			}

			static IDictionary<int, int> Total(IDictionary<int, int> one, IDictionary<int, int> other)
			{
				Dictionary<int, int> total = new Dictionary<int, int>();
				foreach (int key in one.Keys)
				{
					total[key] = one[key] + other[key];
				}
				return total;
			}

		}
		#endregion


		#region [static]DailyCsvDestinationGeneratorクラス
		public static class DailyCsvDestinationGenerator
		{
			// 2015年7月2日を表すDateTimeから， "public/data/Y2015_07/D02_01.csv"のような文字列を生成する．

			// ルートの処理はどこで行う？

			public static string Generate(DateTime date)
			{
				return date.ToString(@"\Yyyyy_MM/\Ddd_01.c\sv");
			}
		}
		#endregion

	}
}
