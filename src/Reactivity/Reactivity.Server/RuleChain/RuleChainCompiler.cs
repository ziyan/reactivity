using System;
using System.Text.RegularExpressions;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;
using Reactivity.Objects;

namespace Reactivity.Server
{
    static class RuleChainCompiler
    {
        public static Assembly Compile(string source)
        {
            // Compile the code
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();

            parameters.ReferencedAssemblies.Add("system.dll");
            parameters.ReferencedAssemblies.Add("system.xml.dll");
            parameters.ReferencedAssemblies.Add("system.data.dll");
            parameters.ReferencedAssemblies.Add("system.web.dll");
            parameters.ReferencedAssemblies.Add(System.Configuration.ConfigurationManager.AppSettings["Reactivity.Server.RuleChainCompiler.ReactivityAssemblyPath"]);

            // add reference
            MatchCollection matches = Regex.Matches(source, @"\/\/\/[\s]+\<reference\>(.*)\<\/reference\>");
            foreach (Match match in matches)
                parameters.ReferencedAssemblies.Add(match.Result("$1"));

            parameters.CompilerOptions = "/t:library";
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;
            parameters.IncludeDebugInformation = false;
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, source);
            if (results.Errors.Count == 0)
            {
                Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.RuleChainReloadCompileSuccess, String = results.CompiledAssembly.FullName, Timestamp = DateTime.Now });
                return results.CompiledAssembly;
            }

            // notification about errors
            foreach (CompilerError error in results.Errors)
            {
                Clients.ClientSession.Notify(new ClientEvent { Type = ClientEventType.RuleChainReloadCompileError, String = error.ToString(), Timestamp = DateTime.Now });
                Util.EventLog.WriteEntry("Reactivity.Server.RuleChainCompiler", error.ToString());
            }
            return null;
        }
    }
}
