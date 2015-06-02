using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RoslynTester.DiagnosticResults;
using RoslynTester.Helpers;
using RoslynTester.Helpers.CSharp;

namespace StringFormatAnalyzer.Test.Verifiers
{
	/// <summary>
	/// Superclass of all Unit Tests for DiagnosticAnalyzers
	/// </summary>
	public class StringFormatDiagnosticVerifier : CSharpDiagnosticVerifier
	{
		protected override DiagnosticAnalyzer DiagnosticAnalyzer { get; }
	}
}
