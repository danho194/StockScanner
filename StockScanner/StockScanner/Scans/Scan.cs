using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Forms;

namespace StockScanner.Scans
{
	public class Scan : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public string Name { get; set; }
		public string DisplayName { get; set; }
		public string Description { get; set;} 
		public string StorageUrl { get; set;}
		public string SourceUrl { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime StopTime { get; set; }
		//protected List<string> exportList;
		protected string exportString;

		private string statusMessage;
		public string StatusMessage
		{
			get
			{
				return statusMessage;
			}
			set
			{
				statusMessage = value;
				OnPropertyChanged("StatusMessage");	
			}
		}


		private bool isRunning = false;
		public bool IsRunning
		{
			get
			{
				return isRunning;
			}
			set
			{
				isRunning = value;				
			}
		}

		private TimeSpan elapsedTime;
		public TimeSpan ElapsedTime
		{
			get
			{
				return elapsedTime;
			}
			set
			{
				elapsedTime = value;
				OnPropertyChanged("ElapsedTime");				
			}
		}

		internal void SetStartTime()
		{
			StopTime = DateTime.Now;
			StartTime = DateTime.Now;	
			IsRunning = true;
		}

		internal void SetStopTime()
		{
			StopTime = DateTime.Now;
			System.Threading.Thread.Sleep(1000);
			IsRunning = false;
		}


		

		protected void OnPropertyChanged(string name)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}


		public virtual List<string> Import()
		{
			return null;
		}
		//#region Import
		
		//public virtual List<string> Import()
		//{
		//	StatusMessage = "Importing from " + SourceUrl;
		//	var list = GetStocksList();
		//	StatusMessage = "Importing " + list.Count.ToString() + " data items";
		//	exportString = string.Join(",", list.Select(x => x).ToArray());
		//	return null;
		//}

		//private List<string> GetStocksList()
		//{
		//	try
		//	{
		//		List<string> stockList = new List<string>();
		//		var hw = new HtmlWeb();
		//		var htmlDoc = hw.Load(SourceUrl);
		//		foreach (HtmlNode row in htmlDoc.DocumentNode.SelectNodes("//table[@id='scc-scans-resultstable']//tr").Skip(1))
		//		{
		//			var open = Convert.ToDouble(row.ChildNodes[13].InnerText);
		//			var volume = Convert.ToInt32(row.ChildNodes[21].InnerText);
		//			if (open > 4 && volume > 10000)
		//				stockList.Add(row.ChildNodes[3].InnerText);
					
		//		}
		//		return stockList;
		//	}
		//	catch(Exception e)
		//	{

		//	}

		//	return null;
		//}

		//#endregion

		#region Export

		//public virtual void Export()
		//{
		//	//string joinedList = string.Join(",", exportList.Select(x => x).ToArray());
		//	Export();
			
		//}

		public void Export()
		{
			StatusMessage = "Exporting to: " + StorageUrl;
			WebBrowser webBrowser = new WebBrowser();
			webBrowser.Url = new Uri(StorageUrl);
			WebBrowserDocumentCompletedEventHandler handler = null;
			//webBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(OnDocumentCompletedWebBrowser);
			handler = (sender, eventArgs) =>
			{
				if (eventArgs.Url.Equals(webBrowser.Url))
				{
					webBrowser.Document.GetElementById("QuotesString").SetAttribute("Value", exportString);
					var element = webBrowser.Document.GetElementById("sb");
					element.InvokeMember("click");
					StatusMessage = "Scan finished";
					SetStopTime();
					webBrowser.DocumentCompleted -= handler;

				}
			};
			webBrowser.DocumentCompleted += handler;

		}

		//private string TimeStampExportString()
		//{			
		//	var ces = "Central Europe Standard Time";
		//	DateTime cesTime = GetDateTimeByTimeZoneInfo(ces);
		//	exportString = cesTime.ToString("G") + "," + exportString;
		//	var es = "Eastern Standard Time";
		//	DateTime esTime = GetDateTimeByTimeZoneInfo(es);
		//	exportString = esTime.ToString("G") + "," + exportString;
		//	return exportString;
		//}

		//public static DateTime GetDateTimeByTimeZoneInfo(string ZoneId)
		//{
		//	// Get time information by TimeZone Id
		//	DateTime localtime = DateTime.Now;
		//	TimeZoneInfo timeZoneInfo = System.TimeZoneInfo.FindSystemTimeZoneById(ZoneId);
		//	DateTime dataTimeByZoneId = System.TimeZoneInfo.ConvertTime(localtime, System.TimeZoneInfo.Local, timeZoneInfo);
		//	return dataTimeByZoneId;
		//}

		//private static void OnDocumentCompletedWebBrowser(object sender, WebBrowserDocumentCompletedEventArgs e)
		//{
		//	//WebBrowser webBrowser = (WebBrowser)sender;
		//	//if (e.Url.Equals(webBrowser.Url))
		//	//{
		//	//	webBrowser.Document.GetElementById("igob").SetAttribute("Value", exportString);
		//	//	var element = webBrowser.Document.GetElementById("spaeicherknopf");
		//	//	element.InvokeMember("click");
		//	//	StatusMessage = "Scan finished";
		//	//	webBrowser.DocumentCompleted -= new WebBrowserDocumentCompletedEventHandler(OnDocumentCompletedWebBrowser);
		//	//}			
		//}
		#endregion

		
	}
}
