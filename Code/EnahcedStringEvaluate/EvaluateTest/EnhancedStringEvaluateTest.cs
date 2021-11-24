using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnhancedStringEvaluate;
using TestEvaluation.ProcessEvaluate;
using System.Text.RegularExpressions;
using System.IO;


namespace EvaluateSampleTest
{
	/// <summary>
	/// Summary description for EnhancedStringEvaluateTest
	/// </summary>
	[TestClass]
	public class EnhancedStringEvaluateTest
	{
		public EnhancedStringEvaluateTest()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

		[TestMethod]
		public void CounterTest()
		{
			var context = new List<IProcessEvaluate>();
			context.Add(new ProcessCounter());

			var eval = new EnhancedStringEval(context);
			string counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::}");
			Assert.AreEqual(@"Counter1: 0", counterN1);

			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1}");
			Assert.AreEqual(@"Counter1: 1", counterN1);

			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::Init}");
			Assert.AreEqual(@"Counter1: 0", counterN1);

			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::Next}");
			Assert.AreEqual(@"Counter1: 1", counterN1);

			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::Previous}");
			Assert.AreEqual(@"Counter1: 0", counterN1);

			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::=10}");
			Assert.AreEqual("Counter1: 10", counterN1);

			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::+10}");
			Assert.AreEqual("Counter1: 20", counterN1);

			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::-1}");
			Assert.AreEqual("Counter1: 19", counterN1);

			try
			{
				string counterN2 = eval.EvaluateString("Counter2: {Counter::CounterN2::Next}");
				Assert.Fail();
			}
			catch (Exception ex)
			{
				// Passing through the catch leg of the try/catch is good
			}

			try
			{
				string counterN2 = eval.EvaluateString("Counter2: {Counter::CounterN2::Previous}");
				Assert.Fail();
			}
			catch (Exception ex)
			{
				// Passing through the catch leg of the try/catch is good
			}

			//
			// Complex delimiters
			//

			IDelimitersAndSeparator delim = new DelimitersAndSeparator("<Delim>", "</Delim>");
			var contextCmplx = new List<IProcessEvaluate>();
			contextCmplx.Add(new ProcessCounter(delim));
			var evalCmplx = new EnhancedStringEval(contextCmplx, delim);
			string counterCmplx = evalCmplx.EvaluateString("Complex delim counter: <Delim>Counter::Complex Delim::</Delim>");
			Assert.AreEqual(@"Complex delim counter: 0", counterCmplx);
			counterCmplx = evalCmplx.EvaluateString("Counter1: <Delim>Counter::CounterName1</Delim>");
			Assert.AreEqual(@"Counter1: 0", counterCmplx);
			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::Init}");	// Still using old ProcessXxx!!!
			Assert.AreEqual(@"Counter1: 0", counterN1);

			counterCmplx = evalCmplx.EvaluateString("Complex delim counter: <Delim>Counter::Complex Delim</Delim>");
			Assert.AreEqual(@"Complex delim counter: 1", counterCmplx);
			counterCmplx = evalCmplx.EvaluateString("Counter1: <Delim>Counter::CounterName1</Delim>");
			Assert.AreEqual(@"Counter1: 1", counterCmplx);
			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1}");
			Assert.AreEqual(@"Counter1: 1", counterN1);

			counterCmplx = evalCmplx.EvaluateString("Complex delim counter: <Delim>Counter::Complex Delim</Delim>");
			Assert.AreEqual(@"Complex delim counter: 2", counterCmplx);
			counterCmplx = evalCmplx.EvaluateString("Counter1: <Delim>Counter::CounterName1</Delim>");
			Assert.AreEqual(@"Counter1: 2", counterCmplx);
			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1}");
			Assert.AreEqual(@"Counter1: 2", counterN1);

			counterCmplx = evalCmplx.EvaluateString("Complex delim counter: <Delim>Counter::Complex Delim::Init</Delim>");
			Assert.AreEqual(@"Complex delim counter: 0", counterCmplx);
			counterCmplx = evalCmplx.EvaluateString("Counter1: <Delim>Counter::CounterName1::Init</Delim>");
			Assert.AreEqual(@"Counter1: 0", counterCmplx);
			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::Init}");
			Assert.AreEqual(@"Counter1: 0", counterN1);

			counterCmplx = evalCmplx.EvaluateString("Complex delim counter: <Delim>Counter::Complex Delim::Next</Delim>");
			Assert.AreEqual(@"Complex delim counter: 1", counterCmplx);
			counterCmplx = evalCmplx.EvaluateString("Counter1: <Delim>Counter::CounterName1::Next</Delim>");
			Assert.AreEqual(@"Counter1: 1", counterCmplx);
			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::Next}");
			Assert.AreEqual(@"Counter1: 1", counterN1);

			counterCmplx = evalCmplx.EvaluateString("Complex delim counter: <Delim>Counter::Complex Delim::Previous</Delim>");
			Assert.AreEqual(@"Complex delim counter: 0", counterCmplx);
			counterCmplx = evalCmplx.EvaluateString("Counter1: <Delim>Counter::CounterName1::Previous</Delim>");
			Assert.AreEqual(@"Counter1: 0", counterCmplx);
			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::Previous}");
			Assert.AreEqual(@"Counter1: 0", counterN1);

			counterCmplx = evalCmplx.EvaluateString("Complex delim counter: <Delim>Counter::Complex Delim:: Next </Delim>");
			Assert.AreEqual(@"Complex delim counter: 1", counterCmplx);
			counterCmplx = evalCmplx.EvaluateString("Counter1: <Delim>Counter::CounterName1:: Next </Delim>");
			Assert.AreEqual(@"Counter1: 1", counterCmplx);
			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::Next}");
			Assert.AreEqual(@"Counter1: 1", counterN1);

			counterCmplx = evalCmplx.EvaluateString("Complex delim counter: <Delim>Counter::Complex Delim:: = 10 </Delim>");
			Assert.AreEqual(@"Complex delim counter: 10", counterCmplx);
			counterCmplx = evalCmplx.EvaluateString("Counter1: <Delim>Counter::CounterName1:: = 10</Delim>");
			Assert.AreEqual("Counter1: 10", counterCmplx);
			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::=10}");
			Assert.AreEqual("Counter1: 10", counterN1);

			counterCmplx = evalCmplx.EvaluateString("Complex delim counter: <Delim>Counter::Complex Delim:: + 10</Delim>");
			Assert.AreEqual(@"Complex delim counter: 20", counterCmplx);
			counterCmplx = evalCmplx.EvaluateString("Counter1: <Delim>Counter::CounterName1::+10</Delim>");
			Assert.AreEqual("Counter1: 20", counterCmplx);
			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::+ 10}");
			Assert.AreEqual("Counter1: 20", counterN1);

			counterCmplx = evalCmplx.EvaluateString("Complex delim counter: <Delim>Counter::Complex Delim:: - 1</Delim>");
			Assert.AreEqual(@"Complex delim counter: 19", counterCmplx);
			counterCmplx = evalCmplx.EvaluateString("Counter1: <Delim>Counter::CounterName1::-1</Delim>");
			Assert.AreEqual("Counter1: 19", counterCmplx);
			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::-1}");
			Assert.AreEqual("Counter1: 19", counterN1);

			counterCmplx = evalCmplx.EvaluateString("Complex delim counter: <Delim>Counter::Complex Delim:: = -10 </Delim>");
			Assert.AreEqual(@"Complex delim counter: -10", counterCmplx);
			counterCmplx = evalCmplx.EvaluateString("Counter1: <Delim>Counter::CounterName1:: = - 10 </Delim>");
			Assert.AreEqual("Counter1: -10", counterCmplx);
			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1:: = -10 }");
			Assert.AreEqual("Counter1: -10", counterN1);

			counterCmplx = evalCmplx.EvaluateString("Complex delim counter: <Delim>Counter::Complex Delim:: + -10 </Delim>");
			Assert.AreEqual(@"Complex delim counter: -20", counterCmplx);
			counterCmplx = evalCmplx.EvaluateString("Counter1: <Delim>Counter::CounterName1::+-10</Delim>");
			Assert.AreEqual("Counter1: -20", counterCmplx);
			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::+ -10}");
			Assert.AreEqual("Counter1: -20", counterN1);

			counterCmplx = evalCmplx.EvaluateString("Complex delim counter: <Delim>Counter::Complex Delim:: - - 1 </Delim>");
			Assert.AreEqual(@"Complex delim counter: -19", counterCmplx);
			counterCmplx = evalCmplx.EvaluateString("Counter1: <Delim>Counter::CounterName1::--1 </Delim>");
			Assert.AreEqual("Counter1: -19", counterCmplx);
			counterN1 = eval.EvaluateString("Counter1: {Counter::CounterName1::- -1 }");
			Assert.AreEqual("Counter1: -19", counterN1);
		}

		/// <summary>
		/// Will work only if Option1 compiler-directive in the TestEvaluation project is set
		/// </summary>
		[TestMethod()]
		public void TestCounter2()
		{
			// Possibility 1: Know how to handle trailing colon internally
			var cxt1 = new List<IProcessEvaluate>();
			cxt1.Add(new ProcessCounter());
			var eval1 = new EnhancedStringEval(cxt1);
			string test1 = eval1.EvaluateString("c1: {Counter::Name:::Init}");
			Assert.AreEqual("c1: 0", test1);

			test1 = eval1.EvaluateString("c1: {Counter::Name:}");
			Assert.AreEqual("c1: 1", test1);

			test1 = eval1.EvaluateString("Name without colon trailing is a different name: {Counter::Name}");
			Assert.AreEqual("Name without colon trailing is a different name: 0", test1);

			test1 = eval1.EvaluateString("Name ending with a space: {Counter::Name: }");
			Assert.AreEqual("Name ending with a space: 0", test1);

			// Possibility 2: handle colon specially in an inherited class
			var cxt2 = new List<IProcessEvaluate>();
			var counter = new ProcessCounter { PosibilityOption = Possibility.Substitution };
			cxt2.Add(counter);
			var eval2 = new EnhStringTrailingColon(cxt2);
			string test2 = eval2.EvaluateString("c2: {Counter::Name:::Init}");
			Assert.AreEqual("c2: 0", test2);

			test2 = eval2.EvaluateString("c2: {Counter::Name:}");
			Assert.AreEqual("c2: 1", test2);

			test2 = eval2.EvaluateString("Name without colon trailing is a different name: {Counter::Name}");
			Assert.AreEqual("Name without colon trailing is a different name: 0", test2);

			test2 = eval2.EvaluateString("Name ending with a space: {Counter::Name: }");
			Assert.AreEqual("Name ending with a space: 0", test2);
		}

		[TestMethod]
		public void TestCurrentDir()
		{
			var context = new List<IProcessEvaluate>();
			var currDir = new ProcessCurrentDir();
			context.Add(currDir);
			context.Add(new ProcessCounter());

			currDir.CurrentDir = @"C:\Accounting";
			var eval = new EnhancedStringEval(context);
			string dir1 = eval.EvaluateString("CurrDir: {CurrentDir::}");
			Assert.AreEqual(@"CurrDir: C:\Accounting", dir1);

			dir1 = eval.EvaluateString("CurrDir: {CurrentDir::-0}");
			Assert.AreEqual(@"CurrDir: C:\Accounting", dir1);

			dir1 = eval.EvaluateString("CurrDir: {CurrentDir::-1}");
			Assert.AreEqual(@"CurrDir: C:\", dir1);

			dir1 = eval.EvaluateString("CurrDir: {CurrentDir::-2}");
			Assert.AreEqual(@"CurrDir: --\\no path at given count: -2\\--", dir1);

			currDir.CurrentDir = Path.Combine(currDir.CurrentDir, "Book1");
			string dir2 = eval.EvaluateString("CurrDir: {CurrentDir::}");
			Assert.AreEqual(@"CurrDir: C:\Accounting\Book1", dir2);

			currDir.CurrentDir = Path.Combine(currDir.CurrentDir, "{Counter::Dir}");
			string dirR = eval.EvaluateString("CurrDir: {CurrentDir::}");
			Assert.AreEqual(@"CurrDir: C:\Accounting\Book1\0", dirR);

			dirR = eval.EvaluateString("CurrDir: {CurrentDir::}");
			Assert.AreEqual(@"CurrDir: C:\Accounting\Book1\1", dirR);

			dirR = eval.EvaluateString("CurrDir: {CurrentDir::}");
			Assert.AreEqual(@"CurrDir: C:\Accounting\Book1\2", dirR);

			//
			/////   Different delimiters
			/////   C suffix for Complex-delimiter
			//

			IDelimitersAndSeparator delim = new DelimitersAndSeparator("#{", "}");
			var contextC = new List<IProcessEvaluate>();
			var currDirC = new ProcessCurrentDir(delim);
			contextC.Add(currDirC);
			contextC.Add(new ProcessCounter(delim));

			currDirC.CurrentDir = @"C:\Desk";
			var evalC = new EnhancedStringEval(contextC, delim);
			string dirC1 = evalC.EvaluateString("CurrDir: #{CurrentDir::}");
			Assert.AreEqual(@"CurrDir: C:\Desk", dirC1);

			dirC1 = evalC.EvaluateString("CurrDir: #{CurrentDir::-0}");
			Assert.AreEqual(@"CurrDir: C:\Desk", dirC1);

			dirC1 = evalC.EvaluateString("CurrDir: #{CurrentDir::-1}");
			Assert.AreEqual(@"CurrDir: C:\", dirC1);

			dirC1 = evalC.EvaluateString("CurrDir: #{CurrentDir::-2}");
			Assert.AreEqual(@"CurrDir: --\\no path at given count: -2\\--", dirC1);

			currDirC.CurrentDir = Path.Combine(currDirC.CurrentDir, "Book1");
			string dirC2 = evalC.EvaluateString("CurrDir: #{CurrentDir::}");
			Assert.AreEqual(@"CurrDir: C:\Desk\Book1", dirC2);

			currDirC.CurrentDir = Path.Combine(currDirC.CurrentDir, "#{Counter::Dir}");
			string dirCR = evalC.EvaluateString("CurrDir: #{CurrentDir::}");
			Assert.AreEqual(@"CurrDir: C:\Desk\Book1\0", dirCR);

			dirCR = evalC.EvaluateString("CurrDir: #{CurrentDir::}");
			Assert.AreEqual(@"CurrDir: C:\Desk\Book1\1", dirCR);

			dirCR = evalC.EvaluateString("CurrDir: #{CurrentDir::}");
			Assert.AreEqual(@"CurrDir: C:\Desk\Book1\2", dirCR);
		}

		[TestMethod]
		public void TestCurrentTime()
		{
			var context = new List<IProcessEvaluate>();
			context.Add(new ProcessCurrentTime());
			var eval = new EnhancedStringEval(context);
			string ct = eval.EvaluateString("{CurrentTime::yyyyMMddHHmmssfff}");
			DateTime dtNow = DateTime.Now;
			int yr = int.Parse(ct.Substring(0, 4));
			int mo = int.Parse(ct.Substring(4, 2));
			int dy = int.Parse(ct.Substring(6, 2));
			int hr = int.Parse(ct.Substring(8, 2));
			int mi = int.Parse(ct.Substring(10, 2));
			int sc = int.Parse(ct.Substring(12, 2));
			int ms = int.Parse(ct.Substring(14, 3));
			DateTime dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			TimeSpan span = dtNow - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ct = eval.EvaluateString("{CurrentTime::yyyyMMddHHmmssfff::}");
			dtNow = DateTime.Now;
			yr = int.Parse(ct.Substring(0, 4));
			mo = int.Parse(ct.Substring(4, 2));
			dy = int.Parse(ct.Substring(6, 2));
			hr = int.Parse(ct.Substring(8, 2));
			mi = int.Parse(ct.Substring(10, 2));
			sc = int.Parse(ct.Substring(12, 2));
			ms = int.Parse(ct.Substring(14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = dtNow - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ct = eval.EvaluateString("{CurrentTime::yyyy/MM/dd HH:mm:ss.fff:: +2 y}");
			dtNow = DateTime.Now;
			yr = dtNow.Year;
			mo = dtNow.Month;
			dy = dtNow.Day;
			hr = dtNow.Hour;
			mi = dtNow.Minute;
			sc = dtNow.Second;
			ms = dtNow.Millisecond;
			DateTime next2Yr = new DateTime(yr + 2, mo, dy, hr, mi, sc, ms);
			yr = int.Parse(ct.Substring(0, 4));		// yyyy -- /MM/dd HH:mm:ss.fff
			mo = int.Parse(ct.Substring(5, 2));		// MM   -- /dd HH:mm:ss.fff
			dy = int.Parse(ct.Substring(8, 2));		// dd   --  HH:mm:ss.fff
			hr = int.Parse(ct.Substring(11, 2));	// HH   -- :mm:ss.fff
			mi = int.Parse(ct.Substring(14, 2));	// mm   -- :ss.fff
			sc = int.Parse(ct.Substring(17, 2));	// ss   -- .fff
			ms = int.Parse(ct.Substring(20, 3));	// fff  -- 
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = next2Yr - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ct = eval.EvaluateString("{CurrentTime::yyyyMMddHHmmssfff:: +2 M}");
			dtNow = DateTime.Now;
			yr = dtNow.Year;
			mo = dtNow.Month;
			if (mo == 11) { mo = 1; ++yr; }
			else if (mo == 12) { mo = 2; ++yr; }
			else mo += 2;
			dy = dtNow.Day;
			hr = dtNow.Hour;
			mi = dtNow.Minute;
			sc = dtNow.Second;
			ms = dtNow.Millisecond;
			DateTime next2mos = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			yr = int.Parse(ct.Substring(0, 4));
			mo = int.Parse(ct.Substring(4, 2));
			dy = int.Parse(ct.Substring(6, 2));
			hr = int.Parse(ct.Substring(8, 2));
			mi = int.Parse(ct.Substring(10, 2));
			sc = int.Parse(ct.Substring(12, 2));
			ms = int.Parse(ct.Substring(14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = next2mos - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ct = eval.EvaluateString("{CurrentTime::yyyyMMddHHmmssfff::+3w}");
			DateTime next3wks = DateTime.Now + TimeSpan.FromDays(21.0);
			dtNow = DateTime.Now;
			yr = int.Parse(ct.Substring(0, 4));
			mo = int.Parse(ct.Substring(4, 2));
			dy = int.Parse(ct.Substring(6, 2));
			hr = int.Parse(ct.Substring(8, 2));
			mi = int.Parse(ct.Substring(10, 2));
			sc = int.Parse(ct.Substring(12, 2));
			ms = int.Parse(ct.Substring(14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = next3wks - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ct = eval.EvaluateString("{CurrentTime::yyyy/MM/dd HH:mm:ss.fff:: +1 d}");
			DateTime tomorrow = DateTime.Now + TimeSpan.FromDays(1.0);
			yr = int.Parse(ct.Substring(0, 4));		// yyyy -- /MM/dd HH:mm:ss.fff
			mo = int.Parse(ct.Substring(5, 2));		// MM   -- /dd HH:mm:ss.fff
			dy = int.Parse(ct.Substring(8, 2));		// dd   --  HH:mm:ss.fff
			hr = int.Parse(ct.Substring(11, 2));	// HH   -- :mm:ss.fff
			mi = int.Parse(ct.Substring(14, 2));	// mm   -- :ss.fff
			sc = int.Parse(ct.Substring(17, 2));	// ss   -- .fff
			ms = int.Parse(ct.Substring(20, 3));	// fff  -- 
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = tomorrow - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ct = eval.EvaluateString("{CurrentTime::yyyyMMddHHmmssfff:: +5 h}");
			DateTime next5hrs = DateTime.Now + TimeSpan.FromHours(5.0);
			dtNow = DateTime.Now;
			yr = int.Parse(ct.Substring(0, 4));
			mo = int.Parse(ct.Substring(4, 2));
			dy = int.Parse(ct.Substring(6, 2));
			hr = int.Parse(ct.Substring(8, 2));
			mi = int.Parse(ct.Substring(10, 2));
			sc = int.Parse(ct.Substring(12, 2));
			ms = int.Parse(ct.Substring(14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = next5hrs - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ct = eval.EvaluateString("{CurrentTime::yyyyMMddHHmmssfff:: +6 m}");
			DateTime next6mins = DateTime.Now + TimeSpan.FromMinutes(6.0);
			dtNow = DateTime.Now;
			yr = int.Parse(ct.Substring(0, 4));
			mo = int.Parse(ct.Substring(4, 2));
			dy = int.Parse(ct.Substring(6, 2));
			hr = int.Parse(ct.Substring(8, 2));
			mi = int.Parse(ct.Substring(10, 2));
			sc = int.Parse(ct.Substring(12, 2));
			ms = int.Parse(ct.Substring(14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = next6mins - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ct = eval.EvaluateString("{CurrentTime::yyyyMMddHHmmssfff:: +7 s}");
			DateTime next7secs = DateTime.Now + TimeSpan.FromSeconds(7.0);
			dtNow = DateTime.Now;
			yr = int.Parse(ct.Substring(0, 4));
			mo = int.Parse(ct.Substring(4, 2));
			dy = int.Parse(ct.Substring(6, 2));
			hr = int.Parse(ct.Substring(8, 2));
			mi = int.Parse(ct.Substring(10, 2));
			sc = int.Parse(ct.Substring(12, 2));
			ms = int.Parse(ct.Substring(14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = next7secs - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			//
			/////   Different delimiters
			/////   C suffix for Complex-delimiter
			//

			IDelimitersAndSeparator delim = new DelimitersAndSeparator("%<", ">%");
			var contextC = new List<IProcessEvaluate>();
			contextC.Add(new ProcessCurrentTime(delim));
			var evalC = new EnhancedStringEval(contextC, delim);
			ct = evalC.EvaluateString("%<CurrentTime::yyyyMMddHHmmssfff>%");
			dtNow = DateTime.Now;
			yr = int.Parse(ct.Substring(0, 4));
			mo = int.Parse(ct.Substring(4, 2));
			dy = int.Parse(ct.Substring(6, 2));
			hr = int.Parse(ct.Substring(8, 2));
			mi = int.Parse(ct.Substring(10, 2));
			sc = int.Parse(ct.Substring(12, 2));
			ms = int.Parse(ct.Substring(14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = dtNow - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ct = evalC.EvaluateString("%<CurrentTime::dd/MM/yyyy HH:mm:ss.fff::-1y>%");
			dtNow = DateTime.Now;
			yr = dtNow.Year;
			mo = dtNow.Month;
			dy = dtNow.Day;
			hr = dtNow.Hour;
			mi = dtNow.Minute;
			sc = dtNow.Second;
			ms = dtNow.Millisecond;
			DateTime lastYr = new DateTime(yr - 1, mo, dy, hr, mi, sc, ms);
			dy = int.Parse(ct.Substring(0, 2));
			mo = int.Parse(ct.Substring(3, 2));
			yr = int.Parse(ct.Substring(6, 4));
			hr = int.Parse(ct.Substring(11, 2));
			mi = int.Parse(ct.Substring(14, 2));
			sc = int.Parse(ct.Substring(17, 2));
			ms = int.Parse(ct.Substring(20, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = lastYr - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ct = evalC.EvaluateString("%<CurrentTime::yyyyMMddHHmmssfff::-1m>%");
			dtNow = DateTime.Now;
			yr = dtNow.Year;
			mo = dtNow.Month;
			dy = dtNow.Day;
			hr = dtNow.Hour;
			mi = dtNow.Minute;
			sc = dtNow.Second;
			ms = dtNow.Millisecond;
			if (mo == 1) { mo = 12; --yr; }
			else --mo;
			DateTime lastMo = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			yr = int.Parse(ct.Substring(0, 4));
			mo = int.Parse(ct.Substring(4, 2));
			dy = int.Parse(ct.Substring(6, 2));
			hr = int.Parse(ct.Substring(8, 2));
			mi = int.Parse(ct.Substring(10, 2));
			sc = int.Parse(ct.Substring(12, 2));
			ms = int.Parse(ct.Substring(14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = lastMo - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ct = evalC.EvaluateString("%<CurrentTime::yyyy/MM/dd HH:mm:ss.fff::-1d>%");
			DateTime yesterday = DateTime.Now - TimeSpan.FromDays(1.0);
			yr = int.Parse(ct.Substring(0, 4));		// yyyy -- /MM/dd HH:mm:ss.fff
			mo = int.Parse(ct.Substring(5, 2));		// MM   -- /dd HH:mm:ss.fff
			dy = int.Parse(ct.Substring(8, 2));		// dd   --  HH:mm:ss.fff
			hr = int.Parse(ct.Substring(11, 2));	// HH   -- :mm:ss.fff
			mi = int.Parse(ct.Substring(14, 2));	// mm   -- :ss.fff
			sc = int.Parse(ct.Substring(17, 2));	// ss   -- .fff
			ms = int.Parse(ct.Substring(20, 3));	// fff  -- 
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = yesterday - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ct = evalC.EvaluateString("%<CurrentTime::yyyyMMddHHmmssfff::-2w>%");
			DateTime TwoWksAgo = DateTime.Now - TimeSpan.FromDays(14.0);
			yr = int.Parse(ct.Substring(0, 4));
			mo = int.Parse(ct.Substring(4, 2));
			dy = int.Parse(ct.Substring(6, 2));
			hr = int.Parse(ct.Substring(8, 2));
			mi = int.Parse(ct.Substring(10, 2));
			sc = int.Parse(ct.Substring(12, 2));
			ms = int.Parse(ct.Substring(14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = TwoWksAgo - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ct = evalC.EvaluateString("%<CurrentTime::yyyyMMddHHmmssfff::-3h>%");
			DateTime ThreeHrsAgo = DateTime.Now - TimeSpan.FromHours(3.0);
			yr = int.Parse(ct.Substring(0, 4));
			mo = int.Parse(ct.Substring(4, 2));
			dy = int.Parse(ct.Substring(6, 2));
			hr = int.Parse(ct.Substring(8, 2));
			mi = int.Parse(ct.Substring(10, 2));
			sc = int.Parse(ct.Substring(12, 2));
			ms = int.Parse(ct.Substring(14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = ThreeHrsAgo - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ct = evalC.EvaluateString("%<CurrentTime::yyyyMMddHHmmssfff:: - 4 m >%");
			DateTime FourMinsAgo = DateTime.Now - TimeSpan.FromMinutes(4.0);
			yr = int.Parse(ct.Substring(0, 4));
			mo = int.Parse(ct.Substring(4, 2));
			dy = int.Parse(ct.Substring(6, 2));
			hr = int.Parse(ct.Substring(8, 2));
			mi = int.Parse(ct.Substring(10, 2));
			sc = int.Parse(ct.Substring(12, 2));
			ms = int.Parse(ct.Substring(14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = FourMinsAgo - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ct = evalC.EvaluateString("%<CurrentTime::yyyyMMddHHmmssfff:: -5 s >%");
			DateTime FiveSecsAgo = DateTime.Now - TimeSpan.FromSeconds(5.0);
			yr = int.Parse(ct.Substring(0, 4));
			mo = int.Parse(ct.Substring(4, 2));
			dy = int.Parse(ct.Substring(6, 2));
			hr = int.Parse(ct.Substring(8, 2));
			mi = int.Parse(ct.Substring(10, 2));
			sc = int.Parse(ct.Substring(12, 2));
			ms = int.Parse(ct.Substring(14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = FiveSecsAgo - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);
		}

		[TestMethod]
		public void TestCurrentDate()
		{
			var context = new List<IProcessEvaluate>();
			context.Add(new ProcessDate());
			DateTime today = DateTime.Today;
			var eval = new EnhancedStringEval(context);
			string sDt = eval.EvaluateString("{Date::yyyy-MM-dd}");
			int yr = int.Parse(sDt.Substring(0, 4));
			int mo = int.Parse(sDt.Substring(5, 2));
			int dy = int.Parse(sDt.Substring(8, 2));
			var dEval = new DateTime(yr, mo, dy);
			TimeSpan span = today - dEval;
			Assert.AreEqual(true, span.TotalDays <= 1);

			//
			/////   Different delimiters
			/////   C suffix for Complex-delimiter
			//

			IDelimitersAndSeparator delim = new DelimitersAndSeparator("<begin>", "<end>");
			var contextC = new List<IProcessEvaluate>();
			contextC.Add(new ProcessDate(delim));
			today = DateTime.Today;
			var evalC = new EnhancedStringEval(contextC, delim);
			sDt = evalC.EvaluateString("<begin>Date::yyyy-MM-dd<end>");
			yr = int.Parse(sDt.Substring(0, 4));
			mo = int.Parse(sDt.Substring(5, 2));
			dy = int.Parse(sDt.Substring(8, 2));
			dEval = new DateTime(yr, mo, dy);
			span = today - dEval;
			Assert.AreEqual(true, span.TotalDays <= 1);
		}

		[TestMethod]
		public void TestForeignKey()
		{
			string tmpPath = Path.GetTempPath();
			string fileNm = System.Reflection.MethodBase.GetCurrentMethod().Name;
			string filePath = Path.Combine(tmpPath, fileNm + ".txt");
			using (var fs = new FileStream(filePath, FileMode.Create))
			using (var sw = new StreamWriter(fs))
			{
				sw.WriteLine("Key1=First Simple Key");
				sw.WriteLine("Key2=Second Key: {Counter::Simple Key}");
				sw.WriteLine("Key3=Third key on {CurrentTime::dd/MM/yyyy hh:mm:ss.fff tt}, {Counter::Simple Key}");
			}

			var context = new List<IProcessEvaluate>();
			context.Add(new ProcessCounter());
			context.Add(new ProcessCurrentTime());
			context.Add(new ProcessForeignKey());
			var eval = new EnhancedStringEval(context);
			
			string key1 = string.Format("Key1:  {{ForeignKey::{0}::Key1}}", filePath);
			string key1eval = eval.EvaluateString(key1);
			Assert.AreEqual("Key1:  First Simple Key", key1eval);

			string key2 = string.Format("Key2:  {{ForeignKey::{0}::Key2}}", filePath);
			string key2eval = eval.EvaluateString(key2);
			Assert.AreEqual("Key2:  Second Key: 0", key2eval);

			string key3 = string.Format("Key3:  {{ForeignKey::{0}::Key3}}", filePath);
			string key3eval = eval.EvaluateString(key3);
			Assert.AreEqual(true, key3eval.StartsWith("Key3:  Third key on "));
			Assert.AreEqual(true, key3eval.EndsWith(", 1"));

			File.Delete(filePath);

			//
			/////   Different delimiters
			/////   C suffix for keeping up with tradition
			//

			using (var fs = new FileStream(filePath, FileMode.Create))
			using (var sw = new StreamWriter(fs))
			{
				sw.WriteLine("Key1=First Simple Key");
				sw.WriteLine("Key2=Second Key: <Counter::Simple Key>");
				sw.WriteLine("Key3=Third key on <CurrentTime::dd/MM/yyyy hh:mm:ss.fff tt>, <Counter::Simple Key>");
			}

			IDelimitersAndSeparator delim = new DelimitersAndSeparator("<", ">");
			var contextC = new List<IProcessEvaluate>();
			contextC.Add(new ProcessCounter(delim));
			contextC.Add(new ProcessCurrentTime(delim));
			contextC.Add(new ProcessForeignKey(delim));
			var evalC = new EnhancedStringEval(contextC, delim);

			key1 = string.Format("Key1:  <ForeignKey::{0}::Key1>", filePath);
			key1eval = evalC.EvaluateString(key1);
			Assert.AreEqual("Key1:  First Simple Key", key1eval);

			key2 = string.Format("Key2:  <ForeignKey::{0}::Key2>", filePath);
			key2eval = evalC.EvaluateString(key2);
			Assert.AreEqual("Key2:  Second Key: 0", key2eval);

			key3 = string.Format("Key3:  <ForeignKey::{0}::Key3>", filePath);
			key3eval = evalC.EvaluateString(key3);
			Assert.AreEqual(true, key3eval.StartsWith("Key3:  Third key on "));
			Assert.AreEqual(true, key3eval.EndsWith(", 1"));

			File.Delete(filePath);
		}

		[TestMethod]
		public void TestIf()
		{
			var context = new List<IProcessEvaluate>();
			context.Add(new ProcessCounter());
			var currDir = new ProcessCurrentDir();
			context.Add(currDir);
			context.Add(new ProcessCurrentTime());
			context.Add(new ProcessForeignKey());
			context.Add(new ProcessIf());
			var eval = new EnhancedStringEval(context);

			string tmpPath = Path.GetTempPath();
			string fileNm = System.Reflection.MethodBase.GetCurrentMethod().Name;
			string filePath = Path.Combine(tmpPath, fileNm + ".txt");
			using (var fs = new FileStream(filePath, FileMode.Create))
			using (var sw = new StreamWriter(fs))
			{
				sw.WriteLine("Key1=First Simple Key");
				sw.WriteLine("Key2=Second Key: {Counter::Simple Key}");
				sw.WriteLine("Key3=Third key on {CurrentTime::dd/MM/yyyy hh:mm:ss.fff tt}, {Counter::Simple Key}");
				sw.WriteLine("Key4={if::{CurrentTime::dd/MM/yyyy hh:mm:ss.fff tt} = {CurrentTime::dd/MM/yyyy hh:mm:ss.fff tt::-3d}::Yes::No}");
				sw.WriteLine("Key5={if::{Counter::Simple key} = 2::2-{CurrentDir::}::No-{CurrentDir::}}");
			}

			string key = string.Format("{{ForeignKey::{0}::Key4}}", filePath);
			string e1 = eval.EvaluateString(key);
			Assert.AreEqual("No", e1);

			currDir.CurrentDir = @"C:\";
			key = string.Format("{{ForeignKey::{0}::Key5}}", filePath);
			e1 = eval.EvaluateString(key);
			Assert.AreEqual(@"No-C:\", e1);

			currDir.CurrentDir = @"C:\abc\";
			e1 = eval.EvaluateString(key);
			Assert.AreEqual(@"No-C:\abc\", e1);

			currDir.CurrentDir = @"C:\abc\def";
			e1 = eval.EvaluateString(key);
			Assert.AreEqual(@"2-C:\abc\def", e1);

			key = string.Format("{{if::DirectoryExists({0})::Yes::No}}", tmpPath);
			string e2 = eval.EvaluateString(key);
			Assert.AreEqual("Yes", e2);

			key = string.Format("{{if::FileExists({0})::Yes::No}}", filePath);
			string e3 = eval.EvaluateString(key);
			Assert.AreEqual("Yes", e3);

			File.Delete(filePath);

			//
			/////   Different delimiters
			/////   C suffix for Complex-delimiter
			//

			IDelimitersAndSeparator delim = new DelimitersAndSeparator("$<", ">$");
			var contextC = new List<IProcessEvaluate>();
			contextC.Add(new ProcessCounter(delim));
			var currDirC = new ProcessCurrentDir(delim);
			contextC.Add(currDirC);
			contextC.Add(new ProcessCurrentTime(delim));
			contextC.Add(new ProcessForeignKey(delim));
			contextC.Add(new ProcessIf(delim));
			var evalC = new EnhancedStringEval(contextC, delim);

			if (tmpPath.EndsWith("\\")) tmpPath = tmpPath.Substring(0, tmpPath.Length - 1);
			using (var fs = new FileStream(filePath, FileMode.Create))
			using (var sw = new StreamWriter(fs))
			{
				sw.WriteLine("Key1=First Simple Key");
				sw.WriteLine("Key2=Second Key: $<Counter::Simple Key>$");
				sw.WriteLine("Key3=Third key on $<CurrentTime::dd/MM/yyyy hh:mm:ss.fff tt>$, $<Counter::Simple Key>$");
				sw.WriteLine("Key4=$<if::$<CurrentTime::dd/MM/yyyy>$ = $<CurrentTime::dd/MM/yyyy::-3d>$::Yes::No>$");
				sw.WriteLine("Key5=$<if::$<Counter::Simple key>$ = 2::2-$<CurrentDir::>$::No-$<CurrentDir::>$>$");
			}

			key = string.Format("$<ForeignKey::{0}::Key4>$", filePath);
			e1 = evalC.EvaluateString(key);
			Assert.AreEqual("No", e1);

			currDirC.CurrentDir = @"D:\";
			key = string.Format("$<ForeignKey::{0}::Key5>$", filePath);
			e1 = evalC.EvaluateString(key);
			Assert.AreEqual(@"No-D:\", e1);

			currDirC.CurrentDir = @"D:\abc\";
			e1 = evalC.EvaluateString(key);
			Assert.AreEqual(@"No-D:\abc\", e1);

			currDirC.CurrentDir = @"D:\abc\def";
			e1 = evalC.EvaluateString(key);
			Assert.AreEqual(@"2-D:\abc\def", e1);

			key = string.Format("$<if::DirectoryExists({0})::Yes::No>$", tmpPath);
			e2 = evalC.EvaluateString(key);
			Assert.AreEqual("Yes", e2);

			key = string.Format("$<if ::FileExists({0}):: Yes:: No>$", filePath);
			e3 = evalC.EvaluateString(key);
			Assert.AreEqual("Yes", e3);

			File.Delete(filePath);
		}

		[TestMethod]
		public void TestKey()
		{
			var elems = new Dictionary<string, string>();
			elems.Add("Flat", @"testing {Counter::page}");
			elems.Add("Static flat", @"Evaluation of flat: {key::flat}");
			elems.Add("Flat indirect", "Flat");
			elems.Add("Flat2", "Eval2 of flat: {key::{Key::Flat indirect}}");
			elems.Add("Static date", @"Evaluation of date: {date::yyyy.MM.dd}");
			elems.Add("Current directory", @"current dir: {CurrentDir::}");
			elems.Add("temp", @"SpecialDirectory\{key::flat}\{key::Stamp}");
			elems.Add("Current path", @"{key::current directory}\{key::temp}");
			elems.Add("Stamp", @"{CurrentTime::yyyyMMddHHmmssfff::-1d}");
			elems.Add("CompoundStamp", "{CurrentTime::{Key::fmt1}::{Key::DaysAgo}}");
			elems.Add("CompoundStamp2", "{CurrentTime::{Key::fmt2}::{Key::DaysAgo}}");
			elems.Add("fmt1", "yyyyMMddHHmmssfff");
			elems.Add("DaysAgo", "-1d");
			elems.Add("fmt2", "dd/MM/yyyy");

			// handling
			var context = new List<IProcessEvaluate>();
			context.Add(new ProcessCounter());
			var currDir = new ProcessCurrentDir();
			context.Add(currDir);
			context.Add(new ProcessCurrentTime());
			context.Add(new ProcessDate());
			context.Add(new ProcessForeignKey());
			context.Add(new ProcessKey(elems));
			context.Add(new ProcessIf());
			var eval = new EnhancedStringEval(context);

			string ev = eval.EvaluateString("{key::Flat}");
			Assert.AreEqual("testing 0", ev);

			ev = eval.EvaluateString("{key::Static flat}");
			Assert.AreEqual("Evaluation of flat: testing 1", ev);

			ev = eval.EvaluateString("{key::flat indirect}");
			Assert.AreEqual("Flat", ev);

			ev = eval.EvaluateString("{key::{key::flat indirect}}");
			Assert.AreEqual("testing 2", ev);

			ev = eval.EvaluateString("{key::Static {key::flat indirect}}");
			Assert.AreEqual("Evaluation of flat: testing 3", ev);

			ev = eval.EvaluateString("{key::{key::flat indirect}2}");
			Assert.AreEqual("Eval2 of flat: testing 4", ev);

			ev = eval.EvaluateString("{key::Static date}");
			DateTime now = DateTime.Now;
			int yr = int.Parse(ev.Substring(20, 4));
			int mo = int.Parse(ev.Substring(25, 2));
			int dy = int.Parse(ev.Substring(28, 2));
			TimeSpan ts = new DateTime(yr, mo, dy) - now;
			Assert.AreEqual(true, ts.TotalDays <= 1.0);

			currDir.CurrentDir = @"C:\";
			ev = eval.EvaluateString("{key::current directory}");
			Assert.AreEqual(@"current dir: C:\", ev);

			currDir.CurrentDir = @"C:\Bin";
			ev = eval.EvaluateString("{key::current directory}");
			Assert.AreEqual(@"current dir: C:\Bin", ev);

			ev = eval.EvaluateString("{key::temp}");
			DateTime yesterday = DateTime.Now - TimeSpan.FromDays(1.0);
			int inx = @"SpecialDirectory\testing 5\".Length;
			Assert.AreEqual(@"SpecialDirectory\testing 5\", ev.Substring(0, inx));
			yr = int.Parse(ev.Substring(inx, 4));
			mo = int.Parse(ev.Substring(inx + 4, 2));
			dy = int.Parse(ev.Substring(inx + 6, 2));
			int hr = int.Parse(ev.Substring(inx + 8, 2));
			int mi = int.Parse(ev.Substring(inx + 10, 2));
			int sc = int.Parse(ev.Substring(inx + 12, 2));
			int ms = int.Parse(ev.Substring(inx + 14, 3));
			DateTime dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			TimeSpan span = yesterday - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			currDir.CurrentDir = @"C:\Doc";
			ev = eval.EvaluateString("{key::Current path}");
			yesterday = DateTime.Now - TimeSpan.FromDays(1.0);
			string res = @"current dir: C:\Doc\SpecialDirectory\testing 6\";
			Assert.AreEqual(true, ev.StartsWith(res));
			inx = res.Length;
			yr = int.Parse(ev.Substring(inx, 4));
			mo = int.Parse(ev.Substring(inx + 4, 2));
			dy = int.Parse(ev.Substring(inx + 6, 2));
			hr = int.Parse(ev.Substring(inx + 8, 2));
			mi = int.Parse(ev.Substring(inx + 10, 2));
			sc = int.Parse(ev.Substring(inx + 12, 2));
			ms = int.Parse(ev.Substring(inx + 14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = yesterday - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ev = eval.EvaluateString("{key::CompoundStamp}");
			yesterday = DateTime.Now - TimeSpan.FromDays(1.0);
			yr = int.Parse(ev.Substring(0, 4));
			mo = int.Parse(ev.Substring(4, 2));
			dy = int.Parse(ev.Substring(6, 2));
			hr = int.Parse(ev.Substring(8, 2));
			mi = int.Parse(ev.Substring(10, 2));
			sc = int.Parse(ev.Substring(12, 2));
			ms = int.Parse(ev.Substring(14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = yesterday - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ev = eval.EvaluateString("{key::CompoundStamp2}");
			yesterday = DateTime.Now - TimeSpan.FromDays(1.0);
			dy = int.Parse(ev.Substring(0, 2));
			mo = int.Parse(ev.Substring(3, 2));
			yr = int.Parse(ev.Substring(6, 4));
			dtEval = new DateTime(yr, mo, dy);
			span = yesterday - dtEval;
			Assert.AreEqual(true, span.TotalDays <= 1);

			//
			/////   Different delimiters
			/////   C suffix for Complex-delimiter
			//

			elems = new Dictionary<string, string>();
			elems.Add("Flat", @"testing ${Counter::page}$");
			elems.Add("Static flat", @"Evaluation of flat: ${key::flat}$");
			elems.Add("Flat indirect", "Flat");
			elems.Add("Flat2", "Eval2 of flat: ${key::${Key::Flat indirect}$}$");
			elems.Add("Static date", @"Evaluation of date: ${date::yyyy.MM.dd}$");
			elems.Add("Current directory", @"current dir: ${CurrentDir::}$");
			elems.Add("temp", @"SpecialDirectory\${key::flat}$\${key::Stamp}$");
			elems.Add("Current path", @"${key::current directory}$\${key::temp}$");
			elems.Add("Stamp", @"${CurrentTime::yyyyMMddHHmmssfff::-1d}$");
			elems.Add("CompoundStamp", "${CurrentTime::${Key::fmt1}$::${Key::DaysAgo}$}$");
			elems.Add("CompoundStamp2", "${CurrentTime::${Key::fmt2}$::${Key::DaysAgo}$}$");
			elems.Add("fmt1", "yyyyMMddHHmmssfff");
			elems.Add("DaysAgo", "-1d");
			elems.Add("fmt2", "dd/MM/yyyy");

			// handling
			IDelimitersAndSeparator delim = new DelimitersAndSeparator("${", "}$");
			context = new List<IProcessEvaluate>();
			context.Add(new ProcessCounter(delim));
			currDir = new ProcessCurrentDir(delim);
			context.Add(currDir);
			context.Add(new ProcessCurrentTime(delim));
			context.Add(new ProcessDate(delim));
			context.Add(new ProcessForeignKey(delim));
			context.Add(new ProcessKey(elems, delim));
			context.Add(new ProcessIf(delim));
			eval = new EnhancedStringEval(context, delim);

			ev = eval.EvaluateString("${key::Flat}$");
			Assert.AreEqual("testing 0", ev);

			ev = eval.EvaluateString("${key::Static flat}$");
			Assert.AreEqual("Evaluation of flat: testing 1", ev);

			ev = eval.EvaluateString("${key::flat indirect}$");
			Assert.AreEqual("Flat", ev);

			ev = eval.EvaluateString("${key::${key::flat indirect}$}$");
			Assert.AreEqual("testing 2", ev);

			ev = eval.EvaluateString("${key::Static ${key::flat indirect}$}$");
			Assert.AreEqual("Evaluation of flat: testing 3", ev);

			ev = eval.EvaluateString("${key::${key::flat indirect}$2}$");
			Assert.AreEqual("Eval2 of flat: testing 4", ev);

			ev = eval.EvaluateString("${key::Static date}$");
			now = DateTime.Now;
			yr = int.Parse(ev.Substring(20, 4));
			mo = int.Parse(ev.Substring(25, 2));
			dy = int.Parse(ev.Substring(28, 2));
			ts = new DateTime(yr, mo, dy) - now;
			Assert.AreEqual(true, ts.TotalDays <= 1.0);

			currDir.CurrentDir = @"C:\";
			ev = eval.EvaluateString("${key::current directory}$");
			Assert.AreEqual(@"current dir: C:\", ev);

			currDir.CurrentDir = @"C:\Bin";
			ev = eval.EvaluateString("${key::current directory}$");
			Assert.AreEqual(@"current dir: C:\Bin", ev);

			ev = eval.EvaluateString("${key::temp}$");
			yesterday = DateTime.Now - TimeSpan.FromDays(1.0);
			inx = @"SpecialDirectory\testing 5\".Length;
			Assert.AreEqual(@"SpecialDirectory\testing 5\", ev.Substring(0, inx));
			yr = int.Parse(ev.Substring(inx, 4));
			mo = int.Parse(ev.Substring(inx + 4, 2));
			dy = int.Parse(ev.Substring(inx + 6, 2));
			hr = int.Parse(ev.Substring(inx + 8, 2));
			mi = int.Parse(ev.Substring(inx + 10, 2));
			sc = int.Parse(ev.Substring(inx + 12, 2));
			ms = int.Parse(ev.Substring(inx + 14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = yesterday - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			currDir.CurrentDir = @"C:\Doc";
			ev = eval.EvaluateString("${key::Current path}$");
			yesterday = DateTime.Now - TimeSpan.FromDays(1.0);
			res = @"current dir: C:\Doc\SpecialDirectory\testing 6\";
			Assert.AreEqual(true, ev.StartsWith(res));
			inx = res.Length;
			yr = int.Parse(ev.Substring(inx, 4));
			mo = int.Parse(ev.Substring(inx + 4, 2));
			dy = int.Parse(ev.Substring(inx + 6, 2));
			hr = int.Parse(ev.Substring(inx + 8, 2));
			mi = int.Parse(ev.Substring(inx + 10, 2));
			sc = int.Parse(ev.Substring(inx + 12, 2));
			ms = int.Parse(ev.Substring(inx + 14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = yesterday - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ev = eval.EvaluateString("${key::CompoundStamp}$");
			yesterday = DateTime.Now - TimeSpan.FromDays(1.0);
			yr = int.Parse(ev.Substring(0, 4));
			mo = int.Parse(ev.Substring(4, 2));
			dy = int.Parse(ev.Substring(6, 2));
			hr = int.Parse(ev.Substring(8, 2));
			mi = int.Parse(ev.Substring(10, 2));
			sc = int.Parse(ev.Substring(12, 2));
			ms = int.Parse(ev.Substring(14, 3));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc, ms);
			span = yesterday - dtEval;
			Assert.AreEqual(true, span.TotalMilliseconds < 50);

			ev = eval.EvaluateString("${key::CompoundStamp2}$");
			yesterday = DateTime.Now - TimeSpan.FromDays(1.0);
			dy = int.Parse(ev.Substring(0, 2));
			mo = int.Parse(ev.Substring(3, 2));
			yr = int.Parse(ev.Substring(6, 4));
			dtEval = new DateTime(yr, mo, dy);
			span = yesterday - dtEval;
			Assert.AreEqual(true, span.TotalDays <= 1);
		}

		[TestMethod]
		public void TestLiteral()
		{
			var elems = new Dictionary<string, string>();
			elems.Add("testing", "{Literal Testing}");
			var cxt = new List<IProcessEvaluate>();
			cxt.Add(new ProcessLiteral());
			cxt.Add(new ProcessKey(elems));
			var eval = new EnhancedStringEval(cxt);
			string ev = eval.EvaluateString("{key::testing}");
			Assert.AreEqual("Literal Testing", ev);

			ev = eval.EvaluateString("{Literal testing without using key processing}");
			Assert.AreEqual("Literal testing without using key processing", ev);

			//
			/////   Different delimiters
			/////   C suffix for Complex-delimiter
			//

			IDelimitersAndSeparator delim = new DelimitersAndSeparator("[Beg]", "[enD]");
			elems = new Dictionary<string, string>();
			elems.Add("testing", @"[Beg]Literal Testing[enD]");
			cxt = new List<IProcessEvaluate>();
			cxt.Add(new ProcessLiteral(delim));
			cxt.Add(new ProcessKey(elems, delim));
			eval = new EnhancedStringEval(cxt, delim);
			ev = eval.EvaluateString("[Beg]key::testing[enD]");
			Assert.AreEqual("Literal Testing", ev);

			ev = eval.EvaluateString("[Beg]Literal testing without using key processing[enD]");
			Assert.AreEqual("Literal testing without using key processing", ev);
		}

		[TestMethod]
		public void TestMemory()
		{
			var elems = new Dictionary<string, string>();
			elems.Add("m1Get", "{Memory::m1}");
			elems.Add("m1Set", "{Memory::m1::432}");
			elems.Add("m2Get", "{Memory::m2}");
			elems.Add("m2Set", "{Memory::m2::234}");

			// handling
			var cxt = new List<IProcessEvaluate>();
			cxt.Add(new ProcessKey(elems));
			cxt.Add(new ProcessMemory());
			var eval = new EnhancedStringEval(cxt);

			var ev = eval.EvaluateString("{key::m1Get}");
			Assert.AreEqual(string.Empty, ev);

			ev = eval.EvaluateString("{key::m1Set}");
			Assert.AreEqual("432", ev);

			ev = eval.EvaluateString("{key::m1Get}");
			Assert.AreEqual("432", ev);

			ev = eval.EvaluateString("{key::m2Set}");
			Assert.AreEqual("234", ev);

			ev = eval.EvaluateString("{key::m2Get}");
			Assert.AreEqual("234", ev);

			ev = eval.EvaluateString("{Memory::FootNote::*}");
			Assert.AreEqual("*", ev);

			ev = eval.EvaluateString("{Memory::FootNote}");
			Assert.AreEqual("*", ev);

			//
			/////   Different delimiters
			/////   C suffix for Complex-delimiter
			//

			elems.Clear();
			elems.Add("m1Get", "<<<Memory::m1>>>");
			elems.Add("m1Set", "<<<Memory::m1::432>>>");
			elems.Add("m2Get", "<<<Memory::m2>>>");
			elems.Add("m2Set", "<<<Memory::m2::234>>>");

			IDelimitersAndSeparator delim = new DelimitersAndSeparator("<<<", ">>>");
			cxt = new List<IProcessEvaluate>();
			cxt.Add(new ProcessKey(elems, delim));
			cxt.Add(new ProcessMemory(delim));
			eval = new EnhancedStringEval(cxt, delim);

			ev = eval.EvaluateString("<<<key::m1Get>>>");
			Assert.AreEqual(string.Empty, ev);

			ev = eval.EvaluateString("<<<key::m1Set>>>");
			Assert.AreEqual("432", ev);

			ev = eval.EvaluateString("<<<key::m1Get>>>");
			Assert.AreEqual("432", ev);

			ev = eval.EvaluateString("<<<key::m2Set>>>");
			Assert.AreEqual("234", ev);

			ev = eval.EvaluateString("<<<key::m2Get>>>");
			Assert.AreEqual("234", ev);

			ev = eval.EvaluateString("<<<Memory::FootNote::*>>>");
			Assert.AreEqual("*", ev);

			ev = eval.EvaluateString("<<<Memory::FootNote>>>");
			Assert.AreEqual("*", ev);
		}

		[TestMethod]
		public void TestSpcialHandlingOfDate()
		{
			IDelimitersAndSeparator delim = new DelimitersAndSeparator("${", "}$");
			var ctx = new List<IProcessEvaluate>();
			ctx.Add(new ProcessCounter(delim));
			ctx.Add(new ProcessCurrentTime(delim));
			ctx.Add(new ProcessMemory(delim));
			ctx.Add(new ProcessLiteral(delim));
			ctx.Add(new ProcessForeignKey(delim));
			var eval = new EnhancedStringEvalNoyyyMMdd(ctx, delim);

			string fmtD = eval.EvaluateString("#{Memory::fmtD::dd/MM/yyyy}#");
			string fmtT = eval.EvaluateString("#{Memory::fmtT::HH:mm:ss}#");
			string ev = eval.EvaluateString("#{CurrentTime::#{Memory::fmtD}# #{Memory::fmtT}#::-#{Counter::c1}#d}#");
			DateTime today = DateTime.Now;

			int dy = int.Parse(ev.Substring(0, 2));
			int mo = int.Parse(ev.Substring(3, 2));
			int yr = int.Parse(ev.Substring(6, 4));
			int hr = int.Parse(ev.Substring(11, 2));
			int mi = int.Parse(ev.Substring(14, 2));
			int sc = int.Parse(ev.Substring(17, 2));
			DateTime dtEval = new DateTime(yr, mo, dy, hr, mi, sc);
			TimeSpan span = today - dtEval;
			Assert.AreEqual(true, span.TotalSeconds < 1.0);

			fmtD = eval.EvaluateString("${Memory::fmtD::dd/MM/yyyy}$");
			fmtT = eval.EvaluateString("${Memory::fmtT::HH:mm:ss}$");
			ev = eval.EvaluateString("#{CurrentTime::#{Memory::fmtD}# #{Memory::fmtT}#::-#{Counter::c1}#d}#");
			DateTime yesterday = DateTime.Now - TimeSpan.FromDays(1.0);
			dy = int.Parse(ev.Substring(0, 2));
			mo = int.Parse(ev.Substring(3, 2));
			yr = int.Parse(ev.Substring(6, 4));
			hr = int.Parse(ev.Substring(11, 2));
			mi = int.Parse(ev.Substring(14, 2));
			sc = int.Parse(ev.Substring(17, 2));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc);
			span = yesterday - dtEval;
			Assert.AreEqual(true, span.TotalSeconds < 1.0);

			string tmpPath = Path.GetTempPath();
			string fileNm = System.Reflection.MethodBase.GetCurrentMethod().Name;
			string filePath = Path.Combine(tmpPath, fileNm + ".txt");
			using (var fs = new FileStream(filePath, FileMode.Create))
			using (var sw = new StreamWriter(fs))
			{
				sw.WriteLine("fmtD=#{dd/MM/yyyy}#");
				sw.WriteLine("fmtT=#{HH:mm:ss}#");
			}

			fmtD = string.Format("#{{ForeignKey::{0}::fmtD}}#", filePath);
			fmtT = string.Format("#{{ForeignKey::{0}::fmtT}}#", filePath);
			var evalStr = string.Format("#{{CurrentTime::{0} {1}::-#{{Counter::c1}}#d}}#", fmtD, fmtT);
			ev = eval.EvaluateString(evalStr);
			DateTime TwoDaysAgo = DateTime.Now - TimeSpan.FromDays(2.0);
			dy = int.Parse(ev.Substring(0, 2));
			mo = int.Parse(ev.Substring(3, 2));
			yr = int.Parse(ev.Substring(6, 4));
			hr = int.Parse(ev.Substring(11, 2));
			mi = int.Parse(ev.Substring(14, 2));
			sc = int.Parse(ev.Substring(17, 2));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc);
			span = TwoDaysAgo - dtEval;
			Assert.AreEqual(true, span.TotalSeconds < 1.0);

			File.Delete(filePath);

			//
			// Key test
			//
			var elems = new Dictionary<string, string>();
			elems.Add("fmtD", "#{dd/MM/yyyy}#");
			elems.Add("fmtT", "#{HH:mm:ss}#");

			IProcessEvaluate pKeyEval = new ProcessKey(elems, delim);
			eval.OnEvaluateContext += pKeyEval.Evaluate;
			ev = eval.EvaluateString("#{CurrentTime::#{Key::fmtD}# #{Key::fmtT}#::-#{Counter::c1}#d}#");
			DateTime ThreeDaysAgo = DateTime.Now - TimeSpan.FromDays(3.0);
			dy = int.Parse(ev.Substring(0, 2));
			mo = int.Parse(ev.Substring(3, 2));
			yr = int.Parse(ev.Substring(6, 4));
			hr = int.Parse(ev.Substring(11, 2));
			mi = int.Parse(ev.Substring(14, 2));
			sc = int.Parse(ev.Substring(17, 2));
			dtEval = new DateTime(yr, mo, dy, hr, mi, sc);
			span = ThreeDaysAgo - dtEval;
			Assert.AreEqual(true, span.TotalSeconds < 1.0);

			// Try again without the Key Evaluate ProcessXxx
			eval.OnEvaluateContext -= pKeyEval.Evaluate;
			ev = eval.EvaluateString("#{CurrentTime::#{Key::fmtD}# #{Key::fmtT}#::-#{Counter::c1}#d}#");
			// The only thing to evaluate is #{Counter::c1}# (we took ${Key::value}$ away, therefore
			// ${CurrentTime::value...}$ will not evaluate correctly...
			// Also, the class EnhancedStringEvalNoyyyMMdd transforms the delimiters "#{" and "}#" to 
			// "${" and "}$" respectively, but it does not transform the delimiters back to "#{" and "}#" 
			// so our expectation is that the evaluation will yield:
			Assert.AreEqual("${CurrentTime::${Key::fmtD}$ ${Key::fmtT}$::-4d}$", ev);
		}

		[TestMethod]
		public void TestOpenClose()
		{
			////string pattern = string.Format(@"^(((?<Open>{)[^{}]*)+((?<Close-Open>(}))[^{}]*)+)*(?(Open)(?!))$", _openDelimiter, _closeDelimiter);
			//string pattern = @"^(((?<Open>{0})[^{0}{1}]*)+((?<Close-Open>({1}))[^{0}{1}]*)+)*(?(Open)(?!))$";
			//return string.Format(pattern, OpenDelimEquivalent, CloseDelimEquivalent);

			string pattern = "^[^<>]*(((?'Open'<)[^<>]*)+((?'Close-Open'>)[^<>]*)+)*(?(Open)(?!))$";
			string input = "<abc><mno<xyz>>";

			Match m = Regex.Match(input, pattern);
			Assert.AreEqual(true, m.Success);

			Regex _reOpenClose = new Regex(pattern, RegexOptions.Singleline);
			m = _reOpenClose.Match(input);
			Assert.AreEqual(true, m.Success);

			m = _reOpenClose.Match("");
			Assert.AreEqual(true, m.Success);

			Assert.AreEqual(string.Empty, m.ToString());

			pattern = @"^[^{}]*(((?<Open>{)[^{}]*)+((?<Close-Open>(}))[^{}]*)+)*(?(Open)(?!))$";
			_reOpenClose = new Regex(pattern, RegexOptions.Singleline);
			m = _reOpenClose.Match(string.Empty);
			Assert.AreEqual(true, m.Success);

			m = _reOpenClose.Match(@"{abc}");
			Assert.AreEqual(true, m.Success);

			m = _reOpenClose.Match(@"abc{def}");
			Assert.AreEqual(true, m.Success);

			m = _reOpenClose.Match(@"abc{def::{ghi::jkl}}");
			Assert.AreEqual(true, m.Success);

			m = _reOpenClose.Match(@"abc{def::{ghi:{}:jkl}}");
			Assert.AreEqual(true, m.Success);

			m = _reOpenClose.Match(@"abc{def::{ghi:{}:jkl}}{");
			Assert.AreEqual(false, m.Success);
		}
	}

	sealed internal class EnhancedStringEvalNoyyyMMdd : EnhancedStringEval
	{
		public EnhancedStringEvalNoyyyMMdd(List<IProcessEvaluate> context) : base(context) {}
		public EnhancedStringEvalNoyyyMMdd(List<IProcessEvaluate> context, IDelimitersAndSeparator delim) : base(context, delim) {}

		/// <summary>
		/// Change "#{" and "}#" to "${" and "}$" respectively
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		protected override string PreEvaluate(string text)
		{
			return text.Replace("#{", "${").Replace("}#", "}$");
		}
	}

	sealed internal class EnhStringTrailingColon : EnhancedStringEval
	{
		public EnhStringTrailingColon(List<IProcessEvaluate> context) : base(context)
		{
			// If {Identifier::Value} is of type:
			//		{Identifier::Name::value} where name is a string ending with a colon then
			//		>	identifier has no colons
			//		>	double colon
			//		>	Name MAY ends with a colon
			//		>	may be followed with a double colon etc
			string pat1 = @"(?<pre>({)[^{}:]+(::)(:?)[^{}:]+(:[^{}:]+)*)(?<trailingColon>:?)(?<post>(::[^{}]*)?(}))";
			_reSeparator = new Regex(pat1, RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}

		protected override string PreEvaluate(string text)
		{
			if (!_reSeparator.IsMatch(text)) return text;
			string preText = _reSeparator.Replace(text,
				m => m.Groups["pre"].Value + (m.Groups["trailingColon"].Value == string.Empty ? "" : "\u0004") + m.Groups["post"].Value);
			return preText;
		}

		protected override string  PostEvaluate(string text)
		{
			 return text.Replace('\u0004', ':');
		}

		private Regex _reSeparator;

	}
}
