using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InfiniteWorldLibrary;
using InfiniteWorldLibrary.World.Region;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using ReLogic.Threading;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Capture;
using Terraria.Graphics.Light;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.Utilities;
using Collision = Terraria.Collision;
using Main = Terraria.Main;
using Player = Terraria.Player;
using WorldFile = Terraria.IO.WorldFile;
using WorldGen = Terraria.WorldGen;

namespace InfWorld.Patching.Detours
{
    static partial class Detours
    {
        public static void Load()
        {
            
            var t = typeof(Main).Assembly.GetType("Terraria.ModLoader.IO.WorldIO");
            /*MonoModHooks.Modify(typeof(Main).Assembly.GetType("Terraria.ModLoader.IO.WorldIO").GetMethod("LoadNPCBestiaryKills", BindingFlags.Public | BindingFlags.Static),
                il =>
                {
                    il.Body.Instructions.Clear();
                    il.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                });*/
            
            MonoModHooks.Modify(typeof(Main).Assembly.GetType("Terraria.ModLoader.IO.WorldIO").GetMethod("LoadModData", BindingFlags.NonPublic | BindingFlags.Static),
                il =>
                {
                    il.Body.Instructions.Clear();
                    il.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                });

            Terraria.IO.On_WorldFile.ConvertOldTileEntities += orig => { };
            Terraria.IO.On_WorldFile.ClearTempTiles += orig => { };
            Terraria.IO.On_WorldFile.LoadWorldTiles += NewLoadWorldTiles;
            Terraria.On_Liquid.QuickWater += (orig, verbose, y, maxY) => { };
            Terraria.On_WorldGen.WaterCheck += orig => { };
            // Terraria.On_Collision.InTileBounds += (orig, i, i1, lx, ly, hx, hy) => true; // no lol
            Terraria.On_Collision.SolidTiles_int_int_int_int += (orig, startX, endX, startY, endY) => SolidTiles(startX, endX, startY, endY, false);
            Terraria.On_Collision.SolidTiles_int_int_int_int_bool += (orig, startX, endX, startY, endY, surfaces) => SolidTiles(startX, endX, startY, endY, surfaces);
            Terraria.On_WorldGen.clearWorld += OnClearWorld;
            Terraria.On_Framing.WallFrame += WallFrame;
            Terraria.On_WorldGen.UpdateWorld_UndergroundTile += (orig, i, i1, spawns, dist) => { };
            Terraria.On_WorldGen.UpdateWorld_OvergroundTile += (orig, i, i1, spawns, dist) => { };
            Terraria.On_WorldGen.PlantAlch += orig => { };
            Terraria.On_WorldGen.PlaceTile += (orig, i, i1, type, mute, forced, plr, style) =>
            {
                return PlaceTile(i, i1, type, mute, forced, plr, style);
            };
            Terraria.On_Main.UpdateWindyDayState += delegate(On_Main.orig_UpdateWindyDayState orig, Main self)
            {
                orig(self);
            };
            Terraria.On_Main.CheckInvasionProgressDisplay += orig =>
            {
                orig();

            };
            Terraria.On_Main.DoDraw += (orig, self, time) =>
            {
                // Main.LocalPlayer.position = new Vector2(5000, 5000);
                orig(self, time);
            };
            // Terraria.Graphics.Light.On_TileLightScanner.ExportTo += On_TileLightScanner_ExportTo;
            Terraria.GameInput.On_KeyConfiguration.Processkey += (orig, self, set, key, mode) =>
            {
                orig(self, set, key, mode);
                // Main.LocalPlayer.active = true;
                // var a = Main.LocalPlayer.active;
            };
            Terraria.On_WorldGen.InWorld += RemoveInWorldCheck;
            // Terraria.On_WorldGen.InWorld += (orig, i, i1, fluff) => true; // no lol
            Terraria.On_WorldGen.playWorldCallBack += (orig, context) =>
            {
                if (Main.rand == null)
                {
                    Main.rand = new UnifiedRandom((int)DateTime.Now.Ticks);
                }

                for (int i = 0; i < 255; i++)
                {
                    if (i != Main.myPlayer)
                    {
                        Main.player[i].active = false;
                    }
                }

                WorldGen.noMapUpdate = true;
                WorldFile.LoadWorld(Main.ActiveWorldFileData.IsCloudSave);
                if (WorldGen.loadFailed || !WorldGen.loadSuccess)
                {
                    WorldFile.LoadWorld(Main.ActiveWorldFileData.IsCloudSave);
                    if (WorldGen.loadFailed || !WorldGen.loadSuccess)
                    {
                        bool isCloudSave = Main.ActiveWorldFileData.IsCloudSave;
                        if (FileUtilities.Exists(Main.worldPathName + ".bak", isCloudSave))
                        {
                            WorldGen.worldBackup = true;
                        }
                        else
                        {
                            WorldGen.worldBackup = false;
                        }

                        if (!Main.dedServ)
                        {
                            if (WorldGen.worldBackup)
                            {
                                Main.menuMode = 200;
                            }
                            else
                            {
                                Main.menuMode = 201;
                            }

                            return;
                        }

                        if (!WorldGen.worldBackup)
                        {
                            string text = Language.GetTextValue("Error.LoadFailedNoBackup");
                            /*if (Terraria.ModLoader.IO.WorldIO.customDataFail != null)
                                {
                                    text = Terraria.ModLoader.IO.WorldIO.customDataFail.modName + " " + text;
                                    text = text + "\n" + Terraria.ModLoader.IO.WorldIO.customDataFail.InnerException;
                                }*/

                            Console.WriteLine(text);
                            return;
                        }

                        FileUtilities.Copy(Main.worldPathName, Main.worldPathName + ".bad", isCloudSave);
                        FileUtilities.Copy(Main.worldPathName + ".bak", Main.worldPathName, isCloudSave);
                        FileUtilities.Delete(Main.worldPathName + ".bak", isCloudSave);
                        //Terraria.ModLoader.IO.WorldIO.LoadDedServBackup(Main.worldPathName, isCloudSave);
                        WorldFile.LoadWorld(Main.ActiveWorldFileData.IsCloudSave);
                        if (WorldGen.loadFailed || !WorldGen.loadSuccess)
                        {
                            WorldFile.LoadWorld(Main.ActiveWorldFileData.IsCloudSave);
                            if (WorldGen.loadFailed || !WorldGen.loadSuccess)
                            {
                                FileUtilities.Copy(Main.worldPathName, Main.worldPathName + ".bak", isCloudSave);
                                FileUtilities.Copy(Main.worldPathName + ".bad", Main.worldPathName, isCloudSave);
                                FileUtilities.Delete(Main.worldPathName + ".bad", isCloudSave);
                                //Terraria.ModLoader.IO.WorldIO.RevertDedServBackup(Main.worldPathName, isCloudSave);
                                string text2 = Language.GetTextValue("Error.LoadFailed");
                                /*if (Terraria.ModLoader.IO.WorldIO.customDataFail != null)
                                    {
                                        text2 = Terraria.ModLoader.IO.WorldIO.customDataFail.modName + " " + text2;
                                        text2 = text2 + "\n" +
                                                Terraria.ModLoader.IO.WorldIO.customDataFail.InnerException;
                                    }*/

                                Console.WriteLine(text2);
                                return;
                            }
                        }
                    }
                }

                if (Main.mapEnabled)
                {
                    Main.Map.Load();
                }
                

                if (Main.netMode != 2)
                {
                    if (Main.sectionManager == null) Main.sectionManager = new WorldSections(Main.maxTilesX / 200, Main.maxTilesY / 150);
                    Main.sectionManager.SetAllSectionsLoaded();
                }

                while (Main.loadMapLock)
                {
                    float num = (float)Main.loadMapLastX / (float)Main.maxTilesX;
                    Main.statusText = Lang.gen[68].Value + " " + (int)(num * 100f + 1f) + "%";
                    Thread.Sleep(0);
                    if (!Main.mapEnabled)
                    {
                        break;
                    }
                }

                if (Main.gameMenu)
                {
                    Main.gameMenu = false;
                }

                if (Main.netMode == 0 && Main.anglerWhoFinishedToday.Contains(Main.player[Main.myPlayer].name))
                {
                    Main.anglerQuestFinished = true;
                }

                Main.OnTickForInternalCodeOnly += FinishPlayWorld;
            };
            Terraria.On_Main.ClampScreenPositionToWorld += OnClampScreenPositionToWorld;
            Terraria.On_Main.DrawMap += DrawMap;
            Terraria.On_Player.BordersMovement += OnBordersMovement;
            Terraria.On_WorldGen.clearWorld += orig =>
            {
                /*
                Main.maxTilesY = short.MaxValue - 10;

                int intendedMaxX = Math.Max(Main.maxTilesX + 1, 8401);
                int intendedMaxY = Math.Max(Main.maxTilesY + 1, 2401);

                Main.mapTargetX = 4;
                Main.mapTargetY = 20;

                Main.instance.mapTarget = new RenderTarget2D[intendedMaxX, intendedMaxY];
                Main.initMap = new bool[intendedMaxX, intendedMaxY];
                Main.mapWasContentLost = new bool[intendedMaxX, intendedMaxY];
                Main.Map = new WorldMap(intendedMaxX, intendedMaxY);*/
                orig();
            };
        }

