using System.Windows.Input;


namespace HirosakiUniversity.Aldente.ElectricPowerBrother.ControlPanel
{
	// (0.2.0)ファイルを分離．
	#region Commandsクラス
	public static class Commands
	{
		/// <summary>
		/// データロガーのデータを取得します．
		/// </summary>
		public static RoutedCommand RetrieveLoggerDataCommand = new RoutedCommand();


		/// <summary>
		/// 1日分の出力を行います．パラメータで対象日を与えます．
		/// </summary>
		public static RoutedCommand OneDayOutputCommand = new RoutedCommand();

		/// <summary>
		/// 対象日までの一斉出力を行います．パラメータで対象日を与えます．
		/// </summary>
		public static RoutedCommand OutputAllCommand = new RoutedCommand();


		public static RoutedCommand CreateArchiveCommand = new RoutedCommand();
	}
	#endregion
}
