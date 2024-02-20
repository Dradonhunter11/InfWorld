using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using MonoMod.Utils;
using Terraria;
using Terraria.ModLoader;
using EOpCode = System.Reflection.Emit.OpCode;
using EOpCodes = System.Reflection.Emit.OpCodes;
using COpCodes = Mono.Cecil.Cil.OpCodes;
using CStackBehaviour = Mono.Cecil.Cil.StackBehaviour;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.IO;
using InfiniteWorldLibrary.Utils;
using InfiniteWorldLibrary.World;
using Mono.Cecil.Rocks;
using InfWorld.Patching.ILPatches;

namespace InfWorld.Patching
{
    internal static class MassPatcher
    {
        private const BindingFlags RequiredFlags =
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
            BindingFlags.Static | BindingFlags.DeclaredOnly;

        public static void StartPatching(Type sourceType)
        {
            // StartPatching(sourceType.Assembly);
        }

        public static void PatchType(TypeDefinition type) {
            var inspectedMethods = new HashSet<MethodDefinition>();

            var toPatch = new List<MethodDefinition>(4096);
            var patchQueue = new List<MethodDefinition>();
            object addLock = new object();

            
            foreach (MethodDefinition method in type.GetMethods())
            {
                toPatch.Add(method);
                inspectedMethods.Add(method);
            }
            

            void TryQueueMethod(MethodDefinition method)
            {
                lock (addLock)
                {
                    if (!inspectedMethods.Contains(method))
                    {
                        patchQueue.Add(method);
                        inspectedMethods.Add(method);
                    }
                }
            }
            void DoPatch(MethodDefinition method)
            {
                PatchMethod(method, TryQueueMethod);
            }

            while (toPatch.Count > 0)
            {
                //toPatch.Sort((a, b) => (a.DeclaringType.FullName + ':' + a.Name).CompareTo(b.DeclaringType.FullName + ':' + b.Name));
                //foreach (var n in toPatch)
                //    DoPatch(n);
                Parallel.For(0, toPatch.Count, i => DoPatch(toPatch[i]));
                toPatch.Clear();
                lock (addLock)
                {
                    // swap
                    var temp = patchQueue;
                    patchQueue = toPatch;
                    toPatch = temp;
                }
            }
        }
        /*
        public static void StartPatching(Assembly asm)
        {
            Type[] array = asm.GetTypes();
            SetLoadingStatusText("Currently patching " + asm.FullName, 0);

            // removing blacklisted types from the array
            int typeCount = array.Length;

            var inspectedMethods = new HashSet<MethodBase>();

            var toPatch = new List<MethodBase>(4096);
            var patchQueue = new List<MethodBase>();
            object addLock = new object();

            for (int i = 0; i < typeCount; i++)
            {
                foreach (MethodInfo method in array[i].GetMethods(RequiredFlags))
                {
                    toPatch.Add(method);
                    inspectedMethods.Add(method);
                }
            }

            void TryQueueMethod(MethodBase method)
            {
                lock (addLock)
                {
                    if (!inspectedMethods.Contains(method))
                    {
                        patchQueue.Add(method);
                        inspectedMethods.Add(method);
                    }
                }
            }
            void DoPatch(MethodBase method)
            {
                PatchMethod(method, TryQueueMethod);
            }

            while(toPatch.Count > 0)
            {
                //toPatch.Sort((a, b) => (a.DeclaringType.FullName + ':' + a.Name).CompareTo(b.DeclaringType.FullName + ':' + b.Name));
                //foreach (var n in toPatch)
                //    DoPatch(n);
                Parallel.For(0, toPatch.Count, i=>DoPatch(toPatch[i]));
                toPatch.Clear();
                lock (addLock)
                {
                    // swap
                    var temp = patchQueue;
                    patchQueue = toPatch;
                    toPatch = temp;
                }
            }
        }
        */
        private static bool PatchMethod(MethodDefinition method, Action<MethodDefinition> onFindLdftn)
        {
            try
            {

                if (InspectForPatch(ConvertToMethodBase(method), null))
                {
                    IlEditingV3(new ILContext(method));
                    // MonoModHooks.Modify(method, new ILContext.Manipulator(IlEditing));
                    return true;
                }
            }
            catch (Exception e)
            {
                InfWorld.Instance.Logger.Debug(method.DeclaringType.FullName + " " + method.Name + " Error");
                InfWorld.Instance.Logger.Error(e.Message, e);
            }
            return false;
        }

