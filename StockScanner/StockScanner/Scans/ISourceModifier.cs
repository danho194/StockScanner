using System;
using System.Collections.Generic;
namespace StockScanner.Scans
{
	interface ISourceModifier
	{
		List<string> SourceModifierFilter { get; set; }
		List<string> SourceModifierFilterOut { get; set; }
		void Filter();
	}
}
