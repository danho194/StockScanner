using StockScanner.Scans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockScanner
{
	public class ScanFactory
	{
		const string shortleveragedETFUrl = "http://botcharting.somee.com/Charting/edit?id=1";
		const string longLeveragedETFUrl = "http://botcharting.somee.com/Charting/edit?id=2";
		const string nonLeveragedETFUrl = "http://botcharting.somee.com/Charting/edit?id=3";
		const string bolingerCrashStockUrl = "http://botcharting.somee.com/Charting/edit?id=4";
		const string adxDownTrendStocksUrl = "http://botcharting.somee.com/Charting/edit?id=5";
		const string bolingerCrashList = "http://stockcharts.com/def/servlet/SC.scan?s=TSAL[t.t_eq_s]![as0,20,tv_gt_40000]![tc1_ge_ac1]![tc0_lt_ac0]&report=predefall";
		const string adxDownTrendList = "http://stockcharts.com/def/servlet/SC.scan?s=TSAL[t.t_eq_s]![as0,20,tv_gt_40000]![bm0,14_gt_20]![bm1,14_le_20]![bm2,14_le_20]![bn0,14_lt_bo0,14]&report=predefall";
		
		
		public ScanFactory()
		{
			CSVImporter.CreateImportLists();
		}

		public List<Scan> CreateScans()
		{
			List<Scan> scanList = new List<Scan>();
			scanList.Add(ShortLeveragedETFScan());
			scanList.Add(LongLeveragedETFScan());
			scanList.Add(NonLeveragedETFScan());
			scanList.Add(BolingerCraschStockScan());
			scanList.Add(ADXDownTrendStockScan());

			return scanList;
		}

		public Scan LongLeveragedETFScan()
		{
			ProcessingScan scan = new ProcessingScan()
			{
				DisplayName = "Long leveraged ETF Scan",
				StorageUrl = longLeveragedETFUrl,
				InputList = CSVImporter.LevLongETFs,
				StopTime = DateTime.Now
			};
			scan.OrderHandler = new ProcessingScan.QuoteFiveDayListOrderHandler(scan.OrderByDescending);
			scan.CalculationHandler = new ProcessingScan.ScanCalculationHandler(scan.CalculateAvgFromLows);
			return scan;
		}

		public Scan ShortLeveragedETFScan()
		{
			ProcessingScan scan = new ProcessingScan()
			{
				DisplayName = "Short leveraged ETF Scan",
				StorageUrl = shortleveragedETFUrl,
				InputList = CSVImporter.LevShortETFs,
				StopTime = DateTime.Now
			};
			scan.OrderHandler = new ProcessingScan.QuoteFiveDayListOrderHandler(scan.OrderByDescending);
			scan.CalculationHandler = new ProcessingScan.ScanCalculationHandler(scan.CalculateAvgFromLows);
			return scan;
		}

		public Scan NonLeveragedETFScan()
		{
			ProcessingScan scan = new ProcessingScan()
			{
				DisplayName = "Non leveraged ETF Scan",
				StorageUrl = nonLeveragedETFUrl,
				InputList = CSVImporter.NonLevLongETFs,
				StopTime = DateTime.Now
				};
			scan.OrderHandler = new ProcessingScan.QuoteFiveDayListOrderHandler(scan.OrderBy);
			scan.CalculationHandler = new ProcessingScan.ScanCalculationHandler(scan.CalculateAvgFromHighs);
			return scan;
		}

		public Scan BolingerCraschStockScan()
		{
			SourceModifyingProcessingScan scan = new SourceModifyingProcessingScan()			
			{
				DisplayName = "Bolinger Crash Stock Scan",
				StorageUrl = bolingerCrashStockUrl,
				SourceUrl = bolingerCrashList,
				StopTime = DateTime.Now
			};
			scan.OrderHandler = new ProcessingScan.QuoteFiveDayListOrderHandler(scan.OrderBy);
			scan.CalculationHandler = new ProcessingScan.ScanCalculationHandler(scan.CalculateAvgFromHighs);
			return scan;
		}

		public Scan ADXDownTrendStockScan()
		{
			SourceModifyingProcessingScan scan = new SourceModifyingProcessingScan()
			{
				DisplayName = "ADX Downtrend Stock Scan",
				StorageUrl = adxDownTrendStocksUrl,
				SourceUrl = adxDownTrendList,
				StopTime = DateTime.Now
			};
			scan.OrderHandler = new ProcessingScan.QuoteFiveDayListOrderHandler(scan.OrderBy);
			scan.CalculationHandler = new ProcessingScan.ScanCalculationHandler(scan.CalculateAvgFromHighs);
			return scan;
		}
	}
}
