using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.PulseLoggers
{
	using RetrieveData;

	namespace Hioki
	{

		// WebException(タイムアウト時を想定)を拾いたいが，ライブラリで発生した例外を呼び出し元で拾うにはどうします？
		// →呼び出し元でcatchすれば？
		// →すると，ライブラリ側で処理を続行できないのでは？
		// →そうだけど...例外の性質に依るよね．タイムアウトならそれでも問題ないと思うし．

		public class Logger8420 : StoragingPulseLogger
		{
			public IPAddress Address { get; set; }

			#region StoragingPulseLogger実装

			// (0.0.5)とりあえず正常系のみ実装．
			public override IEnumerable<TimeSeriesDataInt> RetrieveCountsAfter(DateTime time, int max = -1)
			{
				WebClient client = new WebClient();
				DateTime now = DateTime.Now;

				var path = string.Format("MEMORY.HTM?%3ACOMPORT%3AWEBORGDATE%20%20TOP,={0}&,={1}&,={2}&,={3}&,={4}&,=0&,=0&%3B%3ADUM=SEP&%3ACOMPORT%3AWEBORGDATE%20%20BOT,={5}&,={6}&,={7}&,={8}&,={9}&,=0&,=0&%3B%3ADUM=SEP",
					time.Year % 100, time.Month,time.Day, time.Hour, time.Minute, now.Year % 100, now.Month, now.Day, now.Hour, now.Minute);

				client.OpenRead(string.Format("http://{0}/{1}", Address, path));

				using (StreamReader reader = new StreamReader(client.OpenRead(string.Format("http://{0}/MEMPART.TXT", Address))))
				{
					bool header = true;
					string date_string = string.Empty;
					string time_string = string.Empty;
					DateTime? origin = null;

					while (!reader.EndOfStream)
					{
						var line = reader.ReadLine();
						if (header)
						{
							// ヘッダモード

							var cells = line.Split(',');
							switch (cells[0].Trim())
							{
								case "\"DATE\"":
									// "MM-dd-yyyy"
									date_string = cells[1].Trim().Trim('"');
									break;
								case "\"TIME\"":
									// "HH:mm:ss"
									time_string = cells[1].Trim().Trim('"');
									break;
								case "\"DATA\"":
									header = false;
									// 開始時刻をどうにかする．
									if (!string.IsNullOrEmpty(date_string) && !string.IsNullOrEmpty(time_string))
									{
										origin = DateTime.Parse(string.Format("{0} {1}", date_string, time_string));
									}
									break;
							}
						}
						else if (origin.HasValue)
						{
							// データモード
							var data_reg = new Regex(@"\+\d\.\d+E\+\d\d");
							var mc = data_reg.Matches(line);
							if (mc.Count > 0)
							{
								// もし指定された時刻のデータがなければ、メモリハイロガーの保持する全データが返ってくる。
								// (そうなると、データが多いとタイムアウトになるのでよくない。)
								// 最初の行の時刻欄を用いてその判定を行う。
								var offset = double.Parse(mc[0].Value);
								if (offset == 0)
								{ 
									yield break;
								}
								
								DateTime data_time = origin.Value.AddSeconds(offset);
								if (data_time >= time)
								{
									Dictionary<int, int> data = new Dictionary<int, int>();
									for(int i=1; i<mc.Count;i++)
									{
										data[i-1] = Convert.ToInt32(double.Parse(mc[i].Value));
									}
									yield return new TimeSeriesDataInt { Data = data, Time = data_time };
								}
							}

						}
						else
						{
							break;
						}
					}

				}
			}

			#endregion


			// <Hioki.Logger8420 IpAddress="" />

			public override void Configure(XElement config)
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

				// Chに関する情報は親クラスで設定する．
				base.Configure(config);

			}

		}

	}

}

