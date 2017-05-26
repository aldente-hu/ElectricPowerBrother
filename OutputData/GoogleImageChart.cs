using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Data;
	using Helpers;

	// (1.3.5)
	#region GoogleImageChartクラス
	public class GoogleImageChart : ConsumptionData, IPlugin
	{
		public GoogleImageChart(string databaseFile) : base(databaseFile) { }

		#region *グラフを出力(DrawChart)
		public void DrawChart(DateTime dataTime)
		{
			var client = new System.Net.WebClient();
			var query = new Dictionary<string, string>();
			query["cht"] = "lc";			// LineChart
			query["chs"] = string.Format("{0}x{1}", this.ChartWidth, this.ChartHeight);	// 出力サイズ(横x縦)
			if (DataCh.HasValue)
			{
				query["chd"] = "t:" + string.Join(",", GetDataArray(dataTime, DataCh.Value));		// データ
			}
			else
			{
				query["chd"] = "t:" + string.Join(",", GetDataArray(dataTime));		// データ
			}
			query["chco"] = this.ChartLineColor;	// 線の色
			query["chxt"] = "x,y,y";	// 表示する軸(2つめのyは，単位を表示するためだけに使っている．)
			query["chxp"] = "2,100";	// 軸ラベルの位置(縦軸のMAXの位置に，縦軸の単位を表示させる．)

			var x_label = GetXLabel(dataTime);
			query["chxr"] = string.Format("1,{0},{1},{2}", ChartMinY, ChartMaxY, ChartLabelYInterval);	// y軸のラベル範囲
			query["chxl"] = "2:|[kW]|0:" + BuildXLabel(x_label);	// 軸のラベル
			query["chg"] = string.Format("8.333,{0},5,5,{1},0", Math.Truncate(100.0 * 1000 * ChartLabelYInterval / ChartRangeY) / 1000, x_label.Keys.Min() / 2.16);	// GridLine．格子状のライン．xのステップ，yのステップ,破線の長さ,破線の間隔,xのオフセット,yのオフセット．

			query["chxs"] = "0,000000,16|1,000000,16|2,000000,16,1";	// 軸ラベルのスタイル．インデックス,文字色,フォントサイズ,アラインメント．
			if (!string.IsNullOrEmpty(this.ChartCaption))
			{
				query["chtt"] = this.ChartCaption;	// タイトル
			}
			//query["chf"] = "c,ls,90,....";	// 背景のストライプ(使わない？)


			var uri = new Uri(string.Format("http://chart.apis.google.com/chart?{0}",
						string.Join("&", query.Select((pair) => string.Format("{0}={1}", pair.Key, pair.Value)).ToArray())
			));

			Console.WriteLine(uri);
			using (Stream stream = new FileStream(Destination, FileMode.Create))
			using (BinaryWriter writer = new BinaryWriter(stream))
			{
				writer.Write(client.DownloadData(uri));
			}
			

		}
		#endregion

		#region プロパティ

		/// <summary>
		/// グラフの出力先を取得／設定します．
		/// </summary>
		public string Destination { get; set; }

		public int ChartWidth { get; set; }
		public int ChartHeight { get; set; }
		public string ChartCaption { get; set; }

		/// <summary>
		/// グラフのy目盛りの最大値です．
		/// </summary>
		public int ChartMaxY { get; set; }
		/// <summary>
		/// グラフのy目盛りの最小値です．
		/// </summary>
		public int ChartMinY { get; set; }

		public int ChartRangeY { get
			{ return ChartMaxY - ChartMinY; }
		}

		/// <summary>
		/// Y軸の目盛りの間隔(目盛りでの単位)を取得／設定します．
		/// ChartRangeYの約数にするのがいいでしょう．
		/// </summary>
		public int ChartLabelYInterval { get; set; }

		/// <summary>
		/// Y軸のデータから差し引くオフセットを取得／設定します．
		/// もともとのデータにOffsetを作用させてからMagnificationをかけます．
		/// </summary>
		public double ChartOffsetY { get; set; }

		/// <summary>
		/// Y軸のデータにかける倍率を取得／設定します．
		/// もともとのデータにOffsetを作用させてからMagnificationをかけます．
		/// </summary>
		public double ChartMagnificationY { get; set; }

		/// <summary>
		/// いわゆる16進6桁で，線の色を取得／設定します．
		/// </summary>
		public string ChartLineColor { get; set; }

		/// <summary>
		/// データのチャンネルを取得／設定します．
		/// nullであれば，ch1とch2の合計を使います．
		/// </summary>
		public int? DataCh { get; set; }

		#endregion

		ICollection<int> GetDataArray(DateTime latestTime)
		{
			// 100分率データを返す．
			return this.GetDetailConsumptions(latestTime.AddHours(-36), latestTime).OrderBy(pair => pair.Key).Select(pair => ConvertYData(pair.Value)).ToArray();
		}

		ICollection<int> GetDataArray(DateTime latestTime, int ch)
		{
			return this.GetParticularConsumptions(latestTime.AddHours(-36), latestTime).OrderBy(pair => pair.Key).Select(pair => ConvertYData(pair.Value[ch])).ToArray();
		}

		int ConvertYData(int original)
		{
			return System.Convert.ToInt32(Math.Truncate(((original - ChartOffsetY) * ChartMagnificationY)));
		}

		#region *[static]X軸ラベルのデータを取得(GetXLabel)
		public static IDictionary<int, string> GetXLabel(DateTime latestTime)
		{
			// キーがデータ点の位置(先頭が0)，値がその位置に対応するキャプション．
			// {5 => "15:00", 23 => "18:00", 41 => "21:00", ...}
			// キーは0から216の整数値をとる(36時間，10分間隔で決め打ち)．
			var labels = new Dictionary<int, string>();
			labels.Add(216, latestTime.ToString("HH:mm"));

			// 3時間で割った余りを10分で割る．
			int n = 216 - (latestTime.Hour % 3) * 6 - latestTime.Minute / 10;  // (199..216)
			while (n >= 0)
			{
				if (n < 204)
				{
					labels.Add(n, latestTime.AddMinutes((n - 216) * 10).ToString("HH:mm"));
				}
				n -= 18;
			}
			return labels;
		}
		#endregion

		#region *[static]設定コマンド用のX軸ラベルを出力(BuildXLabel)
		public static string BuildXLabel(IDictionary<int, string> labelData)
		{
			// "||||||15:00||||||||||||||||||18:00||||||||||||||||||21:00||||(中略)|||02:10"のような感じの文字列になる．
			// データが217個なので，"|"が217本．
			StringBuilder label_builder = new StringBuilder();
			for (int i = 0; i < 217; i++)
			{
				label_builder.Append("|");
				if (labelData.Keys.Contains(i))
				{
					label_builder.Append(labelData[i]);
				}
			}
			return label_builder.ToString();
		}
		#endregion


		#region *設定を行う(Configure)
		public void Configure(System.Xml.Linq.XElement config)
		{
			// <Config Destination=""
			//
			//
			//
			//
			foreach (var attribute in config.Attributes())
			{
				switch(attribute.Name.LocalName)
				{
					case "Destination":
						this.Destination = attribute.Value;
						break;
					case "ChartWidth":
						this.ChartWidth = (int)attribute;
						break;
					case "ChartHeight":
						this.ChartHeight = (int)attribute;
						break;
					case "ChartCaption":
						this.ChartCaption = attribute.Value;
						break;
					case "ChartMaxY":
						this.ChartMaxY = (int)attribute;
						break;
					case "ChartMinY":
						this.ChartMinY = (int)attribute;
						break;
					case "IntervalY":
						this.ChartLabelYInterval = (int)attribute;
						break;
					case "OffsetY":
						this.ChartOffsetY = (double)attribute;
						break;
					case "MagnificationY":
						this.ChartMagnificationY = (double)attribute;
						break;
					case "LineColor":
						this.ChartLineColor = attribute.Value;
						break;
					case "Ch":
						this.DataCh = (int?)attribute;
						break;

				}
			}
			this.UpdateAction = (time) => { DrawChart(time); };
		}
		#endregion

	}
	#endregion

}
