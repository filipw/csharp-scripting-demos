#r "bin/FakeLib.dll"
#r "bin/FSharp.Core.dll"
#r "bin/FSharpx.Extras.dll"

using Fake;
using FSharpx;
using System.Linq;
using System.IO;
using static FSharpx.FSharpFunc;
using static Fake.TargetHelper;
using static Fake.FileHelper;
using static Fake.DotNetCli;

var projectFolder = "../NetCore/ScriptingDemos";
var outputPath = "../artifacts";

Target("Default", FromAction(() => {
  Console.WriteLine("Woohoo, nothing to do!");
}));

Target("Clean", FromAction(() => {
  DeleteDirs(new[] { "../artifacts", $"{projectFolder}/bin", $"{projectFolder}/obj" });
}));

Target("Restore", FromAction(() => {
  Restore(Fun<RestoreParams>(r => {
    r.WorkingDir = projectFolder;
    return r;
  }));
}));

Target("Build", FromAction(() => {
  Build(Fun<BuildParams>(b => {
    b.Configuration = "Release";
    b.WorkingDir = projectFolder;
    return b;
  }));
}));

Target("Pack", FromAction(() => {
  if (!Directory.Exists(outputPath)) {
    Directory.CreateDirectory(outputPath);
  }
  Pack(Fun<PackParams>(p => {
    p.WorkingDir = projectFolder;
    p.Configuration = "Release";
    p.OutputPath = Path.Combine("..", outputPath);
    p.VersionSuffix = "beta";
    return p;
  }));
}));

dependency("Build", "Clean");
dependency("Build", "Restore");
dependency("Pack", "Build");

var targetName = Args.FirstOrDefault() ?? "Default";
run(targetName);

static Microsoft.FSharp.Core.FSharpFunc<TParams, TParams> Fun<TParams>(Func<TParams, TParams> func) => FSharpx.FSharpFunc.FromFunc(func);