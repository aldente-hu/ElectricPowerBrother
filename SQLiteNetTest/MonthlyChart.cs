using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother
{
	using Helpers;

	public class MonthlyChart : PltFileGeneratorBase
	{
		// これらは基底クラスで用意されている！
		//public string RootDirectory { get; set; }
		//public string Destination { get; set; }

		public int ChartWidth { get; set; }
		public int ChartHeight { get; set; }
		public string ChartTitle { get; set; }
		public int MaximumY { get; set; }
		public int MinimumY { get; set; }

		public string BasePltFile { get; set; }

		public override void Generate(System.IO.StreamWriter writer)
		{
			throw new NotImplementedException();
		}

	}
}
