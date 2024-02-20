using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace InfWorld.CoreLoader
{
    internal static partial class CoreLoaderHandler
    {
        public static void PatchWorldGenClear(TypeDefinition definition, AssemblyDefinition terraria)
        {
            var method = definition.Methods.FirstOrDefault(i => i.Name == "clearWorld");
            if (method != null)
            {
                method.Body.Instructions.Clear();
                method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            }
        }
    }
}
