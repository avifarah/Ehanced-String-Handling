using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EnhancedStringEvaluate;


namespace TestEvaluation.ProcessEvaluate
{
	/// <summary>
	/// See article explanation possibility1 and possibility2
	/// </summary>
	public enum Possibility { DirectColonKnowledge, Substitution }

	/// <summary>
	/// Purpose:
	///		Counter supports a construct like {Counter::name} (where name may 
	///		be a name like "PageNumber" etc...).  The first time it is encountered 
	///		it returns the value of 0.  Then each subsequent time that it is 
	/// encountered it returns the previous value incremented by 1.  So after 
	/// 0 {Counter::name} will yield 1, then 2 ...
	/// 
	/// There may be multiple counters each one with its own name.
	/// 
	/// Other variations on Counter are:
	///		{Counter::name}
	///		{Counter::name::Init}		// Initialize or re-initialize the counter named "name".  Default action for 1st {Counter::name} evaluation
	///									// equivalent to {counter::name::=0}
	///		{Counter::name::next}		// Default action for {Counter::name} evaluation except for first evaluation, equivalent to {Counter::name::+1}
	///		{Counter::name::previous}	// Takes counter's current value and decrement it by 1, equivalent to {Counter::name::-1}
	///		{Counter::name::=n}			// Initialize the Counter name "name" to  Where n is an integer value (may be negative).
	///		{Counter::name::+n}			// Increment the counter's current value by n.
	///		{Counter::name::-n}			// Decrement the counter's current value by n.
	/// </summary>
	public sealed class ProcessCounter : IProcessEvaluate
	{
		/// <summary>Counter regular expression identifying the Counter based on the format given in the pattern supplied</summary>
		private Regex _reCounter;

		/// <summary></summary>
		private readonly IDelimitersAndSeparator _delim;

		/// <summary>Keep track of all the counters</summary>
		private Dictionary<string, int> _counters;

		/// <summary>Inject Possibility</summary>
		private Possibility _possibilityOption;
		public Possibility PosibilityOption
		{
			get { return _possibilityOption; }
			set
			{
				_possibilityOption = value;
				string pattern;
				RegexOptions reo = RegexOptions.Singleline | RegexOptions.IgnoreCase;
				if (_possibilityOption == Possibility.DirectColonKnowledge)
				{
					pattern = string.Format(
						@"({0})\s*Counter\s*::(?<name>:?[^{0}{1}:]+(:[^{0}{1}:]+)*:?)" +
							@"(::\s*(?<extras>(init)|(next)|(previous)|" +
							@"((?<op>[=+-])\s*(?<direction>[+-])?\s*(?<val>[0-9]+)))?)?\s*({1})",
						_delim.OpenDelimEquivalent, _delim.CloseDelimEquivalent);
				}
				else
				{
					pattern = string.Format(
						@"({0})\s*Counter\s*{2}(?<name>[^{0}{1}{2}]+)" +
						@"({2}\s*(?<extras>(init)|(next)|(previous)|" +
						@"((?<op>[=+-])\s*(?<direction>[+-])?\s*(?<val>[0-9]+)))?)?\s*({1})",
						_delim.OpenDelimEquivalent, _delim.CloseDelimEquivalent, _delim.SeparatorAlternate);
				}
				_reCounter = new Regex(pattern, reo);
			}
		}

		/// <summary>
		/// Action type given.  Action is the third field in the format pattern:
		///		{Counter::Name::Action}
		///	Action may be:
		///		Default: not given, in which it will be set to INIT if first encounter or NEXT if subsequent encounters
		///		INIT: set variable to 0 if the word "init" is given or set to a value if "= n" is given
		///		NEXT: Add one to identifier if the word "Next" is given, or n will be added to the counter if "+ n" is given
		///		PREVIOUS: similar to Next, but in the oposite direction.
		/// </summary>
		private enum Action { Default, Init, Next, Previous }

