using HtmlAgilityPack;
using MaasOne.Base;
using MaasOne.Finance;
using MaasOne.Finance.YahooFinance;
using StockScanner.Scans;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockScanner
{
	internal class Scanner
	{
		const string leveragedETFUrl = "http://shrib.com/Rq1kKczX";
		const string nonLeveragedETFUrl = "http://shrib.com/AWQxehLf";
		const string stocksUrl = "http://shrib.com/b77TPnkO";

		const string predefinedStocksDownList = "http://stockcharts.com/def/servlet/SC.scan?s=TSAL[t.t_eq_s]![as0,20,tv_gt_40000]![tc1_ge_ac1]![tc0_lt_ac0]&report=predefall";
		const string predifinedStockDowntrendADX = "http://stockcharts.com/def/servlet/SC.scan?s=TSAL[t.t_eq_s]![as0,20,tv_gt_40000]![bm0,14_gt_20]![bm1,14_le_20]![bm2,14_le_20]![bn0,14_lt_bo0,14]&report=predefall";

		public string storeUrl { get; set; }
		const string _ETFList = "http://masterdata.com/HelpFiles/ETF_List_Downloads/AllTypes.csv";
		private readonly List<string> leverageIdentificationWords = new List<string>() { "ENHANCED", "EXTENDED", "LEVERAGED", "2X", "3X", "ULTRA" };
		
		public List<string> LeveragedETFs { get; set; }
		public List<string> NonLeveragedETFs { get; set; }

		public delegate List<string> GetInputQuotesListHandler();
		//public List<QuoteFiveDay> quoteFiveDayList { get; set; }

		public Scanner()
		{
			
		}

		public Scanner(int index)
		{
			// TODO: Complete member initialization
			
			ConnectToYahoo(index);
		}

		public Scanner(Scan scanItem)
		{
			
		}

		public static string GetCSV(string url)
		{
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			StreamReader sr = new StreamReader(resp.GetResponseStream());
			string results = sr.ReadToEnd();
			sr.Close();
			return results;
		}


		private void CreateETFLists(List<string> list)
		{

			LeveragedETFs = new List<string>();
			NonLeveragedETFs = new List<string>();

			for (int i = 0; i < list.Count; i += 3)
			{
				if ((i + 1) > list.Count - 1)
					return;

				if (leverageIdentificationWords.Any((list[i]).ToUpper().Contains))
				{
					LeveragedETFs.Add(list[i + 1]);
				}
				else
				{
					NonLeveragedETFs.Add(list[i + 1]);
				}
			}
		}


		public static List<string> SplitCSV()
		{
			List<string> splitted = new List<string>();
			string fileList = GetCSV(_ETFList);
			string[] tempStr;
			tempStr = fileList.Split(',');
			foreach (string item in tempStr)
			{
				if (!string.IsNullOrWhiteSpace(item))
				{
					splitted.Add(item);
				}
			}
			splitted.RemoveRange(0, 3);
			return splitted;			
		}

		public static List<List<string>> SplitList(List<string> quotesList, int nSize = 5)
		{
			List<List<string>> list = new List<List<string>>();

			for (int i = 0; i < quotesList.Count; i += nSize)
			{
				list.Add(quotesList.GetRange(i, Math.Min(nSize, quotesList.Count - i)));
			}
			return list;
		}

		private void ConnectToYahoo(int index)
		{

			//get input list
			if (index == 2)
			{
				
				var stocks1 = GetStocksList(predifinedStockDowntrendADX);
				var stocks2 = GetStocksList(predefinedStocksDownList);
				var mergedList = stocks1.Union(stocks2).ToList();
				storeUrl = stocksUrl;
				Export(mergedList.Distinct().ToList());
				return;
			}

			List<string> inputList = null;

			CreateETFLists(SplitCSV());
			if (index == 0)
			{
				inputList = LeveragedETFs.Distinct().ToList();
				storeUrl = leveragedETFUrl;
			}
			if (index == 1)
			{
				inputList = NonLeveragedETFs.Distinct().ToList();
				storeUrl = nonLeveragedETFUrl;
			}
			
			//optional perform  scan
			List<QuoteFiveDay> quoteFiveDayList = new List<QuoteFiveDay>();
			List<List<string>> etfListList = SplitList(inputList);
			//etfListList.RemoveRange(0, etfListList.Count - 1);
			for (int i = 0; i < etfListList.Count; i++)
			{
				DownLoadLastPrice(etfListList[i], ref quoteFiveDayList);
				DownloadHistPriceAndCalculateAverage(etfListList[i], ref quoteFiveDayList, (index==0));
			}
			if (index == 0)
			{
				quoteFiveDayList = quoteFiveDayList.OrderByDescending(x => x.LastFourDaysAvgPctDiff).ToList();
				quoteFiveDayList = quoteFiveDayList.Where(x => x.LastFourDaysAvgPctDiff != double.NegativeInfinity).Take(100).ToList();
			}
			else
			{
				quoteFiveDayList = quoteFiveDayList.OrderBy(x => x.LastFourDaysAvgPctDiff).ToList();
				quoteFiveDayList = quoteFiveDayList.Where(x => x.LastFourDaysAvgPctDiff != double.NegativeInfinity).Take(100).ToList();
			}

			//export same for all
			Export(quoteFiveDayList);
		}

		private void Export(List<string> stockList)
		{
			string joinedList = string.Join(",", stockList.Select(x => x).ToArray());
			Export(joinedList);	
		}

		private List<string> GetStocksList(string list)
		{
			List<string> stockList = new List<string>();
			var hw = new HtmlWeb();
			var htmlDoc = hw.Load(list);			
			foreach (HtmlNode row in htmlDoc.DocumentNode.SelectNodes("//table[@id='scc-scans-resultstable']//tr").Skip(1))
			{				
				var open = Convert.ToDouble(row.ChildNodes[13].InnerText);
				var volume = Convert.ToInt32(row.ChildNodes[21].InnerText);
				if (open > 4 && volume > 70000)
					stockList.Add(row.ChildNodes[3].InnerText);				
			}
			return stockList;
		}

		private void Export(List<QuoteFiveDay> quoteFiveDayList)
		{			
			string joinedList = string.Join(",", quoteFiveDayList.Select(x => x.Id).ToArray());
			Export(joinedList);	
		}

		private void Export(string joinedList)
		{
			WebBrowser webBrowser = new WebBrowser();
			webBrowser.Url = new Uri(storeUrl);
			webBrowser.DocumentCompleted += (sender, eventArgs) =>
			{
				webBrowser.Document.GetElementById("igob").SetAttribute("Value", joinedList);
				var element = webBrowser.Document.GetElementById("spaeicherknopf");
				element.InvokeMember("click");
			};
		}

		private static void DownloadHistPriceAndCalculateAverage(List<string> ETFList, ref List<QuoteFiveDay> quoteFiveDayList, bool calcLows)
		{

			string[] quotesArray = ETFList.ToArray();
			System.DateTime fromDate = System.DateTime.Today.AddDays(-5);
			System.DateTime toDate = System.DateTime.Today.AddDays(-1);
			MaasOne.Finance.YahooFinance.HistQuotesInterval interval = MaasOne.Finance.YahooFinance.HistQuotesInterval.Daily;

			//Download
			MaasOne.Finance.YahooFinance.HistQuotesDownload dl2 = new HistQuotesDownload();
			Response<HistQuotesResult> hqResp = dl2.Download(quotesArray, fromDate, toDate, interval);

			if (hqResp.Connection.State == MaasOne.Base.ConnectionState.Success)
			{

				HistQuotesResult result = hqResp.Result;
				HistQuotesDataChain[] chains = result.Chains;

				foreach (HistQuotesDataChain chain in chains)
				{
					var quoteFiveDayItem = quoteFiveDayList.FirstOrDefault(i => i.Id == chain.ID);
					if (calcLows)
						CalculateAvgFromLows(chain, quoteFiveDayItem);
					else
						CalculateAvgFromHighs(chain, quoteFiveDayItem);
				}


			}
		}

		private static void CalculateAvgFromLows(HistQuotesDataChain chain, QuoteFiveDay quoteFiveDayItem)
		{
			var lows = chain.Select(x => x.Low).ToList();
			//lows.Remove(lows.OrderByDescending(c => c).First());
			quoteFiveDayItem.LowsAverage = lows.Average(x => x);
			quoteFiveDayItem.LastFourDaysAvgPctDiff = (quoteFiveDayItem.Last - lows.Average(x => x)) / quoteFiveDayItem.Last;
		}

		private static void CalculateAvgFromHighs(HistQuotesDataChain chain, QuoteFiveDay quoteFiveDayItem)
		{
			var lows = chain.Select(x => x.High).ToList();
			//lows.Remove(lows.OrderByDescending(c => c).First());
			quoteFiveDayItem.LowsAverage = lows.Average(x => x);
			quoteFiveDayItem.LastFourDaysAvgPctDiff = (quoteFiveDayItem.Last - lows.Average(x => x)) / quoteFiveDayItem.Last;
		}

		private static void DownLoadLastPrice(List<string> ETFList, ref List<QuoteFiveDay> quoteFiveDayList)
		{
			string[] quotesArray = ETFList.ToArray();

			IEnumerable<MaasOne.Finance.YahooFinance.QuoteProperty> properties = new MaasOne.Finance.YahooFinance.QuoteProperty[] {
						MaasOne.Finance.YahooFinance.QuoteProperty.Symbol,
						MaasOne.Finance.YahooFinance.QuoteProperty.LastTradePriceOnly
			  };
			//Download
			MaasOne.Finance.YahooFinance.QuotesDownload dl = new MaasOne.Finance.YahooFinance.QuotesDownload();
			MaasOne.Base.Response<MaasOne.Finance.YahooFinance.QuotesResult> resp = dl.Download(quotesArray, properties);
			//Response/Result
			if (resp.Connection.State == MaasOne.Base.ConnectionState.Success)
			{
				foreach (MaasOne.Finance.YahooFinance.QuotesData qd in resp.Result.Items)
				{
					quoteFiveDayList.Add(new QuoteFiveDay() { Id = qd.ID, Last = qd.LastTradePriceOnly });
				}
			}
		}		
	}
}




	
