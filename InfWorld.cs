using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InfiniteWorldLibrary;
using InfiniteWorldLibrary.World;
//using InfWorld.Map;
using InfWorld.Patching;
using InfWorld.Patching.Detours;
using InfWorld.Patching.ILPatches;
using log4net;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using MonoMod.Utils;
using Newtonsoft.Json.Linq;
using Terraria;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using WorldGen = Terraria.WorldGen;
using static Terraria.WorldGen;
using TypeAttributes = Mono.Cecil.TypeAttributes;
using System.Runtime.Loader;
using CorePatcher;

namespace InfWorld
{
    public class InfWorld : Mod
    {

        internal delegate Tile orig_getitem(ref Tilemap instance, int x, int y);
        internal delegate Tile hook_getitem(orig_getitem orig, ref Tilemap instance, int x, int y);
        
        internal static event hook_getitem GetItem_Hook
        {
            add
            {
                MonoModHooks.Add(MethodBase.GetMethodFromHandle(typeof(Tilemap).GetProperty("Item", BindingFlags.Public | BindingFlags.Instance).GetMethod.MethodHandle), value);
            }
            remove
            {
                // MonoModHooks.(MethodBase.GetMethodFromHandle(typeof(Tilemap).GetProperty("Item", BindingFlags.Public | BindingFlags.Instance).GetMethod.MethodHandle), value);
            }
        }
        
        public static InfWorld Instance;

        public override void Load()
        {
#if DEBUG
            DirectoryInfo di = new DirectoryInfo("MonoModDump");
            if (di.Exists)
            {
                foreach (var fileInfo in di.GetFiles())
                {
                    fileInfo.Delete();
                }
            }
            #endif
            
            // GetItem_Hook += (orig_getitem orig, ref Tilemap instance, int x, int y) =>  InfWorld.Tile[x, y];
        }

        public override void AddRecipes()/* tModPorter Note: Removed. Use ModSystem.AddRecipes */
        {
            Recipe recipe = Recipe.Create(ItemID.MagicConch, 1);
            recipe.AddIngredient(ItemID.DirtBlock);
            recipe.Register();
            
            recipe = Recipe.Create(ItemID.Hoverboard, 1);
            recipe.AddIngredient(ItemID.DirtBlock);
            recipe.Register();
            
            recipe = Recipe.Create(ItemID.RedsWings, 1);
            recipe.AddIngredient(ItemID.DirtBlock);
            recipe.Register();
            
            recipe = Recipe.Create(ItemID.LunarHook, 1);
            recipe.AddIngredient(ItemID.DirtBlock);
            recipe.Register();
            
            recipe = Recipe.Create(4989, 1);
            recipe.AddIngredient(ItemID.DirtBlock);
            recipe.Register();
        }

        public override void PostAddRecipes()/* tModPorter Note: Removed. Use ModSystem.PostAddRecipes */
        {
            var b = 1;
            if (b == 2)
            {
                var a = StaticInstance.WorldInstance[0, 0];
                a.BlockType = 0;
            }
        }

        

        public static void InitMonoModDumps()
        {
            Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", "Auto");
            Environment.SetEnvironmentVariable("MONOMOD_DMD_DEBUG", "1");
            string dumpDir = Path.GetFullPath("MonoModDump");
            Directory.CreateDirectory(dumpDir);
            Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", dumpDir);
        }

        public static void DisableMonoModDumps()
        {
            Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", "");
            Environment.SetEnvironmentVariable("MONOMOD_DMD_DEBUG", "0");
            Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", "");
        }
    }
}