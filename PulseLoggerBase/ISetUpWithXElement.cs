namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Base
{
	// (0.0.1)
	#region ISetUpWithXElementインターフェイス
	public interface ISetUpWithXElement
	{
		/// <summary>
		/// XML要素から設定を行います．
		/// </summary>
		/// <param name="config">その機種に対応したXElement．</param>
		void SetUp(System.Xml.Linq.XElement config);
	}
	#endregion

}
