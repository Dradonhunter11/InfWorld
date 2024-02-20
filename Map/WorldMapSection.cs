// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using InfWorld.World.Region;
// using log4net;
// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Graphics;
// using Terraria;
// using Terraria.Map;
// using static Terraria.Map.MapHelper;
//
// namespace InfWorld.Map
// {
//     class WorldMapSection
//     {
//         public Texture2D CachedWorldMapSection;
//
//         public MapTile[,] MapTile;
//         public readonly int SectionWidth = Chunk.ChunkWidth;
//         public readonly int SectionHeight = Chunk.ChunkHeight;
//
//         public Ref<Chunk> ChunkReference;
//
//         public bool Loaded = true;
//         private bool _hasChanged = false;
//
//
//
//         public WorldMapSection(Chunk chunk)
//         {
//             MapTile = new MapTile[SectionWidth, SectionHeight];
//             ChunkReference = new Ref<Chunk>(chunk);
//
//             UpdateSection(); // Init the section
//
//             CachedWorldMapSection = new RenderTarget2D(Main.graphics.GraphicsDevice, SectionWidth, SectionHeight);
//         }
//
//         // Will be revealed by default for now
//         // public void revealSection()
//
//         public void UpdateSection()
//         {
//             if (ChunkReference.Value == null)
//             {
//                 Loaded = false;
//                 return;
//             }
//
//             Chunk chunk = ChunkReference.Value;
//             for (int i = 0; i < SectionWidth; i++)
//             {
//                 for (int j = 0; j < SectionHeight; j++)
//                 {
//
//                     Tile tile = chunk[i, j];
//                     MapTile mapTile = CreateMapTile(chunk, i, j, 255); /*Terraria.Map.MapTile.Create(chunk[i, j].type, 1, GetMapTileColor(tile));*/
//                     MapTile[i, j] = mapTile;
//                 }
//             }
//         }
//
//         public void MakeRenderTarget()
//         {
//
//             Color[] textureData = new Color[SectionWidth * SectionHeight];
//             for (int j = 0; j < SectionWidth; j++)
//             {
//                 for (int i = 0; i < SectionHeight; i++)
//                 {
//                     MapTile tile = MapTile[j, i];
//                     Color tileColor = MapHelper.GetMapTileXnaColor(ref tile);
//                     if (tileColor == default(Color))
//                     {
//                         tileColor = new Color(255, 255, 255, 255);
//                     }
//
//                     textureData[i * SectionWidth + j] = tileColor;
//                 }
//             }
//             CachedWorldMapSection.SetData(textureData);
//         }
// 		
//         /*private byte GetMapTileColor(Tile tile)
//         {
//             if (tile == null)
//             {
//                 return 255;
//             }
//
//             if (tile.IsActive)
//             {
//                 return tile.Color;
//             }
//
//             if (tile.wall > 0)
//             {
//                 return tile.WallColor;
//             }
//
//             return 0;
//         }
//
// 		public static MapTile CreateMapTile(Chunk chunk, int i, int j, byte Light)
// 		{
// 			Tile tile = chunk[i, j];
// 			if (tile == null)
// 				tile = (chunk[i, j] = new Tile());
//
// 			int num = 0;
// 			int num2 = Light;
// 			
// 			int num3 = 0;
// 			int num4 = 0;
// 			if (tile.IsActive)
// 			{
// 				int type2 = tile.type;
// 				num3 = tileLookup[type2];
// 				if (type2 == 51 && (i + j) % 2 == 0)
// 					num3 = 0;
//
// 				if (num3 != 0)
// 				{
// 					num = ((type2 != 160) ? tile.Color : 0);
// 					switch (type2)
// 					{
// 						case 4:
// 							if (tile.frameX < 66)
// 								num4 = 1;
// 							num4 = 0;
// 							break;
// 						case 21:
// 						case 441:
// 							switch (tile.frameX / 36)
// 							{
// 								case 1:
// 								case 2:
// 								case 10:
// 								case 13:
// 								case 15:
// 									num4 = 1;
// 									break;
// 								case 3:
// 								case 4:
// 									num4 = 2;
// 									break;
// 								case 6:
// 									num4 = 3;
// 									break;
// 								case 11:
// 								case 17:
// 									num4 = 4;
// 									break;
// 								default:
// 									num4 = 0;
// 									break;
// 							}
// 							break;
// 						case 467:
// 						case 468:
// 							switch (tile.frameX / 36)
// 							{
// 								case 0:
// 									num4 = 0;
// 									break;
// 								case 1:
// 									num4 = 1;
// 									break;
// 								default:
// 									num4 = 0;
// 									break;
// 							}
// 							break;
// 						case 28:
// 							num4 = ((tile.frameY >= 144) ? ((tile.frameY < 252) ? 1 : ((tile.frameY >= 360 && (tile.frameY <= 900 || tile.frameY >= 1008)) ? ((tile.frameY >= 468) ? ((tile.frameY >= 576) ? ((tile.frameY >= 684) ? ((tile.frameY >= 792) ? ((tile.frameY >= 898) ? ((tile.frameY >= 1006) ? ((tile.frameY >= 1114) ? ((tile.frameY >= 1222) ? 7 : 3) : 0) : 7) : 8) : 6) : 5) : 4) : 3) : 2)) : 0);
// 							break;
// 						case 27:
// 							num4 = ((tile.frameY < 34) ? 1 : 0);
// 							break;
// 						case 31:
// 							num4 = ((tile.frameX >= 36) ? 1 : 0);
// 							break;
// 						case 26:
// 							num4 = ((tile.frameX >= 54) ? 1 : 0);
// 							break;
// 						case 137:
// 							num4 = ((tile.frameY != 0) ? 1 : 0);
// 							break;
// 						case 82:
// 						case 83:
// 						case 84:
// 							num4 = ((tile.frameX >= 18) ? ((tile.frameX < 36) ? 1 : ((tile.frameX >= 54) ? ((tile.frameX >= 72) ? ((tile.frameX >= 90) ? ((tile.frameX >= 108) ? 6 : 5) : 4) : 3) : 2)) : 0);
// 							break;
// 						case 105:
// 							num4 = ((tile.frameX >= 1548 && tile.frameX <= 1654) ? 1 : ((tile.frameX >= 1656 && tile.frameX <= 1798) ? 2 : 0));
// 							break;
// 						case 133:
// 							num4 = ((tile.frameX >= 52) ? 1 : 0);
// 							break;
// 						case 134:
// 							num4 = ((tile.frameX >= 28) ? 1 : 0);
// 							break;
// 						case 149:
// 							num4 = j % 3;
// 							break;
// 						case 160:
// 							num4 = j % 3;
// 							break;
// 						case 165:
// 							num4 = ((tile.frameX >= 54) ? ((tile.frameX < 106) ? 1 : ((tile.frameX >= 216) ? 1 : ((tile.frameX >= 162) ? 3 : 2))) : 0);
// 							break;
// 						case 178:
// 							num4 = ((tile.frameX >= 18) ? ((tile.frameX < 36) ? 1 : ((tile.frameX >= 54) ? ((tile.frameX >= 72) ? ((tile.frameX >= 90) ? ((tile.frameX >= 108) ? 6 : 5) : 4) : 3) : 2)) : 0);
// 							break;
// 						case 184:
// 							num4 = ((tile.frameX >= 22) ? ((tile.frameX < 44) ? 1 : ((tile.frameX >= 66) ? ((tile.frameX >= 88) ? ((tile.frameX >= 110) ? 5 : 4) : 3) : 2)) : 0);
// 							break;
// 						case 185:
// 							if (tile.frameY < 18)
// 							{
// 								int num5 = tile.frameX / 18;
// 								if (num5 < 6 || num5 == 28 || num5 == 29 || num5 == 30 || num5 == 31 || num5 == 32)
// 									num4 = 0;
// 								else if (num5 < 12 || num5 == 33 || num5 == 34 || num5 == 35)
// 									num4 = 1;
// 								else if (num5 < 28)
// 									num4 = 2;
// 								else if (num5 < 48)
// 									num4 = 3;
// 								else if (num5 < 54)
// 									num4 = 4;
// 							}
// 							else
// 							{
// 								int num5 = tile.frameX / 36;
// 								if (num5 < 6 || num5 == 19 || num5 == 20 || num5 == 21 || num5 == 22 || num5 == 23 || num5 == 24 || num5 == 33 || num5 == 38 || num5 == 39 || num5 == 40)
// 									num4 = 0;
// 								else if (num5 < 16)
// 									num4 = 2;
// 								else if (num5 < 19 || num5 == 31 || num5 == 32)
// 									num4 = 1;
// 								else if (num5 < 31)
// 									num4 = 3;
// 								else if (num5 < 38)
// 									num4 = 4;
// 							}
// 							break;
// 						case 186:
// 							{
// 								int num5 = tile.frameX / 54;
// 								if (num5 < 7)
// 									num4 = 2;
// 								else if (num5 < 22 || num5 == 33 || num5 == 34 || num5 == 35)
// 									num4 = 0;
// 								else if (num5 < 25)
// 									num4 = 1;
// 								else if (num5 == 25)
// 									num4 = 5;
// 								else if (num5 < 32)
// 									num4 = 3;
//
// 								break;
// 							}
// 						case 187:
// 							{
// 								int num5 = tile.frameX / 54;
// 								if (num5 < 3 || num5 == 14 || num5 == 15 || num5 == 16)
// 									num4 = 0;
// 								else if (num5 < 6)
// 									num4 = 6;
// 								else if (num5 < 9)
// 									num4 = 7;
// 								else if (num5 < 14)
// 									num4 = 4;
// 								else if (num5 < 18)
// 									num4 = 4;
// 								else if (num5 < 23)
// 									num4 = 8;
// 								else if (num5 < 25)
// 									num4 = 0;
// 								else if (num5 < 29)
// 									num4 = 1;
//
// 								break;
// 							}
// 						case 227:
// 							num4 = tile.frameX / 34;
// 							break;
// 						case 240:
// 							{
// 								int num5 = tile.frameX / 54;
// 								int num6 = tile.frameY / 54;
// 								num5 += num6 * 36;
// 								if ((num5 >= 0 && num5 <= 11) || (num5 >= 47 && num5 <= 53))
// 								{
// 									num4 = 0;
// 									break;
// 								}
//
// 								if (num5 >= 12 && num5 <= 15)
// 								{
// 									num4 = 1;
// 									break;
// 								}
//
// 								switch (num5)
// 								{
// 									case 16:
// 									case 17:
// 										num4 = 2;
// 										break;
// 									case 18:
// 									case 19:
// 									case 20:
// 									case 21:
// 									case 22:
// 									case 23:
// 									case 24:
// 									case 25:
// 									case 26:
// 									case 27:
// 									case 28:
// 									case 29:
// 									case 30:
// 									case 31:
// 									case 32:
// 									case 33:
// 									case 34:
// 									case 35:
// 										num4 = 1;
// 										break;
// 									default:
// 										if (num5 >= 41 && num5 <= 45)
// 											num4 = 3;
// 										else if (num5 == 46)
// 											num4 = 4;
// 										break;
// 								}
//
// 								break;
// 							}
// 						case 242:
// 							{
// 								int num5 = tile.frameY / 72;
// 								num4 = ((num5 >= 22 && num5 <= 24) ? 1 : 0);
// 								break;
// 							}
// 						case 440:
// 							{
// 								int num5 = tile.frameX / 54;
// 								if (num5 > 6)
// 									num5 = 6;
//
// 								num4 = num5;
// 								break;
// 							}
// 						case 457:
// 							{
// 								int num5 = tile.frameX / 36;
// 								if (num5 > 4)
// 									num5 = 4;
//
// 								num4 = num5;
// 								break;
// 							}
// 						case 453:
// 							{
// 								int num5 = tile.frameX / 36;
// 								if (num5 > 2)
// 									num5 = 2;
//
// 								num4 = num5;
// 								break;
// 							}
// 						case 419:
// 							{
// 								int num5 = tile.frameX / 18;
// 								if (num5 > 2)
// 									num5 = 2;
//
// 								num4 = num5;
// 								break;
// 							}
// 						case 428:
// 							{
// 								int num5 = tile.frameY / 18;
// 								if (num5 > 3)
// 									num5 = 3;
//
// 								num4 = num5;
// 								break;
// 							}
// 						case 420:
// 							{
// 								int num5 = tile.frameY / 18;
// 								if (num5 > 5)
// 									num5 = 5;
//
// 								num4 = num5;
// 								break;
// 							}
// 						case 423:
// 							{
// 								int num5 = tile.frameY / 18;
// 								if (num5 > 6)
// 									num5 = 6;
//
// 								num4 = num5;
// 								break;
// 							}
// 						default:
// 							num4 = 0;
// 							break;
// 					}
// 				}
// 			}
//
// 			if (num3 == 0)
// 			{
// 				/*if (tile.liquid > 32)
// 				{
// 					int num7 = tile.liquidType();
// 					num3 = liquidPosition + num7;
// 				}*/
// 				if (tile.wall > 0)
// 				{
// 					int wall = tile.wall;
// 					num3 = wallLookup[wall];
// 					num = tile.WallColor;
// 					switch (wall)
// 					{
// 						case 21:
// 						case 88:
// 						case 89:
// 						case 90:
// 						case 91:
// 						case 92:
// 						case 93:
// 						case 168:
// 							num = 0;
// 							break;
// 						case 27:
// 							num4 = i % 2;
// 							break;
// 						default:
// 							num4 = 0;
// 							break;
// 					}
// 				}
// 			}
// 			
// 			ushort mapType = (ushort)(num3 + num4);
// 			//MapLoader.ModMapOption(ref mapType, i, j);
// 			return Terraria.Map.MapTile.Create(mapType, (byte)num2, (byte)num);
// 		}
//
// 	}
// }
