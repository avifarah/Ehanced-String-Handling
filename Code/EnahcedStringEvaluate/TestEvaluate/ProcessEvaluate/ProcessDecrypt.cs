using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnhancedStringEvaluate;
using System.Security.Cryptography;
using System.IO;
using System.Text.RegularExpressions;


namespace TestEvaluation.ProcessEvaluate
{
	/// <summary>
	/// Purpose:
	///		Can be used for strings you need to publish yet protected.  
	///		Like database connection string in a configuration file.
	///	
	/// Format:
	///		{Decrypt::encrypted-string}
	/// </summary>
	public class ProcessDecrypt : IProcessEvaluate
	{
		private readonly TripleDESCryptoServiceProvider _tDes;
		private readonly IDelimitersAndSeparator _delim;
		private readonly Regex _reDecript;

		public ProcessDecrypt() : this(DelimitersAndSeparator.DefaultDelimitedString) { }
		public ProcessDecrypt(IDelimitersAndSeparator delim, byte[] key = null, byte[] iv = null)
		{
			_tDes = new TripleDESCryptoServiceProvider();
			_tDes.Key = key;
			if (key == null)
				_tDes.Key = Array.ConvertAll<byte, byte>(_tDes.Key, b => (byte)(((int)b + 7) % ((int)byte.MaxValue + 1)));

			_tDes.IV = iv;
			if (iv == null)
				_tDes.IV = Array.ConvertAll<byte, byte>(_tDes.IV, b => (byte)(((int)b - 7) % ((int)byte.MaxValue + 1)));

			_delim = delim;

			RegexOptions reo = RegexOptions.Singleline | RegexOptions.IgnoreCase;
			string pattern = @"({)\s*Decrypt\s*::()(})";
		}

		#region IProcessEvaluate Members

		public void Evaluate(object src, EnhancedStringEventArgs ea)
		{
			throw new NotImplementedException();
		}

		#endregion

		/// <summary>
		/// Decrypt encrypted text
		/// </summary>
		/// <param name="sEncripted"></param>
		/// <returns></returns>
		private string DecryptTextFromMemory(string sEncripted)
		{
			try
			{
				byte[] Data = new ASCIIEncoding().GetBytes(sEncripted);
				using (var msDecrypt = new MemoryStream(Data))
				{
					var csDecrypt = new CryptoStream(msDecrypt, _tDes.CreateDecryptor(), CryptoStreamMode.Read);

					byte[] fromEncrypt = new byte[Data.Length];
					csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);
					return new ASCIIEncoding().GetString(fromEncrypt);
				}
			}
			catch (CryptographicException e)
			{
				Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
				return null;
			}
		}

		/// <summary>
		/// Give the ability to encript text
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public string EncryptTextToMemory(string text)
		{
			try
			{
				byte[] ret;
				using (var mStream = new MemoryStream())
				using (var cStream = new CryptoStream(mStream, _tDes.CreateEncryptor(), CryptoStreamMode.Write))
				{
					byte[] toEncrypt = new ASCIIEncoding().GetBytes(text);
					cStream.Write(toEncrypt, 0, toEncrypt.Length);
					cStream.FlushFinalBlock();
					ret = mStream.ToArray();
				}

				return new ASCIIEncoding().GetString(ret);
			}
			catch (CryptographicException e)
			{
				Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
				return null;
			}
		}
	}
}
