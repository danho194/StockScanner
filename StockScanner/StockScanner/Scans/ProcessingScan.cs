using MaasOne.Base;
using MaasOne.Finance;
using MaasOne.Finance.YahooFinance;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StockScanner.Scans
{
	public class ProcessingScan : Scan
	{
		
		public List<string> InputList { get; set; }
		

		public QuoteFiveDayListOrderHandler OrderHandler { get; set; }
		public ScanCalculationHandler CalculationHandler { get; set; }
	
		public delegate List<QuoteFiveDay> QuoteFiveDayListOrderHandler(List<QuoteFiveDay> quoteFiveDayList);
		public delegate void ScanCalculationHandler(HistQuotesDataChain chain, QuoteFiveDay quoteFiveDay);
		#region import

		public override List<string> Import()
		{
			return InputList;

		}

	
		
		#endregion

		#region Process
		public void Process(List<string> inputList)
		{
			List<QuoteFiveDay> quoteFiveDayList = new List<QuoteFiveDay>();
			List<List<string>> etfListList = SplitList(inputList);
			//etfListList.RemoveRange(0, etfListList.Count - 1);
			for (int i = 0; i < etfListList.Count; i++)
			{
				StatusMessage = "Processing block " + (i + 1) + " of " + (etfListList.Count);
				DownLoadLastPrice(etfListList[i], ref quoteFiveDayList);
				DownloadHistPriceAndCalculateAverage(etfListList[i], ref quoteFiveDayList);
			}
			quoteFiveDayList = OrderHandler(quoteFiveDayList);
			quoteFiveDayList = quoteFiveDayList.Where(x => x.LastFourDaysAvgPctDiff != double.NegativeInfinity).Take(100).ToList();
			string joinedList = string.Join(",", quoteFiveDayList.Select(x => x.Id).ToArray());
			exportString = joinedList;
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

		public List<QuoteFiveDay> OrderByDescending(List<QuoteFiveDay> quoteFiveDayList)
		{
			return quoteFiveDayList.OrderByDescending(x => x.LastFourDaysAvgPctDiff).ToList();
		}

		public List<QuoteFiveDay> OrderBy(List<QuoteFiveDay> quoteFiveDayList)
		{
			return quoteFiveDayList.OrderBy(x => x.LastFourDaysAvgPctDiff).ToList();
		}


		private void DownloadHistPriceAndCalculateAverage(List<string> ETFList, ref List<QuoteFiveDay> quoteFiveDayList)
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
					try
					{
						var quoteFiveDayItem = quoteFiveDayList.FirstOrDefault(i => i.Id == chain.ID);
						CalculationHandler(chain, quoteFiveDayItem);
					}
					catch (Exception e)
					{

					}

				}
			}
		}

		public void CalculateAvgFromLows(HistQuotesDataChain chain, QuoteFiveDay quoteFiveDayItem)
		{
			var lows = chain.Select(x => x.Low).ToList();
			//lows.Remove(lows.OrderByDescending(c => c).First());
			quoteFiveDayItem.LowsAverage = lows.Average(x => x);
			quoteFiveDayItem.LastFourDaysAvgPctDiff = (quoteFiveDayItem.Last - lows.Average(x => x)) / quoteFiveDayItem.Last;
		}

		public void CalculateAvgFromHighs(HistQuotesDataChain chain, QuoteFiveDay quoteFiveDayItem)
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
				int i = 0;
				foreach (MaasOne.Finance.YahooFinance.QuotesData qd in resp.Result.Items)
				{
					quoteFiveDayList.Add(new QuoteFiveDay() { Id = quotesArray[i], Last = qd.LastTradePriceOnly });
					i++;
				}
			}
		}
		#endregion

		
	}
}
