using System;
using System.Text.RegularExpressions;


namespace EnhancedStringEvaluate
{
	/// <summary>
	/// Purpose:
	///		Encapsulate delimiter and separator handling.
	/// </summary>
	public class DelimitersAndSeparator : IDelimitersAndSeparator
	{
		private const string cDefaultSeparator = "::";
		private string _separator;

		/// <summary>
		/// Evaluating if an expression is "simple" is entirely in the syntax of the delimiters and separator.
		/// An expression is simple if it has:
		///		-	Open delimiter
		///		-	Non-empty identifier
		///		-	Separator
		///		-	Close delimiter
		///		Moreover, this expression does not contain a nested expression.
		/// </summary>
		private Regex _smplExprEvaluator;

		/// <summary>Alternate, single character, for open delimiter and single char for close delimiter</summary>
		private const string cOpenAlternate = "\u0001";
		private const string cCloseAlternate = "\u0002";
		private const string cSeparatorAlternate = "\u0003";

		/// <summary>Default Open/Close delimiters</summary>
		private const string cDefaultOpen = "{";
		private const string cDefaultClose = "}";

		/// <summary>
		/// Default delimitedString
		/// </summary>
		public static readonly IDelimitersAndSeparator DefaultDelimitedString = new DelimitersAndSeparator(cDefaultOpen, cDefaultClose);

		/// <summary>
		/// If you create this library using VS prior to VS2010 then
		/// you will need multiple constructors:
		///		DelimitedString();
		///		DelimitedString(string openDelim, string closeDelim);
		///		DelimitedString(string separator);
		///	which will call the complete constructor:
		///		DelimitedString(string openDelim, string closeDelim, string separator);
		/// </summary>
		/// <param name="openDelim"></param>
		/// <param name="closeDelim"></param>
		/// <param name="separator"></param>
		public DelimitersAndSeparator(string openDelim = cDefaultOpen, string closeDelim = cDefaultClose, string separator = cDefaultSeparator)
		{
			Validator(openDelim, closeDelim, separator);

			OpenDelimOrig = openDelim;
			CloseDelimOrig = closeDelim;

			IsOpenDelimOrigSingleChar = Regex.Unescape(OpenDelimOrig).Length == 1;
			IsCloseDelimOrigSingleChar = Regex.Unescape(CloseDelimOrig).Length == 1;

			string balanceOpenClose = BalancePattern;
			BalancedEvaluator = new Regex(balanceOpenClose, RegexOptions.Singleline);

			_separator = separator;

			//string pattern = @"({)(?<SmplExpr>[^{}]+::[^{}]*?)(})";
			string pattern = string.Format(@"({0})(?<reSmplExpr>[^{0}{1}]+{2}[^{0}{1}]*?)({1})",
												OpenDelimEquivalent, CloseDelimEquivalent, _separator);
			_smplExprEvaluator = new Regex(pattern, RegexOptions.Singleline);
		}

		/// <summary>
		/// Purpose:
		///		Validate open/close delimiters.
		///		*	They are not allowed to be ws (white space)
		///		*	They are not allowed to be the same as one another.
		///		*	Constructor employs this function
		///		Validate separator.
		///		*	It may not be ws.
		///		*	It may not equal neither open nor close delimiters
		/// </summary>
		/// <param name="openDelim"></param>
		/// <param name="closeDelim"></param>
		/// <param name="separator"></param>
		protected virtual void Validator(string openDelim, string closeDelim, string separator)
		{
			// The string.isNullOrWhiteSpace is new to .Net 4.0 in prior versions you will need
			// to issue: (string.IsNullOrEmpty(value) || value.Trim().Length == 0)
			if (string.IsNullOrWhiteSpace(openDelim))
				throw new EnhancedStringException(null, EnhancedStrPairElement.Empty, "Open delimiter cannot be null, empty or white-space");

			if (string.IsNullOrWhiteSpace(closeDelim))
				throw new EnhancedStringException(null, EnhancedStrPairElement.Empty, "Close delimiter cannot be null, empty or white-space");

			if (string.Compare(openDelim, closeDelim, StringComparison.CurrentCultureIgnoreCase) == 0)
				throw new EnhancedStringException(null, EnhancedStrPairElement.Empty, "Open/close delimiters are one and the same (up to case difference)");

			if (string.IsNullOrWhiteSpace(separator))
				throw new EnhancedStringException(null, EnhancedStrPairElement.Empty, "Separator cannot be null, empty or white-space");

			if (string.Compare(separator, openDelim, StringComparison.CurrentCultureIgnoreCase) == 0)
				throw new EnhancedStringException(null, EnhancedStrPairElement.Empty, "Separator cannot equal open delimiter");

			if (string.Compare(separator, closeDelim, StringComparison.CurrentCultureIgnoreCase) == 0)
				throw new EnhancedStringException(null, EnhancedStrPairElement.Empty, "Separator cannot equal close deimiter");
		}

