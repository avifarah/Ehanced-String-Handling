using System;


namespace EnhancedStringEvaluate
{
	public interface IProcessEvaluate
	{
		/// <summary>
		/// Signature of a standard event driven delegate
		/// This method is used in the ProcessXxx classes
		/// </summary>
		/// <param name="src"></param>
		/// <param name="ea"></param>
		void Evaluate(object src, EnhancedStringEventArgs ea);
	}
}
