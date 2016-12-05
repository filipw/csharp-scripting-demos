#r "bin/FakeLib.dll"
#r "bin/FSharp.Core.dll"
#r "bin/FSharpx.Extras.dll"
#r "bin/System.Runtime.dll"

using Fake;
using FSharpx;
using System.Linq;
using System.IO;
using static FSharpx.FSharpFunc;
using static Fake.TargetHelper;
using static Fake.MSBuildHelper;
using static Fake.NuGetHelper;
using static Fake.RestorePackageHelper;
using static Fake.FileHelper;

var projectFolder = "../ScriptingDemos";
var outputPath = "../artifacts";
var nugetPath = "../tools/nuget.exe";

Target("Default", FromAction(() => {
  Console.WriteLine("Woohoo, nothing to do!");
}));

Target("Clean", FromAction(() => {
  DeleteDirs(new[] { "../artifacts", $"{projectFolder}/bin", $"{projectFolder}/obj" });
}));

Target("Restore", FromAction(() => {
  RestorePackage(Fun<RestorePackageParams>(restore => {
    restore.ToolPath = nugetPath;
    restore.OutputPath = "../packages";
    return restore;
    }), $"{projectFolder}/packages.config");
}));

Target("Build", FromAction(() => {
  MSBuildLoggers = new string[0].ToFSharpList();
  
  build(Fun<MSBuildParams>(msBuild => {
    msBuild.Verbosity = MSBuildVerbosity.Detailed.ToFSharpOption();
    msBuild.NoLogo = true;
    msBuild.RestorePackagesFlag = true;
    return msBuild;
  }), $"{projectFolder}/ScriptingDemos.csproj");
}));

Target("Pack", FromAction(() => {
  if (!Directory.Exists(outputPath)) {
    Directory.CreateDirectory(outputPath);
  }
  NuGetPack(Fun<NuGetParams>(nuget => {
    nuget.Version = "0.1.0-rc";
    nuget.ToolPath = nugetPath;
    nuget.IncludeReferencedProjects = true;
    nuget.WorkingDir = outputPath;
    nuget.OutputPath = outputPath;
    return nuget;
  }), $"{projectFolder}/ScriptingDemos.csproj");
}));

dependency("Build", "Clean");
dependency("Build", "Restore");
dependency("Pack", "Build");

var targetName = Args.FirstOrDefault() ?? "Default";
run(targetName);

static Microsoft.FSharp.Core.FSharpFunc<TParams, TParams> Fun<TParams>(Func<TParams, TParams> func) => FSharpx.FSharpFunc.FromFunc(func);