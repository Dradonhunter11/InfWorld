using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InfiniteWorldLibrary;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Social;
using Terraria.Utilities;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.WorldGen;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace InfWorld.Patching.Detours
{
    static partial class Detours
    {
        private static bool RemoveInWorldCheck(Terraria.On_WorldGen.orig_InWorld orig, int i, int i1, int fluff) => true;

        private static void OnClearWorld(Terraria.On_WorldGen.orig_clearWorld orig)
        {
            return;
        }

		public static void LoadWorld(bool loadFromCloud)
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
						string message = Language.GetTextValue("Error.LoadFailedNoBackup");
						if (WorldIO.customDataFail != null)
						{
							message = WorldIO.customDataFail.modName + " " + message;
							message = message + "\n" + WorldIO.customDataFail.InnerException;
						}
						Console.WriteLine(message);
						return;
					}
					FileUtilities.Copy(Main.worldPathName, Main.worldPathName + ".bad", isCloudSave);
					FileUtilities.Copy(Main.worldPathName + ".bak", Main.worldPathName, isCloudSave);
					FileUtilities.Delete(Main.worldPathName + ".bak", isCloudSave);
					// WorldIO.LoadDedServBackup(Main.worldPathName, isCloudSave);
					WorldFile.LoadWorld(Main.ActiveWorldFileData.IsCloudSave);
					if (WorldGen.loadFailed || !WorldGen.loadSuccess)
					{
						WorldFile.LoadWorld(Main.ActiveWorldFileData.IsCloudSave);
						if (WorldGen.loadFailed || !WorldGen.loadSuccess)
						{
							FileUtilities.Copy(Main.worldPathName, Main.worldPathName + ".bak", isCloudSave);
							FileUtilities.Copy(Main.worldPathName + ".bad", Main.worldPathName, isCloudSave);
							FileUtilities.Delete(Main.worldPathName + ".bad", isCloudSave);
							// WorldIO.RevertDedServBackup(Main.worldPathName, isCloudSave);
							string message2 = Language.GetTextValue("Error.LoadFailed");
							if (WorldIO.customDataFail != null)
							{
								message2 = WorldIO.customDataFail.modName + " " + message2;
								message2 = message2 + "\n" + WorldIO.customDataFail.InnerException;
							}
							Console.WriteLine(message2);
							return;
						}
					}
				}
			}
			if (Main.mapEnabled)
			{
				Main.Map.Load();
			}
			if (Main.netMode != NetmodeID.Server)
			{
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
			if (Main.netMode == NetmodeID.SinglePlayer && Main.anglerWhoFinishedToday.Contains(Main.player[Main.myPlayer].name))
			{
				Main.anglerQuestFinished = true;
			}
			Main.OnTickForInternalCodeOnly += FinishPlayWorld;
		}

		internal static void FinishPlayWorld()
        {
			Main.OnTickForInternalCodeOnly -= FinishPlayWorld;
			Main.player[Main.myPlayer].Spawn(PlayerSpawnContext.SpawningIntoWorld);
            Main.player[Main.myPlayer].Update(Main.myPlayer);
            Main.ActivePlayerFileData.StartPlayTimer();
            WorldGen._lastSeed = Main.ActiveWorldFileData.Seed;
            Player.Hooks.EnterWorld(Main.myPlayer);
            WorldFile.SetOngoingToTemps();
            //Main.PlaySound(11);
            Main.resetClouds = true;
            WorldGen.noMapUpdate = false;
        }

        public static bool PlaceTile(int i, int j, int Type, bool mute = false, bool forced = false, int plr = -1, int style = 0)
        {
            int num = Type;
            if (gen && StaticInstance.WorldInstance[i, j].HasTile && StaticInstance.WorldInstance[i, j].TileType == 488)
                return false;

            /*
            if (num >= TileID.Count)
                return false;
            */

            bool result = false;
            if (i >= 0 && j >= 0)
            {
                Tile tile = StaticInstance.WorldInstance[i, j];
                if (tile == null)
                {
                    tile = new Tile();
                    StaticInstance.WorldInstance[i, j] = tile;
                }

                if (tile.HasTile)
                {
                    if (num == 23 && tile.TileType == 59)
                        num = 661;

                    if (num == 199 && tile.TileType == 59)
                        num = 662;
                }

                if (forced || Collision.EmptyTile(i, j) || !Main.tileSolid[num] || (num == 23 && tile.TileType == 0 && tile.HasTile) || (num == 199 && tile.TileType == 0 && tile.HasTile) || (num == 2 && tile.TileType == 0 && tile.HasTile) || (num == 109 && tile.TileType == 0 && tile.HasTile) || (num == 60 && tile.TileType == 59 && tile.HasTile) || (num == 661 && tile.TileType == 59 && tile.HasTile) || (num == 662 && tile.TileType == 59 && tile.HasTile) || (num == 70 && tile.TileType == 59 && tile.HasTile) || (num == 633 && tile.TileType == 57 && tile.HasTile) || (Main.tileMoss[num] && (tile.TileType == 1 || tile.TileType == 38) && tile.HasTile))
                {
                    if (num == 23 && (tile.TileType != 0 || !tile.HasTile))
                        return false;

                    if (num == 199 && (tile.TileType != 0 || !tile.HasTile))
                        return false;

                    if (num == 2 && (tile.TileType != 0 || !tile.HasTile))
                        return false;

                    if (num == 109 && (tile.TileType != 0 || !tile.HasTile))
                        return false;

                    if (num == 60 && (tile.TileType != 59 || !tile.HasTile))
                        return false;

                    if (num == 661 && (tile.TileType != 59 || !tile.HasTile))
                        return false;

                    if (num == 662 && (tile.TileType != 59 || !tile.HasTile))
                        return false;

                    if (num == 70 && (tile.TileType != 59 || !tile.HasTile))
                        return false;

                    if (num == 633 && (tile.TileType != 57 || !tile.HasTile))
                        return false;

                    if (Main.tileMoss[num])
                    {
                        if ((tile.TileType != 1 && tile.TileType != 38) || !tile.HasTile)
                            return false;

                        if (tile.TileType == 38)
                        {
                            switch (num)
                            {
                                case 381:
                                    num = 517;
                                    break;
                                case 534:
                                    num = 535;
                                    break;
                                case 536:
                                    num = 537;
                                    break;
                                case 539:
                                    num = 540;
                                    break;
                                case 625:
                                    num = 626;
                                    break;
                                case 627:
                                    num = 628;
                                    break;
                                default:
                                    num = 512 + num - 179;
                                    break;
                            }
                        }
                    }

                    if (num == 81)
                    {
                        if (StaticInstance.WorldInstance[i, j - 1] == null)
                            StaticInstance.WorldInstance[i, j - 1] = new Tile();

                        if (StaticInstance.WorldInstance[i, j + 1] == null)
                            StaticInstance.WorldInstance[i, j + 1] = new Tile();

                        if (StaticInstance.WorldInstance[i, j - 1].HasTile)
                            return false;

                        if (!StaticInstance.WorldInstance[i, j + 1].HasTile || !Main.tileSolid[StaticInstance.WorldInstance[i, j + 1].TileType] || StaticInstance.WorldInstance[i, j + 1].IsHalfBlock || StaticInstance.WorldInstance[i, j + 1].Slope != 0)
                            return false;
                    }

                    if ((num == 373 || num == 375 || num == 374 || num == 461) && (StaticInstance.WorldInstance[i, j - 1] == null || StaticInstance.WorldInstance[i, j - 1].BottomSlope))
                        return false;

                    if (tile.LiquidAmount > 0 || tile.CheckingLiquid)
                    {
                        switch (num)
                        {
                            case 4:
                                if (style != 8 && style != 11 && style != 17)
                                    return false;
                                break;
                            case int _ when TileID.Sets.Torch[num]:
                                if (TileObjectData.GetTileData(num, style).WaterPlacement != LiquidPlacement.Allowed)
                                    return false;
                                break;
                            case 3:
                            case int _ when TileID.Sets.TreeSapling[num]:
                            case 24:
                            case 27:
                            case 32:
                            case 51:
                            case 69:
                            case 72:
                            case 201:
                            case 352:
                            case 529:
                            case 624:
                            case 637:
                            case 656:
                                return false;
                        }
                    }

                    if (TileID.Sets.ResetsHalfBrickPlacementAttempt[num] && (!tile.HasTile || !Main.tileFrameImportant[tile.TileType]))
                    {
                        tile.IsHalfBlock = false;
                        tile.TileFrameY = 0;
                        tile.TileFrameX = 0;
                    }
                    /*
                    if (num == 624)
                    {
                        if ((!tile.HasTile || Main.tileCut[tile.TileType] || TileID.Sets.BreakableWhenPlacing[tile.TileType]) && WorldGen.HasValidGroundForAbigailsFlowerBelowSpot(i, j))
                        {
                            tile.active(active: true);
                            tile.TileType = 624;
                            tile.halfBrick(halfBrick: false);
                            tile.slope(0);
                            tile.frameX = 0;
                            tile.frameY = 0;
                        }
                    }
                    else if (num == 656)
                    {
                        if ((!tile.HasTile || Main.tileCut[tile.TileType] || TileID.Sets.BreakableWhenPlacing[tile.TileType]) && HasValidGroundForGlowTulipBelowSpot(i, j))
                        {
                            tile.active(active: true);
                            tile.TileType = 656;
                            tile.halfBrick(halfBrick: false);
                            tile.slope(0);
                            tile.frameX = 0;
                            tile.frameY = 0;
                        }
                    }*/
                    if (num == 3 || num == 24 || num == 110 || num == 201 || num == 637)
                    {
                        if (IsFitToPlaceFlowerIn(i, j, num))
                        {
                            if (num == 24 && genRand.Next(13) == 0)
                            {
                                tile.HasTile = true;
                                tile.TileType = 32;
                                SquareTileFrame(i, j);
                            }
                            else if (num == 201 && genRand.Next(13) == 0)
                            {
                                tile.HasTile = true;
                                tile.TileType = 352;
                                SquareTileFrame(i, j);
                            }
                            else if (StaticInstance.WorldInstance[i, j + 1].TileType == 78 || StaticInstance.WorldInstance[i, j + 1].TileType == 380 || StaticInstance.WorldInstance[i, j + 1].TileType == 579)
                            {
                                tile.HasTile = true;
                                tile.TileType = (ushort)num;
                                int num2 = genRand.NextFromList<int>(6, 7, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 24, 27, 30, 33, 36, 39, 42);
                                switch (num2)
                                {
                                    case 21:
                                    case 24:
                                    case 27:
                                    case 30:
                                    case 33:
                                    case 36:
                                    case 39:
                                    case 42:
                                        num2 += genRand.Next(3);
                                        break;
                                }

                                tile.TileFrameX = (short)(num2 * 18);
                            }
                            else if (tile.WallType >= 0 && WallID.Sets.AllowsPlantsToGrow[tile.WallType] && StaticInstance.WorldInstance[i, j + 1].WallType >= 0 && StaticInstance.WorldInstance[i, j + 1].WallType < WallLoader.WallCount && WallID.Sets.AllowsPlantsToGrow[StaticInstance.WorldInstance[i, j + 1].WallType])
                            {
                                if (genRand.Next(50) == 0 || ((num == 24 || num == 201) && genRand.Next(40) == 0))
                                {
                                    tile.HasTile = true;
                                    tile.TileType = (ushort)num;
                                    if (num == 201)
                                        tile.TileFrameX = 270;
                                    else
                                        tile.TileFrameX = 144;
                                }
                                else if (genRand.Next(35) == 0 || (StaticInstance.WorldInstance[i, j].WallType >= 63 && StaticInstance.WorldInstance[i, j].WallType <= 70))
                                {
                                    tile.HasTile = true;
                                    tile.TileType = (ushort)num;
                                    int num3 = genRand.NextFromList<int>(6, 7, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20);
                                    if (num == 201)
                                        num3 = genRand.NextFromList<int>(6, 7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 18, 19, 20, 21, 22);

                                    if (num == 637)
                                        num3 = genRand.NextFromList<int>(6, 7, 8, 9, 10);

                                    tile.TileFrameX = (short)(num3 * 18);
                                }
                                else
                                {
                                    tile.HasTile = true;
                                    tile.TileType = (ushort)num;
                                    tile.TileFrameX = (short)(genRand.Next(6) * 18);
                                }
                            }
                        }
                    }
                    else if (num == 61)
                    {
                        if (j + 1 < Main.maxTilesY && StaticInstance.WorldInstance[i, j + 1].HasTile && StaticInstance.WorldInstance[i, j + 1].Slope == 0 && !StaticInstance.WorldInstance[i, j + 1].IsHalfBlock && StaticInstance.WorldInstance[i, j + 1].TileType == 60)
                        {
                            bool flag = (double)j > Main.rockLayer || Main.remixWorld || remixWorldGen;
                            if (genRand.Next(16) == 0 && (double)j > Main.worldSurface)
                            {
                                tile.HasTile = true;
                                tile.TileType = 69;
                                SquareTileFrame(i, j);
                            }
                            else if (genRand.Next(60) == 0 && flag)
                            {
                                tile.HasTile = true;
                                tile.TileType = (ushort)num;
                                tile.TileFrameX = 144;
                            }
                            else if (genRand.Next(230) == 0 && flag)
                            {
                                tile.HasTile = true;
                                tile.TileType = (ushort)num;
                                tile.TileFrameX = 162;
                            }
                            else if (genRand.Next(15) == 0)
                            {
                                tile.HasTile = true;
                                tile.TileType = (ushort)num;
                                if (genRand.Next(3) != 0)
                                    tile.TileFrameX = (short)(genRand.Next(2) * 18 + 108);
                                else
                                    tile.TileFrameX = (short)(genRand.Next(13) * 18 + 180);
                            }
                            else
                            {
                                tile.HasTile = true;
                                tile.TileType = (ushort)num;
                                tile.TileFrameX = (short)(genRand.Next(6) * 18);
                            }
                        }
                    }
                    else if (num == 518)
                    {
                        PlaceLilyPad(i, j);
                    }
                    else if (num == 519)
                    {
                        PlaceCatTail(i, j);
                    }
                    else if (num == 529)
                    {
                        // PlantSeaOat(i, j);
                    }
                    else if (num == 571)
                    {
                        // PlaceBamboo(i, j);
                    }
                    else if (num == 549)
                    {
                        PlaceUnderwaterPlant(549, i, j);
                    }
                    else if (num == 71)
                    {
                        if (j + 1 < Main.maxTilesY && StaticInstance.WorldInstance[i, j + 1].HasTile && StaticInstance.WorldInstance[i, j + 1].Slope == 0 && !StaticInstance.WorldInstance[i, j + 1].IsHalfBlock && StaticInstance.WorldInstance[i, j + 1].TileType == 70)
                        {
                            Point point = new Point(-1, -1);
                            if ((double)j > Main.worldSurface)
                                point = PlaceCatTail(i, j);

                            if (InWorld(point.X, point.Y))
                            {
                                if (gen)
                                {
                                    int num4 = genRand.Next(14);
                                    for (int k = 0; k < num4; k++)
                                    {
                                        GrowCatTail(point.X, point.Y);
                                    }

                                    SquareTileFrame(point.X, point.Y);
                                }
                            }
                            else
                            {
                                tile.HasTile = true;
                                tile.TileType = (ushort)num;
                                tile.TileFrameX = (short)(genRand.Next(5) * 18);
                            }
                        }
                    }
                    else if (num == 129)
                    {
                        if (SolidTile(i - 1, j) || SolidTile(i + 1, j) || SolidTile(i, j - 1) || SolidTile(i, j + 1))
                        {
                            tile.HasTile = true;
                            tile.TileType = (ushort)num;
                            tile.TileFrameX = (short)(style * 18);
                            SquareTileFrame(i, j);
                        }
                    }
                    else if (num == 178)
                    {
                        if (SolidTile(i - 1, j, noDoors: true) || SolidTile(i + 1, j, noDoors: true) || SolidTile(i, j - 1) || SolidTile(i, j + 1))
                        {
                            tile.HasTile = true;
                            tile.TileType = (ushort)num;
                            tile.TileFrameX = (short)(style * 18);
                            tile.TileFrameY = (short)(genRand.Next(3) * 18);
                            SquareTileFrame(i, j);
                        }
                    }
                    else if (num == 184)
                    {
                        if ((Main.tileMoss[StaticInstance.WorldInstance[i - 1, j].TileType] && SolidTile(i - 1, j)) || (Main.tileMoss[StaticInstance.WorldInstance[i + 1, j].TileType] && SolidTile(i + 1, j)) || (Main.tileMoss[StaticInstance.WorldInstance[i, j - 1].TileType] && SolidTile(i, j - 1)) || (Main.tileMoss[StaticInstance.WorldInstance[i, j + 1].TileType] && SolidTile(i, j + 1)))
                        {
                            tile.HasTile = true;
                            tile.TileType = (ushort)num;
                            tile.TileFrameX = (short)(style * 18);
                            tile.TileFrameY = (short)(genRand.Next(3) * 18);
                            SquareTileFrame(i, j);
                        }

                        if ((TileID.Sets.tileMossBrick[StaticInstance.WorldInstance[i - 1, j].TileType] && SolidTile(i - 1, j)) || (TileID.Sets.tileMossBrick[StaticInstance.WorldInstance[i + 1, j].TileType] && SolidTile(i + 1, j)) || (TileID.Sets.tileMossBrick[StaticInstance.WorldInstance[i, j - 1].TileType] && SolidTile(i, j - 1)) || (TileID.Sets.tileMossBrick[StaticInstance.WorldInstance[i, j + 1].TileType] && SolidTile(i, j + 1)))
                        {
                            tile.HasTile = true;
                            tile.TileType = (ushort)num;
                            tile.TileFrameX = (short)(style * 18);
                            tile.TileFrameY = (short)(genRand.Next(3) * 18);
                            SquareTileFrame(i, j);
                        }
                    }
                    else if (num == 485)
                    {
                        PlaceObject(i, j, num, mute, style);
                    }
                    else if (num == 171)
                    {
                        PlaceXmasTree(i, j, 171);
                    }
                    else if (num == 254)
                    {
                        Place2x2Style(i, j, (ushort)num, style);
                    }
                    else if (num == 335 || num == 564 || num == 594)
                    {
                        Place2x2(i, j, (ushort)num, 0);
                    }
                    else if (num == 654 || num == 319 || num == 132 || num == 484 || num == 138 || num == 664 || num == 142 || num == 143 || num == 282 || (num >= 288 && num <= 295) || (num >= 316 && num <= 318))
                    {
                        Place2x2(i, j, (ushort)num, 0);
                    }
                    else if (num == 411)
                    {
                        Place2x2(i, j, (ushort)num, 0);
                    }
                    else if (num == 457)
                    {
                        Place2x2Horizontal(i, j, 457, style);
                    }
                    else if (num == 137)
                    {
                        tile.HasTile = true;
                        tile.TileType = (ushort)num;
                        tile.TileFrameY = (short)(18 * style);
                    }
                    else if (num == 136)
                    {
                        if (StaticInstance.WorldInstance[i - 1, j] == null)
                            StaticInstance.WorldInstance[i - 1, j] = new Tile();

                        if (StaticInstance.WorldInstance[i + 1, j] == null)
                            StaticInstance.WorldInstance[i + 1, j] = new Tile();

                        if (StaticInstance.WorldInstance[i, j + 1] == null)
                            StaticInstance.WorldInstance[i, j + 1] = new Tile();

                        if ((StaticInstance.WorldInstance[i - 1, j].IsActuated && !StaticInstance.WorldInstance[i - 1, j].IsHalfBlock && !TileID.Sets.NotReallySolid[StaticInstance.WorldInstance[i - 1, j].TileType] && StaticInstance.WorldInstance[i - 1, j].Slope == 0 && (SolidTile(i - 1, j) || TileID.Sets.IsBeam[StaticInstance.WorldInstance[i - 1, j].TileType] || (StaticInstance.WorldInstance[i - 1, j].TileType == 5 && StaticInstance.WorldInstance[i - 1, j - 1].TileType == 5 && StaticInstance.WorldInstance[i - 1, j + 1].TileType == 5))) || (StaticInstance.WorldInstance[i + 1, j].IsActuated && !StaticInstance.WorldInstance[i + 1, j].IsHalfBlock && !TileID.Sets.NotReallySolid[StaticInstance.WorldInstance[i + 1, j].TileType] && StaticInstance.WorldInstance[i + 1, j].Slope == 0 && (SolidTile(i + 1, j) || TileID.Sets.IsBeam[StaticInstance.WorldInstance[i + 1, j].TileType] || (StaticInstance.WorldInstance[i + 1, j].TileType == 5 && StaticInstance.WorldInstance[i + 1, j - 1].TileType == 5 && StaticInstance.WorldInstance[i + 1, j + 1].TileType == 5))) || (StaticInstance.WorldInstance[i, j + 1].IsActuated && !StaticInstance.WorldInstance[i, j + 1].IsHalfBlock && SolidTile(i, j + 1) && StaticInstance.WorldInstance[i, j + 1].Slope == 0) || tile.WallType > 0)
                        {
                            tile.HasTile = true;
                            tile.TileType = (ushort)num;
                            SquareTileFrame(i, j);
                        }
                    }
                    else if (num == 442)
                    {
                        if (StaticInstance.WorldInstance[i - 1, j] == null)
                            StaticInstance.WorldInstance[i - 1, j] = new Tile();

                        if (StaticInstance.WorldInstance[i + 1, j] == null)
                            StaticInstance.WorldInstance[i + 1, j] = new Tile();

                        if (StaticInstance.WorldInstance[i, j + 1] == null)
                            StaticInstance.WorldInstance[i, j + 1] = new Tile();

                        if ((StaticInstance.WorldInstance[i - 1, j].IsActuated && !StaticInstance.WorldInstance[i - 1, j].IsHalfBlock && !TileID.Sets.NotReallySolid[StaticInstance.WorldInstance[i - 1, j].TileType] && StaticInstance.WorldInstance[i - 1, j].Slope == 0 && (SolidTile(i - 1, j) || TileID.Sets.IsBeam[StaticInstance.WorldInstance[i - 1, j].TileType] || (StaticInstance.WorldInstance[i - 1, j].TileType == 5 && StaticInstance.WorldInstance[i - 1, j - 1].TileType == 5 && StaticInstance.WorldInstance[i - 1, j + 1].TileType == 5))) || (StaticInstance.WorldInstance[i + 1, j].IsActuated && !StaticInstance.WorldInstance[i + 1, j].IsHalfBlock && !TileID.Sets.NotReallySolid[StaticInstance.WorldInstance[i + 1, j].TileType] && StaticInstance.WorldInstance[i + 1, j].Slope == 0 && (SolidTile(i + 1, j) || TileID.Sets.IsBeam[StaticInstance.WorldInstance[i + 1, j].TileType] || (StaticInstance.WorldInstance[i + 1, j].TileType == 5 && StaticInstance.WorldInstance[i + 1, j - 1].TileType == 5 && StaticInstance.WorldInstance[i + 1, j + 1].TileType == 5))) || (StaticInstance.WorldInstance[i, j + 1].IsActuated && !StaticInstance.WorldInstance[i, j + 1].IsHalfBlock && SolidTile(i, j + 1) && StaticInstance.WorldInstance[i, j + 1].Slope == 0))
                        {
                            tile.HasTile = true;
                            tile.TileType = (ushort)num;
                            SquareTileFrame(i, j);
                        }
                    }
                    else if (TileID.Sets.Torch[num])
                    {
                        if (StaticInstance.WorldInstance[i - 1, j] == null)
                            StaticInstance.WorldInstance[i - 1, j] = new Tile();

                        if (StaticInstance.WorldInstance[i + 1, j] == null)
                            StaticInstance.WorldInstance[i + 1, j] = new Tile();

                        if (StaticInstance.WorldInstance[i, j + 1] == null)
                            StaticInstance.WorldInstance[i, j + 1] = new Tile();

                        Tile tile2 = StaticInstance.WorldInstance[i - 1, j];
                        Tile tile3 = StaticInstance.WorldInstance[i + 1, j];
                        Tile tile4 = StaticInstance.WorldInstance[i, j + 1];
                        if (tile.WallType > 0 || (tile2.HasTile && (tile2.Slope == 0 || (int)tile2.Slope % 2 != 1) && ((Main.tileSolid[tile2.TileType] && !Main.tileSolidTop[tile2.TileType] && !TileID.Sets.NotReallySolid[tile2.TileType]) || TileID.Sets.IsBeam[tile2.TileType] || (IsTreeType(tile2.TileType) && IsTreeType(StaticInstance.WorldInstance[i - 1, j - 1].TileType) && IsTreeType(StaticInstance.WorldInstance[i - 1, j + 1].TileType)))) || (tile3.HasTile && (tile3.Slope == 0 || (int)tile3.Slope % 2 != 0) && ((Main.tileSolid[tile3.TileType] && !Main.tileSolidTop[tile3.TileType] && !TileID.Sets.NotReallySolid[tile3.TileType]) || TileID.Sets.IsBeam[tile3.TileType] || (IsTreeType(tile3.TileType) && IsTreeType(StaticInstance.WorldInstance[i + 1, j - 1].TileType) && IsTreeType(StaticInstance.WorldInstance[i + 1, j + 1].TileType)))) || (tile4.HasTile && Main.tileSolid[tile4.TileType] && ((TileID.Sets.Platforms[tile4.TileType] && TopEdgeCanBeAttachedTo(i, j + 1)) || ((!Main.tileSolidTop[tile4.TileType] || (tile4.TileType == 380 && tile4.Slope == 0)) && !TileID.Sets.NotReallySolid[tile4.TileType] && !tile4.IsHalfBlock && tile4.Slope == 0))))
                        {
                            tile.HasTile = true;
                            tile.TileType = (ushort)num;
                            tile.TileFrameY = (short)(22 * style);
                            SquareTileFrame(i, j);
                        }
                    }
                    else if (num == 10)
                    {
                        if (StaticInstance.WorldInstance[i, j - 1] == null)
                            StaticInstance.WorldInstance[i, j - 1] = new Tile();

                        if (StaticInstance.WorldInstance[i, j - 2] == null)
                            StaticInstance.WorldInstance[i, j - 2] = new Tile();

                        if (StaticInstance.WorldInstance[i, j - 3] == null)
                            StaticInstance.WorldInstance[i, j - 3] = new Tile();

                        if (StaticInstance.WorldInstance[i, j + 1] == null)
                            StaticInstance.WorldInstance[i, j + 1] = new Tile();

                        if (StaticInstance.WorldInstance[i, j + 2] == null)
                            StaticInstance.WorldInstance[i, j + 2] = new Tile();

                        if (StaticInstance.WorldInstance[i, j + 3] == null)
                            StaticInstance.WorldInstance[i, j + 3] = new Tile();

                        if (!StaticInstance.WorldInstance[i, j - 1].HasTile && !StaticInstance.WorldInstance[i, j - 2].HasTile && StaticInstance.WorldInstance[i, j - 3].HasTile && Main.tileSolid[StaticInstance.WorldInstance[i, j - 3].TileType])
                        {
                            PlaceDoor(i, j - 1, num, style);
                            SquareTileFrame(i, j);
                        }
                        else
                        {
                            if (StaticInstance.WorldInstance[i, j + 1].HasTile || StaticInstance.WorldInstance[i, j + 2].HasTile || !StaticInstance.WorldInstance[i, j + 3].HasTile || !Main.tileSolid[StaticInstance.WorldInstance[i, j + 3].TileType])
                                return false;

                            PlaceDoor(i, j + 1, num, style);
                            SquareTileFrame(i, j);
                        }
                    }
                    else if ((num >= 275 && num <= 281) || num == 296 || num == 297 || num == 309 || num == 358 || num == 359 || num == 413 || num == 414 || num == 542)
                    {
                        Place6x3(i, j, (ushort)num);
                    }
                    else if (num == 237 || num == 244 || num == 285 || num == 286 || num == 298 || num == 299 || num == 310 || num == 339 || num == 538 || (num >= 361 && num <= 364) || num == 532 || num == 533 || num == 486 || num == 488 || num == 544 || num == 582 || num == 619 || num == 629)
                    {
                        Place3x2(i, j, (ushort)num);
                    }
                    else if (num == 128)
                    {
                        PlaceMan(i, j, style);
                        SquareTileFrame(i, j);
                    }
                    else if (num == 269)
                    {
                        PlaceWoman(i, j, style);
                        SquareTileFrame(i, j);
                    }
                    else if (num == 334)
                    {
                        int style2 = 0;
                        if (style == -1)
                            style2 = 1;

                        Place3x3Wall(i, j, 334, style2);
                        SquareTileFrame(i, j);
                    }
                    else if (num == 149)
                    {
                        if (SolidTile(i - 1, j) || SolidTile(i + 1, j) || SolidTile(i, j - 1) || SolidTile(i, j + 1))
                        {
                            tile.TileFrameX = (short)(18 * style);
                            tile.HasTile = true;
                            tile.TileType = (ushort)num;
                            SquareTileFrame(i, j);
                        }
                    }
                    else if (num == 139 || num == 35)
                    {
                        PlaceMB(i, j, (ushort)num, style);
                        SquareTileFrame(i, j);
                    }
                    else if (num == 165)
                    {
                        PlaceTight(i, j);
                        SquareTileFrame(i, j);
                    }
                    else if (num == 235)
                    {
                        Place3x1(i, j, (ushort)num);
                        SquareTileFrame(i, j);
                    }
                    else if (num == 240)
                    {
                        Place3x3Wall(i, j, (ushort)num, style);
                    }
                    else if (num == 440)
                    {
                        Place3x3Wall(i, j, (ushort)num, style);
                    }
                    else if (num == 245)
                    {
                        Place2x3Wall(i, j, (ushort)num, style);
                    }
                    else if (num == 246)
                    {
                        Place3x2Wall(i, j, (ushort)num, style);
                    }
                    else if (num == 241)
                    {
                        Place4x3Wall(i, j, (ushort)num, style);
                    }
                    else if (num == 242)
                    {
                        Place6x4Wall(i, j, (ushort)num, style);
                    }
                    else if (num == 34)
                    {
                        PlaceChand(i, j, (ushort)num, style);
                        SquareTileFrame(i, j);
                    }
                    else if (num == 106 || num == 212 || num == 219 || num == 220 || num == 228 || num == 231 || num == 243 || num == 247 || num == 283 || (num >= 300 && num <= 308) || num == 354 || num == 355 || num == 491 || num == 642)
                    {
                        Place3x3(i, j, (ushort)num, style);
                        SquareTileFrame(i, j);
                    }
                    else
                    {
                        switch (num)
                        {
                            case 13:
                            case 33:
                            case 49:
                            case 50:
                            case 78:
                            case 174:
                            case 372:
                            case 646:
                                PlaceOnTable1x1(i, j, num, style);
                                SquareTileFrame(i, j);
                                break;
                            case 14:
                            case 26:
                            case 86:
                            case 87:
                            case int _ when TileID.Sets.BasicDresser[num]:
                            case 89:
                            case 114:
                            case 186:
                            case 187:
                            case 215:
                            case 217:
                            case 218:
                            case 377:
                            case 469:
                                Place3x2(i, j, (ushort)num, style);
                                SquareTileFrame(i, j);
                                break;
                            case 236:
                                PlaceJunglePlant(i, j, (ushort)num, genRand.Next(3), 0);
                                SquareTileFrame(i, j);
                                break;
                            case 238:
                                PlaceJunglePlant(i, j, (ushort)num, 0, 0);
                                SquareTileFrame(i, j);
                                break;
                            case int _ when TileID.Sets.TreeSapling[num]:
                            {
                                if (StaticInstance.WorldInstance[i, j + 1] == null)
                                    StaticInstance.WorldInstance[i, j + 1] = new Tile();

                                int type = StaticInstance.WorldInstance[i, j + 1].TileType;
                                int dummyType = TileID.Saplings;
                                int dummyStyle = 0;
                                if (StaticInstance.WorldInstance[i, j + 1].HasTile && (type == 2 || type == 109 || type == 147 || type == 60 || type == 23 || type == 199 || type == 661 || type == 662 || type == 53 || type == 234 || type == 116 || type == 112 || TileLoader.SaplingGrowthType(type, ref dummyType, ref dummyStyle)))
                                {
                                    Place1x2(i, j, (ushort)dummyType, dummyStyle);
                                    SquareTileFrame(i, j);
                                }

                                break;
                            }
                            case 15:
                            case 216:
                            case 338:
                            case 390:
                                if (StaticInstance.WorldInstance[i, j - 1] == null)
                                    StaticInstance.WorldInstance[i, j - 1] = new Tile();
                                if (StaticInstance.WorldInstance[i, j] == null)
                                    StaticInstance.WorldInstance[i, j] = new Tile();
                                Place1x2(i, j, (ushort)num, style);
                                SquareTileFrame(i, j);
                                break;
                            case 227:
                                PlaceDye(i, j, style);
                                SquareTileFrame(i, j);
                                break;
                            case 567:
                                PlaceGnome(i, j, style);
                                SquareTileFrame(i, j);
                                break;
                            case 16:
                            case 18:
                            case 29:
                            case 103:
                            case 134:
                            case 462:
                                Place2x1(i, j, (ushort)num, style);
                                SquareTileFrame(i, j);
                                break;
                            case 92:
                            case 93:
                            case 453:
                                Place1xX(i, j, (ushort)num, style);
                                SquareTileFrame(i, j);
                                break;
                            case 104:
                            case 105:
                            case 320:
                            case 337:
                            case 349:
                            case 356:
                            case 378:
                            case 456:
                            case 506:
                            case 545:
                            case 663:
                                Place2xX(i, j, (ushort)num, style);
                                SquareTileFrame(i, j);
                                break;
                            case 17:
                            case 77:
                            case 133:
                                Place3x2(i, j, (ushort)num, style);
                                SquareTileFrame(i, j);
                                break;
                            case 207:
                                Place2xX(i, j, (ushort)num, style);
                                SquareTileFrame(i, j);
                                break;
                            case 410:
                            case 480:
                            case 509:
                            case 657:
                            case 658:
                                Place2xX(i, j, (ushort)num, style);
                                SquareTileFrame(i, j);
                                break;
                            case 465:
                            case 531:
                            case 591:
                            case 592:
                                Place2xX(i, j, (ushort)num, style);
                                SquareTileFrame(i, j);
                                break;
                            default:
                                if (TileID.Sets.BasicChest[num])
                                {
                                    PlaceChest(i, j, (ushort)num, notNearOtherChests: false, style);
                                    SquareTileFrame(i, j);
                                    break;
                                }
                                switch (num)
                                {
                                    case 91:
                                        PlaceBanner(i, j, (ushort)num, style);
                                        SquareTileFrame(i, j);
                                        break;
                                    case 419:
                                    case 420:
                                    case 423:
                                    case 424:
                                    case 429:
                                    case 445:
                                        PlaceLogicTiles(i, j, num, style);
                                        SquareTileFrame(i, j);
                                        break;
                                    case 36:
                                    case 135:
                                    case 141:
                                    case 144:
                                    case 210:
                                    case 239:
                                    case 324:
                                    case 476:
                                    case 494:
                                        Place1x1(i, j, num, style);
                                        SquareTileFrame(i, j);
                                        break;
                                    case 101:
                                    case 102:
                                    case 463:
                                        Place3x4(i, j, (ushort)num, style);
                                        SquareTileFrame(i, j);
                                        break;
                                    case 464:
                                    case 466:
                                        Place5x4(i, j, (ushort)num, style);
                                        SquareTileFrame(i, j);
                                        break;
                                    case 27:
                                        PlaceSunflower(i, j, 27);
                                        SquareTileFrame(i, j);
                                        break;
                                    case 28:
                                        PlacePot(i, j, 28, genRand.Next(4));
                                        SquareTileFrame(i, j);
                                        break;
                                    case 42:
                                    case 270:
                                    case 271:
                                        Place1x2Top(i, j, (ushort)num, style);
                                        SquareTileFrame(i, j);
                                        break;
                                    case 55:
                                    case 425:
                                    case 510:
                                    case 511:
                                        PlaceSign(i, j, (ushort)num, style);
                                        break;
                                    case 85:
                                    case 376:
                                        Place2x2Horizontal(i, j, (ushort)num, style);
                                        break;
                                    default:
                                        if (Main.tileAlch[num])
                                        {
                                            PlaceAlch(i, j, style);
                                            break;
                                        }
                                        switch (num)
                                        {
                                            case 94:
                                            case 95:
                                            case 97:
                                            case 98:
                                            case 99:
                                            case 100:
                                            case 125:
                                            case 126:
                                            case 172:
                                            case 173:
                                            case 287:
                                                Place2x2(i, j, (ushort)num, style);
                                                break;
                                            case 96:
                                                Place2x2Style(i, j, (ushort)num, style);
                                                break;
                                            case 79:
                                            case 90:
                                            {
                                                int direction = 1;
                                                if (plr > -1)
                                                    direction = Main.player[plr].direction;

                                                Place4x2(i, j, (ushort)num, direction, style);
                                                break;
                                            }
                                            case 209:
                                                PlaceCannon(i, j, (ushort)num, style);
                                                break;
                                            case 81:
                                                tile.TileFrameY = (short)(26 * genRand.Next(6));
                                                tile.HasTile = true;
                                                tile.TileType = (ushort)num;
                                                break;
                                            case 19:
                                                tile.TileFrameY = (short)(18 * style);
                                                tile.HasTile = true;
                                                tile.TileType = (ushort)num;
                                                break;
                                            case 380:
                                                tile.TileFrameY = (short)(18 * style);
                                                tile.HasTile = true;
                                                tile.TileType = (ushort)num;
                                                break;
                                            case 314:
                                                Minecart.PlaceTrack(tile, style);
                                                break;
                                            case int _ when num >= TileID.Count && TileObjectData.GetTileData(num, style) != null:
                                                PlaceObject(i, j, (ushort)num, mute, style);
                                                break;
                                            default:
                                                tile.HasTile = true;
                                                tile.TileType = (ushort)num;
                                                if (Main.tenthAnniversaryWorld && !Main.remixWorld && (num == 53 || num == 396 || num == 397))
                                                    tile.TileColor = 7;
                                                break;
                                        }
                                        break;
                                }
                                break;
                        }
                    }

                    if (tile.HasTile)
                    {
                        if (TileID.Sets.BlocksWaterDrawingBehindSelf[tile.TileType])
                            SquareWallFrame(i, j);

                        SquareTileFrame(i, j);
                        result = true;
                        //TML: To fix a number of errors post-sound-rework, tile placements are now automagically muted during initial worldgen.
                        if (!mute && !generatingWorld)
                        {
                            Vector2 position = new Vector2(i * 16, j * 16);
                            switch (num)
                            {
                                case 127:
                                    SoundEngine.PlaySound(SoundID.Item30, position);
                                    break;
                                case 314:
                                    SoundEngine.PlaySound(SoundID.Item52, position);
                                    break;
                                case TileID.CopperCoinPile:
                                case TileID.SilverCoinPile:
                                case TileID.GoldCoinPile:
                                case TileID.PlatinumCoinPile:
                                    SoundEngine.PlaySound(SoundID.Coins, position);
                                    break;
                                default:
                                    SoundEngine.PlaySound(SoundID.Dig, position);
                                    break;
                            }

                            if (num == 22 || num == 140)
                            {
                                for (int l = 0; l < 3; l++)
                                {
                                    Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, 14);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

    }
}
