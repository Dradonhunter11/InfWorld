using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfiniteWorldLibrary;
using InfiniteWorldLibrary.World;
using InfiniteWorldLibrary.World.Region;
using log4net;
using log4net.Repository.Hierarchy;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using static System.Net.WebRequestMethods;

namespace InfWorld.Patching.Detours
{
    static partial class Detours
    {
        private static void NewLoadWorldTiles(Terraria.IO.On_WorldFile.orig_LoadWorldTiles orig, BinaryReader reader, bool[] importance)
        {
            for (int i = 0; i < Main.maxTilesX - 1000; i += ChunkMap.ChunkWidth)
            {
                ChunkData<LiquidData>.EnsureAllocate((uint)(i / ChunkMap.ChunkWidth), false);
                ChunkData<WallTypeData>.EnsureAllocate((uint)(i / ChunkMap.ChunkWidth), false);
                ChunkData<TileTypeData>.EnsureAllocate((uint)(i / ChunkMap.ChunkWidth), false);
                ChunkData<TileWallWireStateData>.EnsureAllocate((uint)(i / ChunkMap.ChunkWidth), false);
                ChunkData<TileWallBrightnessInvisibilityData>.EnsureAllocate((uint)(i / ChunkMap.ChunkWidth), false);
            }

			for (int i = 0; i < Main.maxTilesX; i++)
			{
				float num = (float)i / (float)Main.maxTilesX;
				Main.statusText = Lang.gen[51].Value + " " + (int)((double)num * 100.0 + 1.0) + "%";
				for (int j = 0; j < Main.maxTilesY; j++)
				{
                    int num2 = -1;
                    byte b2;
                    byte b;
                    byte b3 = (b2 = (b = 0));
                    Tile tile = StaticInstance.WorldInstance[i, j];
                    byte b4 = reader.ReadByte();
                    bool flag = false;
                    if ((b4 & 1) == 1)
                    {
                        flag = true;
                        b3 = reader.ReadByte();
                    }

                    bool flag2 = false;
                    if (flag && (b3 & 1) == 1)
                    {
                        flag2 = true;
                        b2 = reader.ReadByte();
                    }

                    if (flag2 && (b2 & 1) == 1)
                        b = reader.ReadByte();

                    byte b5;
                    if ((b4 & 2) == 2)
                    {
                        tile.HasTile = true;
                        if ((b4 & 0x20) == 32)
                        {
                            b5 = reader.ReadByte();
                            num2 = reader.ReadByte();
                            num2 = (num2 << 8) | b5;
                        }
                        else
                        {
                            num2 = reader.ReadByte();
                        }

                        tile.TileType = (ushort)num2;
                        if (importance[num2])
                        {
                            tile.TileFrameX = reader.ReadInt16();
                            tile.TileFrameY = reader.ReadInt16();
                            if (tile.TileType == 144)
                                tile.TileFrameY = 0;
                        }
                        else
                        {
                            tile.TileFrameY = -1;
                            tile.TileFrameY = -1;
                        }

                        if ((b2 & 8) == 8)
                            tile.TileColor = reader.ReadByte();
                    }

                    if ((b4 & 4) == 4)
                    {
                        tile.WallType = reader.ReadByte();
                        if (tile.WallType >= WallID.Count)
                            tile.WallType = 0;

                        if ((b2 & 0x10) == 16)
                            tile.WallColor = reader.ReadByte();
                    }

                    b5 = (byte)((b4 & 0x18) >> 3);
                    if (b5 != 0)
                    {
                        tile.LiquidAmount = reader.ReadByte();
                        if ((b2 & 0x80) == 128)
                        {
                            tile.LiquidType = LiquidID.Shimmer;
                        }
                        else if (b5 > 1)
                        {
                            if (b5 == 2)
                                tile.LiquidType = LiquidID.Lava;
                            else
                                tile.LiquidType = LiquidID.Honey;
                        }
                    }

                    if (b3 > 1)
                    {
                        if ((b3 & 2) == 2)
                            tile.RedWire = true;

                        if ((b3 & 4) == 4)
                            tile.BlueWire = true;

                        if ((b3 & 8) == 8)
                            tile.GreenWire = true;

                        b5 = (byte)((b3 & 0x70) >> 4);
                        if (b5 != 0 && (Main.tileSolid[tile.TileType] || TileID.Sets.NonSolidSaveSlopes[tile.TileType]))
                        {
                            if (b5 == 1)
                                tile.BlockType = BlockType.HalfBlock;
                            else
                                tile.BlockType = ((BlockType)(b5 - 1));
                        }
                    }

                    if (b2 > 1)
                    {
                        if ((b2 & 2) == 2)
                            tile.HasActuator  = true;

                        if ((b2 & 4) == 4)
                            tile.IsActuated = true;

                        if ((b2 & 0x20) == 32)
                            tile.YellowWire = true;

                        if ((b2 & 0x40) == 64)
                        {
                            b5 = reader.ReadByte();
                            tile.WallType = (ushort)((b5 << 8) | tile.WallType);
                            if (tile.WallType >= WallID.Count)
                                tile.WallType = 0;
                        }
                    }

                    if (b > 1)
                    {
                        if ((b & 2) == 2)
                            tile.IsTileInvisible = true;

                        if ((b & 4) == 4)
                            tile.IsWallInvisible = true;

                        if ((b & 8) == 8)
                            tile.IsTileFullbright = true;

                        if ((b & 0x10) == 16)
                            tile.IsWallFullbright = true;
                    }

                    int num3;
                    switch ((byte)((b4 & 0xC0) >> 6))
                    {
                        case 0:
                            num3 = 0;
                            break;
                        case 1:
                            num3 = reader.ReadByte();
                            break;
                        default:
                            num3 = reader.ReadInt16();
                            break;
                    }

                    if (num2 != -1)
                    {
                        if ((double)j <= Main.worldSurface)
                        {
                            if ((double)(j + num3) <= Main.worldSurface)
                            {
                                WorldGen.tileCounts[num2] += (num3 + 1) * 5;
                            }
                            else
                            {
                                int num4 = (int)(Main.worldSurface - (double)j + 1.0);
                                int num5 = num3 + 1 - num4;
                                WorldGen.tileCounts[num2] += num4 * 5 + num5;
                            }
                        }
                        else
                        {
                            WorldGen.tileCounts[num2] += num3 + 1;
                        }
                    }
                    while (num3 > 0)
                    {
                        j++;
                        /*
                        Main.tile[i, j].CopyFrom(tile);
                        */
                        num3--;

                        // TML:
                        // Significantly improve performance by directly accessing the relevant blocking data to copy.
                        // No need to copy mod data in this method.
                        //var tile2 = StaticInstance.WorldInstance[i, j];
                        //TileIdConverter converter = new TileIdConverter(tile);
                        //TileIdConverter converter2 = new TileIdConverter(i, j);

                        
                        //tile2.Get<LiquidData>() = tile.Get<LiquidData>();
                        //tile2.Get<TileWallBrightnessInvisibilityData>() = tile.Get<TileWallBrightnessInvisibilityData>();
                        //tile2.Get<WallTypeData>() = tile.Get<WallTypeData>();
                        //tile2.Get<TileTypeData>() = tile.Get<TileTypeData>();
                        //tile2.Get<TileWallWireStateData>() = tile.Get<TileWallWireStateData>();
                    }
                }
			}
            Terraria.WorldGen.AddUpAlignmentCounts(clearCounts: true);
        }
    }
}
