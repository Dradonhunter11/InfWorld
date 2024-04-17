using System.IO;
using System;
using Terraria.ModLoader;
using System.Reflection;
using CorePatcher;

namespace InfWorld;

public class InfiniteWorldModSystem : ModSystem
{
    internal static AssemblyDefinition InfiniteWorldLibraryDef;

    public override void Load()
    {

        CorePatcher.PatchLoader.RegisterPrePatchOperation(() =>
        {
            SavePhysicalAssembly("InfWorld.dll");
            SavePhysicalAssembly("lib/InfiniteWorldLibrary.dll");
            InfiniteWorldLibraryDef = LoadPhysicalAssemblyFromModules("InfiniteWorldLibrary.dll");
            PatchLoader.AddDeps(InfiniteWorldLibraryDef);
        });
    }

    private static void SavePhysicalAssembly(string assemblyName)
    {

        var module = InfWorld.Instance.GetFileBytes(assemblyName);
        Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "modules"));
        File.WriteAllBytes(Path.Combine(Environment.CurrentDirectory, "modules", Path.GetFileName(assemblyName)), module);
    }

    private static void SaveModDll(Mod mod)
    {
        var module = mod.GetFileBytes(mod.Name + ".dll");
        Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "modules"));
        File.WriteAllBytes(Path.Combine(Environment.CurrentDirectory, "modules", Path.GetFileName(mod.Name + ".dll")), module);
    }

    private static AssemblyDefinition LoadPhysicalAssemblyFromModules(string assemblyName)
    {
        string path = Path.Combine(Environment.CurrentDirectory, "modules", assemblyName);
        return AssemblyDefinition.ReadAssembly(path, new ReaderParameters(ReadingMode.Immediate)
        {
            ReadWrite = true,
            InMemory = true
        });
    }

}