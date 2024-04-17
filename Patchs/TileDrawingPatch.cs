using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CorePatcher;
using CorePatcher.Attributes;
using MonoMod.Cil;

namespace InfWorld.Patchs
{
    /*
    [PatchType("Terraria.GameContent.Drawing.TileDrawing")]
    internal class TileDrawingPatch : ModCorePatch
    {
        public void PatchDraw(TypeDefinition definiton, AssemblyDefinition asm)
        {
            var draw = definiton.Methods.First(i => i.Name == "Draw");
            if (draw != null)
            {
                ILCursor cursor = new ILCursor(new ILContext(draw));

                int x = 0, y = 0;

                if (cursor.TryGotoNext(i => i.MatchLdsflda(out _),
                        i => i.MatchLdloc(out x),
                        i => i.MatchLdloc(out y),
                        i => i.MatchCall(out _),
                        i => i.MatchStloc(out _)))
                {
                    cursor.EmitLdloc(x);
                    cursor.EmitLdloc(y);
                    cursor.EmitDelegate<Action<int, int>>(delegate(int i, int i1) {  });
                }
            }
        }
    }*/
}
