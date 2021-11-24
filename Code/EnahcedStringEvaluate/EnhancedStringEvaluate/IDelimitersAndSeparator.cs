using System;


namespace EnhancedStringEvaluate
{
	public interface IDelimitersAndSeparator
	{
		/// <summary>Original delimiter as user intended it to be</summary>
		string OpenDelimOrig { get; }
		string CloseDelimOrig { get; }

		/// <summary>A single character to be used in case OpenDelimOrig multi-character</summary>
		string OpenDelimAlternate { get; }
		string CloseDelimAlternate { get; }

		/// <summary>
		/// If OpenDelimOrig/CloseDelimOrig is a single character then use Orig or else
		/// use OpenDelimAlternate/CloseDelimAlternate
		/// </summary>
		string OpenDelimEquivalent { get; }
		string CloseDelimEquivalent { get; }

		/// <summary>
		/// Checks for balanced open-close symbol
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		bool IsBalancedOpenClose(string text);

		/// <summary>
		/// A place for the system to transform the delimiters to their equivalent
		/// counterpart.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		string PreMatch(string text);

		/// <summary>
		/// A place for the system to transform the counterparts back to their
		/// original delimiters.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		string PostMatch(string text);

		/// <summary>Separator string</summary>
		string Separator { get; }

		/// <summary>Single character string alternate to the separator.  Used in a search for "not separator"</summary>
		string SeparatorAlternate { get; }

		/// <summary>Does text contains a simple construct</summary>
		/// <param name="text"></param>
		/// <returns></returns>
		bool IsSimpleExpression(string text);
	}
}
