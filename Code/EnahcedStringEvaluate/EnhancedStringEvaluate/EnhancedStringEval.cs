using System;
using System.Linq;
using System.Collections.Generic;


namespace EnhancedStringEvaluate
{
	/// <summary>
	/// Purpose:
	///		Provide a vehicle that will:
	///			1	Transform a single substrings of the format of {identifier::value} 
	///				according to the rules set forth in the appropriate identifier's Process 
	///				class.  In the textual explanation, the article, this Process class
	///				is referred to as ProcessXxx class.  
	///			2	Transform a Dictionary<string, string> of (identifier, value) collection, 
	///				according to the rules set forth by the ProcessXxx classes.
	/// </summary>
	public class EnhancedStringEval
	{
		/// <summary>Prevent infinite looping in the evaluator</summary>
		private const int PassThroughUpperLimit = 1000;

		private const string TEMPKEY = "*** Temporary string element Key that is not likely to clash with another StringElement key !!!";
		private readonly IDelimitersAndSeparator _delim;

		/// <summary>
		/// The evaluation context pointing to the various ProcessXxx.Evaluate() routines.
		/// </summary>
		public event EventHandler<EnhancedStringEventArgs> OnEvaluateContext;

		/// <summary>
		/// .ctor
		/// </summary>
		/// <returns></returns>
		/// <param name="context"></param>
		/// <param name="delim"></param>
		public EnhancedStringEval() : this(null, DelimitersAndSeparator.DefaultDelimitedString) { }
		public EnhancedStringEval(IEnumerable<IProcessEvaluate> context) : this(context, DelimitersAndSeparator.DefaultDelimitedString) { }
		public EnhancedStringEval(IEnumerable<IProcessEvaluate> context, IDelimitersAndSeparator delim)
		{
			_delim = delim;

			if (context != null)
				foreach (IProcessEvaluate processXxx in context)
					if (processXxx != null)
						OnEvaluateContext += processXxx.Evaluate;
		}

