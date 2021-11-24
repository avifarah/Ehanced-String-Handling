using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using ConcordPairEvaluate;

namespace ConcordPairTest
{
	/// <summary>
	/// Summary description for ConcordPairEvalTest
	/// </summary>
	//[TestClass]
	public class ConcordPairEvalTest
	{
		public ConcordPairEvalTest()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		//private TestContext testContextInstance;

		///// <summary>
		/////Gets or sets the test context which provides
		/////information about and functionality for the current test run.
		/////</summary>
		//public TestContext TestContext
		//{
		//    get
		//    {
		//        return testContextInstance;
		//    }
		//    set
		//    {
		//        testContextInstance = value;
		//    }
		//}

		private static  ConcordPairEval _eval;

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext) { }


		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		public static void MyClassCleanup() { }

		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		public void MyTestInitialize() { }

		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		public void MyTestCleanup() { }

		#endregion

		//[TestMethod]
		public static void Main(string[] args)
		{
			//
			// TODO: Add test logic here
			//
			var elems = new Dictionary<string, ConcordPairElement>();
			elems.Add("Flat", new ConcordPairElement("Flat", @"testing"));
			elems.Add("Static flat", new ConcordPairElement("Static flat", @"Static evaluation of flat: {key::flat}"));
			elems.Add("Static date", new ConcordPairElement("Static date", @"Static evaluation of date: {date::yyyy.MM.dd}"));
			elems.Add("Dynamic current directory", new ConcordPairElement("Dynamic current directory", @"current dir: {CurrentDir::}"));
			elems.Add("temp", new ConcordPairElement("temp", @"SpecialDirectory\{key::flat}\{key::Stamp}"));
			elems.Add("Dynamic current path", new ConcordPairElement("Dynamic current path", @"{key::dynamic current directory}\{key::temp}"));
			elems.Add("Stamp", new ConcordPairElement("Stamp", @"{CurrentTime::yyyyMMddHHmmssfff}"));

			_eval = new ConcordPairEval(elems);
		}
	}
}
