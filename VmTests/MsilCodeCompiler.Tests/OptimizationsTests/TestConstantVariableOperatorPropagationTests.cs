﻿#region Usings

using System.Collections.Generic;
using CodeRefractor.MiddleEnd.Optimizations.Common;
using CodeRefractor.MiddleEnd.Optimizations.ConstantFoldingAndPropagation;
using MsilCodeCompiler.Tests.Shared;
using NUnit.Framework;

#endregion

namespace MsilCodeCompiler.Tests.OptimizationsTests
{
    /// <summary>
    /// Test with specified body :)
    /// and with one optimization
    /// </summary>
    [TestFixture]
    public class TestConstantVariableOperatorPropagationTests : CompilingProgramBase
    {
        [Test]
        public void TestSimple()
        {
            var optimizations = new List<ResultingOptimizationPass>
            {
                new ConstantVariableOperatorPropagation()
            };
            var mainBody =
                @"var a = 30.0;
            var b = 9.0 - (int)a / 5;
           
            Console.WriteLine(b);";
            TryCSharpMain(mainBody, optimizations);
        }
    }
}