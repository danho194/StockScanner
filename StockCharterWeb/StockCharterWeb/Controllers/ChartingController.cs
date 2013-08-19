using HtmlAgilityPack;
using StockCharterWeb.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace StockCharterWeb.Controllers
{
	 
    public class ChartingController : Controller
    {
		 const string leveragedETFUrl = "http://shrib.com/Rq1kKczX";
		 const string nonLeveragedETFUrl = "http://shrib.com/AWQxehLf";
		 const string stocksUrl = "http://shrib.com/b77TPnkO";

		 public string storeUrl { get; set; }
        // GET: /Charting/

		 //
		 // GET: /Test/

		 public ActionResult Index()
		 {
			 return View();
		 }

		 public ActionResult Edit(int id = -1)
		 {
			 
			 return View();
		 }

		  [HttpPost]
		 public ActionResult Edit(int id, ChartingModel m)
		 {
			 var filepath = GetFilePathById(id.ToString());
			 System.IO.File.WriteAllText(Server.MapPath(filepath), TimeStampString(m.QuotesString));			
			 return null;
		 }

		  private string TimeStampString(string s)
		  {			   
			  var ces = "Central Europe Standard Time";
			  CultureInfo ci = new CultureInfo("sv-SE");
			  DateTime cesTime = GetDateTimeByTimeZoneInfo(ces);
			  string exportString = cesTime.ToString("T", ci) + "," + s;
			  var es = "Eastern Standard Time";
			  DateTime esTime = GetDateTimeByTimeZoneInfo(es);
			  exportString = esTime.ToString("G") + "," + exportString;
			  return exportString;
		  }

		  public static DateTime GetDateTimeByTimeZoneInfo(string ZoneId)
		  {
			  // Get time information by TimeZone Id
			  DateTime localtime = DateTime.Now;
			  TimeZoneInfo timeZoneInfo = System.TimeZoneInfo.FindSystemTimeZoneById(ZoneId);
			  DateTime dataTimeByZoneId = System.TimeZoneInfo.ConvertTime(localtime, System.TimeZoneInfo.Local, timeZoneInfo);
			  return dataTimeByZoneId;
		  }

		  private string GetFilePathById(string id)
		  {
			  if (id == "1")
				  return "~/App_Data/shortleveraged.txt";
			  if (id == "2")
				  return "~/App_Data/longleveraged.txt";
			  if (id == "3")
				  return "~/App_Data/nonleveraged.txt";
			  if (id == "4")
				  return "~/App_Data/bolingercrash.txt";
			  if (id == "5")
				  return "~/App_Data/adxdowntrend.txt";
			  return "~/App_Data/shortleveraged.txt";
		  }

		  private string GetTitleById(string id)
		  {
			  if (id == "1")
				  return "Short Leveraged ETFs";
			  if (id == "2")
				  return "Long Leveraged ETFs";
			  if (id == "3")
				  return "Non Leveraged ETFs";
			  if (id == "4")
				  return "Bolinger Crash Stocks";
			  if (id == "5")
				  return "ADX Downtrend Stocks";
			  return "Access by id in querystring";
		  }

		 public ActionResult Charting()
		 {
			 string id = Request.QueryString["t"];
			 storeUrl = leveragedETFUrl;
			 ViewBag.h2 = "Leveraged ETFs";
			 string fileContents = string.Empty;			 
			ViewBag.h2 = GetTitleById(id);
			fileContents = System.IO.File.ReadAllText(Server.MapPath(GetFilePathById(id)));
			 ViewBag.Message = "Page for testing Charts";			 
			 ChartingModel model = new ChartingModel();
			 model.quotes = SplitCommaString(fileContents.ToString());
			 model.LastUpdate = model.quotes[0];
			 model.LastUpdate2 = model.quotes[1];
			 model.quotes.RemoveRange(0, 2); 
			 return View(model);
		 }

		 private List<string> SplitCommaString(string quotesString)
		 {
			 List<string> splitted = new List<string>();
			 var tempStr = quotesString.Split(',');
			 foreach (string item in tempStr)
			 {
				 if (!string.IsNullOrWhiteSpace(item))
				 {
					 splitted.Add(item);
				 }
			 }
			 return splitted;
		 }

		 
    }
}
