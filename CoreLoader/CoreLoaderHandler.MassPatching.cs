using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using InfiniteWorldLibrary;
using InfiniteWorldLibrary.World;
using MonoMod.Cil;
using Terraria;
using Mono.CompilerServices.SymbolWriter;

namespace InfWorld.CoreLoader
{
    // TODO: Make this a proper class that is automatically loaded
    internal static partial class CoreLoaderHandler
    {
        internal static PropertyInfo IndexerInfo = typeof(World).GetProperty("Item",
            BindingFlags.Public | BindingFlags.Instance, null, typeof(Tile),
            new Type[] { typeof(Int32), typeof(Int32) }, null);
        internal static MethodInfo GetItem = IndexerInfo.GetGetMethod();
        internal static MethodInfo SetItem = IndexerInfo.GetSetMethod();


        public static void PatchEveryMethod(TypeDefinition type, AssemblyDefinition terraria) {
            var methodDefintions = type.Methods;

            if (type.Name.Contains("TileIOImpl"))
            {
                var a = type;
            }

            var filteredMethodDefintions = methodDefintions.Where(p => p.HasBody);

            
            foreach (var methodDef in filteredMethodDefintions)
            {
                var instructions = methodDef.Body.Instructions;

                var getItemReference = methodDef.Module.ImportReference(GetItem);
                var setItemReference = methodDef.Module.ImportReference(SetItem);
                

                FieldReference tileReference =
                                methodDef.Module.ImportReference(typeof(StaticInstance).GetField("WorldInstance",
                                    BindingFlags.Public | BindingFlags.Static));
                int i = 0;
                foreach (var instruction in instructions)
                {
                    i++;
                    if (instruction.OpCode == OpCodes.Ldsfld) {
                        if (instruction.Operand is FieldReference fieldRef && fieldRef.DeclaringType.FullName == "Terraria.Main" && fieldRef.Name == "tile")
                        {
                            instruction.Operand = tileReference;
                            continue;
                        }
                    }
                    if (instruction.OpCode == OpCodes.Ldfld)
                    {
                        if (instruction.Operand is FieldReference fieldRef && fieldRef.FullName == "Terraria.World")
                        {
                            instruction.OpCode = OpCodes.Ldsfld;
                            instruction.Operand = tileReference;
                            continue;
                        }
                    }

                    if (instruction.OpCode == OpCodes.Ldsflda)
                    {
                        if (instruction.Operand is FieldReference fieldRef && fieldRef.DeclaringType.FullName == "Terraria.Main" && fieldRef.Name == "tile")
                        {
                            instruction.OpCode = OpCodes.Ldsfld;
                            instruction.Operand = tileReference;
                            continue;
                        }
                    }

                    if (instruction.OpCode == OpCodes.Ldflda)
                    {
                        if (instruction.Operand is FieldReference fieldRef)
                        {
                            if (IsWallDrawingTileMap(fieldRef))
                            {
                                int previousIndex = i - 2;
                                
                                if (instructions[previousIndex].OpCode == OpCodes.Ldarg_0)
                                {
                                    instructions[previousIndex].OpCode = OpCodes.Nop;
                                }

                                instruction.OpCode = OpCodes.Ldsfld;
                                instruction.Operand = tileReference;
                                continue;
                            }

                            if (IsMainTileMap(fieldRef))
                            {
                                instruction.OpCode = OpCodes.Ldsfld;
                                instruction.Operand = tileReference;
                                continue;
                            }
                        }
                    }

                    if ((instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt) && instruction.Operand is Mono.Cecil.MethodReference methodReference)
                    {
                        switch (methodReference.FullName)
                        {
                            case "Terraria.Tile Terraria.Tilemap::get_Item(System.Int32,System.Int32)":
                                instruction.OpCode = OpCodes.Callvirt;
                                instruction.Operand = getItemReference;
                                break;
                            case "System.Void Terraria.Tilemap[0...,0...]::set_Item(System.Int32,System.Int32,Terraria.Tile)":
                                instruction.OpCode = OpCodes.Callvirt;
                                instruction.Operand = setItemReference;
                                break;
                            case "Terraria.Tile[0..., 0...] Terraria.World::get_Tiles()":
                                instruction.OpCode = OpCodes.Callvirt;
                                instruction.Operand = getItemReference;
                                break;
                            default:
                            {
                                if (methodReference.FullName.Contains("Terraria.Tile[0..., 0...] Terraria.World::set_Tiles(System.Int32,System.Int32,Terraria.Tile)"))
                                {
                                    instruction.OpCode = OpCodes.Callvirt;
                                    instruction.Operand = setItemReference;
                                }
                                else if (methodReference.FullName.Contains("set_Item(System.Int32,System.Int32,Terraria.Tile)"))
                                {
                                    instruction.OpCode = OpCodes.Callvirt;
                                    instruction.Operand = setItemReference;
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static bool IsMainTileMap(FieldReference fieldRef)
        {
            return fieldRef.DeclaringType.FullName == "Terraria.Main" && fieldRef.Name == "tile";
        }

        private static bool IsWallDrawingTileMap(FieldReference fieldRef)
        {
            return fieldRef.DeclaringType.FullName == "Terraria.GameContent.Drawing.WallDrawing" && fieldRef.Name == "_tileArray";
        }
    }
}
