#region Usings

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#endregion

namespace StringFormatAnalyzer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class StringFormatAnalyzerAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "StringFormatAnalyzer";

		internal const string Category = "Naming";

		// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
		internal static readonly LocalizableString Title = new LocalizableResourceString(
			nameof(Resources.AnalyzerTitle),
			Resources.ResourceManager,
			typeof(Resources));

		internal static readonly LocalizableString MessageFormat = new LocalizableResourceString(
			nameof(Resources.AnalyzerMessageFormat),
			Resources.ResourceManager,
			typeof(Resources));

		internal static readonly LocalizableString Description = new LocalizableResourceString(
			nameof(Resources.AnalyzerDescription),
			Resources.ResourceManager,
			typeof(Resources));

		internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
			DiagnosticId,
			Title,
			MessageFormat,
			Category,
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true);

		private Regex m_tokensRegex = new Regex(@"(\{.*?\})", RegexOptions.None);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			// TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
			context.RegisterSyntaxNodeAction(Action, SyntaxKind.InvocationExpression);
		}

		private void Action(SyntaxNodeAnalysisContext _context)
		{
			var expression = _context.Node as InvocationExpressionSyntax;
			if (expression == null)
			{
				return;
			}

			Debug.WriteLine("node: {0}", expression);

			if (IsStringFormat(expression, _context.SemanticModel))
			{
				Debug.WriteLine("String format detected: {0}", expression);

				var formatString = expression.ArgumentList.Arguments[0].ToString();
				var tokens = BreakToTokens(formatString);
				Debug.WriteLine("Tokens:\r\n\t{0}", String.Join("\r\n\t", tokens.Select(x => x.ToString())));

				if (!IsOrdered(tokens))
				{
					var msgParameter = String.Join(" ", tokens.Select(x => x.Index));
					var diagnostic = Diagnostic.Create(Rule, _context.Node.SyntaxTree.GetLocation(expression.ArgumentList.Arguments[0].Span), msgParameter);
					_context.ReportDiagnostic(diagnostic);
				}
			}
		}

		private bool IsOrdered(StringFormatToken[] _tokens)
		{
			var maxTokenIndex = int.MinValue;
			foreach (var token in _tokens)
			{
				if (token.Index > maxTokenIndex)
				{
					maxTokenIndex = token.Index;
				} else
				{
					return false;
				}
			}
			return true;
		}

		private StringFormatToken[] BreakToTokens(string _formatString)
		{
			if (_formatString == null)
			{
				throw new ArgumentNullException(nameof(_formatString));
			}
			var result = new List<StringFormatToken>(0);

			var matches = m_tokensRegex.Matches(_formatString);
			foreach (var match in matches.OfType<Match>().Where(match => match.Success))
			{
				var token = new StringFormatToken(match);
				result.Add(token);
			}

			return result.ToArray();
		}

		private bool IsStringFormat(InvocationExpressionSyntax _invocationExpression, SemanticModel _semanticModel)
		{
			var argumentListSyntax = _invocationExpression?.ArgumentList;
			if (argumentListSyntax == null || !argumentListSyntax.Arguments.Any())
			{
				return false;
			}
			var args = argumentListSyntax.Arguments;

			if (args.Count < 2)
			{
				return false;
			}

			var firstArg = args[0];
			if (!firstArg.Expression.IsKind(SyntaxKind.StringLiteralExpression))
			{
				return false;
			}
			var firstArgType = _semanticModel.GetTypeInfo(firstArg);

			var secondArg = args[1];
			var secondArgType = _semanticModel.GetTypeInfo(secondArg);

			return true;
		}

		private static void AnalyzeSymbol(SymbolAnalysisContext context)
		{
			// TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
			var methodSymbol = (IMethodSymbol)context.Symbol;
			Debug.WriteLine("Symbol: {0}", methodSymbol);

			// Find just those named type symbols with names containing lowercase letters.
			if (methodSymbol.Name.ToCharArray().Any(char.IsLower))
			{
				// For all such symbols, produce a diagnostic.
				var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.Name);
				context.ReportDiagnostic(diagnostic);
			}
		}

		private class StringFormatToken
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
}