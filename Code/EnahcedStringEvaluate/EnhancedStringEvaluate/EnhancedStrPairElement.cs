using System;
using System.Collections.Generic;


namespace EnhancedStringEvaluate
{
	/// We would like the identifier to be case insensitive, so we employ a 
	/// Dictionary<string, EnhancedStrPairElement> where the 1st generic parameter, 
	/// a string, is the identifier of the pair, operated on with the ToUpper() 
	/// function.  In this way we ensure that the search for the identifier is:
	///		>	case insensitive 
	///		>	we do not loose the original casing (it is still stored within 
	///			the EnhancedStrPairElement (2nd generic parameter)
	///		>	and this mechanism is fairly simple to implement and understand
	/// </summary>
	[Serializable]
	public sealed class EnhancedStrPairElement
	{
		public EnhancedStrPairElement(string identifier, string value)
		{
			Indetifier = identifier;
			Value = value;
		}

		/// <summary>
		/// Identifier--is immutable not only because string is immutable but 
		/// also because there is no way to change it outside the constructor.
		/// Now it is suited for a Dictionary<..> key, or a Hashtable(..) key.
		/// </summary>
		public string Indetifier { get; private set; }

		/// <summary>Value may change by the outside world</summary>
		public string Value { get; set; }

		public static explicit operator KeyValuePair<string, string>(EnhancedStrPairElement elem)
		{
			return new KeyValuePair<string, string>(elem.Indetifier, elem.Value);
		}

		public static implicit operator EnhancedStrPairElement(KeyValuePair<string, string> elem)
		{
			return new EnhancedStrPairElement(elem.Key, elem.Value);
		}

		public KeyValuePair<string, string> ToKeyValuePair() { return (KeyValuePair<string, string>)this; }

		public override string ToString()
		{
			return string.Format("({0}, {1})", Indetifier, Value);
		}

		private static EnhancedStrPairElement EmptyEnhancedStrElement = new EnhancedStrPairElement(null, null);
		public static EnhancedStrPairElement Empty { get { return EmptyEnhancedStrElement; } }
	}
}
