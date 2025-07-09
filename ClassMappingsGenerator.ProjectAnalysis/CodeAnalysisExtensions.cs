#if NET48 || NET7_0 || NET8_0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace vsc.ProjectAnalysis
{
    /// <summary>
    /// Provides extension methods for Roslyn project analysis.
    /// <b>NOT</b> available on .NET 6.
    /// </summary>
    public static class CodeAnalysisExtensions
    {
        /// <summary>
        /// Opens a Roslyn project from the specified path.
        /// <b>NOT</b> available on .NET 6.
        /// </summary>
        /// <param name="projectPath">The path to the project file.</param>
        /// <returns>The opened <see cref="Project"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="projectPath"/> is null or empty.</exception>
        public static async Task<Project> GetProject(string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                throw new ArgumentNullException(nameof(projectPath), "Project path cannot be null or empty.");

            using var workspace = MSBuildWorkspace.Create();

            var sw = Stopwatch.StartNew();

            var project = await workspace.OpenProjectAsync(projectPath);
            Console.WriteLine($"Project opened in {sw.ElapsedMilliseconds / 1000d:F2}s");

            return project;
        }

        /// <summary>
        /// Compiles a Roslyn project from the specified path.
        /// <b>NOT</b> available on .NET 6.
        /// </summary>
        /// <param name="projectPath">The path to the project file.</param>
        /// <returns>A tuple containing the <see cref="Project"/> and its <see cref="Compilation"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if compilation fails.</exception>
        public static async Task<(Project project, Compilation compilation)> CompileProject(string projectPath)
        {
            var project = await GetProject(projectPath);

            var sw = Stopwatch.StartNew();

            var compilation = await project.GetCompilationAsync();

            Console.WriteLine($"Project compiled in {sw.ElapsedMilliseconds / 1000d:F2}s");

            if (compilation == null)
            {
                throw new InvalidOperationException("Failed to get project compilation.");
            }

            return (project, compilation);
        }

        /// <summary>
        /// ompiles a Roslyn project and loads its referenced assemblies.
        /// <b>NOT</b> available on .NET 6.
        /// </summary>
        /// <param name="project">The <see cref="Project"/> to compile.</param>
        /// <returns>
        /// A tuple containing the <see cref="Project"/>, its <see cref="Compilation"/>, and a list of loaded <see cref="Assembly"/> instances.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="project"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if compilation fails.</exception>
        public static async Task<(Project project, Compilation compilation, List<Assembly> assemblies)> CompileProject(Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project), "Project cannot be null.");

            var sw = Stopwatch.StartNew();

            var compilation = await project.GetCompilationAsync();
            Console.WriteLine($"Project compiled in {sw.ElapsedMilliseconds / 1000d:F2}s");

            if (compilation == null)
                throw new InvalidOperationException("Failed to get project compilation.");

            var assemblies = project.MetadataReferences
                .Where(m => m.Display is not null)
                .Select(m => Assembly.LoadFrom(m.Display!))
                .ToList();

            return (project, compilation, assemblies);
        }
    }
}

#endif