        static FieldInfo tileField = typeof(Main).GetField(nameof(Main.tile));
        static MethodBase tileArraySetter = typeof(Tile[,]).GetMethod("Set");
        static MethodBase tileArrayGetter = typeof(Tile[,]).GetMethod("Get");


        static MethodBase ConvertToMethodBase(MethodDefinition method)
        {
            // Get the corresponding System.Reflection.MethodInfo
            TypeDefinition typeDefinition = method.DeclaringType;
            Type type = Assembly.LoadFrom(typeDefinition.Module.FileName).GetType(typeDefinition.FullName);
            MethodInfo methodInfo = type.GetMethod(
                method.Name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static
                //null, // You may need to adjust the parameter types to match your method
                //method.Parameters.Select(p => p.ParameterType).ToArray(),
                //null
            );
            
            // If the method is not found, return null or throw an exception, depending on your requirements
            if (methodInfo == null)
            {
                throw new InvalidOperationException("Method not found in System.Reflection.");
            }

            // Convert MethodInfo to MethodBase
            MethodBase methodBase = methodInfo;

            return methodBase;
        }

        private static bool InspectForPatch(MethodBase method, Action<MethodBase> onFindLdftn)
        {
            if (method.IsAbstract)
                return false;

            // byte[] bytes = method.run.Instructions
            byte[] ilBytes = method.GetMethodBody()?.GetILAsByteArray();
            if (ilBytes is null || ilBytes.Length <
                + 1 + 4 // ldsfld Main::tile
                + 1) // ret
                return false;
            
            var reader = new MethodOpCodeReader(ilBytes, method.Module);
            bool candidate = false;

            // inspect the instructions
            try
            {
                while (reader.MoveNext())
                {
                    var p = reader.Current;

                    var opcode = p.Key;
                    var operand = p.Value;
                    object operandObj;

                    if (opcode == EOpCodes.Ldftn && onFindLdftn != null)
                    {
                        if (operand.TryResolve(out operandObj))
                        {
                            onFindLdftn((MethodBase)operandObj);
                        }
                    }
                    else if (candidate) // let it inspect only ldftns
                    {
                        if (onFindLdftn == null) break; 
                        continue;
                    }
                    else if (opcode == EOpCodes.Ldsfld)
                    {
                        if (operand.TryResolve(out operandObj) && operandObj is FieldInfo f && f == tileField)
                            candidate = true;
                    }
                    else if ((opcode == EOpCodes.Call || opcode == EOpCodes.Callvirt) && operand.TryResolve(out operandObj) && operandObj is MethodBase methodBase)
                    {
                        if (methodBase == tileArraySetter || methodBase == tileArrayGetter) 
                            candidate = true;
                    }
                }
            }
            finally
            {
                reader.Dispose();
            }

            return candidate;
    }
    private static void PatchModLoader(ILContext context)
        {
            ILCursor cursor = new ILCursor(context);
            cursor.EmitDelegate<Action>(() =>
            {
                Console.WriteLine(context.Method.Name);
            });
        }


        private static List<string> _blacklistLoadFnt = new List<string>()
            {
                "FinishPlayWorld",
                "OnDisconnect",
                "DoClientSizeChanged",
                "SetMonitorOnce",
                "ResolveRule",
                "OnLobbyEntered"
            };

        private static List<TypeDefinition> _definitions = new List<TypeDefinition>();

        internal static PropertyInfo IndexerInfo = typeof(World).GetProperty("Item",
            BindingFlags.Public | BindingFlags.Instance, null, typeof(Tile),
            new Type[] { typeof(Int32), typeof(Int32) }, null);
        internal static MethodInfo GetItem = IndexerInfo.GetGetMethod();
        internal static MethodInfo SetItem = IndexerInfo.GetSetMethod();

