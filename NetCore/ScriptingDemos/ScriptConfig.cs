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
        private readonly string _scriptName;
        public ScriptConfig(string scriptName)
        {
            _scriptName = scriptName;
        }

        public Task<TConfig> Create<TConfig>() where TConfig : new()
        {
            return Create(new TConfig());
        }

        public async Task<TConfig> Create<TConfig>(TConfig config)
        {
            var code = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, _scriptName));
            var opts = ScriptOptions.Default.
                AddImports("System", "System.IO", "System.Linq", "System.Collections.Generic", typeof(TConfig).Namespace).
                AddReferences(typeof(object).GetTypeInfo().Assembly, typeof(Enumerable).GetTypeInfo().Assembly, typeof(TConfig).GetTypeInfo().Assembly);

            var script = await CSharpScript.RunAsync(code, opts, config);
            return config;
        }
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