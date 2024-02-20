using InfWorld.Patching;
using Mono.Cecil;
using Mono.Collections.Generic;
using MonoMod.Cil;
using MonoMod.Utils;
using ReLogic.Localization.IME;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InfiniteWorldLibrary;
using Terraria;
using Terraria.Utilities;

namespace InfWorld.CoreLoader
{
    internal static partial class CoreLoaderHandler
    {
        private static AssemblyDefinition asm = LoadPhysicalAssemblyFromModules("InfiniteWorldLibrary.dll");

        public static void WriteAssembly() { }

        public static void PatchAssembly() {
            if (typeof(Main).Assembly.GetName().Version == new Version(2, 0, 0, 0))
            {
                return;
            }

            var currentFile = Path.Combine(Environment.CurrentDirectory, "tmodloader.dll");
            var newFile = Path.Combine(Environment.CurrentDirectory, "tmodloaderInfWorld.dll");
            if (File.Exists(newFile)) { 
                File.Delete(newFile);
            }
            File.Copy(currentFile, newFile);
            currentFile = newFile;

            using var a = AssemblyDefinition.ReadAssembly(currentFile, new ReaderParameters(ReadingMode.Immediate) 
            { 
                ReadWrite = true, 
                InMemory = true
            });

            // For loop that will patch every method in an assembly
            /*
            foreach(var m in a.Modules)
            {
                Parallel.ForEach(m.Types, (type) =>
                {
                    if (type.HasNestedTypes)
                    {
                        foreach (var typeDefinition in type.NestedTypes)
                        {
                            PatchEveryMethod(typeDefinition, a);
                        }
                    }
                    PatchEveryMethod(type, a);
                });
            }*/
            // Method that can patch the module initializer
            // PatchInitializer(null, a);
            // Method to extract mod code directly
            SavePhysicalAssembly("InfWorld.dll");
            SavePhysicalAssembly("lib/InfiniteWorldLibrary.dll");
            // Method that add a field to the main class
            ModifyTilemapGetIndexer(a.MainModule.Types.First(p => p.FullName == "Terraria.Tilemap"), a);
            //ModifyMain(a.MainModule.Types.First(p => p.Name == "Main"), a);
            ModifySetTitle(a.MainModule.Types.First(p => p.Name == "Main"), a);
            //PatchMainClass(a.MainModule.Types.First(p => p.Name == "Program"), a);
            //ModifyDrawWalls(a.MainModule.Types.First(p => p.Name == "WallDrawing"), a);
            //ModifyAssemblyManager(a.MainModule.Types.First(p => p.FullName == "Terraria.ModLoader.Core.AssemblyManager"), a);
            //ModifyWorldIO(a.MainModule.Types.First(p => p.FullName == "Terraria.ModLoader.IO.WorldIO"), a);
            ModifyTileID(a.MainModule.Types.First(p => p.Name == "Tile"), a);
            ModifyGenericGet(a.MainModule.Types.First(p => p.Name == "Tile"), a);
            // Method that modify the default value of versionString in the assembly 
            ModifyVersionStringDefault(a.MainModule.Types.First(p => p.Name == "Main"), a);
            // Method that inject reference to the assembly
            // InjectReference(a);
            PatchWorldGenClear(a.MainModule.Types.First(p => p.Name == "WorldGen"), a);
            ModifyTileMapClearEverything(a.MainModule.Types.First(p => p.FullName == "Terraria.Tilemap"), a);
            ModifyCopyFrom(a.MainModule.Types.First(p => p.Name == "Tile"), a);
            // Change access level of the class TileIO
            // ChangeAccessLevel(a.MainModule.Types.First(p => p.FullName == "Terraria.ModLoader.IO.TileIO"), a);
            // Change assembly version, kinda useless
            a.Name.Version = new Version(2, 0, 0, 0);
            // Write the patched assembly
            a.Write(Path.ChangeExtension(currentFile, ".patched.dll")); 
            if (File.Exists(newFile))
            {
                File.Delete(newFile);
            }

            return;
            //return a;
        }

        private static void InjectReference(AssemblyDefinition definition) {
            if(definition.MainModule.AssemblyReferences.All(m => m.FullName != "InfiniteWorldLibrary")) {
                var asmnameref = AssemblyNameReference.Parse(typeof(StaticInstance).Assembly.FullName);
                definition.MainModule.AssemblyReferences.Add(asmnameref);
            }
        }

        private static void SavePhysicalAssembly(string assemblyName) {

            var module = InfWorld.Instance.GetFileBytes(assemblyName);
            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "modules"));
            File.WriteAllBytes(Path.Combine(Environment.CurrentDirectory, "modules", Path.GetFileName(assemblyName)), module);
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

        private static void ChangeAppDomain(string newAssemblyPath) { 
            if(Platform.IsWindows) { 
                MessageBox.Show("Restarting the game with patch applied...", "Core Loader");
            }

            Process process = new Process();
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = "tmodloaderInfWorld.dll";
            process.Start();
            Environment.Exit(0);
        }
    }
}