		#region IDelimiters Members

		public virtual string OpenDelimOrig { get; private set; }
		public virtual string OpenDelimAlternate { get { return cOpenAlternate; } }
		public virtual string OpenDelimEquivalent { get { return IsOpenDelimOrigSingleChar ? OpenDelimOrig : OpenDelimAlternate; } }

		public virtual string CloseDelimOrig { get; private set; }
		public virtual string CloseDelimAlternate { get { return cCloseAlternate; } }
		public virtual string CloseDelimEquivalent { get { return IsCloseDelimOrigSingleChar ? CloseDelimOrig : CloseDelimAlternate; } }

		public string Separator { get { return _separator; } }
		public string SeparatorAlternate { get { return cSeparatorAlternate; } }

		public virtual bool IsBalancedOpenClose(string text)
		{
			string preText = PreMatch(text);
			Match m = BalancedEvaluator.Match(preText);
			return m.Success;
		}

		/// <summary>
		/// Transform multi-char delimiters to single-char delimiters
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public virtual string PreMatch(string text)
		{
			string pre1 = IsOpenDelimOrigSingleChar ? text : text.Replace(OpenDelimOrig, OpenDelimAlternate);
			string pre2 = IsCloseDelimOrigSingleChar ? pre1 : pre1.Replace(CloseDelimOrig, CloseDelimAlternate);
			return pre2;
		}

		/// <summary>
		/// Transform back to the original delimiters
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public virtual string PostMatch(string text)
		{
			string post1 = IsCloseDelimOrigSingleChar ? text : text.Replace(CloseDelimAlternate, CloseDelimOrig);
			string post2 = IsOpenDelimOrigSingleChar ? post1 : post1.Replace(OpenDelimAlternate, OpenDelimOrig);
			return post2;
		}

		/// <summary>
		/// Purpose:
		///		Determines if the text contains a simple express.
		///	
		/// Comment:
		///		Simple expression has no inner expressions.  Therefore, {key::Abc} is a simple 
		///		expression, while {key::Abc_{key::ip address}} is not a simple expression.  
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public virtual bool IsSimpleExpression(string text)
		{
			string preText = PreMatch(text);
			Match m = _smplExprEvaluator.Match(preText);
			return m.Success;
		}

		#endregion

		public virtual bool IsOpenDelimOrigSingleChar { get; private set; }
		public virtual bool IsCloseDelimOrigSingleChar { get; private set; }
		public Regex BalancedEvaluator { get; private set; }

		/// <summary>
		/// Note: OpenDelimEquivalent and CloseDelimEquivalent are single character in length
		/// </summary>
		public string BalancePattern
		{
			get
			{
				//string fmt = @"^[^{}]*(((?<Open>{)[^{}]*)+((?<Close-Open>(}))[^{}]*)+)*(?(Open)(?!))$";
				string fmt = @"^[^{0}{1}]*(((?<Open>{0})[^{0}{1}]*)+((?<Close-Open>({1}))[^{0}{1}]*)+)*(?(Open)(?!))$";
				return string.Format(fmt, OpenDelimEquivalent, CloseDelimEquivalent);
			}
		}
	}
}
