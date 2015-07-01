using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using RetrieveData;
	namespace PulseLoggers.Hioki
	{

		public class LR8400 : StoragingPulseLogger
		{

			public IPAddress Address { get; set; }
			public NetworkCredential Credential {
				get { return this._credential;}
			}
			NetworkCredential _credential = new NetworkCredential();


			// (1.0.1.1)GetDataViaFtpの結果を配列として保持してみる．
			// time，もしくはそれより後のデータを取得します．(timeのデータが欲しい！それ以降のデータもあれば欲しい！)
			public override IEnumerable<TimeSeriesDataInt> RetrieveCountsAfter(DateTime time, int max = -1)
			{
				int n = 0;

				if (DateTime.Now < time)
				{
					// timeになる前はデータを取得できるはずがないので，終了する．
					yield break;
				}


				// 最新データをHTTP経由で取得する．
				var new_data = GetLatestCounts().Result;
				if (new_data == null || new_data.Time < time)
				{
					// 前者を後者と同列に扱っていいものか？(後者は正常系，前者は異常系？)
					yield break;
				}

				while (new_data.Time > time)
				{
					// 取りこぼしたデータがある(はずな)ので，ftp経由で補完する．
					var old_data_series = GetDataViaFtp(time).ToArray();
					if (old_data_series.Count() == 0)
					{
						yield break;
					}

					foreach (var data in old_data_series)
					{
						yield return data;
					}
					var last_data = old_data_series.Max(d => d.Time);
					
					time = last_data.AddMinutes(10);

					// ★timeが0:00なら翌日のGetDataViaFtpにトライ！
					// ★timeがnew_data(あるいはlast_data)のTimeに等しければ無問題！

					// ★どっちでもないときが困ってしまう．
					// →気にしないで書き込んでしまう方針でいいのでは？
					// →データに抜けが生じるという問題点があるけど...
										
				}

				yield return new_data;
			}


			#region 最新データ取得関連

			// (0.1.0)とりあえずpublic．
			public async Task<TimeSeriesDataInt> GetLatestCounts()
			{
				using (WebClient client = new WebClient())
				{
					if (this.Credential != null)
					{
						client.Credentials = this.Credential;
					}
					var source = string.Format("http://{0}/REALDATA.HTM?%3ACOMPORT%3AWEBORGUNIT=Pulse_Logic_Alarm%3B", this.Address);

					//using (var reader = new StreamReader(client.OpenRead(source), Encoding.GetEncoding("Shift_JIS")))
					using (var reader = new StreamReader(await client.OpenReadTaskAsync(source), Encoding.GetEncoding("Shift_JIS")))
					{
						return ParseRealTimeCounts(reader).Result;
					}
				}
			}

			public async Task<TimeSeriesDataInt> ParseRealTimeCounts(StreamReader reader)
			{
				var time_reg = new Regex(@"<td colspan=3>\'(\d\d-\d\d-\d\d \d\d:\d\d:\d\d)<\/td>");
				var count_reg = new Regex(@"<td><nobr>PLS(\d)<\/nobr><\/td><td><nobr>\s*(\d+) c<\/nobr><\/td>");
				DateTime? time = null;
				Dictionary<int, int> data = new Dictionary<int, int>();

				while (!reader.EndOfStream)
				{
					//var line = reader.ReadLine();
					var line = await reader.ReadLineAsync();
					// 正規表現を駆使してデータを取得．

					if (!time.HasValue)
					{
						var m = time_reg.Match(line);
						if (m.Success)
						{
							time = DateTime.Parse(m.Groups[1].Value);
						}
					}

					if (time.HasValue)
					{
						var mc = count_reg.Matches(line);
						for (int i = 0; i < mc.Count; i++)
						{
							int ch = int.Parse(mc[i].Groups[1].Value);
							int count = int.Parse(mc[i].Groups[2].Value);
							data[ch-1] = count;	// もともとのチャンネル番号と，TimeSeriesDataで使うチャンネル番号の関係の定め方が危ない！
						}
					}
				}

				if (time.HasValue)
				{
					return new TimeSeriesDataInt { Data = data, Time = time.Value };
				}
				else
				{
					return null;
				}
			}

			#endregion

			#region FTPデータ取得関連

			// (0.0.4.0)とりあえず．
			public IEnumerable<TimeSeriesDataInt> GetDataViaFtp(DateTime time)
			{
				var directory = string.Format("ftp://{1}/CF/HIOKI_LR8400/DATA/{0}/", time.ToString("yy-MM-dd"), this.Address);
				Regex file_line = new Regex(@"(... \d+ \d\d:\d\d) (.+\.CSV)");

				var file_lines = ListDirectory(directory).ToArray();
				foreach (var line in file_lines)
				{
					var m = file_line.Match(line);
					// "Apr 15 23:59"みたいな文字列をDateTime.Parseするにはなかなかな困難を伴う．
					// それが面倒なので，タイムスタンプのチェックは割愛する．
					if (m.Success  /* && DateTime.Parse(m.Groups[1].Value) >= time */ )
					{
						string file_name = directory + m.Groups[2].Value;
						WebRequest req = FtpWebRequest.Create(file_name);

						// LISTコマンドはListDirectoryDetails, NLISTコマンドはListDirectory．
						req.Method = WebRequestMethods.Ftp.DownloadFile;
						req.Credentials = this.Credential;
						var response = req.GetResponse();
						using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("Shift_JIS")))
						{
							foreach (var data in GetDataFromFile(stream, time))
							{
								yield return data;
							}
						}

					}
				}

				
			}

			// 汎用できるメソッド？→static化できると思ったら，Credentialでthisを使っていた！
			IEnumerable<string> ListDirectory(string uri)
			{
				WebRequest req = FtpWebRequest.Create(uri);

				// LISTコマンドはListDirectoryDetails, NLISTコマンドはListDirectory．
				req.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
				req.Credentials = this.Credential;
				var response = req.GetResponse();
				using (var stream = new StreamReader(response.GetResponseStream()))
				{
					while (!stream.EndOfStream)
					{
						yield return stream.ReadLine();
					}
				}

			}

			// 非同期化はいったん断念．yield returnと相性が良くない？(いずれもメソッドの返り値の型に影響するし...)
			// というか，両者のカンケイを一度きちんと調べる必要があるカモね．
			/// <summary>
			/// time以降のデータ(timeも含む！)を返します．
			/// </summary>
			/// <param name="reader"></param>
			/// <param name="time"></param>
			/// <returns></returns>
			public IEnumerable<TimeSeriesDataInt> GetDataFromFile(StreamReader reader, DateTime time)
			{
				var origin_reg = new Regex(@"トリガ時刻"",""'(\d\d-\d\d-\d\d \d\d:\d\d:\d\d)");
				var data_reg = new Regex(@"(\d\.\d+E\+\d\d),");
				//DateTime? trigger_time = null;
				//double? offset = null;
				DateTime? origin = null;

				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();

					if (!origin.HasValue)
					//if (!offset.HasValue)
					{
						var m = origin_reg.Match(line);
						if (m.Success)
						{
							origin = DateTime.Parse(m.Groups[1].Value);
							//offset = (time - DateTime.Parse(m.Groups[1].Value)).TotalSeconds;
						}
					}
					else
					{
						// offset should has value.
						var mc = data_reg.Matches(line);
						if (mc.Count > 0)
						{
							var data_time = origin.Value.AddSeconds(double.Parse(mc[0].Groups[1].Value));
							if (data_time >= time)
							{
								Dictionary<int, int> data = new Dictionary<int, int>();
								for (int i = 1; i < mc.Count; i++)
								{
									data[i-1] = Convert.ToInt32(double.Parse(mc[i].Groups[1].Value));
								}

								// ↓をどうにかして返す．
								yield return new TimeSeriesDataInt{ Data = data, Time = data_time};
							}
						}

					}
					

				}

			}

			#endregion

			#region 設定関連

			public override void Configure(System.Xml.Linq.XElement config)
			{
				// config.Name.LocalNameをチェックしますか？
				try
				{
					this.Address = System.Net.IPAddress.Parse((string)config.Attribute("IpAddress"));
				}
				catch
				{
					this.Address = System.Net.IPAddress.Loopback;	// IPv4で決め打ちしていいんだっけ？
				}

				var name = (string)config.Attribute("UserName");
				var pass = (string)config.Attribute("Password");

				if (!string.IsNullOrEmpty(name))
				{
					this.Credential.UserName = name;
				}
				if (!string.IsNullOrEmpty(pass))
				{
					this.Credential.Password = pass;
				}

				// Chに関する情報は親クラスで設定する．
				base.Configure(config);
			}

			#endregion

		}

	}



}
