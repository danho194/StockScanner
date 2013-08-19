using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockScanner
{
	public class QuoteFiveDay
	{
		public string Id { get; set; }
		public double Last { get; set; }
		public double LastFourDaysAvgPctDiff { get; set; }
		public double LowsAverage { get; set; }
	}
}
