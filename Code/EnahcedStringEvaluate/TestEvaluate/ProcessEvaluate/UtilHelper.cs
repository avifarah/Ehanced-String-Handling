using System;
using System.Text.RegularExpressions;
using System.IO;


namespace TestEvaluation.ProcessEvaluate
{
	/// <summary>
	/// IO path and directory and file helper
	/// A helper class for file processing like ProcessForeignKey and ProcessIf
	/// </summary>
	public static class Util
	{
		/// <summary>Define a "good" relative path</summary>
		private static Regex _reGoodRelativePath;

		/// <summary>Define a "good" path</summary>
		private static Regex _reGoodPath;

		/// <summary>
		/// .cctor
		/// </summary>
		static Util()
		{
			// FileName restricted character set (characters not allowed in a file name)
			char[] cR = Path.GetInvalidFileNameChars();

			// Convert the restricted characters to a unicode string understood by the 
			// regular expression evaluator ("\u9999") and make a single string out of it.
			// Note that if your first instinct is to form a string like:
			//		string restricted = new string(cR);
			// or
			//		byte[] bR = Array.ConvertAll<char, byte>(cR, c => (byte)c);
			//		string restricted = Encoding.UTF8.GetString(bR);
			// Then resist this urge, it leads to nothing but trouble when running it through 
			// a regular expression pattern matching.  The string has characters like a back 
			// slash ("\") affecting regular expression pattern matching adversely.
			// Instead do the following:
			string[] sR = Array.ConvertAll<char, string>(cR, c => string.Format("\\u{0:X4}", (int)c));
			string restricted = string.Join(string.Empty, sR);

			// A relative path is one not starting with a back-slash ("\") and 
			// between back-slash characters it contains no restricted character
			string relativePattern = string.Format(@"[^{0}]+(\\[^{0}]+)*(\\)?", restricted);
			_reGoodRelativePath = new Regex(string.Format(@"^{0}$", relativePattern), RegexOptions.Singleline);

			// A full path starts with either a drive letter followed by a colon
			// or a double back-slash character
			// then followed by a relative path.
			string pathPattern = string.Format(@"^((\w:\\{0})|(\\{{2}}{0}))$", relativePattern);
			_reGoodPath = new Regex(pathPattern, RegexOptions.Singleline);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="testPath"></param>
		/// <returns></returns>
		public static bool IsValidRelativeFilePath(this string testPath)
		{
			return _reGoodRelativePath.IsMatch(testPath);
		}

		/// <summary>
		/// Makes sure that the path starts with either a drive letter 
		/// followed by a colon or starts with a double back slash ("\").
		/// </summary>
		/// <param name="testPath"></param>
		/// <returns></returns>
		public static bool IsValidFullPath(this string testPath)
		{
			return _reGoodPath.IsMatch(testPath);
		}

		/// <summary>
		/// Ensure that a file is created even if the system needs to create
		/// some of the path supporting the file name.  The routine is recursive
		/// in order to accomplish the goal of creating all the supporting 
		/// directories.
		/// </summary>
		/// <param name="fileNameFullPath"></param>
		/// <returns></returns>
		public static bool CreateDirectory(this string directoryName)
		{
			if (directoryName == null || directoryName == string.Empty)
				return false;

			bool bRc = Directory.Exists(directoryName);
			if (bRc)
				return true;

			DirectoryInfo di = null;
			try
			{
				di = Directory.GetParent(directoryName);
			}
			catch (Exception ex)
			{
				string msg = ex.Message;
				for (Exception iEx = ex.InnerException; iEx != null; iEx = iEx.InnerException)
					msg += string.Format("{0}\t{1}", Environment.NewLine, iEx.Message);

				Console.WriteLine("{0}, {1}", DateTime.Now, msg);
				return false;
			}

			string parent = di.FullName;
			bRc = parent.CreateDirectory();
			if (!bRc) return false;

			Directory.CreateDirectory(directoryName);
			return true;
		}

		public static string ExtendToFullPath(this string pathNm)
		{
			// If the path pointed to by pathNm is a full path then we are good to go and no 
			// further path processing is needed.  But if, on the other hand, the path is a 
			// relative path, then prepend pathNm with the path of the executing program's path.
			bool rc = pathNm.IsValidFullPath();
			if (rc) return pathNm;

			rc = pathNm.IsValidRelativeFilePath();
			if (!rc)
			{
				// A better choice for this exception would be a more specialized exception 
				// structure, potentially employing Enterprise Library.  However, worrying about 
				// the exception will take us away from our main topic.
				throw new ArgumentException(string.Format("Invalid file name path: {0}", pathNm), "pathNm");
			}

			// The fileNm is a valid relative path
			string basedir = AppDomain.CurrentDomain.BaseDirectory;
			pathNm = Path.Combine(basedir, pathNm);
			return pathNm;
		}
	}
}
