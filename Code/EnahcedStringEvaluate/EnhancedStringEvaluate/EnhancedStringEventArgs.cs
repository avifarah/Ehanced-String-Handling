using System;


namespace EnhancedStringEvaluate
{
	[Serializable()]
	public sealed class EnhancedStringEventArgs : EventArgs
	{
		/// <summary>EnhancedStrPairElement that this node is operating on</summary>
		public readonly EnhancedStrPairElement EhancedPairElem;

		/// <summary>Was pattern resolved</summary>
		public bool IsHandled { get; set; }

		public EnhancedStringEventArgs(EnhancedStrPairElement enhancedPairElem)
		{
			EhancedPairElem = enhancedPairElem;
			IsHandled = false;
		}
	}
}
