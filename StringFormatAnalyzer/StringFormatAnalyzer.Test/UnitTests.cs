﻿using Microsoft.CodeAnalysis;
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

		//No diagnostics expected to show up
		[TestMethod]
		public void TestMethod1()
		{
			var test = @"";

			this.VerifyDiagnostic(test);
		}

		[TestMethod]
		public void TestMethodNaming()
		{
			var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
namespace ConsoleApplication1
{
	class TypeName
	{   
			public void TestMethod()
			{
				var str = String.Format(""test {0} {1} {3} {2} {5} {4}"", 1,2,3,4,5, 6);

			}
	}
}";

			var expected = new DiagnosticResult
			{
				Id = StringFormatAnalyzerAnalyzer.DiagnosticId,
				Message = String.Format(Resources.AnalyzerMessageFormat,"0 1 3 2 5 4"),
				Severity = DiagnosticSeverity.Warning,
				Locations =
					new[] {
							new DiagnosticResultLocation("Test0.cs", 14, 29)
						}
			};


			VerifyDiagnostic(test, expected);
		}


		protected override DiagnosticAnalyzer DiagnosticAnalyzer => new StringFormatAnalyzerAnalyzer();
	}
}