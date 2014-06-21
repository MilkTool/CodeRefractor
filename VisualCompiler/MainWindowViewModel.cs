﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Documents;
using CodeRefactor.OpenRuntime;
using CodeRefractor.CompilerBackend.Optimizations.Util;
using CodeRefractor.CompilerBackend.ProgramWideOptimizations.ConstParameters;
using CodeRefractor.CompilerBackend.ProgramWideOptimizations.Virtual;
using CodeRefractor.RuntimeBase;
using CodeRefractor.RuntimeBase.Backend.ProgramWideOptimizations;
using CodeRefractor.RuntimeBase.Config;
using CodeRefractor.RuntimeBase.MiddleEnd.Methods;
using CodeRefractor.RuntimeBase.Runtime;
using Microsoft.CSharp;

namespace VisualCompiler
{
    public class MainWindowViewModel : NotificationViewModel
    {
        public MainWindow Window;
        private static CSharpCodeProvider codeProvider = new CSharpCodeProvider();
        public static ICodeCompiler icc = codeProvider.CreateCompiler();

        public static CompilerParameters parameters = new CompilerParameters()
        {
            GenerateExecutable = true,

        };

        public  string LastCompiledExecutable;
        public void CompileAndRunCode(string name, string code)
        {
            try
            {

           
             CompilerErrors = string.Empty;
            var outputExeName = "Test" +DateTime.Now.Ticks + name + ".exe";
            var outputNativeName = "TestNative" + name + ".exe";

            //Structure: Generate File, Compile  and Run It

            //Call Mcs / Csc
            LastCompiledExecutable = outputExeName;
            parameters.OutputAssembly = outputExeName;

            CompilerResults results = icc.CompileAssemblyFromSource(parameters, code);

            if (results.Errors.Count > 0)
            {
                

                foreach (var error in results.Errors)
                {
                    CompilerErrors += "\n" + error;
                }
                

                return;
            }

            //Invoke CRCC
            CallCompiler(outputExeName, outputNativeName);

            }
            catch (Exception ex)
            {

                CompilerErrors += "\n" + ex.Message + ex.StackTrace;
            }
        }


        public  void CallCompiler(string inputAssemblyName, string outputExeName)
        {
            var commandLineParse = CommandLineParse.Instance;
            if (!String.IsNullOrEmpty(inputAssemblyName))
            {
                commandLineParse.ApplicationInputAssembly = inputAssemblyName;
            }
            if (!String.IsNullOrEmpty(outputExeName))
            {
                commandLineParse.ApplicationNativeExe = outputExeName;
            }
            var dir = Directory.GetCurrentDirectory();
            inputAssemblyName = Path.Combine(dir, commandLineParse.ApplicationInputAssembly);
            var asm = Assembly.LoadFile(inputAssemblyName);
            var definition = asm.EntryPoint;
            var start = Environment.TickCount;

            var optimizationsTable = new ProgramOptimizationsTable
            {
                new DevirtualizerIfOneImplemetor(),
                new CallToFunctionsWithSameConstant()
            };
             
            var crRuntime = new CrRuntimeLibrary();
            crRuntime.ScanAssembly(typeof(CrString).Assembly);
            OptimizationLevelBase.ClearOptimizations();
            OptimizationLevelBase.Instance = new OptimizationLevels();
            OptimizationLevelBase.Instance.EnabledCategories.Clear();
            OptimizationLevelBase.Instance.EnabledCategories.AddRange(OptimizationList);
            OptimizationLevelBase.UpdateOptimizationsFromCategories(OptimizationLevelBase.OptimizationPasses);
            OptimizationLevelBase.SortOptimizations();
            var programClosure = new ProgramClosure(definition, crRuntime);
            var sb = programClosure.BuildFullSourceCode(programClosure.Runtime);
            var end = Environment.TickCount - start;
             CompilerErrors +=String.Format("Compilation time: {0} ms", end);

            var opcodes = programClosure.MethodClosure;

            var intermediateOutput = "";

            foreach (var opcode in opcodes)
            {
                intermediateOutput += " " + opcode.Key + ": \n";

                if (opcode.Value.Kind != MethodKind.Default)
                {
                    intermediateOutput += "// Provided By Framework     \n\n";
                    continue;
                }

                foreach (var op in opcode.Value.MidRepresentation.LocalOperations)
                {
                    var oper = string.Format("{1}\t({0})", op.Kind, op.Value ?? ""); ;
                    intermediateOutput += "     " + oper + "\n";
                }

                intermediateOutput += "\n";
            }


            OutputCode = sb.ToString();

            ILCode = intermediateOutput;



            //TODO: Make this call all the different compilers i.e. TestHelloWorld_GCC.exe etc...

        }
        

        private string _sourceCode;
        public string SourceCode
        {
            get { return _sourceCode; }
            set
            {
                if (_sourceCode != value)
                {
                   
                    _sourceCode = value;
                 
                    RecompileSource();
                }
            }
        }

        public void RecompileSource()
        {
           
            OutputCode = "";
            ILCode = "";
            Recompile();
            Changed(() => SourceCode);
        }


        private string _ilCode;
             public string ILCode
        {
            get { return _ilCode; }
            set
            {
                if (_ilCode != value)
                {
                    _ilCode = value;
                    Window.IL.Text = value;
                    Changed(() => ILCode);
                }
            }
        }

        private string _outputCode;
        public string OutputCode
        {
            get { return _outputCode; }
            set
            {
                if (_outputCode != value)
                {
                    _outputCode = value;
                    try
                    {

                        Window.Output.Text = value;
                    }
                    catch (Exception)
                    {


                    }
                    Changed(() => OutputCode);
                }
            }
        }

        private string _compilerErrors;
        public string CompilerErrors
        {
            get { return _compilerErrors; }
            set
            {
                _compilerErrors = value;
                Changed(() => CompilerErrors);
            }
        }

        public List<string> OptimizationList { get; set; }



        public MainWindowViewModel( )
        {


            OptimizationList = new List<string>();
          
        }

        public void Recompile()
        {

            CompileAndRunCode("HelloWorld",SourceCode);

        }

    }
}