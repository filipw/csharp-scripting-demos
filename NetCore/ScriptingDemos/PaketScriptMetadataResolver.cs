using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace ScriptingDemos
{
    public class PaketScriptMetadataResolver : MetadataReferenceResolver
    {
        private StringBuilder PaketDependencies = new StringBuilder().AppendLine("source https://nuget.org/api/v2");
        private static string Tfm = "netstandard16";
        private static string PaketPrefix = "paket: ";
        private readonly ScriptMetadataResolver _inner;
        private HashSet<string> ResolvedReferences = new HashSet<string>();
        private HashSet<Assembly> ResolvedAssemblies = new HashSet<Assembly>();

        public ScriptOptions CreateScriptOptions(ScriptOptions scriptOptions)
        {
            return scriptOptions.
                WithMetadataResolver(this).
                WithReferences(ResolvedReferences.Select(x => MetadataReference.CreateFromFile(x))).
                AddReferences(ResolvedAssemblies).AddReferences(typeof(DataContractAttribute).GetTypeInfo().Assembly);
        }

        public PaketScriptMetadataResolver(string code, string workingDirectory = null)
        {
            workingDirectory = workingDirectory ?? Directory.GetCurrentDirectory();
            _inner = ScriptMetadataResolver.Default;

            var syntaxTree = CSharpSyntaxTree.ParseText(code, CSharpParseOptions.Default.WithKind(SourceCodeKind.Script));
            var refs = syntaxTree.GetCompilationUnitRoot().GetReferenceDirectives().Select(x => x.File.ToString().Replace("\"", string.Empty)).Where(x => x.StartsWith(PaketPrefix));
            foreach (var reference in refs)
            {
                PaketDependencies.AppendLine(reference.Replace(PaketPrefix, "nuget "));
            }

            File.WriteAllText(Path.Combine(workingDirectory, "paket.dependencies"), PaketDependencies.ToString());
            var processStartInfo = new ProcessStartInfo(@".paket/paket.exe", "install --generate-load-scripts load-script-framework " + Tfm)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = _inner.BaseDirectory
            };
            using (var process = new Process() { StartInfo = processStartInfo })
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    Console.WriteLine(e.Data);
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    Console.Error.WriteLine(e.Data);
                };
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();

                process.WaitForExit();
            }

            var restoredDefs = File.ReadAllText(Path.Combine(workingDirectory, $".paket/load/{Tfm}/main.group.csx"));
            var restoredDefsSyntaxTree = CSharpSyntaxTree.ParseText(restoredDefs, CSharpParseOptions.Default.WithKind(SourceCodeKind.Script));
            var restoredRefs = restoredDefsSyntaxTree.GetCompilationUnitRoot().GetReferenceDirectives().Select(x => x.File.ToString().Replace("\"", string.Empty));
            foreach (var restoredRef in restoredRefs)
            {
                if (restoredRef.StartsWith(".."))
                {
                    ResolvedReferences.Add(Path.Combine(workingDirectory, $".paket/load/{Tfm}", restoredRef));
                }
                else
                {
                    //skip GAC by design
                    //Console.WriteLine($"Skipping GAC: {restoredRef}");
                }
            }
        }

        public override bool Equals(object other)
        {
            return _inner.Equals(other);
        }

        public override int GetHashCode()
        {
            return _inner.GetHashCode();
        }

        public override bool ResolveMissingAssemblies => true;

        public override PortableExecutableReference ResolveMissingAssembly(MetadataReference definition, AssemblyIdentity referenceIdentity)
        {
            return _inner.ResolveMissingAssembly(definition, referenceIdentity);
        }

        public override ImmutableArray<PortableExecutableReference> ResolveReference(string reference, string baseFilePath, MetadataReferenceProperties properties)
        {
            if (reference.StartsWith(PaketPrefix))
            {
                // dummy reference
                return ImmutableArray.Create(PortableExecutableReference.CreateFromFile(typeof(PaketScriptMetadataResolver).GetTypeInfo().Assembly.Location));
            }
            else
            {
                return _inner.ResolveReference(reference, baseFilePath, properties);
            }
        }
    }
}
