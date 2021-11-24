using System;
using System.Collections.Generic;
using EnhancedStringEvaluate;
using System.Configuration;
using TestEvaluation.ProcessEvaluate;
using System.Text.RegularExpressions;

namespace TestEvaluation
{
	class Program
	{
		static void Main(string[] args)
		{
			var config = new Dictionary<string,string>();
			foreach (var name in ConfigurationManager.AppSettings.AllKeys)
				config.Add(name, ConfigurationManager.AppSettings[name]);

			// handling
			var context = new List<IProcessEvaluate>();
			context.Add(new ProcessDate());
			context.Add(new ProcessKey(config));
			context.Add(new ProcessForeignKey());
			context.Add(new ProcessIf());
			context.Add(new ProcessLiteral());
			IProcessEvaluate evalCurrentDir = new ProcessCurrentDir();
			context.Add(evalCurrentDir);
			context.Add(new ProcessCurrentTime());

			EnhancedStringEval eval = new EnhancedStringEval(context);

			try
			{
				ProcessCurrentDir currDir = (ProcessCurrentDir)evalCurrentDir;
				currDir.CurrentDir = @"C:\Directory 1";
				string key = "Flat";
				string test = eval.EvaluateString(config[key]);
				Console.WriteLine("{0} (orig: {1}): {2}", key, ConfigurationManager.AppSettings[key], test);

				key = "Static flat";
				test = eval.EvaluateString(config[key]);
				Console.WriteLine("{0} (orig: {1}): {2}", key, ConfigurationManager.AppSettings[key], test);

				key = "Static date";
				test = eval.EvaluateString(config[key]);
				Console.WriteLine("{0} (orig: {1}): {2}", key, ConfigurationManager.AppSettings[key], test);

				key = "Dynamic current directory";
				test = eval.EvaluateString(config[key]);
				Console.WriteLine("{0} (orig: {1}): {2}", key, ConfigurationManager.AppSettings[key], test);

				key = "temp";
				test = eval.EvaluateString(config[key]);
				Console.WriteLine("{0} (orig: {1}): {2}", key, ConfigurationManager.AppSettings[key], test);

				key = "Dynamic current path";
				test = eval.EvaluateString(config[key]);
				Console.WriteLine("{0} (orig: {1}): {2}", key, ConfigurationManager.AppSettings[key], test);

				key = "Stamp";
				test = eval.EvaluateString(config[key]);
				Console.WriteLine("{0} (orig: {1}): {2}", key, ConfigurationManager.AppSettings[key], test);

				key = "Stamp2";
				test = eval.EvaluateString(config[key]);
				Console.WriteLine("{0} (orig: {1}): {2}", key, ConfigurationManager.AppSettings[key], test);

				key = "Stamp3";
				test = eval.EvaluateString(config[key]);
				Console.WriteLine("{0} (orig: {1}): {2}", key, ConfigurationManager.AppSettings[key], test);
			}
			catch (EnhancedStringException ex1)
			{
				string msg = ex1.Message;
				int counter = 1;
				for (Exception iEx = ex1.InnerException; iEx != null; iEx = iEx.InnerException)
					msg += string.Format("{0}\t{1}.\t{2}", Environment.NewLine, counter++, iEx.Message);
				Console.WriteLine("ID: \"{0}\", {1}, {2}", ex1.Identifier, ex1.Element, msg);
			}

			Console.ReadKey();
		}
	}
}
