using System;
using System.Text.RegularExpressions;
using EnhancedStringEvaluate;


namespace TestEvaluation.ProcessEvaluate
{
	/// <summary>
	/// Processes a construct like:
	/// 	{Date::yyyy.MM.dd}
	/// 	{Date::yy.M.d}
	/// 	Other date combinations involving:
	/// 		year: yyyy or yy
	/// 		Month: MM or M
	/// 		Day: dd or d
	/// 
	/// There is no good reason for the format to be provided in a case 
	/// sensitive manner.  If you wish to process the format in a case
	/// insensitive you may care to use regular expression replace in
	/// the DateReplace() method.
	/// 
	/// Notice that this processing has been supplanted by ProcessCurrentTime.
	/// Nevertheless, it is instructional to see how this is done in a simple
	/// case.
	/// </summary>
	public sealed class ProcessDate : IProcessEvaluate
	{
		private readonly Regex _reDate;
		private IDelimitersAndSeparator _delim;

		/// <summary> </summary>
		public ProcessDate() : this(DelimitersAndSeparator.DefaultDelimitedString) {}
		public ProcessDate(IDelimitersAndSeparator delim)
		{
			_delim = delim;
			string pattern = string.Format(@"({0})\s*Date\s*::(?<Format>([^{0}{1}])*?)({1})",
				_delim.OpenDelimEquivalent, _delim.CloseDelimEquivalent);
			RegexOptions reo = RegexOptions.Singleline | RegexOptions.IgnoreCase;
			_reDate = new Regex(pattern, reo);
		}

		#region IProcessEvaluate Members

		public void Evaluate(object src, EnhancedStringEventArgs ea)
		{
			// Initialize return code
			ea.IsHandled = false;

			string text = ea.EhancedPairElem.Value;
			if (string.IsNullOrWhiteSpace(text)) return;

			bool rc = _reDate.IsMatch(text);
			if (!rc) return;

			string replacement = _reDate.Replace(text, DateReplace);
			if (replacement == text) return;

			// Announce that expression was successfully handled
			ea.IsHandled = true;

			// Keep new value
			ea.EhancedPairElem.Value = replacement;
			return;
		}

		#endregion

		private string DateReplace(Match m)
		{
			DateTime today = DateTime.Today;
			string txt = m.Groups["Format"].Value;
			txt = txt.Replace("yyyy", today.ToString("yyyy"));
			txt = txt.Replace("yy", today.ToString("yy"));
			txt = txt.Replace("MM", today.ToString("MM"));	// 2 digit month
			txt = txt.Replace("M", today.ToString("%M"));	// 1 or 2 digit month
			txt = txt.Replace("dd", today.ToString("dd"));	// 2 digit day
			txt = txt.Replace("d", today.ToString("%d"));	// 1 or 2 digit day
			return txt;
		}
	}
}
