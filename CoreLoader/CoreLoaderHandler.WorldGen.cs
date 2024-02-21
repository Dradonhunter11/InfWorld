using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;

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

        public static void PatchTileFrame(TypeDefinition definition, AssemblyDefinition terraria)
        {
            var method = definition.Methods.FirstOrDefault(i => i.Name == "TileFrame");
            if (method != null)
            {
                var il = new ILContext(method);
                var cursor = new ILCursor(il);
                Instruction jumpPoint = null;
                int positionToSave;

                cursor.Index = 2;
                for (int i = 2; i < 25; i++) 
                {
                    method.Body.Instructions[i].OpCode = OpCodes.Nop;
                    method.Body.Instructions[i].Operand = null;
                }
            }
        }
    }
}
