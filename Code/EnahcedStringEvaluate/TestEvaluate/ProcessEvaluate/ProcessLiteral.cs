using System;
using System.Text.RegularExpressions;
using EnhancedStringEvaluate;


namespace TestEvaluation.ProcessEvaluate
{
	/// <summary>
	/// A static evaluation
	/// </summary>
	public sealed class ProcessLiteral : IProcessEvaluate
	{
		private readonly Regex _reLiteral;
		private readonly Regex _reNonLiterals;

		public ProcessLiteral() : this(DelimitersAndSeparator.DefaultDelimitedString) {}
		public ProcessLiteral(IDelimitersAndSeparator delim)
		{
			//string pattern = @"({)(?<Literal>([^{}])*?)(})";
			string pattern = string.Format(@"({0})(?<Literal>([^{0}{1}])*?)({1})", delim.OpenDelimEquivalent, delim.CloseDelimEquivalent);
			RegexOptions reo = RegexOptions.Singleline | RegexOptions.IgnoreCase;
			_reLiteral = new Regex(pattern, reo);

			// We need to be vigilant over the possibility of a {key::body} constructs.  These are not literals.
			_reNonLiterals = new Regex(delim.Separator, reo);
		}

		#region IProcessEvaluate Members

		public void Evaluate(object src, EnhancedStringEventArgs ea)
		{
			// Initialize return code
			ea.IsHandled = false;

			string text = ea.EhancedPairElem.Value;
			if (string.IsNullOrWhiteSpace(text)) return;

			Match m = _reLiteral.Match(text);
			if (!m.Success) return;
	
			bool rc = _reNonLiterals.IsMatch(text);
			if (rc) return;

			// Announce that expression was successfully handled
			ea.IsHandled = true;

			// Keep new value
			ea.EhancedPairElem.Value = _reLiteral.Replace(text, mtch => mtch.Groups["Literal"].Value);
			return;
		}

		#endregion
	}
}
