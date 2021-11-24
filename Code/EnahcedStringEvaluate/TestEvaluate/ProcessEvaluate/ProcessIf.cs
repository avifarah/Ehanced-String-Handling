using System;
using System.Text.RegularExpressions;
using System.IO;
using EnhancedStringEvaluate;


namespace TestEvaluation.ProcessEvaluate
{
	/// <summary>
	/// Process if logic having the format:
	///		{if::condition::tExp::fExp}
	///	where condition can be one of the following:
	///		*	DirectoryExists(directory-full-path)
	///		*	FileExists(file-full-path)
	///		*	exp1=exp2
	/// </summary>
	public sealed class ProcessIf : IProcessEvaluate
	{
		private readonly Regex _reIf;
		IDelimitersAndSeparator _delim;

		/// <summary>
		/// There is an implicit assumption that the separator does not appear anywhere in the expressions
		/// except as a separator.  If this assumption is incorrect then the regular expression patterns will
		/// need changing.
		/// </summary>
		public ProcessIf() : this(DelimitersAndSeparator.DefaultDelimitedString) {}
		public ProcessIf(IDelimitersAndSeparator delim)
		{
			_delim = delim;

			//string pattIf1 = @"({)\s*if\s*::\s*DirectoryExists\s*\(\s*(?<directoryName>([^{}])*?)\s*\)\s*::\s*(?<texp>([^{}])*?)\s*::\s*(?<fexp>([^{}])*?)(})";
			//string pattIf2 = @"({)\s*if\s*::\s*FileExists\s*\(\s*(?<fileName>([^{}])*?)\)\s*\)\s*::\s*(?<texp>([^{}])*?)\s*::\s*(?<fexp>([^{}])*?)(})";
			//string pattIf3 = @"({)\s*if\s*::\s*(?<exp1>([^{}])*?)\s*=\s*(?<exp2>([^{}])*?)\s*::\s*(?<texp>([^{}])*?)\s*::\s*(?<fexp>([^{}])*?)(})";
			string pattIf1 = string.Format(@"({0})\s*if\s*{2}\s*DirectoryExists\s*\(\s*(?<directoryName>([^{0}{1}])*?)\s*\)\s*{2}\s*(?<texp>([^{0}{1}])*?)\s*{2}\s*(?<fexp>([^{0}{1}])*?)({1})", _delim.OpenDelimEquivalent, _delim.CloseDelimEquivalent, _delim.Separator);
			string pattIf2 = string.Format(@"({0})\s*if\s*{2}\s*FileExists\s*\(\s*(?<fileName>([^{0}{1}])*?)\s*\)\s*{2}\s*(?<texp>([^{0}{1}])*?)\s*{2}\s*(?<fexp>([^{0}{1}])*?)({1})", _delim.OpenDelimEquivalent, _delim.CloseDelimEquivalent, _delim.Separator);
			string pattIf3 = string.Format(@"({0})\s*if\s*{2}\s*(?<exp1>([^{0}{1}])*?)\s*=\s*(?<exp2>([^{0}{1}])*?)\s*{2}\s*(?<texp>([^{0}{1}])*?)\s*{2}\s*(?<fexp>([^{0}{1}])*?)({1})", _delim.OpenDelimEquivalent, _delim.CloseDelimEquivalent, _delim.Separator);
			string pattern = string.Format("({0})|({1})|({2})", pattIf1, pattIf2, pattIf3);
			RegexOptions reo = RegexOptions.Singleline | RegexOptions.IgnoreCase;
			_reIf = new Regex(pattern, reo);
		}

		#region IProcessEvaluate Members

		public void Evaluate(object src, EnhancedStringEventArgs ea)
		{
			// Initialize return code
			ea.IsHandled = false;

			string text = ea.EhancedPairElem.Value;
			if (string.IsNullOrWhiteSpace(text)) return;

			Match m = _reIf.Match(text);
			if (!m.Success) return;

			string dir = m.Groups["directoryName"].Value;
			string replace;
			if (dir != string.Empty)
			{
				dir = dir.ExtendToFullPath();
				if (Directory.Exists(dir))
					replace = m.Groups["texp"].Value;
				else
					replace = m.Groups["fexp"].Value;
				if (replace == text) return;

				// Announce that expression was successfully handled
				ea.IsHandled = true;

				// Keep new value
				ea.EhancedPairElem.Value = replace;
				return;
			}

			ea.IsHandled = false;
			string fn = m.Groups["fileName"].Value;
			if (fn != string.Empty)
			{
				fn = fn.ExtendToFullPath();
				if (File.Exists(fn))
					replace = m.Groups["texp"].Value;
				else
					replace = m.Groups["fexp"].Value;
				if (replace == text) return;

				// Announce that expression was successfully handled
				ea.IsHandled = true;

				// Keep new value
				ea.EhancedPairElem.Value = replace;
				return;
			}

			ea.IsHandled = false;
			string exp1 = m.Groups["exp1"].Value;
			string exp2 = m.Groups["exp2"].Value;
			if (exp1 != string.Empty && exp2 != string.Empty)
			{
				if (string.Compare(exp1, exp2, true) == 0)
					replace = m.Groups["texp"].Value;
				else
					replace = m.Groups["fexp"].Value;
				if (replace == text) return;

				// Announce that expression was successfully handled
				ea.IsHandled = true;

				// Keep new value
				ea.EhancedPairElem.Value = replace;
				return;
			}

			return;
		}

		#endregion
	}
}
