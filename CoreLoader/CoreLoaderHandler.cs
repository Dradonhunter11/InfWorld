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

            ModifyVersionStringDefault(a.MainModule.Types.First(p => p.Name == "Main"), a);
        }
    }
}
