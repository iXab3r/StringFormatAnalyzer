using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace StringFormatAnalyzer
{
	class StringFormatToken
	{
		private static Regex m_indexParser = new Regex(@"\{\s*(.*?)\s*\}");

		private readonly Match m_token;

		private Lazy<int> m_index;

		public StringFormatToken(Match _token)
		{
			if (_token == null)
			{
				throw new ArgumentNullException(nameof(_token));
			}
			m_token = _token;
			m_index = new Lazy<int>(() => ValueFactory(m_token.Value), LazyThreadSafetyMode.PublicationOnly);
		}

		public int Index => m_index.Value;

		public string Token => m_token.Value;

		private static int ValueFactory(string _token)
		{
			if (_token == null)
			{
				return -1;
			}
			var match = m_indexParser.Match(_token);
			var matchingValue = match.Groups[1].Value;
			//Debug.WriteLine("Match for '{0}' is '{1}' (success: {2})", _token, matchingValue, match.Success);
			int result;
			if (!match.Success || !Int32.TryParse(matchingValue, out result))
			{
				return -1;
			}

			return result;
		}

		/// <summary>
		///     Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		///     A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			return $"Index: {Index}, Token: {Token}";
		}
	}
}