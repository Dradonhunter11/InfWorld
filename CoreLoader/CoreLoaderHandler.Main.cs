using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MonoMod.Cil;
using Terraria;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace InfWorld.CoreLoader
{
    internal static partial class CoreLoaderHandler
    {
        private static void ModifyMain(TypeDefinition type, AssemblyDefinition terraria)
        {
            var field = type.Fields.FirstOrDefault(p => p.Name == "tile");

            TypeDefinition definition = asm.MainModule.Types.First(p => p.Name.StartsWith("World"));
            TypeReference reference = type.Module.ImportReference(definition);

            var fieldReference = new FieldDefinition("infTile", Mono.Cecil.FieldAttributes.Public | Mono.Cecil.FieldAttributes.Static, reference);

            if (field != null)
            {
                // type.Fields.Remove(field);
                type.Fields.Add(fieldReference);
            }
        }

        private static void ModifySetTitle(TypeDefinition type, AssemblyDefinition terraria)
        {
            var method = type.Methods.FirstOrDefault(m => m.Name == "SetTitle");

            ILCursor ilCursor = new ILCursor(new ILContext(method));

            ilCursor.GotoNext(MoveType.Before,
                i => i.MatchCall(out _),
                i => i.MatchLdarg0(),
                i => i.MatchCall(out _),
                i => i.MatchLdarg0());
            ilCursor.EmitLdarg0();
            ilCursor.Emit(OpCodes.Ldstr, "Terraria infinite world: Actually infinite!");
            ilCursor.EmitStfld(type.Fields.FirstOrDefault(f => f.Name == "_cachedTitle"));
        }

        private static void ModifyAssemblyManager(TypeDefinition type, AssemblyDefinition terraria)
        {
            var method = type.Methods.FirstOrDefault(m => m.Name == "ForceJITOnMethod");
            
            var reference = type.Module.ImportReference(typeof(MethodBase).GetProperty("Name", BindingFlags.Public | BindingFlags.Instance).GetMethod);
            
            method.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Ldarg_0));
            method.Body.Instructions.Insert(1, Instruction.Create(OpCodes.Callvirt, reference));
            method.Body.Instructions.Insert(2, Instruction.Create(OpCodes.Call, terraria.MainModule.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }))));
        }

        private static void ModifyVersionStringDefault(TypeDefinition type, AssemblyDefinition terraria)
        {
            MethodDefinition staticConstructor = type.Methods.FirstOrDefault(m => m.Name == ".cctor");


            if (staticConstructor == null)
            {
                staticConstructor = new MethodDefinition(".cctor", MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, terraria.MainModule.TypeSystem.Void);
                type.Methods.Add(staticConstructor);
            }


            EditStaticFieldString(type.Fields.FirstOrDefault(p => p.Name == "versionNumber"), "v1.4.4.9 Infinite World");
            EditStaticFieldString(type.Fields.FirstOrDefault(p => p.Name == "versionNumber2"), "v1.4.4.9 Infinite World");

            TypeDefinition definition = terraria.MainModule.TypeSystem.String.Resolve();
            TypeReference stringTypeReference = definition.Resolve();

            var fieldReference = new FieldDefinition("versionNumber3", Mono.Cecil.FieldAttributes.Public | Mono.Cecil.FieldAttributes.Static, stringTypeReference);
            type.Fields.Add(fieldReference);
            EditStaticFieldString(fieldReference, "v1.4.4.9 Infinite World");
        }

        private static void ChangeAccessLevel(TypeDefinition typeDefinition, AssemblyDefinition terraria)
        {
            typeDefinition.IsNotPublic = false;

            var definition = typeDefinition.Methods.FirstOrDefault(m => m.Name == "ClearWorld");

            if(definition != null)
            {
                definition.IsPublic = true;
            }
        }

        private static void ModifyFieldValue(FieldDefinition definition, object? value)
        {
            if (definition != null)
            {
                definition.Constant = value;
            }
        }

        private static void EditStaticFieldString(FieldDefinition definition, string value) {
            MethodDefinition staticConstructor = definition.DeclaringType.Methods.FirstOrDefault(m => m.Name == ".cctor");

            if (staticConstructor != null)
            {
                ILProcessor processor = staticConstructor.Body.GetILProcessor();

                IList<Instruction> instructions = new List<Instruction>();
                instructions.Add(processor.Create(OpCodes.Ldstr, value));
                instructions.Add(processor.Create(OpCodes.Stsfld, definition));
                foreach (Instruction instruction in instructions) { 
                    processor.Body.Instructions.Insert(processor.Body.Instructions.Count - 2, instruction);
                }
            }
        }
    }
}
