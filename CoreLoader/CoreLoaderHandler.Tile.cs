using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using InfiniteWorldLibrary.World;
using InfiniteWorldLibrary.World.Region;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;
using Terraria;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;

namespace InfWorld.CoreLoader
{
    internal static partial class CoreLoaderHandler
    {
        private static void ModifyTileID(TypeDefinition definition, AssemblyDefinition terraria)
        {
            FieldDefinition reference = definition.Fields.First(i => i.Name == "TileId");
            var import = terraria.MainModule.ImportReference(terraria.MainModule.TypeSystem.UInt64);
            var import2 = import.Resolve();
            if (reference != null)
            {
                reference.FieldType = import;
                reference.Resolve();
            }

            var cctor = definition.Methods.First(i => i.Name == ".ctor");
            cctor.IsPublic = true;

            var newcctor = new MethodDefinition(".ctor", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, terraria.MainModule.TypeSystem.Void);
            newcctor.AggressiveInlining = true;
            newcctor.Parameters.Add(new ParameterDefinition("TileId", ParameterAttributes.None, terraria.MainModule.TypeSystem.UInt64));

            var context = new ILContext(newcctor);
            var cursor = new ILCursor(context);

            cursor.EmitLdarg0();
            cursor.EmitLdarg1();
            cursor.EmitStfld(reference);
            cursor.EmitRet();

            definition.Methods.Add(newcctor);
        }

        public static TypeReference MakeGenericType(this TypeReference self, params TypeReference[] arguments)
        {
            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException();

            var instance = new GenericInstanceType(self);
            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }

        public static MethodReference MakeGeneric(this MethodReference self, params TypeReference[] arguments)
        {
            var reference = new MethodReference(self.Name, self.ReturnType)
            {
                DeclaringType = self.DeclaringType.MakeGenericType(arguments),
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention,
            };

            foreach (var parameter in self.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

            foreach (var generic_parameter in self.GenericParameters)
                reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));

            return reference;
        }

        private static void ModifyGenericGet(TypeDefinition definition, AssemblyDefinition terraria)
        {
            
            var methodDefinition = definition.Methods.First(i => i.Name == "Get");
            methodDefinition.Body.Instructions.Clear();

            TypeDefinition TileIdConverterDef = asm.MainModule.Types.First(p => p.Name.StartsWith("TileIdConverter"));

            var context = new ILContext(methodDefinition);
            var cursor = new ILCursor(context);

            var a = cursor.Method.Module.ImportReference(TileIdConverterDef);
            cursor.Method.Body.Variables.Add(new VariableDefinition(a));

            // MethodInfo info = typeof(TileExtension).GetMethod("Get", BindingFlags.Static | BindingFlags.Public);
            // info.MakeGenericMethod(definition.GenericParameters[0].ResolveReflection());
            
            TypeDefinition ChunkMapDef = asm.MainModule.Types.First(p => p.Name.StartsWith("TileExtension"));
            MethodDefinition getDef = ChunkMapDef.Methods.First(i => i.Name == "Get");

            var b = cursor.Method.Module.ImportReference(getDef);
            var c = new GenericInstanceMethod(b);
            c.GenericArguments.Add(cursor.Method.GenericParameters[0]);
            

            cursor.EmitLdarg0();
            cursor.EmitCall(c);
            cursor.EmitRet();
        }

        private static void ModifyTileMapClearEverything(TypeDefinition definition, AssemblyDefinition terraria)
        {
            var methodDefinition = definition.Methods.First(i => i.Name == "ClearEverything");
            methodDefinition.Body.Instructions.Clear();

            TypeDefinition chunkDataDef = asm.MainModule.Types.First(p => p.Name.StartsWith("ChunkData") && !p.HasGenericParameters);
            MethodDefinition clearEverythingRef = chunkDataDef.Methods.First(i => i.Name == "ClearEverything");

            var context = new ILContext(methodDefinition);
            var cursor = new ILCursor(context);

            cursor.EmitCall(cursor.Method.Module.ImportReference(clearEverythingRef));
            cursor.EmitRet();
        }

        private static void ModifyCopyFrom(TypeDefinition definition, AssemblyDefinition terraria)
        {
            var methodDefinition = definition.Methods.First(i => i.Name == "CopyFrom");
            methodDefinition.Body.Instructions.Clear();

            TypeDefinition chunkDataDef = asm.MainModule.Types.First(p => p.Name.StartsWith("ChunkData") && !p.HasGenericParameters);
            MethodDefinition copySingleRef = chunkDataDef.Methods.First(i => i.Name == "CopySingle");

            var context = new ILContext(methodDefinition);
            var cursor = new ILCursor(context);

            cursor.EmitLdarg1();
            cursor.EmitLdarg0();
            cursor.EmitCall(cursor.Method.Module.ImportReference(copySingleRef));
            cursor.EmitRet();
        }
    }
}