        private static void On_TileLightScanner_ExportTo(On_TileLightScanner.orig_ExportTo orig, TileLightScanner self, Rectangle area, LightMap outputMap, TileLightScannerOptions options)
        {
            self._drawInvisibleWalls = options.DrawInvisibleWalls;
            /*FastParallel.For(area.Left, area.Right, delegate (int start, int end, object context)
            {
                for (int i = start; i < end; i++)
                {
                    for (int j = area.Top; j <= area.Bottom; j++)
                    {
                        if (IsTileNullOrTouchingNull(i, j))
                        {
                            outputMap.SetMaskAt(i - area.X, j - area.Y, LightMaskMode.None);
                            outputMap[i - area.X, j - area.Y] = Vector3.Zero;
                        }
                        else
                        {
                            LightMaskMode tileMask = self.GetMaskMode(i, j);
                            outputMap.SetMaskAt(i - area.X, j - area.Y, tileMask);
                            self.GetTileLight(i, j, out var outputColor);
                            outputMap[i - area.X, j - area.Y] = outputColor;
                        }
                    }
                }
            }, null);*/

            
        }

        private static bool IsTileNullOrTouchingNull(int x, int y)
        {
            var a = StaticInstance.WorldInstance[x, y];
            var b = StaticInstance.WorldInstance[x + 1, y];
            var c = StaticInstance.WorldInstance[x - 1, y];
            var d = StaticInstance.WorldInstance[x, y - 1];
            var e = StaticInstance.WorldInstance[x, y + 1];

            if (WorldGen.InWorld(x, y, 1))
            {
                if (a != null && b != null && c != null && d != null)
                {
                    return e == null;
                }
                return true;
            }

            return true;
        }

