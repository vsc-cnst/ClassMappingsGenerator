using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace vsc.ClassMappingsGenerator.ProjectAnalysisUtills
{
    public static class MapAttributeUtils
    {
        public static Assembly LoadAssembly(Compilation compilation) {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            Assembly assembly;

            using (var ms = new MemoryStream())
            {
                var emitResult = compilation.Emit(ms);

                if (emitResult.Success)
                {
                    // Load the emitted assembly
                    ms.Seek(0, SeekOrigin.Begin);
                    assembly = Assembly.Load(ms.ToArray());
                }
                else
                {
                    throw new Exception("No Assembly!");
                }
            }

            Console.WriteLine($"Assembly loaded in {sw.ElapsedMilliseconds / 1000m}s");
            sw.Stop();

            return assembly;
        }

    }
}

