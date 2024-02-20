using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;

namespace InfWorld.Patching.Detours
{
    static partial class Detours
    {
        public static bool SolidTiles(int startX, int endX, int startY, int endY, bool allowTopSurfaces = false)
        {
            for (int i = startX; i < endX + 1; i++)
            {
                for (int j = startY; j < endY + 1; j++)
                {
                    Tile tile = Main.tile[i, j];
                    if (tile == null)
                    {
                        return false;
                    }

                    if (tile.HasTile && !Main.tile[i, j].IsActuated)
                    {
                        ushort type = tile.TileType;
                        bool flag = Main.tileSolid[type] && !Main.tileSolidTop[type];
                        if (allowTopSurfaces)
                        {
                            flag |= Main.tileSolidTop[type] && tile.TileFrameY == 0;
                        }
                        
                        if (flag)
                        {
                            return true;
                        }

                        if (Main.tileSolid[Main.tile[i, j].TileType] && !Main.tileSolidTop[Main.tile[i, j].TileType])
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /*private static bool OnSolidTiles(On.Terraria.Collision.orig_SolidTiles orig, int x, int endX, int y, int endY)
        {
            for (int i = x; i < endX + 1; i++)
            {
                for (int j = y; j < endY + 1; j++)
                {
                    if (InfWorld.Tile[i, j] == null) return false;

                    if (InfWorld.Tile[i, j].HasTile && !InfWorld.Tile[i, j].IsActuated && Main.tileSolid[InfWorld.Tile[i, j].TileType] && !Main.tileSolidTop[InfWorld.Tile[i, j].TileType]) return true;
                }
            }

            return false;
        }*/

        /*private static bool CustomSolidCollision(On.Terraria.Collision.orig_SolidCollision orig, Vector2 position, int width, int height)
        {
            int value = (int)(position.X / 16f) - 1;
            int value2 = (int)((position.X + (float)width) / 16f) + 2;
            int value3 = (int)(position.Y / 16f) - 1;
            int value4 = (int)((position.Y + (float)height) / 16f) + 2;
            int num = Utils.Clamp(value, 0, Main.maxTilesX - 1);
                value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
                value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 1);
                value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 1);
            Vector2 vector = default(Vector2);
            for (int i = value; i < value2; i++)
            {
                for (int j = value3; j < value4; j++)
                {
                    if (Terraria.Main.tile[i, j] != null && !Terraria.Main.tile[i, j].inActive() && Terraria.Main.tile[i, j].active() && Terraria.Main.tileSolid[Terraria.Main.tile[i, j].type] && !Terraria.Main.tileSolidTop[Terraria.Main.tile[i, j].type])
                    {
                        vector.X = i * 16;
                        vector.Y = j * 16;
                        int num2 = 16;
                        if (Main.tile[i, j].halfBrick())
                        {
                            vector.Y += 8f;
                            num2 -= 8;
                        }

                        if (position.X + (float)width > vector.X && position.X < vector.X + 16f && position.Y + (float)height > vector.Y && position.Y < vector.Y + (float)num2) return true;
                    }
                }
            }

            return false;
        }*/
    }
}