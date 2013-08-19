using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StockCharterWeb.Models
{
	public class ChartingModel
	{
		//const string storeUrl = "http://shrib.com/Rq1kKczX";
		public int id { get; set; }
		public List<string> quotes { get; set; }
		public string QuotesString { get; set; }
		public string LastUpdate { get; set; }
		public string LastUpdate2 { get; set; }
	}
}
