using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace StockScanner
{
	public class CSVImporter
	{
		private static readonly List<string> leverageIdentificationWords = new List<string>() { "ENHANCED", "EXTENDED", "LEVERAGED", "2X", "3X", "ULTRA" };
		private static readonly List<string> shortIdentificationWords = new List<string>() { " SHORT ", " INVER ", " INVERSE ", " BEAR ", " INV ", "ULTRASHORT" };
		const string sourceUrl = "http://masterdata.com/HelpFiles/ETF_List_Downloads/AllTypes.csv";
		private static List<List<string>> leveragedETFs = new List<List<string>>();
		private static List<List<string>> nonLeveragedETFs = new List<List<string>>();
		
		public static List<string> LevShortETFs  { get; set; }
		public static List<string> LevLongETFs { get; set; }
		public static List<string> NonLevLongETFs { get; set; }
		

		internal static void CreateImportLists()
		{
			var importList = SplitCSV(sourceUrl);
			SplitOnLeverage(importList);

			List<string> levLongETFs = new List<string>();
			List<string> levShortETFs = new List<string>();
			List<string> nonLevLongETFs = new List<string>();
			List<string> dummy = new List<string>();

			SplitOnBull(leveragedETFs, out levLongETFs, out levShortETFs);
			SplitOnBull(nonLeveragedETFs, out nonLevLongETFs, out dummy);

			LevLongETFs = levLongETFs;
			LevShortETFs = levShortETFs;
			NonLevLongETFs = nonLevLongETFs;
		}

		private static void SplitOnBull(List<List<string>> inputList, out List<string> bullList, out List<string> bearList)
		{
			bullList = new List<string>();
			bearList = new List<string>();
			foreach (var item in inputList)
			{
				if (shortIdentificationWords.Any(item[1].ToUpper().Contains))
				{
					bearList.Add(item[0]);
				}
				else
				{
					bullList.Add(item[0]);
				}
			}
		}

		private static void SplitOnLeverage(List<string> list)
		{
			
			

			for (int i = 0; i < list.Count; i += 3)
			{
				if ((i + 1) > list.Count - 1)
					return;

				if (leverageIdentificationWords.Any((list[i]).ToUpper().Contains))
				{
					leveragedETFs.Add(new List<string>() { list[i + 1], list[i]});
				}
				else
				{
					nonLeveragedETFs.Add(new List<string>() { list[i + 1], list[i]});
				}
			}
		}

		private static List<string> SplitCSV(string url)
		{
			List<string> splitted = new List<string>();
			string fileList = GetCSV(url);
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

		private static string GetCSV(string url)
		{
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			StreamReader sr = new StreamReader(resp.GetResponseStream());
			string results = sr.ReadToEnd();
			sr.Close();
			return results;
		}


		
	}
}
