using System;
using System.Text.RegularExpressions;
using System.IO;
using EnhancedStringEvaluate;


namespace TestEvaluation.ProcessEvaluate
{
	/// <summary>
	/// Process a construct like:
	///		{ForeignKey::fullpath or UNC path or relative path::key}
	///	This class is very similar to the ProcessKey class in the sense 
	///	that, evaluating the key does not guarantee that the result will
	///	need no further evaluations.
	///	
	/// It expects the middle section to point to a flat file having the format
	/// of:
	///		key=value
	/// 
	/// A static key evaluation
	/// </summary>
	public sealed class ProcessForeignKey : IProcessEvaluate
	{
		/// <summary>
		/// Need a place holder for the calling class (Config) data dictionary
		/// </summary>
		private readonly Regex _reKey;
		private readonly Regex _reForeignKey;
		private readonly IDelimitersAndSeparator _delim;

		public ProcessForeignKey() : this(DelimitersAndSeparator.DefaultDelimitedString) {}
		public ProcessForeignKey(IDelimitersAndSeparator delim)
		{
			_delim = delim;

			//string pattern = @"({)\s*ForeignKey\s*::(?<path>([^{}])*?)::(?<Name>([^{}])*?)(})";
			string pattern = string.Format(@"({0})\s*ForeignKey\s*::(?<path>([^{0}{1}])*?)::(?<Name>([^{0}{1}])*?)({1})",
				_delim.OpenDelimEquivalent, _delim.CloseDelimEquivalent);
			RegexOptions reo = RegexOptions.Singleline | RegexOptions.IgnoreCase;
			_reKey = new Regex(pattern, reo);

			string foreignPat = @"^(?<key>.*?)=(?<value>.*)$";
			_reForeignKey = new Regex(foreignPat, reo);
		}

		#region IProcessEvaluate Members

		/// <summary>
		/// The function takes a value like "{ForeignKey::path::EnvSpecific}"
		/// and returns the value as it appears in the file (specified by path).
		/// The routine will look for constructs within the file (specified by path)
		/// that is ForeignKey=value.
		/// </summary>
		/// <param name="src"></param>
		/// <param name="ea"></param>
		public void Evaluate(object src, EnhancedStringEventArgs ea)
		{
			// Initialize return code
			ea.IsHandled = false;

			string text = ea.EhancedPairElem.Value;
			if (string.IsNullOrWhiteSpace(text)) return;

			bool rc = _reKey.IsMatch(text);
			if (!rc) return;

			string replacement = _reKey.Replace(text, KeyReplace);
			if (replacement == null) return;
			if (replacement == text) return;

			// Announce that expression was successfully handled
			ea.IsHandled = true;

			// Keep new value
			ea.EhancedPairElem.Value = replacement;
			return;
		}

		#endregion

		private string KeyReplace(Match m)
		{
			string fileNm = m.Groups["path"].Value;
			string key = m.Groups["Name"].Value;

			fileNm = fileNm.ExtendToFullPath();
			if (!File.Exists(fileNm))
				return null;

			// Substitute a different key if one exists
			//
			// I make the assumption that the number of keys needed from
			// the provided path is small enough that opening and closing
			// the file for each foreign key fetch is not a performance
			// problem.  If this assumption is not a good one then we need
			// to modify the code to open the file once and close it once.
			using (StreamReader sr = new StreamReader(fileNm))
			{
				while (!sr.EndOfStream)
				{
					string line = sr.ReadLine();
					Match mForeign = _reForeignKey.Match(line);
					if (mForeign.Success)
					{
						if (string.Compare(key, mForeign.Groups["key"].Value, true) == 0)
							return mForeign.Groups["value"].Value;
					}
				}
			}

			return m.ToString();
		}
	}
}