        internal static void IlEditingV3(ILContext il)
        {


            MethodReference getItemReference = null;
            MethodReference setItemReference = null;

            foreach (var instruction in il.Body.Instructions)
            {
                /*if (instruction.OpCode == OpCodes.Ldftn) // ldftns are checked on the body inspector
                {
                    MethodReference function = (MethodReference)instruction.Operand;
                    if (!function.Name.Contains(_blacklistLoadFnt[0]) && !function.Name.Contains(_blacklistLoadFnt[1]) && !function.Name.Contains(_blacklistLoadFnt[2]) && !function.Name.Contains(_blacklistLoadFnt[3]) && !function.Name.Contains(_blacklistLoadFnt[4]) && !function.Name.Contains(_blacklistLoadFnt[5]))
                    {
                        HookEndpointManager.Modify(function.ResolveReflection(), new ILContext.Manipulator(IlEditing));
                    }
                }*/
                //else 
                if (instruction.OpCode == COpCodes.Ldsfld)
                {
                    if (instruction.Operand is FieldReference fieldRef && fieldRef.DeclaringType.FullName == "Terraria.Main" && fieldRef.Name == "tile")
                    {
                        FieldReference tileReference =
                            il.Module.ImportReference(typeof(InfWorld).GetField("Tile",
                                BindingFlags.Public | BindingFlags.Static));
                        instruction.Operand = tileReference;
                    }
                }
                else if (instruction.OpCode == COpCodes.Ldfld)
                {
                    if (instruction.Operand is FieldReference fieldRef && fieldRef.FullName == "Terraria.World")
                    {
                        instruction.OpCode = COpCodes.Ldsfld;
                        FieldReference tileReference =
                            il.Module.ImportReference(typeof(InfWorld).GetField("Tile",
                                BindingFlags.Public | BindingFlags.Static));
                        instruction.Operand = tileReference;
                    }
                }
                else if ((instruction.OpCode == COpCodes.Call || instruction.OpCode == COpCodes.Callvirt) && instruction.Operand is Mono.Cecil.MethodReference reference)
                {
                    if (reference.FullName == ("call instance valuetype Terraria.Tile Terraria.Tilemap::get_Item(int32, int32)"))
                    {
                        instruction.OpCode = COpCodes.Callvirt;
                        if (getItemReference == null)
                        {
                            getItemReference = il.Import(GetItem);
                        }
                        instruction.Operand = getItemReference;

                    }
                    else if (reference.FullName == ("System.Void Terraria.Tile[0...,0...]::Set(System.Int32,System.Int32,Terraria.Tile)"))
                    {
                        instruction.OpCode = COpCodes.Callvirt;
                        if (setItemReference == null)
                        {
                            setItemReference = il.Import(SetItem);
                        }
                        instruction.Operand = setItemReference;
                    }
                    else if (reference.FullName == "Terraria.Tile[0..., 0...] Terraria.World::get_Tiles()")
                    {
                        instruction.OpCode = COpCodes.Callvirt;
                        if (setItemReference == null)
                        {
                            setItemReference = il.Import(GetItem);
                        }
                        instruction.Operand = setItemReference;
                    }
                    else if (reference.FullName.Contains("Terraria.Tile[0..., 0...] Terraria.World::set_Tiles(System.Int32,System.Int32,Terraria.Tile)"))
                    {
                        instruction.OpCode = COpCodes.Callvirt;
                        if (setItemReference == null)
                        {
                            setItemReference = il.Import(SetItem);
                        }
                        instruction.Operand = setItemReference;
                    }
                }
            }
            string path = Path.Combine(Main.SavePath, "ILDump");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string name = il.Method.FullName.Replace('<', '_').Replace('>', '_').Replace(':', '_').Replace('?', '_').Replace('(', '_').Replace(')', '_').Replace('[', '_').Replace(']', '_');
            using (FileStream n = File.Open(path + '.' + name + ".dll", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                il.Module.Write(n);
            }

        }

        #region older version
        internal static void IlEditing2(ILContext il)
        {
            MethodReference getItemReference = null;
            MethodReference setItemReference = null;

            foreach (var instruction in il.Body.Instructions)
            {
                /*if (instruction.OpCode == OpCodes.Ldftn) // ldftns are checked on the body inspector
                {
                    MethodReference function = (MethodReference)instruction.Operand;
                    if (!function.Name.Contains(_blacklistLoadFnt[0]) && !function.Name.Contains(_blacklistLoadFnt[1]) && !function.Name.Contains(_blacklistLoadFnt[2]) && !function.Name.Contains(_blacklistLoadFnt[3]) && !function.Name.Contains(_blacklistLoadFnt[4]) && !function.Name.Contains(_blacklistLoadFnt[5]))
                    {
                        HookEndpointManager.Modify(function.ResolveReflection(), new ILContext.Manipulator(IlEditing));
                    }
                }*/
                //else 
                if (instruction.OpCode == COpCodes.Ldsfld)
                {
                    if (instruction.Operand is FieldReference fieldRef && fieldRef.DeclaringType.FullName == "Terraria.Main" && fieldRef.Name == "tile")
                    {
                        FieldReference tileReference =
                            il.Module.ImportReference(typeof(InfWorld).GetField("Tile",
                                BindingFlags.Public | BindingFlags.Static));
                        instruction.Operand = tileReference;
                    }
                }
                else if (instruction.OpCode == COpCodes.Ldfld)
                {
                    if (instruction.Operand is FieldReference fieldRef && fieldRef.FullName == "Terraria.World")
                    {
                        instruction.OpCode = COpCodes.Ldsfld;
                        FieldReference tileReference =
                            il.Module.ImportReference(typeof(InfWorld).GetField("Tile",
                                BindingFlags.Public | BindingFlags.Static));
                        instruction.Operand = tileReference;
                    }
                }
                else if ((instruction.OpCode == COpCodes.Call || instruction.OpCode == COpCodes.Callvirt) && instruction.Operand is Mono.Cecil.MethodReference reference)
                {
                    if (reference.FullName == ("Terraria.Tilemap Terraria.Tile[0...,0...]::Get(System.Int32,System.Int32)"))
                    {
                        instruction.OpCode = COpCodes.Callvirt;
                        if (getItemReference == null)
                        {
                            getItemReference = il.Import(GetItem);
                        }
                        instruction.Operand = getItemReference;

                    }
                    else if (reference.FullName == ("System.Void Terraria.Tile[0...,0...]::Set(System.Int32,System.Int32,Terraria.Tile)"))
                    {
                        instruction.OpCode = COpCodes.Callvirt;
                        if (setItemReference == null)
                        {
                            setItemReference = il.Import(SetItem);
                        }
                        instruction.Operand = setItemReference;
                    }
                    else if (reference.FullName == "Terraria.Tile[0..., 0...] Terraria.World::get_Tiles()")
                    {
                        instruction.OpCode = COpCodes.Callvirt;
                        if (setItemReference == null)
                        {
                            setItemReference = il.Import(GetItem);
                        }
                        instruction.Operand = setItemReference;
                    }
                    else if (reference.FullName.Contains("Terraria.Tile[0..., 0...] Terraria.World::set_Tiles(System.Int32,System.Int32,Terraria.Tile)"))
                    {
                        instruction.OpCode = COpCodes.Callvirt;
                        if (setItemReference == null)
                        {
                            setItemReference = il.Import(SetItem);
                        }
                        instruction.Operand = setItemReference;
                    }
                }
            }
            string path = Path.Combine(Main.SavePath, "ILDump");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            string name = il.Method.FullName.Replace('<', '_').Replace('>', '_').Replace(':', '_').Replace('?', '_').Replace('(', '_').Replace(')', '_').Replace('[', '_').Replace(']', '_');
            using (FileStream n = File.Open(path + '.' + name + ".dll", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                il.Module.Write(n);
            }

        }

        internal static void IlEditing(ILContext il)
        {
            MethodReference getItemReference = null;
            MethodReference setItemReference = null;

            foreach (var instruction in il.Body.Instructions)
            {
                /*if (instruction.OpCode == OpCodes.Ldftn) // ldftns are checked on the body inspector
                {
                    MethodReference function = (MethodReference)instruction.Operand;
                    if (!function.Name.Contains(_blacklistLoadFnt[0]) && !function.Name.Contains(_blacklistLoadFnt[1]) && !function.Name.Contains(_blacklistLoadFnt[2]) && !function.Name.Contains(_blacklistLoadFnt[3]) && !function.Name.Contains(_blacklistLoadFnt[4]) && !function.Name.Contains(_blacklistLoadFnt[5]))
                    {
                        HookEndpointManager.Modify(function.ResolveReflection(), new ILContext.Manipulator(IlEditing));
                    }
                }*/
                //else 
                if (instruction.OpCode == OpCodes.Ldsfld)
                {
                    if (instruction.Operand is FieldReference fieldRef && fieldRef.DeclaringType.FullName == "Terraria.Main" && fieldRef.Name == "tile")
                    {
                        FieldReference tileReference =
                            il.Module.ImportReference(typeof(InfWorld).GetField("Tile",
                                BindingFlags.Public | BindingFlags.Static));
                        instruction.Operand = tileReference;
                    }
                }
                else if (instruction.OpCode == OpCodes.Ldfld)
                {
                    if (instruction.Operand is FieldReference fieldRef && fieldRef.FullName == "Terraria.World") 
                    {
                        instruction.OpCode = OpCodes.Ldsfld;
                        FieldReference tileReference =
                            il.Module.ImportReference(typeof(InfWorld).GetField("Tile",
                                BindingFlags.Public | BindingFlags.Static));
                        instruction.Operand = tileReference;
                    }
                }
                else if ((instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt) && instruction.Operand is Mono.Cecil.MethodReference reference)
                {
                    if (reference.FullName == ("Terraria.Tile Terraria.Tile[0...,0...]::Get(System.Int32,System.Int32)"))
                    {
                        instruction.OpCode = OpCodes.Callvirt;
                        if (getItemReference == null)
                        {
                            getItemReference = il.Import(GetItem);
                        }
                        instruction.Operand = getItemReference;

                    }
                    else if (reference.FullName == ("System.Void Terraria.Tile[0...,0...]::Set(System.Int32,System.Int32,Terraria.Tile)"))
                    {
                        instruction.OpCode = OpCodes.Callvirt;
                        if (setItemReference == null)
                        {
                            setItemReference = il.Import(SetItem);
                        }
                        instruction.Operand = setItemReference;
                    }
                    else if (reference.FullName == "Terraria.Tile[0..., 0...] Terraria.World::get_Tiles()")
                    {
                        instruction.OpCode = OpCodes.Callvirt;
                        if (setItemReference == null)
                        {
                            setItemReference = il.Import(GetItem);
                        }
                        instruction.Operand = setItemReference;
                    }
                    else if (reference.FullName.Contains("Terraria.Tile[0..., 0...] Terraria.World::set_Tiles(System.Int32,System.Int32,Terraria.Tile)"))
                    {
                        instruction.OpCode = OpCodes.Callvirt;
                        if (setItemReference == null)
                        {
                            setItemReference = il.Import(SetItem);
                        }
                        instruction.Operand = setItemReference;
                    }
                }
            }
            //string path = Path.Combine(Main.SavePath, "ILDump");
            //if (!Directory.Exists(path))
            //    Directory.CreateDirectory(path);
            //string name = il.Method.FullName.Replace('<', '_').Replace('>', '_').Replace(':', '_').Replace('?', '_').Replace('(', '_').Replace(')', '_').Replace('[', '_').Replace(']', '_');
            //using (FileStream n = File.Open(path + '.' + name + ".dll", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            //{
            //    il.Module.Write(n);
            //}

        }
        #endregion
        private static Action<string, int> setLoadStageDelegate;
        public static void SetLoadingStatusText(string statusText, int percent)
        {
            if(setLoadStageDelegate != null)
            {
                setLoadStageDelegate(statusText, percent);
                return;
            }
            Assembly assembly = Assembly.GetAssembly(typeof(Mod));

            Type type = assembly.GetType("Terraria.ModLoader.UI.Interface");
            FieldInfo loadModsField = type.GetField("loadMods", BindingFlags.Static | BindingFlags.NonPublic);
            object loadModsValue = loadModsField.GetValue(null);

            Type uiLoadModsType = assembly.GetType("Terraria.ModLoader.UI.UILoadMods");

            MethodInfo setLoadStageMethod = uiLoadModsType.GetMethod("SetLoadStage", BindingFlags.Instance | BindingFlags.Public);

            setLoadStageDelegate = setLoadStageMethod.CreateDelegate<Action<string, int>>(loadModsValue);

            setLoadStageDelegate(statusText, -1);
            //setLoadStageMethod.Invoke(loadModsValue, new object[] { statusText, -1 });
        }
    }

}
