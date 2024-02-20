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
        private static void ModifyDrawWalls(TypeDefinition type, AssemblyDefinition terraria)
        {
            var field = type.Fields.FirstOrDefault(p => p.Name == "_tileArray");

            TypeDefinition definition = asm.MainModule.Types.First(p => p.Name.StartsWith("World"));
            TypeReference reference = type.Module.ImportReference(definition);



            if (field != null)
            {
                field.FieldType = reference;
                // type.Fields.Remove(field);
                // type.Fields.Add(fieldReference);
            }

            PatchBrokenILIOnWallDrawing(type.Methods.FirstOrDefault(m => m.Name == "DrawWalls"));
            PatchBrokenILIOnWallDrawing(type.Methods.FirstOrDefault(m => m.Name == "FullTile"));
        }

        private static void PatchBrokenILIOnWallDrawing(MethodDefinition definition)
        {
            ILContext context = new ILContext(definition);
            var cursor = new ILCursor(context);
            FieldReference reference = null;
            cursor.Index = 0;

            var a = cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdsfld(out reference),
                i => i.MatchLdloc0(),
                i => i.MatchLdloc1(),
                i => i.MatchCallvirt(out _));
            while (cursor.TryGotoNext(MoveType.Before,
                       i => i.MatchLdsfld(out reference),
                       i => i.MatchLdloc0(),
                       i => i.MatchLdloc1(),
                       i => i.MatchCallvirt(out _)))
            {
                if (reference != null)
                    cursor.Body.Instructions.RemoveAt(cursor.Index);
            }
        }
    }
}
