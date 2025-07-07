
#if NET48 || NET7_0 || NET8_0

using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace vsc.ProjectAnalysis
{
    /// <summary>
    /// <b>NOT</b> availabew on .net 6
    /// </summary>
    public static class CodeAnalysisExtensions
    {
        /// <summary>
        /// <b>NOT</b> availabew on .net 6
        /// </summary>
        public static async Task<Project> GetProject(string projectPath)
        {
            using var workspace = MSBuildWorkspace.Create();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            var project = await workspace.OpenProjectAsync(projectPath);
            Console.WriteLine($"Project opened in {sw.ElapsedMilliseconds / 1000d}s");
            sw.Reset();

            return project;
        }

        /// <summary>
        /// <b>NOT</b> availabew on .net 6
        /// </summary>
        public static async Task<(Project project, Compilation compilation)> CompileProject(string projectPath)
        {
            var project = await GetProject(projectPath);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            var compilation = await project.GetCompilationAsync();

            Console.WriteLine($"Project compiled in {sw.ElapsedMilliseconds / 1000d}s");

            if (compilation == null)
            {
                throw new InvalidOperationException("Failed to get project compilation.");
            }

            return (project, compilation);
        }

        /// <summary>
        /// <b>NOT</b> availabew on .net 6
        /// </summary>
        public static async Task<(Project project, Compilation compilation, List<Assembly> assemblies)> CompileProject(Project project)
        {
            using var workspace = MSBuildWorkspace.Create();

            Stopwatch sw = new Stopwatch();

            var compilation = await project.GetCompilationAsync();
            Console.WriteLine($"Project compiled in {sw.ElapsedMilliseconds / 1000d}s");

            if (compilation == null)
                throw new InvalidOperationException("Failed to get project compilation.");

            return (
                project,
                compilation,
                assemblies: project.MetadataReferences.Where(m => m.Display is not null).Select(m => Assembly.LoadFrom(m.Display!)
            ).ToList());
        }

    }
}

#endif
