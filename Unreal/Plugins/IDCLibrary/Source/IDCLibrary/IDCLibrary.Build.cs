using UnrealBuildTool;
using System;
using System.IO;

public class IDCLibrary : ModuleRules {
    public IDCLibrary(ReadOnlyTargetRules Target) : base(Target) {
        PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;

        PrecompileForTargets = PrecompileTargetsType.Any;

        PrivateDependencyModuleNames.AddRange( new string[] { "CoreUObject", "Engine", "Slate", "SlateCore", });
        PublicDependencyModuleNames.AddRange( new string[] { "Core", "Json", "JsonUtilities" });
        LoadIDCLib(Target);
    }
    
    private string ThirdPartyPath {
        get { return Path.GetFullPath(Path.Combine(ModuleDirectory, "../ThirdParty/")); }
    }

    public void LoadIDCLib(ReadOnlyTargetRules Target)
    {
        if (Target.Platform == UnrealTargetPlatform.Win64) {
            string IDCPath = Path.Combine(ThirdPartyPath, "IDCLib");
            string LibPath = Path.Combine(IDCPath, "Libraries", "Win64");
            string DllPath = Path.Combine(IDCPath, "Dlls", "Win64");

            //Add Include path 
            PublicIncludePaths.AddRange(new string[] { Path.Combine(IDCPath, "Includes") });

            //Add Static Libraries
            PublicAdditionalLibraries.Add(Path.Combine(LibPath,"IDCLib2.lib"));

            //Add Dynamic Libraries
            // PublicDelayLoadDLLs.Add("IDCLib2.dll");

            // Copy Binary on Build.
            CopyToBinary(Path.Combine(DllPath, "IDCLib2.dll"), Target);

            // Runtime Dependencies.
            RuntimeDependencies.Add("$(TargetOutputDir)/IDCLib2.dll", Path.Combine(DllPath, "IDCLib2.dll"));
        }
    }

    private void CopyToBinary(string Filepath, ReadOnlyTargetRules Target)
    {

        string binariesDir = Path.Combine(Directory.GetParent(ModuleDirectory).Parent.ToString(), "Binaries", Target.Platform.ToString());
        string filename = Path.GetFileName(Filepath);

        System.Console.WriteLine($"[FPA] Copy {Filepath} to {binariesDir}");
        if (!Directory.Exists(binariesDir))
            Directory.CreateDirectory(binariesDir);
        if (!File.Exists(Path.Combine(binariesDir, filename)))
            File.Copy(Filepath, Path.Combine(binariesDir, filename), true);
    }

}
