using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using RoslynTester.DiagnosticResults;
using RoslynTester.Helpers;
using RoslynTester.Helpers.CSharp;

using StringFormatAnalyzer;
using StringFormatAnalyzer.Test.Verifiers;

namespace StringFormatAnalyzer.Test
{
	[TestClass]
	public class UnitTest : CSharpDiagnosticVerifier
	{

		[TestMethod]
		public void TestMethod1()
		{
			var test = @"";
			this.VerifyDiagnostic(test);
		}

		public void StringFormatExample()
		{
			String.Format("{0} {2} {1}", 0, 1, 2);
		}

		[TestMethod]
		public void AssertThatSimpleReorderingTriggersDiag()
		{
			var test = @"using System; Using System.Text; namespace Test { public class TestClass {
		public void TestMethod()
		{
			var str = String.Format(""test {0} {1} {3} {2} {5} {4}"", 1,2,3,4,5, 6);

		}
}}";

			var expected = new DiagnosticResult
			{
				Id = StringFormatAnalyzerAnalyzer.DiagnosticId,
				Message = String.Format(Resources.AnalyzerMessageFormat,"0 1 3 2 5 4"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 4, 28)
						}
			};


			VerifyDiagnostic(test, expected);
		}

		[TestMethod]
		public void AssertThatDiagnosticIsNotTriggeredWhenArgumentsCountIsWrong()
		{
			var test = @"using System; Using System.Text;
		public void TestMethod()
		{
			var str = String.Format(""test {0} {1} {3} {2} {5} {4}"", 1,2,3,4,5);

		}
";
			VerifyDiagnostic(test);
		}


		protected override DiagnosticAnalyzer DiagnosticAnalyzer => new StringFormatAnalyzerAnalyzer();
	}
}