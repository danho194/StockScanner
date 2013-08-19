using System;
namespace StockScanner.Scans
{
	interface IImporter
	{
		System.Collections.Generic.List<string> Import();
	}
}
