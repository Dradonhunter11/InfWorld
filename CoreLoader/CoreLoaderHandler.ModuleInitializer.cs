using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeAttributes = Mono.Cecil.TypeAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace InfWorld.CoreLoader
{
    internal static partial class CoreLoaderHandler
    {
        public static void PatchMainClass(TypeDefinition type, AssemblyDefinition terraria) {
            var Context = new ILContext(type.Methods.FirstOrDefault(p => p.Name == "RunGame"));
            var ILCursor = new ILCursor(Context);

            /*ILCursor.EmitDelegate<Action>(() => {
                Assembly.LoadFrom(Path.Combine(Environment.CurrentDirectory, "InfWorld.dll"));
            });*/
        }

        static void random() {
            Assembly.LoadFrom(Path.Combine(Environment.CurrentDirectory, "InfWorld.dll"));
        }

        static void PatchInitializer(TypeDefinition type, AssemblyDefinition terraria)
        {
            // Find the module initializer method (module constructor)
            MethodDefinition moduleInitializer = FindOrCreateModuleInitializer(terraria.MainModule);

            var Context = new ILContext(moduleInitializer);
            var ILCursor = new ILCursor(Context);

            ILCursor.EmitDelegate<Action>(() => {
                Assembly.LoadFrom(Path.Combine(Environment.CurrentDirectory, "InfWorld.dll"));
                return;
            });
            ILCursor.EmitRet();
        }

        static MethodDefinition FindOrCreateModuleInitializer(ModuleDefinition module)
        {
            
            // If it doesn't exist, create a new one
            TypeDefinition moduleInitializerType = new TypeDefinition("", "<Module>", TypeAttributes.BeforeFieldInit | TypeAttributes.Sealed | TypeAttributes.Abstract);
            module.Types.Add(moduleInitializerType);

            var moduleInitializer = new MethodDefinition(".cctor", MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, module.TypeSystem.Void);
            moduleInitializerType.Methods.Add(moduleInitializer);

            // Add the module initializer to the module type
            module.Types.Add(moduleInitializerType);
            

            return moduleInitializer;
        }
    }
}
