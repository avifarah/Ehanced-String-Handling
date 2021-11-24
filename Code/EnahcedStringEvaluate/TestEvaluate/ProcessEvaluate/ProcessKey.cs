using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EnhancedStringEvaluate;


namespace TestEvaluation.ProcessEvaluate
{
	/// <summary>
	/// Process {key::value}
	/// 
	/// static evaluation
	/// </summary>
	public sealed class ProcessKey : IProcessEvaluate
	{
		private readonly Regex _reKey;
		private readonly IDelimitersAndSeparator _delim;

		/// <summary>
		/// Process key could handle the old config entries.
		/// </summary>
		/// <param name="getConfigElem"></param>
		public ProcessKey(IDictionary<string, string> pairs) : this(pairs, DelimitersAndSeparator.DefaultDelimitedString) {}
		public ProcessKey(IDictionary<string, string> pairs, IDelimitersAndSeparator delim)
		{
			_delim = delim;
			_pairEntries = new Dictionary<string, EnhancedStrPairElement>();
			EnahancedPairs = pairs;

			RegexOptions reo = RegexOptions.Singleline | RegexOptions.IgnoreCase;
			//string pattern = @"({)\s*Key\s*::(?<Name>([^{}])*?)(})";
			string pattern = string.Format(@"({0})\s*Key\s*::(?<Name>([^{0}{1}])*?)({1})", delim.OpenDelimEquivalent, delim.CloseDelimEquivalent);
			_reKey = new Regex(pattern, reo);

			ResolveKeys();
		}

		/// <summary>
		/// This routine duplicate the entries as we will alter their Value's within
		/// If your application does not "mind" altering the Values of incoming dictionary
		/// then you need not duplicate the values.
		/// </summary>
		private IDictionary<string, string> EnahancedPairs
		{
			set
			{
				foreach (KeyValuePair<string, string> kvp in value)
				{
					// Making the key search case insensitive
					string key = kvp.Key.ToUpper();				// ToUpper() is more optimized than ToLower()
					_pairEntries.Add(key, new EnhancedStrPairElement(kvp.Key, kvp.Value));
				}
			}
		}

		private void ResolveKeys()
		{
			var eval = new EnhancedStringEval(new List<IProcessEvaluate> { this }, _delim);
			eval.EvaluateStrings(_pairEntries);
		}

		/// <summary>
		/// Keep all entry in a dictionary so that we can evaluate all of them for {Date::value}, {key::value}
		/// or other evaluations like {ForeignKey::path::value}
		/// </summary>
		private Dictionary<string, EnhancedStrPairElement> _pairEntries;

		#region IProcessEvaluate Members

		public void Evaluate(object src, EnhancedStringEventArgs ea)
		{
			// Initialize return code
			ea.IsHandled = false;

			string text = ea.EhancedPairElem.Value;
			if (string.IsNullOrWhiteSpace(text)) return;

			bool rc = _reKey.IsMatch(text);
			if (!rc) return;

			string replacement = _reKey.Replace(text, KeyReplace);
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
			string txt = m.Groups["Name"].Value;
			string key = txt.ToUpper();				// Case insensitive key.  ToUpper() is more optimized than ToLower()
			if (!_pairEntries.ContainsKey(key)) return m.ToString();

			string rplcElem = _pairEntries[key].Value;
			if (rplcElem == null) return m.ToString();
			return rplcElem;
		}
	}
}
