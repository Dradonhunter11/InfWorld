using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using InfiniteWorldLibrary;
using Mono.Cecil;
using Mono.CompilerServices.SymbolWriter;
using MonoMod.Cil;

namespace InfWorld.CoreLoader
{
    internal static partial class CoreLoaderHandler
    {
        public static void ModifyTilemapGetIndexer(TypeDefinition definiton, AssemblyDefinition asm)
        {
            MethodDefinition definition = definiton.Methods.First(i => i.Name == "get_Item" && i.Parameters.Count == 2);

            if (definition != null)
            {
                definition.Body.Instructions.Clear();

                FieldReference tileReference =
                    definition.Module.ImportReference(typeof(StaticInstance).GetField("WorldInstance",
                        BindingFlags.Public | BindingFlags.Static));

                ILContext context = new ILContext(definition);
                ILCursor cursor = new ILCursor(context);

                var getItemReference = definition.Module.ImportReference(GetItem);
                var setItemReference = definition.Module.ImportReference(SetItem);

                cursor.EmitLdsfld(tileReference);
                cursor.EmitLdarg1();
                cursor.EmitLdarg2();
                cursor.EmitCall(getItemReference);
                cursor.EmitRet();
            }
            
        }
    }
}
