using System;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyModel;

namespace ScriptingInWebAppDemo.Controllers
{
    [Route("api/[controller]")]
    public class ScriptController : Controller
    {
        private readonly ScriptOptions _opts;

        public ScriptController()
        {
            //var runtimeId = RuntimeEnvironment.GetRuntimeIdentifier();
            _opts = ScriptOptions.Default.AddReferences(
                    typeof(ScriptController).GetTypeInfo().Assembly,
                    typeof(object).GetTypeInfo().Assembly,
                    typeof(HttpResponseWritingExtensions).GetTypeInfo().Assembly).
                AddImports("ScriptingInWebAppDemo", "Microsoft.AspNetCore.Http");

            /*var inheritedAssemblyNames = DependencyContext.Default.GetRuntimeAssemblyNames(runtimeId).Where(x =>
                x.FullName.ToLowerInvariant().StartsWith("system.runtime") ||
                x.FullName.ToLowerInvariant().StartsWith("microsoft.codeanalysis"));

            foreach (var inheritedAssemblyName in inheritedAssemblyNames)
            {
                var assembly = Assembly.Load(inheritedAssemblyName);
                _opts = _opts.AddReferences(assembly);
            }*/
        }

        [HttpPost("")]
        public async Task<IActionResult> Post([FromBody]Script script)
        {
            if (script?.Code == null) return BadRequest();

            var output = new BufferTextWriter();
            var globals = new WebAppGlobals(output, CSharpObjectFormatter.Instance);

            var compiledScript = CSharpScript.Create(script.Code, _opts, typeof(WebAppGlobals));
            var diagnostics = compiledScript.GetCompilation().GetDiagnostics();

            if (diagnostics.Any())
            {
                return Ok(new
                {
                    error = string.Join(Environment.NewLine, diagnostics.Select(x => x.GetMessage()))
                });
            }

            var result = await compiledScript.RunAsync(globals);

            if (result.Exception != null)
            {
                return Ok(new
                {
                    error = result.Exception.Message
                });
            }

            if (!string.IsNullOrWhiteSpace(output.Buffer))
            {
                return Ok($"{output.Buffer}{Environment.NewLine}{result.ReturnValue}");
            }
            return Ok(result.ReturnValue);
        }
    }
}


            /*
NewEndpoint(async (c, next) => {
                if (c.Request.Path.ToString().Contains("foo"))
                {
                    await c.Response.WriteAsync("bar!");
                    return;
                }

                await next();
            });

             */