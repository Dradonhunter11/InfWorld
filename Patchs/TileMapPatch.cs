using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CorePatcher;
using CorePatcher.Attributes;
using InfiniteWorldLibrary;
using InfiniteWorldLibrary.World;
using MonoMod.Cil;
using Terraria;

namespace InfWorld.Patchs
{
    [PatchType("Terraria.Tilemap")]
    public class TileMapPatch : ModCorePatch
    {
        internal static PropertyInfo IndexerInfo = typeof(World).GetProperty("Item",
            BindingFlags.Public | BindingFlags.Instance, null, typeof(Tile),
            new Type[] { typeof(int), typeof(int) }, null);
        internal static MethodInfo GetItem = IndexerInfo.GetGetMethod();
        internal static MethodInfo SetItem = IndexerInfo.GetSetMethod();

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


        private static void ModifyTileMapClearEverything(TypeDefinition definition, AssemblyDefinition terraria)
        {
            var methodDefinition = definition.Methods.First(i => i.Name == "ClearEverything");
            methodDefinition.Body.Instructions.Clear();

            TypeDefinition chunkDataDef = InfiniteWorldModSystem.InfiniteWorldLibraryDef.MainModule.Types.First(p => p.Name.StartsWith("ChunkData") && !p.HasGenericParameters);
            MethodDefinition clearEverythingRef = chunkDataDef.Methods.First(i => i.Name == "ClearEverything");

            var context = new ILContext(methodDefinition);
            var cursor = new ILCursor(context);

            cursor.EmitCall(cursor.Method.Module.ImportReference(clearEverythingRef));
            cursor.EmitRet();
        }
    }
}
