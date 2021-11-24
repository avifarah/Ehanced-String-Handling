using System;
using System.Text.RegularExpressions;
using System.IO;
using EnhancedStringEvaluate;


namespace TestEvaluation.ProcessEvaluate
{
	/// <summary>
	/// Processes a directory config-varialbe -- CurrentDir, {CurrentDir::} or {CurrentDir::-n} construct.
	/// </summary>
	public sealed class ProcessCurrentDir : IProcessEvaluate
	{
		private readonly Regex _reCurrDir;

		public ProcessCurrentDir() : this(DelimitersAndSeparator.DefaultDelimitedString) { }
		public ProcessCurrentDir(IDelimitersAndSeparator delim)
		{
			//string pattern = @"({)\s*CurrentDir\s*::\s*(-\s*[0-9]+)?(})";
			string pattern = string.Format(@"({0})\s*CurrentDir\s*::\s*(?<prevCount>-\s*[0-9]+)?({1})", delim.OpenDelimEquivalent, delim.CloseDelimEquivalent);
			RegexOptions reo = RegexOptions.Singleline | RegexOptions.IgnoreCase;
			_reCurrDir = new Regex(pattern, reo);

			CurrentDir = null;
		}

		/// <summary>
		/// Tracks the directory path we are "currently" in.
		/// </summary>
		public string CurrentDir { get; set; }

		#region IProcessEvaluate Members

		public void Evaluate(object src, EnhancedStringEventArgs ea)
		{
			// Initialize return code
			ea.IsHandled = false;

			string text = ea.EhancedPairElem.Value;
			if (string.IsNullOrWhiteSpace(text)) return;
			bool rc = _reCurrDir.IsMatch(text);
			if (!rc) return;

			// Get the replacement current directory
			string replacement = _reCurrDir.Replace(text, CurrentDirReplace);
			if (replacement == text) return;

			// Announce that we succeeded to find {CurrentDir} and replaced it
			ea.IsHandled = true;

			// Keep new value
			ea.EhancedPairElem.Value = replacement;
			return;
		}

		#endregion

		private string CurrentDirReplace(Match m)
		{
			string prevCount = m.Groups["prevCount"].Value;
			if (string.IsNullOrEmpty(prevCount))
				return CurrentDir;

			int cnt;
			bool rc = int.TryParse(prevCount, out cnt);
			if (!rc) return string.Format(@"--\\no path at the given count: {0}\\--", prevCount);

			if (cnt == 0) return CurrentDir;

			string prevDir = CurrentDir;
			for (int i = 0; i > cnt; --i)
			{
				prevDir = Path.GetDirectoryName(prevDir);
				if (prevDir == null) return string.Format(@"--\\no path at given count: {0}\\--", prevCount);
			}

			return prevDir;
		}
	}
}
