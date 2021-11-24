using System;
using System.Text.RegularExpressions;
using EnhancedStringEvaluate;


namespace TestEvaluation.ProcessEvaluate
{
	/// <summary>
	/// Process {CurrentTime::format}
	///		or {CurrentTime::format::+/-type}
	/// 
	/// Possible format substrings:
	/// 	yyyy	-	4 digit year
	/// 	yy  	-	2 digit year
	/// 	MM  	-	2 digit month
	/// 	M   	-	1/2 digit month
	/// 	dd  	-	2 digit day of month
	/// 	d   	-	1/2 digit day of month
	/// 	hh  	-	2 digit hour
	/// 	h   	-	1/2 digit hour (12 hour clock)
	/// 	HH  	-	2 digit hour (24 hour clock)
	/// 	H   	-	1/2 digit hour (24 hour clock)
	/// 	mm  	-	2 digit minutes count
	/// 	m   	-	1/2 digit minutes count
	/// 	ss  	-	2 digit seconds count
	/// 	s   	-	1/2 digit seconds count
	/// 	fff 	-	3 digit milliseconds count in a second
	/// 	ff  	-	2 digit hundredth of a second
	/// 	f   	-	1 digit tenth of a second
	/// 	tt  	-	AM/PM
	/// 	t   	-	A/P
	/// 
	/// Second entry is optional and consits of
	///		optional + or - followed by a type
	///	So yesterday at the same time is: {CurrentTime::yyyymmdd::-1d}
	///	Last week (different format) will be: {CurrentTime::mm/dd/yyyy::-1w}
	///	
	/// type is:
	///		y - year
	///		M - Month
	///		w - week
	///		d - day
	///		h - hour
	///		m - minute
	///		s - second
	/// </summary>
	public sealed class ProcessCurrentTime : IProcessEvaluate
	{
		private readonly Regex _reTime;

		public ProcessCurrentTime() : this(DelimitersAndSeparator.DefaultDelimitedString) {}
		public ProcessCurrentTime(IDelimitersAndSeparator delim)
		{
			// Allow a single colon (":") but not a double colon
			//string pattSteady = @"(?<Format>(:?(([^{}:])+(:[^{}:]+)?))+)";
			string pattSteady = string.Format(@"(?<Format>(:?(([^{0}{1}:])+(:[^{0}{1}:]+)?))+)",
				delim.OpenDelimEquivalent, delim.CloseDelimEquivalent);

			// A double colon may suffix the expression but it is mandatory if a date arithmetic is to take place.
			string pattSuffix = @"(::\s*((?<direction>[-+]?)\s*(?<count>\d+)\s*(?<period>[yMwdhms]))?)?\s*";

			// Putting it all together
			string pattern = string.Format(@"{0}\s*CurrentTime\s*::{2}{3}{1}",
				delim.OpenDelimEquivalent, delim.CloseDelimEquivalent, pattSteady, pattSuffix);
			_reTime = new Regex(pattern, RegexOptions.Singleline);
		}

		#region IProcessEvaluate Members

		public void Evaluate(object src, EnhancedStringEventArgs ea)
		{
			// Initialize return code
			ea.IsHandled = false;

			string text = ea.EhancedPairElem.Value;
			if (string.IsNullOrWhiteSpace(text)) return;

			bool rc = _reTime.IsMatch(text);
			if (!rc) return;

			string replacement = _reTime.Replace(text, TimeReplace);
			if (replacement == text) return;

			// Announce that expression was successfully handled
			ea.IsHandled = true;

			// Keep new value
			ea.EhancedPairElem.Value = replacement;
			return;
		}

		#endregion

		private string TimeReplace(Match m)
		{
			DateTime now = DateTime.Now;

			// Perform date arithmetic first
			bool future = IsFutureDirection(m);
			DateTime modifiedTm = GetModifiedTime(m, now, future);

			string txt = m.Groups["Format"].Value;
			txt = txt.Replace("yyyy", modifiedTm.ToString("yyyy"));
			txt = txt.Replace("yy", modifiedTm.ToString("yy"));
			txt = txt.Replace("MM", modifiedTm.ToString("MM"));
			txt = txt.Replace("M", modifiedTm.ToString("%M"));
			txt = txt.Replace("dd", modifiedTm.ToString("dd"));
			txt = txt.Replace("d", modifiedTm.ToString("%d"));
			txt = txt.Replace("hh", modifiedTm.ToString("hh"));		// 2 digits hour (12 hour clock)
			txt = txt.Replace("h", modifiedTm.ToString("%h"));		// 1/2 digits hour (12 hour clock)
			txt = txt.Replace("HH", modifiedTm.ToString("HH"));		// 2 digits hour (24 hour clock)
			txt = txt.Replace("H", modifiedTm.ToString("%H"));		// 1/2 digits hour (24 hour clock)
			txt = txt.Replace("mm", modifiedTm.ToString("mm"));
			txt = txt.Replace("m", modifiedTm.ToString("%m"));
			txt = txt.Replace("ss", modifiedTm.ToString("ss"));
			txt = txt.Replace("s", modifiedTm.ToString("%s"));
			txt = txt.Replace("fff", modifiedTm.ToString("fff"));
			txt = txt.Replace("ff", modifiedTm.ToString("ff"));
			txt = txt.Replace("f", modifiedTm.ToString("%f"));
			txt = txt.Replace("tt", modifiedTm.ToString("tt"));
			txt = txt.Replace("t", modifiedTm.ToString("%t"));

			return txt;
		}

		private static bool IsFutureDirection(Match m)
		{
			bool future = true;
			if (m.Groups["direction"].Success)
			{
				string direction = m.Groups["direction"].Value;
				future = (direction == "+");
			}
			return future;
		}

		private static DateTime GetModifiedTime(Match m, DateTime now, bool futureDirection)
		{
			DateTime modifiedTm = now;
			int count;
			bool rc = int.TryParse(m.Groups["count"].Value, out count);
			if (rc)
			{
				char period = m.Groups["period"].Value[0];

				TimeSpan ts; int year; int month;
				switch (period)
				{
					case 'y':
						year = now.Year;
						if (futureDirection) year += count; else year -= count;
						modifiedTm = new DateTime(year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond);
						break;

					case 'M':
						month = now.Month;
						if (futureDirection) month += count; else month -= count;
						modifiedTm = new DateTime(now.Year, month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond);
						break;

					case 'w':
						ts = new TimeSpan(7 * count, 0, 0, 0);
						if (futureDirection) modifiedTm = now + ts; else modifiedTm = now - ts;
						break;

					case 'd':
						ts = new TimeSpan(count, 0, 0, 0);
						if (futureDirection) modifiedTm = now + ts; else modifiedTm = now - ts;
						break;

					case 'h':
						ts = new TimeSpan(count, 0, 0);
						if (futureDirection) modifiedTm = now + ts; else modifiedTm = now - ts;
						break;

					case 'm':
						ts = new TimeSpan(0, count, 0);
						if (futureDirection) modifiedTm = now + ts; else modifiedTm = now - ts;
						break;

					case 's':
						ts = new TimeSpan(0, 0, count);
						if (futureDirection) modifiedTm = now + ts; else modifiedTm = now - ts;
						break;
				}
			}

			return modifiedTm;
		}
	}
}
