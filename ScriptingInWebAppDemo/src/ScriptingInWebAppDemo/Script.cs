using System.Collections.Generic;

namespace ScriptingInWebAppDemo
{
    public class Script
    {
        public string Code { get; set; }
        public HashSet<string> Namespaces { get; set; }
        public HashSet<string> Assemblies { get; set; }
    }
}