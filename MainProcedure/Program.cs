using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Linq;
using System.Reflection;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Helpers;

	namespace Main
	{

		class Program
		{
			//static IList<Ticker> Tickers = new List<Ticker>();


			static Properties.Settings MySettings
			{ get { return Properties.Settings.Default; } }

			static void Main(string[] args)
			{
				//IList<Ticker> Tickers = new List<Ticker>();
				IList<PluginTicker> PluginTickers = new List<PluginTicker>();


				using (System.Threading.Timer timer = new System.Threading.Timer((state) => { TimerTicked(null, EventArgs.Empty); }, null, 0, 1000))
				{


					// 何らかの設定ファイルを読み込む．
					using (XmlReader reader = XmlReader.Create(MySettings.ConfigFile))
					{
						XDocument doc = XDocument.Load(reader);
						var root = doc.Root;
						if (root.Name.LocalName == "ElectricPower")
						{
							// (0.2.2)static変数の設定を行う．
							foreach(var element in root.Element("Values").Elements())
							{
								foreach (var attribute in element.Attributes())
								{
									var assemblies = AppDomain.CurrentDomain.GetAssemblies();
									Type type = null;
									for (int i = 0; i < assemblies.Length; i++ )
									{
										type = assemblies[i].GetType("HirosakiUniversity.Aldente.ElectricPowerBrother." + element.Name.LocalName, false);
										if (type != null)
										{
											//asm = assemblies[i];
											break;
										}
									}
									type.GetProperty(attribute.Name.LocalName).SetValue(null, attribute.Value);
								}
							}


							int n = 1;

							foreach (var task in root.Element("Tasks").Elements())
							{
								// 名前からDLLを特定し，そこからtypeをgetしなければならない！
								var dll = (string)task.Attribute("Dll");
								var name = task.Name.LocalName;

								// ※exeかよ！
								var asm = Assembly.LoadFrom(string.Format("plugins/{0}.dll", string.IsNullOrEmpty(dll) ? name : dll));
								var type_info = asm.GetType("HirosakiUniversity.Aldente.ElectricPowerBrother." + name);

								foreach (var config in task.Elements("Config"))
								{
									// インスタンスを生成する．
									IPluginBase generator = Activator.CreateInstance(type_info, MySettings.DatabaseFile) as IPluginBase;

									if (generator == null)
									{
										// エラー．
									}
									else
									{
										// 設定は，XMLの要素をインスタンスに渡して，中で行う．
										generator.Configure(config);

										// ここでInvokeするワケではない．Invoke時の挙動を設定するのである．
										// ↑Configureでそれも含めて行ってしまえるよね？
										// generator.Invoke();

										// Tickerをどこに置く？
										// ここで生成して，staticに置いておく？

										var interval = (int?)config.Attribute("Interval");
										//AddTicker(new Ticker(generator.Update), interval.HasValue ? interval.Value : 300);
										var ticker = new PluginTicker(generator);
										ticker.Interval = interval ?? 300;
										ticker.Count = ticker.Interval - 3 * (n++);
										TimerTicked += ticker.OnTick;
										PluginTickers.Add(ticker);
									}

								}
							}

						}
						else
						{
							// ルート要素が違う！
						}

					}

					// その記述をもとに，オブジェクトを動的に生成する．
					// →Tickerに関連づける．

					// Tickerは失敗だったかなぁ？


					while (true)
					{
						var key = Console.ReadKey();
						if (key.KeyChar == 'c')
						{
							Console.WriteLine(PluginTickers.Count);
						}
						else if (key.KeyChar == 't')
						{
							foreach (var pt in PluginTickers)
							{
								Console.WriteLine(pt.Count);
							}

						}
						else
						{
							break;
						}
					}
				}
			}

			static event EventHandler TimerTicked = delegate { };
			/*
						static void AddTicker(Ticker ticker, int interval)
						{
							int n = Tickers.Count;
							ticker.StartTimer((3 * n + 2) * 1000, interval * 1000);
							Tickers.Add(ticker);
						}
			*/
		}

	}

	public class PluginTicker
	{
		public int Count { get; set; }
		public int Interval { get; set; }

		readonly IPluginBase _plugin;
		DateTime _latestDataTime;

		public PluginTicker(IPluginBase plugin)
		{
			this._plugin = plugin;
		}

		public void OnTick(object sender, EventArgs e)
		{
			++Count;
			if (Count >= Interval)
			{
				Count = 0;
				// 手抜き実装？
				if (_plugin is IUpdatingPlugin)
				{
					((IUpdatingPlugin)_plugin).Update();
				}
				else if (_plugin is IPlugin)
				{
					_latestDataTime = ((IPlugin)_plugin).Update(_latestDataTime);
				}
			}

		}

	}

}