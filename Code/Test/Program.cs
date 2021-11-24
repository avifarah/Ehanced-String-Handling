using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConcordPairEvaluate;

namespace Test
{
	class Program
	{
		private static ConcordPairEval _eval;

		static void Main(string[] args)
		{
			var elems = new Dictionary<string, ConcordPairElement>();
			elems.Add("Flat", new ConcordPairElement("Flat", @"testing"));
			elems.Add("Static flat", new ConcordPairElement("Static flat", @"Static evaluation of flat: {key::flat}"));
			elems.Add("Static date", new ConcordPairElement("Static date", @"Static evaluation of date: {date::yyyy.MM.dd}"));
			elems.Add("Dynamic current directory", new ConcordPairElement("Dynamic current directory", @"current dir: {CurrentDir::}"));
			elems.Add("temp", new ConcordPairElement("temp", @"SpecialDirectory\{key::flat}\{key::Stamp}"));
			elems.Add("Dynamic current path", new ConcordPairElement("Dynamic current path", @"{key::dynamic current directory}\{key::temp}"));
			elems.Add("Stamp", new ConcordPairElement("Stamp", @"{CurrentTime::yyyyMMddHHmmssfff}"));

			_eval = new ConcordPairEval(elems);
		}
	}
}
