#region Usings

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

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

			if (!IsStringFormat(expression, _context.SemanticModel))
			{
				return;
			}

			Debug.WriteLine("String format detected: {0}", expression);

			var formatString = expression.ArgumentList.Arguments[0].ToString();

			var tokens = BreakToTokens(formatString);
			Debug.WriteLine("Tokens:\r\n\t{0}", String.Join("\r\n\t", tokens.Select(x => x.ToString())));
			if (tokens.Length == 0)
			{
				return;
			}

			var args = expression.ArgumentList.Arguments.Skip(1).ToArray();
			if (args.Length != tokens.Length)
			{
				Debug.WriteLine("Wrong args count. Expected {0}, got {1}", tokens.Length, args.Length);
				return;
			}

			if (IsOrdered(tokens))
			{
				return;
			}

			var msgParameter = String.Join(" ", tokens.Select(x => x.Index));
			var diagnostic = Diagnostic.Create(Rule, _context.Node.SyntaxTree.GetLocation(expression.ArgumentList.Arguments[0].Span), msgParameter);
			_context.ReportDiagnostic(diagnostic);
		}

		private bool IsOrdered(StringFormatToken[] _tokens)
		{
			var maxTokenIndex = int.MinValue;
			foreach (var token in _tokens)
			{
				if (token.Index > maxTokenIndex)
				{
					maxTokenIndex = token.Index;
				}
				else
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
			
			var methodSymbol = _semanticModel.GetSymbolInfo(_invocationExpression).Symbol as IMethodSymbol ;
			if (methodSymbol == null)
			{
				return false;
			}

			var parameters = methodSymbol.Parameters;
			if (!parameters.Any())
			{
				return false;
			}

			if (parameters.Count() < 2)
			{
				return false;
			}
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


	}
}