		public ProcessCounter() : this(DelimitersAndSeparator.DefaultDelimitedString) {}
		public ProcessCounter(IDelimitersAndSeparator delim)
		{
			_delim = delim;
			_counters = new Dictionary<string, int>();
			PosibilityOption = Possibility.DirectColonKnowledge;
		}

		#region IProcessEvaluate Members

		public void Evaluate(object src, EnhancedStringEventArgs ea)
		{
			// Initialize return code
			ea.IsHandled = false;

			string text = ea.EhancedPairElem.Value;
			if (string.IsNullOrWhiteSpace(text)) return;

			if (PosibilityOption == Possibility.DirectColonKnowledge)
			{
				bool rc = _reCounter.IsMatch(text);
				if (!rc) return;

				string replacement = _reCounter.Replace(text, CounterReplace);
				if (replacement == text) return;

				// Announce that expression was successfully handled
				ea.IsHandled = true;

				// Keep new value
				ea.EhancedPairElem.Value = replacement;
				return;
			}
			else
			{
				string preText = text.Replace(_delim.Separator, _delim.SeparatorAlternate);
				bool rc = _reCounter.IsMatch(preText);
				if (!rc) return;

				string replacement = _reCounter.Replace(preText, CounterReplace);
				if (replacement == preText) return;

				// Announce that expression was successfully handled
				ea.IsHandled = true;

				// Keep new value
				string postText = replacement.Replace(_delim.SeparatorAlternate, _delim.Separator);
				ea.EhancedPairElem.Value = postText;
				return;
			}
		}

		#endregion

		/// <summary>
		/// We get here only if a match on _reCounter is true
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		private string CounterReplace(Match m)
		{
			string name = m.Groups["name"].Value;
			if (string.IsNullOrEmpty(name))
				return m.ToString();

			Action action = Action.Default;
			int nExtraBy = 0;					// Either initialized to or Move by...
			string extras = m.Groups["extras"].Value;
			if (string.IsNullOrWhiteSpace(extras))
				action = Action.Default;
			else
			{
				// @"({0})\s*Counter\s*::(?<Name>[^{0}{1}:]+:?)
				//	(::\s*(?<extras>(init)|(next)|(previous)|((?<op>[=+-])\s*(?<direction>[+-])?\s*(?<val>[0-9]+)))?)?\s*(})"
				if (string.Compare(extras, "Init", true) == 0)
				{
					action = Action.Init;
					nExtraBy = 0;
				}
				else if (string.Compare(extras, "Next", true) == 0)
				{
					action = Action.Next;
					nExtraBy = 1;
				}
				else if (string.Compare(extras, "Previous", true) == 0)
				{
					action = Action.Previous;
					nExtraBy = 1;
				}
				else
				{
					string op = m.Groups["op"].Value;
					nExtraBy = int.Parse(m.Groups["val"].Value);
					string direction = m.Groups["direction"].Value;
					if (!string.IsNullOrWhiteSpace(direction))
						if (direction == "-") nExtraBy = -nExtraBy;
					switch (op)
					{
						case "=": action = Action.Init; break;
						case "+": action = Action.Next; break;
						case "-": action = Action.Previous; break;
						default: throw new Exception("This is impossible");
					}
				}
			}

			if (!_counters.ContainsKey(name))
			{
				if (action == Action.Default || action == Action.Init)
				{
					_counters.Add(name, nExtraBy);
					return _counters[name].ToString();
				}

				throw new Exception(string.Format("Counter is ill formatted.  {0}", m.ToString()));
			}

			switch (action)
			{
				case Action.Default: ++_counters[name]; break;
				case Action.Init: _counters[name] = nExtraBy; break;
				case Action.Next: _counters[name] += nExtraBy; break;
				case Action.Previous: _counters[name] -= nExtraBy; break;
				default: throw new Exception(string.Format("Counter is ill formatted.  {0}", m.ToString()));
			}

			return _counters[name].ToString();
		}
	}
}
