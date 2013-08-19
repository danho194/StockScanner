using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockScanner.Scans
{
	public class SourceModifyingProcessingScan : ProcessingScan 
	{
		#region Import
		public override List<string> Import()
		{
			StatusMessage = "Importing from " + SourceUrl;
			var list = GetStocksList();
			StatusMessage = "Importing " + list.Count.ToString() + " data items";
			//return string.Join(",", list.Select(x => x).ToList());
			//exportString = string.Join(",", list.Select(x => x).ToArray());
			return list;
		}

		private List<string> GetStocksList()
		{
			try
			{
				List<string> stockList = new List<string>();
				var hw = new HtmlWeb();
				var htmlDoc = hw.Load(SourceUrl);
				foreach (HtmlNode row in htmlDoc.DocumentNode.SelectNodes("//table[@id='scc-scans-resultstable']//tr").Skip(1))
				{
					var open = Convert.ToDouble(row.ChildNodes[13].InnerText);
					var volume = Convert.ToInt32(row.ChildNodes[21].InnerText);
					if (open > 4 && volume > 10000)
						stockList.Add(row.ChildNodes[3].InnerText);

				}
				return stockList;
			}
			catch (Exception e)
			{

			}

			return null;
		}

		#endregion
	}
}
