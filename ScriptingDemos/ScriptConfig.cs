using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace ScriptingDemos
{
    public class ScriptConfig
    {
        private string _rootPath = AppDomain.CurrentDomain.BaseDirectory;

        private IEnumerable<Assembly> _assemblies = new[] { typeof(object).Assembly, typeof(Enumerable).Assembly };
        private IEnumerable<string> _namespaces = new[] { "System", "System.IO", "System.Linq", "System.Collections.Generic" };
        private readonly string _scriptName;

        public ScriptConfig(string scriptName)
        {
            _scriptName = scriptName;
        }

        public ScriptConfig WithRootPath(string rootPath)
        {
            _rootPath = rootPath;
            return this;
        }

        public ScriptConfig WithReferences(params Assembly[] assemblies)
        {
            _assemblies = assemblies.Union(_assemblies);
            return this;
        }

        public ScriptConfig WithNamespaces(params string[] namespaces)
        {
            _namespaces = namespaces.Union(_namespaces);
            return this;
        }

        public Task<TConfig> Create<TConfig>() where TConfig : new()
        {
            return Create(new TConfig());
        }

        public async Task<TConfig> Create<TConfig>(TConfig config)
        {
            var code = File.ReadAllText(Path.Combine(_rootPath, _scriptName));
            var opts = ScriptOptions.Default.AddImports(_namespaces).AddReferences(_assemblies).AddReferences(typeof(TConfig).Assembly);

            var script = CSharpScript.Create(code, opts, typeof(TConfig));
            var result = await script.RunAsync(config);

            return config;
        }
    }

    public class AppConfiguration
    {
        public int Number { get; set; }

        public string Text { get; set; }
    }

    public enum DataTarget
    {
        Test,
        Production
    }

    public class MyAppConfig
    {
        public DataTarget Target { get; set; }
        public Uri AppUrl { get; set; }
        public int CacheTime { get; set; }
    }
}