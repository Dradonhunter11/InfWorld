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
