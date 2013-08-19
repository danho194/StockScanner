using StockScanner.Scans;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockScanner
{
	public class ScanController
	{
		//private Scan ActiveScan { get; set; }
		public void StartScan(List<Scan> scanList)
		{
			
			foreach (Scan scan in scanList)
			{
				//ActiveScan = scan;
				if (!scan.IsRunning)
					StartScanAsync(scan);				
			}			
		}

		[STAThread]
		private async void StartScanAsync(Scan scan)
		{
			
			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			Task.Factory.StartNew(() => 
			{

				DoScan(scan);
			})
           .ContinueWith(r => scan.Export(), scheduler);

			//Thread thread = new Thread(() => {
			//	DoScan(scan);
			//});
			//thread.SetApartmentState(ApartmentState.STA);
			//thread.Start();			
		}

		//private void Export()
		//{
		//	ActiveScan.Export();
		//	ActiveScan.SetStopTime();
		//}
	
		private void DoScan(Scan scan)
		{
				Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
				scan.SetStartTime();
				List<string> importList = scan.Import();
				if (scan is ProcessingScan)
				{
					((ProcessingScan)scan).Process(importList);					
				}				
				
		}
	}		
}