        public static void WallFrame(On_Framing.orig_WallFrame orig, int i, int j, bool resetFrame = false)
        {
            if (WorldGen.SkipFramingBecauseOfGen || i <= 0 || j <= 0 || i >= Main.maxTilesX - 1 || j >= Main.maxTilesY - 1 || StaticInstance.WorldInstance[i, j] == null)
            {
                return;
            }
            if (StaticInstance.WorldInstance[i, j].WallType >= WallLoader.WallCount)
            {
                StaticInstance.WorldInstance[i, j].WallType = 0;
            }
            WorldGen.UpdateMapTile(i, j);
            Tile tile = StaticInstance.WorldInstance[i, j];
            if (tile.WallType == 0)
            {
                tile.WallColor = 0;
                tile.ClearWallPaintAndCoating();
                return;
            }
            int style = 0;
            bool flag = Main.ShouldShowInvisibleWalls();
            if (j - 1 >= 0)
            {
                Tile tile2 = StaticInstance.WorldInstance[i, j - 1];
                if (tile2 != null && (tile2.WallType > 0 || (tile2.HasTile && TileID.Sets.WallsMergeWith[tile2.TileType])) && (flag || !tile2.IsWallInvisible))
                {
                    style = 1;
                }
            }
            if (i - 1 >= 0)
            {
                Tile tile3 = StaticInstance.WorldInstance[i - 1, j];
                if (tile3 != null && (tile3.WallType > 0 || (tile3.HasTile && TileID.Sets.WallsMergeWith[tile3.TileType])) && (flag || !tile3.IsWallInvisible))
                {
                    style |= 2;
                }
            }
            if (i + 1 <= Main.maxTilesX - 1)
            {
                Tile tile4 = StaticInstance.WorldInstance[i + 1, j];
                if (tile4 != null && (tile4.WallType > 0 || (tile4.HasTile && TileID.Sets.WallsMergeWith[tile4.TileType])) && (flag || !tile4.IsWallInvisible))
                {
                    style |= 4;
                }
            }
            if (j + 1 <= Main.maxTilesY - 1)
            {
                Tile tile5 = StaticInstance.WorldInstance[i, j + 1];
                if (tile5 != null && (tile5.WallType > 0 || (tile5.HasTile && TileID.Sets.WallsMergeWith[tile5.TileType])) && (flag || !tile5.IsWallInvisible))
                {
                    style |= 8;
                }
            }
            int num = 0;
            if (Main.wallLargeFrames[tile.WallType] == 1)
            {
                num = Terraria.Framing.phlebasTileFrameNumberLookup[j % 4][i % 3] - 1;
            }
            else if (Main.wallLargeFrames[tile.WallType] == 2)
            {
                num = Terraria.Framing.lazureTileFrameNumberLookup[i % 2][j % 2] - 1;
            }
            else if (resetFrame)
            {
                num = WorldGen.genRand.Next(0, 3);
                if (tile.WallType == 21 && WorldGen.genRand.Next(2) == 0)
                {
                    num = 2;
                }
            }
            else
            {
                num = tile.WallFrameNumber;
            }
            if (style == 15)
            {
                style += Terraria.Framing.centerWallFrameLookup[i % 3][j % 3];
            }
            if (WallLoader.WallFrame(i, j, tile.WallType, resetFrame, ref style, ref num))
            {
                tile.WallFrameNumber = num;
                Point16 point = Terraria.Framing.wallFrameLookup[style][num];
                tile.WallFrameX = point.X;
                tile.WallFrameY = point.Y;
            }
        }
    }
}
