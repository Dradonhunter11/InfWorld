//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Reflection.Emit;
//using System.Text;
//using System.Threading.Tasks;
//using Mono.Cecil;
//using Mono.Cecil.Cil;

//namespace InfWorld.Builder
//{
//    class AsmBuilder
//    {
//        public readonly BindingFlags FlagsType = BindingFlags.Static | BindingFlags.Instance |
//                                                 BindingFlags.DeclaredOnly | BindingFlags.Public |
//                                                 BindingFlags.NonPublic;

//        public AssemblyBuilder asmbuilder;
//        public ModuleBuilder moduleBuilder;
//        public AssemblyName asmName;

//        public AssemblyDefinition AsmDefinition;
//        public ModuleDefinition ModuleDefinition;

//        public AsmBuilder()
//        {
//            asmName = new AssemblyName("TerrariaInfiniteWorld");
//            asmName.Version = new Version(1, 3, 5, 3);
//            asmbuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);
//            moduleBuilder = asmbuilder.DefineDynamicModule(asmName.Name, asmName.Name + ".exe");

//            AsmDefinition = AssemblyDefinition.PatchAssembly("tModLoader64bit.exe");
//            ModuleDefinition = AsmDefinition.MainModule;
//        }

//        public void Run()
//        {
            
//        }

//        public void RecreateType()
//        {

//        }

//        public void RecreateField(TypeBuilder builder, Type type)
//        {
//            foreach (var fieldInfo in type.GetFields(FlagsType))
//            {
//                builder.DefineField(fieldInfo.Name, fieldInfo.FieldType, fieldInfo.Attributes);
//            }
//        }

//        public void RecreateMethod(TypeBuilder builder, TypeDefinition definition, Type type)
//        {
//            foreach (var methodInfo in type.GetMethods(FlagsType))
//            {
//                var methodBuilder = builder.DefineMethod(methodInfo.Name, methodInfo.Attributes, methodInfo.ReturnType,
//                    methodInfo.GetParameters().Select(i => i.ParameterType).ToArray());
//                var ilGenerator = methodBuilder.GetILGenerator();
//                MethodDefinition methodDefinition = definition.Methods.SingleOrDefault(i => i.Name == methodBuilder.Name && i.Parameters.Count == methodBuilder.GetParameters().Length && i.ReturnType.Name == methodInfo.ReturnType.Name);
//                ILProcessor processor = methodDefinition.Body.GetILProcessor();
//                foreach (var instruction in processor.Body.Instructions)
//                {
//                    System.Reflection.Emit.OpCode opCode = new System.Reflection.Emit.OpCode();
//                    var cecilOpCode = instruction.OpCode;
//                    opCode.Name = cecilOpCode.Name;

//                    ilGenerator.Emit(instruction.OpCode);
//                }
//            }
//        }


//        public void RecreateConstructor()
//        {

//        }

//        public void PatchInstruction()
//        {

//        }

//        public void TranslateCecilToReflection()
//        {

//        }

//    }
//}
