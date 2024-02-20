// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using InfWorld.Utils;
// using InfWorld.Utils.Math;
// using InfWorld.World.Region;
// using log4net;
// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Graphics;
// using Terraria;
// using Terraria.GameContent;
// using Terraria.GameInput;
// using Terraria.Localization;
// using Terraria.Map;
// using static Terraria.Main;
//
// namespace InfWorld.Map
// {
//     /// <summary>
//     /// Vanilla map suck, so let's try to make 1 that does not suck (probably will)
//     /// </summary>
//     public sealed class WorldMap
//     {
//         private RenderTarget2D m_texture;
//         private List2D<WorldMapSection> m_mapSection;
//
//         private readonly int renderMapDistance = 2;
//
//         public Delegate DrawPlayerHead;
//
//         public bool HasChanged = true;
//
//         public WorldMap()
//         {
//             m_mapSection = new List2D<WorldMapSection>();
//         }
//
//         /// <summary>
//         /// 
//         /// </summary>
//         /// <param name="x">RegionX origin</param>
//         /// <param name="y">RegionY origin</param>
//         public void PreRender(int x, int y)
//         {
//             if (!HasChanged)
//             {
//                 return;
//             }
//
//             int x1 = x - renderMapDistance;
//             int y1 = y - renderMapDistance;
//             int x2 = x + renderMapDistance;
//             int y2 = y + renderMapDistance;
//
//             m_texture = new RenderTarget2D(Main.graphics.GraphicsDevice, 800,
//                 800);
//
//             //lock (m_mapSection)
//             {
//                 int localX = 0;
//                 int localY = 0;
//                 
//
//                 Main.graphics.GraphicsDevice.SetRenderTarget(m_texture);
//                 Main.graphics.GraphicsDevice.Clear(Color.Transparent);
//                 Main.spriteBatch.Begin();
//                 for (int i = x1; i < x2 + 1; i++, localX++)
//                 {
//
//                     for (int j = y1; j < y2 + 1; j++, localY++)
//                     {
//                         if (m_mapSection[i, j] == null)
//                         {
//                             continue;
//                         }
//
//                         m_mapSection[i, j].MakeRenderTarget();
//
//                         WorldMapSection section = m_mapSection[i, j];
//                         var sectionRender = section.CachedWorldMapSection;
//                         Main.spriteBatch.Draw(sectionRender, new Vector2(localX * Chunk.ChunkWidth, localY * Chunk.ChunkHeight), null, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1f);
//                     }
//
//                     localY = 0;
//                 }
//                 Main.spriteBatch.End();
//                 Main.graphics.GraphicsDevice.SetRenderTarget(null);
//                 
//             }
//         }
//
// 		public bool AddMapSection(Chunk chunk)
//         {
//             Position2I position = chunk.Position.ToPosition2I();
//             if (m_mapSection[position] != null)
//             {
//                 return false;
//             }
//
//             try
//             {
//                 LogManager.GetLogger("Adding new map section").Debug(position.ToString());
//                 HasChanged = true;
//                 m_mapSection[position] = new WorldMapSection(chunk);
//             }
//             catch
//             {
//                 return false;
//             }
//
//             return true;
//         }
//
//         public bool RemoveMapSection()
//         {
//             return true;
//         }
//
//         public void Draw()
//         {
//             if(m_texture == null) return;
//
//             var graphicDevice = Main.graphics.GraphicsDevice;
//
//             const int width = 200;
//             const int height = 200;
//
//             int positionX = graphicDevice.Viewport.Width - 5 - TextureAssets.Mana.Width() - width - 70;
//             int positionY = 5 + TextureAssets.Heart2.Height() + 10;
//             
//             Rectangle mapDrawingArea = new Rectangle(positionX, positionY, width, height);
//             //Main.graphics.GraphicsDevice.ScissorRectangle = mapDrawingArea;
//
//             Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
//             Main.spriteBatch.Draw(m_texture, new Vector2(positionX, 0), null, Color.White * 0.5f, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 1f);
//             Main.spriteBatch.End();
//
//             //Main.graphics.GraphicsDevice.ScissorRectangle = graphicDevice.Viewport.Bounds;
//         }
//     }
// }