		/// <summary>
		/// Purpose:
		///		Evaluate a simple expression.
		/// </summary>
		/// <param name="elem"></param>
		/// <returns></returns>
		protected virtual bool EvalSimpleExpression(EnhancedStrPairElement elem)
		{
			bool rc = _delim.IsSimpleExpression(elem.Value);
			if (!rc) return false;

			var ea = new EnhancedStringEventArgs(new EnhancedStrPairElement(elem.Indetifier, _delim.PreMatch(elem.Value)));
			foreach (Delegate processXxx in OnEvaluateContext.GetInvocationList())
			{
				try { processXxx.DynamicInvoke(new object[] { this, ea }); }
				catch { }
				if (ea.IsHandled)
				{
					elem.Value = _delim.PostMatch(ea.EhancedPairElem.Value);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Purpose:
		///		Delimiter balance check.  Each open delimiter should have a close delimiter
		///	
		/// Comment:
		///		Called right before the evaluation ...
		/// </summary>
		private string BalancePreEvaluate(string text)
		{
			bool rc = _delim.IsBalancedOpenClose(text);
			if (!rc)
				throw new EnhancedStringException(null, text, "Delimiters are not balanced.");

			return text;
		}

		/// <summary>
		/// This function is to be called right after the evaluation ...
		/// It serves as an opposite to the BalancePreEvaluate(..)
		/// </summary>
		private string BalancePostEvaluate(string text)
		{
			return text;
		}

		/// <summary>
		/// Used as equivalent of the single-string BalancePreEvaluate(..)
		/// 
		/// Comment:
		///		This routine operates insitue (in place--no new memory is needed)
		/// </summary>
		/// <param name="enhStrPairs"></param>
		/// <returns></returns>
		private void BalancePreEvaluate(IDictionary<string, EnhancedStrPairElement> enhStrPairs)
		{
			foreach (var elem in enhStrPairs)
			{
				bool rc = _delim.IsBalancedOpenClose(elem.Value.Value);
				if (!rc)
					throw new EnhancedStringException(elem.Value.Indetifier, elem.Value.Value, "Braces are not balanced.");
			}
		}

		/// <summary>
		/// The opposite functionality to multi-pair BalancePreEvaluate(..)
		/// </summary>
		/// <param name="enhStrPairs"></param>
		/// <returns></returns>
		private void BalancePostEvaluate(IDictionary<string, EnhancedStrPairElement> enhStrPairs)
		{
			return;
		}

		/// <summary>
		/// In order to use it, one needs to override it.
		/// Used to scrub or transform text before it is passed through the Evaluator.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		protected virtual string PreEvaluate(string text)
		{
			return text;
		}

		/// <summary>
		/// In order to use it, one needs to override it.
		/// Used as an opposite for PreEvaluate(..)
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		protected virtual string PostEvaluate(string text)
		{
			return text;
		}

		/// <summary>
		/// Equivalent to the single-string PreEvaluate(..) for a list of elements
		/// </summary>
		/// <param name="enhStrPairs"></param>
		/// <returns></returns>
		protected virtual void PreEvaluate(IDictionary<string, EnhancedStrPairElement> enhStrPairs)
		{
			return;
		}

		/// <summary>
		/// The opposite of multi-pair PreEvaluate(..)
		/// </summary>
		/// <param name="enhStrPairs"></param>
		/// <returns></returns>
		protected virtual void PostEvaluate(IDictionary<string, EnhancedStrPairElement> enhStrPairs)
		{
			return;
		}

		/// <summary>
		/// Transform a string according to the rules laid out by the
		///		>	Pre / post Evaluate overrides
		///		>	Context
		/// </summary>
		/// <returns></returns>
		public string EvaluateString(string text)
		{
			string preText = PreEvaluate(text);
			string balanceText = BalancePreEvaluate(preText);

			string evalText = EvaluateStringPure(balanceText);
	
			string postBalance = BalancePostEvaluate(evalText);
			string postText = PostEvaluate(postBalance);
			return postText;
		}

		/// <summary>
		/// The magic happens here
		/// 
		/// Comment:
		///		The inner loop resembles the EvalSimpleExpression(..) method, with
		///		one notable exception the check for _delim.IsSimpleExpression(..)
		///	This is done deliberately so that the ProcessLiteral, processing 
		///	expressions that are not a simple expressions, could be processed as well.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		private string EvaluateStringPure(string text)
		{
			if (OnEvaluateContext == null) return text;

			var flghtyElem = new EnhancedStrPairElement(TEMPKEY, _delim.PreMatch(text));
			var ea = new EnhancedStringEventArgs(flghtyElem);

			bool bHandled = false;
			Delegate[] context = OnEvaluateContext.GetInvocationList();
			for (int i = 0; i < PassThroughUpperLimit; ++i)
			{
				bHandled = false;
				foreach (Delegate processXxx in context)
				{
					try { processXxx.DynamicInvoke(new object[] { this, ea }); }
					catch { }
					if (ea.IsHandled)
					{
						bHandled = true;
						string val = _delim.PreMatch(ea.EhancedPairElem.Value);
						string preText = PreEvaluate(val);
						string balanceText = BalancePreEvaluate(preText);
						flghtyElem = new EnhancedStrPairElement(TEMPKEY, balanceText);
						ea = new EnhancedStringEventArgs(flghtyElem);
					}
				}

				if (!bHandled) break;
			}

			return _delim.PostMatch(ea.EhancedPairElem.Value);
		}

		/// <summary>
		/// Purpose:
		///		Optimization.
		///	
		/// Comment:
		///		The equivalent to the EvaluateString--evaluating a single string;
		///		EvaluateStrings, evaluates a collection of pairs as a unit
		///			>	As a unit pre-evaluate (user overridden)
		///			>	Check balanced delimiters
		///			>	Run through pure evaluate
		///			>	Post check balanced delimiters
		///			>	Post-evaluate (user overridden)
		/// </summary>
		/// <param name="enhStrPairs"></param>
		/// <returns></returns>
		public void EvaluateStrings(IDictionary<string, EnhancedStrPairElement> enhStrPairs)
		{
			PreEvaluate(enhStrPairs);
			BalancePreEvaluate(enhStrPairs);

			EvaluateStringsPure(enhStrPairs);

			BalancePostEvaluate(enhStrPairs);
			PostEvaluate(enhStrPairs);
		}

		/// <summary>
		/// The magic happens here--for a collection
		/// </summary>
		/// <param name="enhStrPairs"></param>
		private void EvaluateStringsPure(IDictionary<string, EnhancedStrPairElement> enhStrPairs)
		{
			if (OnEvaluateContext == null) return;

			//
			// Retrieve all nodes needing attention, into a linked list.
			// needing attention meanS a node that has a simple expression in it
			//
			var links = new LinkedList<EnhancedStrPairElement>();
			var pairNodes = from elem in enhStrPairs where _delim.IsSimpleExpression(elem.Value.Value) select elem.Value;
			foreach (EnhancedStrPairElement pairNode in pairNodes)
				links.AddLast(pairNode);

			if (links.Count == 0) return;

			// The PassThroughUpperLimit avoids an infinit loop.
			for (int i = 0; i < PassThroughUpperLimit; ++i)
			{
				// Cycle through the linked list
				LinkedListNode<EnhancedStrPairElement> linkNode = links.First;
				while (linkNode != null)
				{
					// The magic of handling the static EnhancedStrPairElement expressions
					// happen in EvalSimpleExpression(..).
					bool bEval = EvalSimpleExpression(linkNode.Value);
					if (!bEval)
					{
						// There is nothing to evaluate any further--Remove the node.
						// Before removing add to return pairs
						LinkedListNode<EnhancedStrPairElement> p = linkNode.Next;
						links.Remove(linkNode);
						linkNode = p;
					}
					else
					{
						PreEvaluate(enhStrPairs);
						BalancePreEvaluate(enhStrPairs);
						linkNode = linkNode.Next;
					}
				}

				if (links.Count == 0) break;
			}
		}
	}
}
