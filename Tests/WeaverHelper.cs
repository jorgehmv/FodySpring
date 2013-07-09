using System;
using System.IO;
using System.Reflection;
using System.Linq;
using Mono.Cecil;

public class WeaverHelper
{

    public static Assembly WeaveAssembly()
    {
        var projectPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\AssemblyToProcess\AssemblyToProcess.csproj"));
        var assemblyPath = Path.Combine(Path.GetDirectoryName(projectPath), @"bin\Debug\AssemblyToProcess.dll");

        var springBinPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\Fody\bin\debug\"));
#if (!DEBUG)
        assemblyPath = assemblyPath.Replace("Debug", "Release");
        springBinPath = springBinPath.Replace("Debug", "Release");
#endif

        var newAssembly = assemblyPath.Replace(".dll", "2.dll");
        File.Copy(assemblyPath, newAssembly, true);

        var moduleDefinition = ModuleDefinition.ReadModule(newAssembly);
        var weavingTask = new ModuleWeaver
        {
            ModuleDefinition = moduleDefinition,
        };

        weavingTask.Execute();
        moduleDefinition.Write(newAssembly);

        return Assembly.LoadFile(newAssembly);
    }
}