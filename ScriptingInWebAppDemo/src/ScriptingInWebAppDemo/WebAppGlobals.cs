using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.Scripting.Hosting;

namespace ScriptingInWebAppDemo
{
    public class WebAppGlobals : InteractiveScriptGlobals
    {
        public void NewEndpoint(Func<HttpContext, Func<Task>, Task> endpoint)
        {
            Startup.DynamicMiddleware.Add(endpoint);
        }

        public WebAppGlobals(TextWriter outputWriter, ObjectFormatter objectFormatter) : base(outputWriter, objectFormatter)
        {
        }
    }
}