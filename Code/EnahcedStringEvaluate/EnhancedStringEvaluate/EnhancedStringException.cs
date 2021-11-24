using System;


namespace EnhancedStringEvaluate
{
	/// <summary>
	/// Provide an exception that is specific to this library
	/// </summary>
	public class EnhancedStringException : Exception
	{
		/// <summary>May be null or string.Empty</summary>
		public string Identifier { get; private set; }

		/// <summary>May be null</summary>
		public EnhancedStrPairElement Element { get; private set; }

		public EnhancedStringException(string key, EnhancedStrPairElement elem)
			: base()
		{
			Element = elem;
			Identifier = key;
		}

		public EnhancedStringException(string key, EnhancedStrPairElement elem, string message)
			: base(message)
		{
			Element = elem;
			Identifier = key;
		}

		public EnhancedStringException(string key, EnhancedStrPairElement elem, string message, Exception innerException)
			: base(message, innerException)
		{
			Element = elem;
			Identifier = key;
		}

		public EnhancedStringException(string key, string value)
			: this(key, new EnhancedStrPairElement(key, value)) { }

		public EnhancedStringException(string key, string value, string message)
			: this(key, new EnhancedStrPairElement(key, value), message) { }

		public EnhancedStringException(string key, string value, string message, Exception innerException)
			: this(key, new EnhancedStrPairElement(key, value), message, innerException) { }
	}
}
