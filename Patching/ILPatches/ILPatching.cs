using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace InfWorld.Patching.ILPatches
{
    internal class IlPatching : ILoadable
    {
        public void Load(Mod mod)
        {
            Terraria.IL_WorldGen.PlaceTile += il =>
            {
                var cursor = new ILCursor(il);
                Instruction jumpPoint = null;
                if (cursor.TryGotoNext(i => i.MatchLdsfld(out _), 
                    i => i.MatchLdarg(out _),
                    i => i.MatchLdarg(out _)))
                {
                    jumpPoint = cursor.Next;
                    cursor.Index = 0;

                    if (cursor.TryGotoNext(i => i.MatchLdarg(out _),
                        i => i.MatchLdcI4(out _),
                        i => i.MatchBlt(out _)))
                    {
                        cursor.Emit(OpCodes.Br, jumpPoint);
                        InfWorld.Instance.Logger.Debug("Patched place tile");
                    }
                }
            };
            
            Terraria.GameContent.Drawing.IL_TileDrawing.GetScreenDrawArea += il =>
            {
                var cursor = new ILCursor(il);

                Instruction jumpPoint = null;

                if (cursor.TryGotoNext(MoveType.Before,
                    i => i.MatchLdsfld(out _),
                    i => i.MatchCallvirt(out _),
                    i => i.MatchBrfalse(out _),
                    i => i.MatchCall(out _)))
                {
                    jumpPoint = cursor.Next;
                }

                cursor.Index = 0;
                if (jumpPoint == null)
                {
                    InfWorld.Instance.Logger.Error("Tiledrawing draw Jump point not found");
                }

                if (cursor.TryGotoNext(i => i.MatchLdarg3(),
                    i => i.MatchLdindI4(),
                    i => i.MatchLdcI4(out _),
                    i => i.MatchBge(out _)))
                {
                    cursor.Emit(OpCodes.Br, jumpPoint);
                }
            };
            Terraria.GameContent.Drawing.IL_TileDrawing.GetScreenDrawArea += il =>
            {
                var cursor = new ILCursor(il);

                Instruction jumpPoint = null;

                if (cursor.TryGotoNext(
                    i => i.MatchLdsfld(out _),
                    i => i.MatchCallvirt(out _),
                    i => i.MatchLdcI4(out _),
                    i => i.MatchCgt()))
                {
                    jumpPoint = cursor.Next;
                }

                cursor.Index = 0;
                if (jumpPoint == null)
                {
                    InfWorld.Instance.Logger.Error("Screen draw area Jump point not found");
                }

                if (cursor.TryGotoNext(i => i.MatchLdarg(out _),
                    i => i.MatchLdcI4(out _),
                    i => i.MatchClt()))
                {
                    cursor.Emit(OpCodes.Br, jumpPoint);
                }
            };

            Terraria.IL_Player.Update += il =>
            {
                var cursor = new ILCursor(il);
                int indexCursor = 0;
                if (cursor.TryGotoNext(MoveType.Before,
                        i => i.MatchCall(out _), 
                        i => i.MatchLdarg0(),
                        i => i.MatchLdfld(out _),
                        i => i.MatchLdarg0(), 
                        i => i.MatchLdfld(out _),
                        i => i.MatchLdelemRef(),
                        i => i.MatchLdfld(out _),
                        i => i.MatchLdcI4(out _),
                        i => i.MatchBle(out _)))
                {
                    cursor.EmitDelegate<Action>(() =>
                    {
                        Player.tileTargetX = (int)(((float)Main.mouseX + Main.screenPosition.X) / 16f);
                        Player.tileTargetY = (int)(((float)Main.mouseY + Main.screenPosition.Y) / 16f);
                    });
                }
            };
            return;
            Terraria.IL_WorldGen.TileRunner += il =>
            {
                var cursor = new ILCursor(il);

                Instruction jumpPoint = null;

                if (cursor.TryGotoNext(
                    i => i.MatchLdloc(out _),
                    i => i.MatchStloc(out _),
                    i => i.MatchBr(out _),
                    i => i.MatchLdloc(out _)))
                {
                    jumpPoint = cursor.Next;
                }

                cursor.Index = 0;
                if (jumpPoint == null)
                {
                    InfWorld.Instance.Logger.Error("Jump point not found");
                }

                if (cursor.TryGotoNext(i => i.MatchLdloc(out _),
                    i => i.MatchLdcI4(out _),
                    i => i.MatchBge(out _)))
                {
                    cursor.Emit(OpCodes.Br, jumpPoint);
                }
            };
            
            
            Terraria.IL_Projectile.VanillaAI += il =>
            {
                var cursor = new ILCursor(il);
                for (int nmbloop = 0; nmbloop < 3; nmbloop++)
                {

                    if (cursor.TryGotoNext(i => i.MatchLdloc(out _),
                        i => i.MatchLdcI4(out _),
                        i => i.MatchBge(out _),
                        i => i.MatchLdcI4(out _),
                        i => i.MatchStloc(out _),
                        i => i.MatchLdloc(out _),
                        i => i.MatchLdsfld(out _)))
                    {
                        int initPoint = cursor.Index;
                        if (cursor.TryGotoNext(i => i.MatchLdsfld(typeof(Main), nameof(Main.maxTilesY)),
                            i => i.MatchStloc(out _)))
                        {
                            cursor.Index += 2;
                            int afterPoint = cursor.Index;
                            var instruction = cursor.Next;
                            cursor.Index = initPoint;
                            cursor.Emit(OpCodes.Br, instruction);
                            cursor.Index = afterPoint + 1;
                        }
                    }
                }
            };
            Terraria.IL_Projectile.Kill += il =>
            {
                var cursor = new ILCursor(il);
                for (int nmbloop = 0; nmbloop < 3; nmbloop++)
                {

                    if (cursor.TryGotoNext(i => i.MatchNop(),
                        i => i.MatchNop(),
                        i => i.MatchLdloc(out _),
                        i => i.MatchNop(),
                        i => i.MatchNop(),
                        i => i.MatchLdcI4(out _),
                        i => i.MatchBge(out _),
                        i => i.MatchLdcI4(out _),
                        i => i.MatchStloc(out _),
                        i => i.MatchNop(),
                        i => i.MatchNop(),
                        i => i.MatchLdloc(out _),
                        i => i.MatchNop(),
                        i => i.MatchNop(),
                        i => i.MatchLdsfld(out _),
                        i => i.MatchBle(out _)))
                    {
                        int initPoint = cursor.Index;
                        ILog log = LogManager.GetLogger("VanillaAI debug");
                        log.Debug("Entrypoint found");
                        log.Debug($"Current instruction : [{cursor.Previous.Offset}] {cursor.Previous.OpCode.Name} {cursor.Previous.Operand}");
                        if (cursor.TryGotoNext(i => i.MatchLdsfld(typeof(Main), nameof(Main.maxTilesY)),
                            i => i.MatchStloc(out _)))
                        {
                            log.Debug("Pattern found");
                            cursor.Index += 2;
                            int afterPoint = cursor.Index;
                            var instruction = cursor.Next;
                            cursor.Index = initPoint;
                            cursor.Emit(OpCodes.Br, instruction);
                            cursor.Index = afterPoint + 1;
                        }
                    }
                }
            };
            Terraria.IL_WorldGen.KillTile += il =>
            {
                var cursor = new ILCursor(il);

                if (cursor.TryGotoNext(i => i.MatchBrfalse(out _)))
                {
                    var jumpPoint = cursor.Next.Next.Next;
                    cursor.Index = 0;
                    cursor.Emit(OpCodes.Br, jumpPoint);
                }
            };
            Terraria.IL_WorldGen.KillWall += il =>
            {
                var cursor = new ILCursor(il);

                if (cursor.TryGotoNext(i => i.MatchBrfalse(out _)))
                {
                    ILog log = LogManager.GetLogger("VanillaAI debug");
                    log.Debug("Entrypoint found");
                    log.Debug($"Current instruction : [{cursor.Previous.Offset}] {cursor.Previous.OpCode.Name} {cursor.Previous.Operand}");
                    var jumpPoint = cursor.Next.Next.Next;
                    cursor.Index = 0;
                    cursor.Emit(OpCodes.Br, jumpPoint);
                }
            };
            Terraria.IL_WorldGen.CanKillTile_int_int_refBoolean += il =>
            {
                var cursor = new ILCursor(il);

                cursor.TryGotoNext(i => i.MatchStindI());

                int cursorIndex = cursor.Index;

                if (cursor.TryGotoNext(i => i.MatchBrfalse(out _)))
                {
                    ILog log = LogManager.GetLogger("VanillaAI debug");
                    var jumpPoint = cursor.Next.Next.Next.Next;
                    cursor.Index = cursorIndex;
                    cursor.Emit(OpCodes.Br, jumpPoint);
                }
            };
            
            return;
            Terraria.IL_Main.DrawBlack += il =>
            {
                var cursor = new ILCursor(il);
                if (cursor.TryGotoNext(i => i.MatchLdloc(out _),
                    i => i.MatchLdcI4(out _),
                    i => i.MatchBge(out _),
                    i => i.MatchLdloc(out _),
                    i => i.MatchStloc(out _),
                    i => i.MatchLdloc(out _),
                    i => i.MatchLdsfld(out _)))
                {
                    int initPoint = cursor.Index;
                    if (cursor.TryGotoNext(i => i.MatchLdarg(out _),
                        i => i.MatchBrtrue(out _)))
                    {
                        int afterPoint = cursor.Index;
                        var instruction = cursor.Next;
                        cursor.Index = initPoint;
                        cursor.Emit(OpCodes.Br, instruction);
                        cursor.Index = afterPoint + 1;
                    }
                }
            };
            //IL.Terraria.Lighting.GetBlackness += PatchLightingCheck;
            //IL.Terraria.Lighting.Brightness += PatchLightingCheck;
            //IL.Terraria.Lighting.GetColor4Slice += PatchLightingCheck;
            /*IL.Terraria.Lighting.AddLight_int_int_float_float_float += il =>
            {
                var cursor = new ILCursor(il);
                if (cursor.TryGotoNext(i => i.MatchLdarg(out _)))
                {
                    int initialPoint = cursor.Index;
                    cursor.Index++;

                    if (cursor.TryGotoNext(i => i.MatchLdarg(out _)))
                    {
                        (int, Instruction) jumpPoint = (cursor.Index, cursor.Next);
                        cursor.Index = initialPoint;
                        cursor.Emit(OpCodes.Br, jumpPoint.Item2);
                    }

                }
            };
            IL.Terraria.Collision.AnyCollision += il =>
            {
                var cursor = new ILCursor(il);

                if (cursor.TryGotoNext(i => i.MatchLdloc(out _),
                    i => i.MatchLdcI4(out _),
                    i => i.MatchBge(out _),
                    i => i.MatchLdcI4(out _),
                    i => i.MatchStloc(out _),
                    i => i.MatchLdloc(out _),
                    i => i.MatchLdsfld(out _)))
                {
                    int initPoint = cursor.Index;
                    if (cursor.TryGotoNext(i => i.MatchLdsfld(typeof(Main), nameof(Main.maxTilesY)),
                        i => i.MatchStloc(out _)))
                    {
                        InfWorld.Instance.Logger.Debug("Collision patch");
                        cursor.Index += 2;
                        int afterPoint = cursor.Index;
                        var instruction = cursor.Next;
                        cursor.Index = initPoint;
                        cursor.Emit(OpCodes.Br, instruction);
                        cursor.Index = afterPoint + 1;
                    }
                }
            };*/
        }

        internal static void PatchLightingCheck(ILContext context)
        {
            var cursor = new ILCursor(context);

            if (cursor.TryGotoNext(i => i.MatchLdloc(out _),
                i => i.MatchLdcI4(out _),
                i => i.MatchBlt(out _),
                i => i.MatchLdloc(out _),
                i => i.MatchLdcI4(out _),
                i => i.MatchBlt(out _)))
            {
                int indexPosition = cursor.Index;
                cursor.Index += 6;
                Instruction jumpPoint = cursor.Next;
                cursor.Index = indexPosition;
                cursor.Emit(OpCodes.Br, jumpPoint);
                InfWorld.Instance.Logger.Debug("Lighting patch applied");
            }
        }

        public void Unload()
        {
            throw new NotImplementedException();
        }
    }

}
