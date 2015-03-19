using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Data;
	using Helpers;

	namespace EvaluateAlerts
	{

		public class AlertJudgement
		{
			IDictionary<int, AlertLevel> levels;

			readonly AlertData data;
			readonly ConsumptionData c_data;

			#region *コンストラクタ(AlertJudgement)
			public AlertJudgement(string databaseFile) : this()
			{
				data = new AlertData(databaseFile);
				c_data = new ConsumptionData(databaseFile);	// ←dataと分ける意味あるの？
			}

			AlertJudgement()
			{
				levels = new Dictionary<int, AlertLevel>();

				// 決め打ちでレベルを用意してみる．
				this.DefineLevel(new AlertLevel
				{
					Rank = 16,
					Name = "警報",
					InBorder = (data) => { return data.Take(2).Min() >= 102; },
					OutBorder = (data) => { return data.Take(2).Max() < 96; }
				});
				this.DefineLevel(new AlertLevel
				{
					Rank = 4,
					Name = "注意報",
					InBorder = (data) => { return data.Take(2).Min() >= 97; },
					OutBorder = (data) => { return data.Take(2).Max() < 92; }
				});

			}
			#endregion


			public void DefineLevel(AlertLevel level)
			{
				if (level.Rank > 0)
				{
					levels.Add(level.Rank, level);
				}
			}


			// とりあえず引数なしで．
			public void Judge()
			{
				// time時点で最新のデータをn件取得する．
				var recent_data = c_data.GetRecentData(3).OrderByDescending(d => d.Key);
				var current_rank = data.GetCurrentRank();

				int new_rank = Check(current_rank, recent_data.Select(d => d.Value).ToArray());
				if (new_rank != current_rank)
				{
					// 記録する．
					data.InsertData(new AlertElement { 
						DataTime=recent_data.First().Key, DeclaredAt = DateTime.Now, Rank = new_rank });
					// ログを出力してもいいかも！
					//Console.WriteLine("New rank : {0}, previous rank : {1}", new_rank, current_rank);
				}
			}

			int Check(int current_rank, int[] recent_data)
			{

				// 現在より上のランクのレベルに達したか？
				foreach (var level in levels.OrderBy(l => -l.Key).Where((l) => l.Key > current_rank))
				{
					// Inborderをチェック．
					if (level.Value.InBorder(recent_data))
					{
						return level.Key;
					}
				}
				// 現在より下のランクのレベルに達したか？
				foreach (var level in levels.OrderBy(l => l.Key).Where((l) => l.Key < current_rank))
				{
					// OutBorderをチェック．
					if (level.Value.OutBorder(recent_data))
					{
						return level.Key;
					}
				}
				return current_rank;
			}



			// (1.0.1)
			public void OutputAtomFeed(string destination, int n = 6)
			{
				var feed = new AtomFeed
				{
					ID = "http://den.st.hirosaki-u.ac.jp/riko-alert",
					Title = "デマンド注意報",
					Author = "電力量計測システム",
					UpdatedAt = DateTime.Now,
					SelfLink = "http://den.st.hirosaki-u.ac.jp/alert.atom",
				};

				// 6件取得．
				var recent_data = data.GetRecentData(n);
				var times = recent_data.Keys.OrderByDescending(k => k).ToArray();

				for (int i = 0; i < times.Length - 1; i++)
				{
					var current = recent_data[times[i]];
					var current_time = current.DataTime.ToString("HH時mm分");
					var current_long_time = current.DataTime.ToString("M月d日HH時mm分");
					AtomEntry entry = new AtomEntry
					{
						ID = "http://den.st.hirosaki-u.ac.jp/alert" + SQLiteData.Convert.TimeToInt(current.DeclaredAt),
						PublishedAt = current.DeclaredAt
					};

					if (recent_data[times[i]].Rank > recent_data[times[i+1]].Rank)
					{
						// レベルが上昇．
						var name = levels[current.Rank].Name;
						entry.Title = string.Format("{0}発令({1})", name, current_time);
						entry.Content = string.Format("{1}に，{0}が発令されました．", name, current_long_time);

						feed.Entries.Add(entry);
					}
					else if (recent_data[times[i]].Rank < recent_data[times[i + 1]].Rank)
					{
						// レベルが下降．
						var name = levels[recent_data[times[i+1]].Rank].Name;
						entry.Title = string.Format("{0}解除({1})", name, current_time);
						entry.Content = string.Format("{1}に，{0}が解除されました．", name, current_long_time);
						if (current.Rank > 0)
						{
							entry.Content += string.Format(" 現在，{0}が発令されています．", levels[current.Rank].Name);
						}

						feed.Entries.Add(entry);
					}
				}

				XmlWriterSettings settings = new XmlWriterSettings
				{
					Indent = true,
					NewLineHandling = NewLineHandling.Replace,
					NewLineChars = "\n"
				};

				using (XmlWriter writer = XmlWriter.Create(destination, settings))
				{
					feed.OutputDocument().WriteTo(writer);
				}
				
			}


		}


		#region AlertLevelクラス
		public class AlertLevel
		{
			public int Rank { get; set; }
			public string Name { get; set; }

			/// <summary>
			/// 発令条件を満たしたときにtrueを返します．
			/// </summary>
			public Predicate<int[]> InBorder { get; set; }

			/// <summary>
			/// 解除条件を満たしたときにtrueを返します．
			/// </summary>
			public Predicate<int[]> OutBorder { get; set; }
		}
		#endregion

	}
}