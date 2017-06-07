using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.ControlPanel
{
	using Base;

	public interface IEnvironment
	{
		List<CachingPulseLogger> PulseLoggers { get; }

		DateTime GetNextDataTime();
		void Run(bool saving = false);

		event EventHandler<TweetEventArgs> Tweet;
}

}
