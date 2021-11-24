using System;
using System.Collections.Generic;
using EnhancedStringEvaluate;
using System.Text.RegularExpressions;


namespace TestEvaluation.ProcessEvaluate
{
	/// <summary>
	/// Purpose:
	///		Similar to a calculator memory, a value may be stored and retrieved.
	///	
	///	Format:
	///	(Delimiters are displayed as "{" and "}" but the system will be able to 
	///	handle any delimiter.)
	///		{Memory::Name::Value}	-- Store Value into Name storage
	///		{Memory::Name}			-- Retrieve from Name storage
	/// </summary>
	public class ProcessMemory : IProcessEvaluate
	{
		/// <summary>Expression identifying the memory</summary>
		private Regex _reMemory;

		/// <summary>Identifying the delimiters</summary>
		private readonly IDelimitersAndSeparator _delim;

		/// <summary>Keep track of all the memory stores</summary>
		private Dictionary<string, string> _memories;

		public ProcessMemory() : this(DelimitersAndSeparator.DefaultDelimitedString) {}
		public ProcessMemory(IDelimitersAndSeparator delim)
		{
			_delim = delim;
			_memories = new Dictionary<string, string>();

			RegexOptions reo = RegexOptions.IgnoreCase | RegexOptions.Singleline;
			//string pattern = @"({)Memory::(?<name>:?(([^{}:]+)(:[^{}:]*)?)*)(::(?<value>[^{}]*)?)?(})";
			string pattern = string.Format(@"({0})Memory::(?<name>:?(([^{0}{1}:]+)(:[^{0}{1}:]*)?)*)(::(?<value>[^{0}{1}]*)?)?({1})", 
				_delim.OpenDelimEquivalent, _delim.CloseDelimEquivalent);
			_reMemory = new Regex(pattern, reo);
		}

		#region IProcessEvaluate Members

		public void Evaluate(object src, EnhancedStringEventArgs ea)
		{
			// Initialize return code
			ea.IsHandled = false;

			string text = ea.EhancedPairElem.Value;
			if (string.IsNullOrWhiteSpace(text)) return;

			bool rc = _reMemory.IsMatch(text);
			if (!rc) return;

			string replacement = _reMemory.Replace(text, MemoryReplace);
			if (replacement == text) return;

			// Announce that expression was successfully handled
			ea.IsHandled = true;

			// Keep new value
			ea.EhancedPairElem.Value = replacement;
			return;
		}

		#endregion

		private string MemoryReplace(Match m)
		{
			string name = m.Groups["name"].Value;
			if (string.IsNullOrWhiteSpace(name))
				return m.ToString();

			string value = m.Groups["value"].Value;
			if (string.IsNullOrWhiteSpace(value))
			{
				if (_memories.ContainsKey(name))
					return _memories[name];

				_memories.Add(name, string.Empty);
				return string.Empty;
			}

			if (_memories.ContainsKey(name))
				_memories[name] = value;
			else
				_memories.Add(name, value);

			return value;
		}
	}
}
