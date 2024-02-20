using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfiniteWorldLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics.Capture;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using static Terraria.Main;

namespace InfWorld.Patching.Detours
{
    static partial class Detours
    {
        private static void OnClampScreenPositionToWorld(Terraria.On_Main.orig_ClampScreenPositionToWorld orig)
        {
            return;
        }

        private static void DrawMap(Terraria.On_Main.orig_DrawMap orig, Main self, GameTime gameTime)
        {
            string text = "";
            if (!mapEnabled || !mapReady)
                return;

            float num = 0f;
            float num2 = 0f;
            float num3 = num;
            float num4 = num2;
            float num5 = 2f;
            byte b = byte.MaxValue;
            _ = maxTilesX / textureMaxWidth;
            int num6 = maxTilesY / textureMaxHeight;
            float num7 = Lighting.OffScreenTiles;
            float num8 = Lighting.OffScreenTiles;
            float num9 = maxTilesX - Lighting.OffScreenTiles - 1;
            float num10 = maxTilesY - Lighting.OffScreenTiles - 42;
            float num11 = 0f;
            float num12 = 0f;
            num7 = 10f;
            num8 = 10f;
            num9 = maxTilesX - 10;
            num10 = maxTilesY - 10;
            for (int i = 0; i < Main.instance.mapTarget.GetLength(0); i++)
            {
                for (int j = 0; j < Main.instance.mapTarget.GetLength(1); j++)
                {
                    if (Main.instance.mapTarget[i, j] != null)
                    {
                        if (Main.instance.mapTarget[i, j].IsContentLost && !mapWasContentLost[i, j])
                        {
                            mapWasContentLost[i, j] = true;
                            refreshMap = true;
                            clearMap = true;
                        }
                        else if (!Main.instance.mapTarget[i, j].IsContentLost && mapWasContentLost[i, j])
                        {
                            mapWasContentLost[i, j] = false;
                        }
                    }
                }
            }

            num = 200f;
            num2 = 300f;
            float num13 = 0f;
            float num14 = 0f;
            float num15 = num9 - 1f;
            float num16 = num10 - 1f;
            num5 = (mapFullscreen ? mapFullscreenScale : ((mapStyle != 1) ? mapOverlayScale : mapMinimapScale));
            bool flag = false;
            float mapScale = MapScale;
            float num17 = 1f / mapScale;
            int num18 = mouseX;
            int num19 = mouseY;
            Matrix uIScaleMatrix = UIScaleMatrix;
            Matrix transformMatrix = uIScaleMatrix;
            Matrix transformMatrix2 = uIScaleMatrix;
            Matrix matrix = Matrix.CreateScale(mapScale);
            CoinLossRevengeSystem.RevengeMarker revengeMarker = null;
            int num20 = -1;
            if (mapStyle != 1)
                transformMatrix = Matrix.Identity;

            if (mapFullscreen)
                transformMatrix = Matrix.Identity;

            if (!mapFullscreen && mapStyle == 1)
            {
                transformMatrix *= matrix;
                transformMatrix2 *= matrix;
            }

            if (!mapFullscreen)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, transformMatrix);
                if (num5 > 1f)
                    flag = true;
            }

            if (mapFullscreen)
            {
                if (mouseLeft && Main.instance.IsActive && !CaptureManager.Instance.UsingMap)
                {
                    if (mouseLeftRelease)
                    {
                        grabMapX = mouseX;
                        grabMapY = mouseY;
                    }
                    else
                    {
                        float num21 = (float)mouseX - grabMapX;
                        float num22 = (float)mouseY - grabMapY;
                        grabMapX = mouseX;
                        grabMapY = mouseY;
                        num21 *= 0.06255f;
                        num22 *= 0.06255f;
                        mapFullscreenPos.X -= num21 * (16f / mapFullscreenScale);
                        mapFullscreenPos.Y -= num22 * (16f / mapFullscreenScale);
                    }
                }

                player[myPlayer].mouseInterface = true;
                float num23 = (float)screenWidth / (float)maxTilesX * 0.599f;
                if (mapFullscreenScale < num23)
                    mapFullscreenScale = num23;

                if (mapFullscreenScale > 31.2f)
                    mapFullscreenScale = 31.18f;

                num5 = mapFullscreenScale;
                b = byte.MaxValue;
                if (mapFullscreenPos.X < num7)
                    mapFullscreenPos.X = num7;

                if (mapFullscreenPos.X > num9)
                    mapFullscreenPos.X = num9;

                if (mapFullscreenPos.Y < num8)
                    mapFullscreenPos.Y = num8;

                if (mapFullscreenPos.Y > num10)
                    mapFullscreenPos.Y = num10;

                float num24 = mapFullscreenPos.X;
                float num25 = mapFullscreenPos.Y;
                if (resetMapFull)
                {
                    resetMapFull = false;
                    num24 = (screenPosition.X + (float)(screenWidth / 2)) / 16f;
                    num25 = (screenPosition.Y + (float)(screenHeight / 2)) / 16f;
                    mapFullscreenPos.X = num24;
                    mapFullscreenPos.Y = num25;
                }

                num24 *= num5;
                num25 *= num5;
                num = 0f - num24 + (float)(screenWidth / 2);
                num2 = 0f - num25 + (float)(screenHeight / 2);
                // Patch note: 'num', 'num2', are used below.
                num += num7 * num5;
                num2 += num8 * num5;
                float num26 = maxTilesX / 840;
                num26 *= mapFullscreenScale;
                float num27 = num;
                float num28 = num2;
                float num29 = TextureAssets.Map.Width();
                float num30 = TextureAssets.Map.Height();
                if (maxTilesX == 8400)
                {
                    num26 *= 0.999f;
                    num27 -= 40.6f * num26;
                    num28 = num2 - 5f * num26;
                    num29 -= 8.045f;
                    num29 *= num26;
                    num30 += 0.12f;
                    num30 *= num26;
                    if ((double)num26 < 1.2)
                        num30 += 1f;
                }
                else if (maxTilesX == 6400)
                {
                    num26 *= 1.09f;
                    num27 -= 38.8f * num26;
                    num28 = num2 - 3.85f * num26;
                    num29 -= 13.6f;
                    num29 *= num26;
                    num30 -= 6.92f;
                    num30 *= num26;
                    if ((double)num26 < 1.2)
                        num30 += 2f;
                }
                else if (maxTilesX == 6300)
                {
                    num26 *= 1.09f;
                    num27 -= 39.8f * num26;
                    num28 = num2 - 4.08f * num26;
                    num29 -= 26.69f;
                    num29 *= num26;
                    num30 -= 6.92f;
                    num30 *= num26;
                    if ((double)num26 < 1.2)
                        num30 += 2f;
                }
                else if (maxTilesX == 4200)
                {
                    num26 *= 0.998f;
                    num27 -= 37.3f * num26;
                    num28 -= 1.7f * num26;
                    num29 -= 16f;
                    num29 *= num26;
                    num30 -= 8.31f;
                    num30 *= num26;
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
                flag = true;
                // DrawMapFullscreenBackground(screenPosition, screenWidth, screenHeight);

                //TML: Map texture drawing replaced by an adaptive drawing below, as mod worlds sometimes aren't regular sizes.
                /*
                Microsoft.Xna.Framework.Rectangle destinationRectangle = new Microsoft.Xna.Framework.Rectangle((int)num27, (int)num28, (int)num29, (int)num30);
                spriteBatch.Draw(TextureAssets.Map.Value, destinationRectangle, Microsoft.Xna.Framework.Color.White);
                */
                {
                    int x = (int)(num + mapFullscreenScale * 10);
                    int y = (int)(num2 + mapFullscreenScale * 10);
                    int width = (int)((maxTilesX - 40) * mapFullscreenScale);
                    int height = (int)((maxTilesY - 40) * mapFullscreenScale);
                    var destinationRectangle = new Rectangle(x, y, width, height);
                    spriteBatch.Draw(TextureAssets.Map.Value, destinationRectangle, new Rectangle(40, 4, 848, 240), Color.White);

                    int edgeWidth = (int)(40 * mapFullscreenScale * 5);
                    int edgeHeight = (int)(4 * mapFullscreenScale * 5);

                    destinationRectangle = new Rectangle(x - edgeWidth, y - edgeHeight, edgeWidth, height + 2 * edgeHeight);
                    spriteBatch.Draw(TextureAssets.Map.Value, destinationRectangle, new Rectangle(0, 0, 40, 248), Color.White);

                    destinationRectangle = new Rectangle(x + width, y - edgeHeight, edgeWidth, height + 2 * edgeHeight);
                    spriteBatch.Draw(TextureAssets.Map.Value, destinationRectangle, new Rectangle(888, 0, 40, 248), Color.White);
                }

                if (mouseLeft && mouseLeftRelease)
                {
                    double totalSeconds = gameTime.TotalGameTime.TotalSeconds;
                    if (totalSeconds - _lastPingMouseDownTime < 0.5 && Vector2.Distance(MouseScreen, _lastPingMousePosition) < 2f)
                        TriggerPing((MouseScreen - new Vector2(num - 10f * num5, num2 - 10f * num5)) / num5);

                    _lastPingMouseDownTime = totalSeconds;
                    _lastPingMousePosition = MouseScreen;
                }

                if (num5 < 1f)
                {
                    spriteBatch.End();
                    spriteBatch.Begin();
                    flag = false;
                }
            }
            else if (mapStyle == 1)
            {
                UpdateMinimapAnchors();
                miniMapWidth = 240;
                miniMapHeight = 240;
                miniMapX = screenWidth - _minimapTopRightAnchorOffsetTowardsLeft;
                miniMapY = _minimapTopRightAnchorOffsetTowardsBottom;
                miniMapX = (int)((float)miniMapX * num17);
                miniMapY = (int)((float)miniMapY * num17);
                mouseX = (int)((float)mouseX * num17);
                mouseY = (int)((float)mouseY * num17);
                _ = (float)miniMapHeight / (float)maxTilesY;
                if ((double)mapMinimapScale < 0.2)
                    mapMinimapScale = 0.2f;

                if (mapMinimapScale > 3f)
                    mapMinimapScale = 3f;

                if ((double)mapMinimapAlpha < 0.01)
                    mapMinimapAlpha = 0.01f;

                if (mapMinimapAlpha > 1f)
                    mapMinimapAlpha = 1f;

                num5 = mapMinimapScale;
                b = (byte)(255f * mapMinimapAlpha);
                num = miniMapX;
                num2 = miniMapY;
                num3 = num;
                num4 = num2;
                float num31 = (screenPosition.X + (float)(PlayerInput.RealScreenWidth / 2)) / 16f;
                float num32 = (screenPosition.Y + (float)(PlayerInput.RealScreenHeight / 2)) / 16f;
                num11 = (0f - (num31 - (float)(int)((screenPosition.X + (float)(PlayerInput.RealScreenWidth / 2)) / 16f))) * num5;
                num12 = (0f - (num32 - (float)(int)((screenPosition.Y + (float)(PlayerInput.RealScreenHeight / 2)) / 16f))) * num5;
                num15 = (float)miniMapWidth / num5;
                num16 = (float)miniMapHeight / num5;
                num13 = (float)(int)num31 - num15 / 2f;
                num14 = (float)(int)num32 - num16 / 2f;
                _ = (float)maxTilesY + num14;
                float num33 = num3 - 6f;
                float num34 = num4 - 6f;
                MinimapFrameManagerInstance.DrawTo(spriteBatch, new Vector2(num33 + 10f, num34 + 10f));
            }
            else if (mapStyle == 2)
            {
                float num35 = (float)screenWidth / (float)maxTilesX;
                if (mapOverlayScale < num35)
                    mapOverlayScale = num35;

                if (mapOverlayScale > 16f * GameViewMatrix.Zoom.X)
                    mapOverlayScale = 16f * GameViewMatrix.Zoom.X;

                if ((double)mapOverlayAlpha < 0.01)
                    mapOverlayAlpha = 0.01f;

                if (mapOverlayAlpha > 1f)
                    mapOverlayAlpha = 1f;

                num5 = mapOverlayScale;
                b = (byte)(255f * mapOverlayAlpha);
                _ = maxTilesX;
                _ = maxTilesY;
                float num36 = (screenPosition.X + (float)(screenWidth / 2)) / 16f;
                float num37 = (screenPosition.Y + (float)(screenHeight / 2)) / 16f;
                num36 *= num5;
                float num38 = num37 * num5;
                num = 0f - num36 + (float)(screenWidth / 2);
                num2 = 0f - num38 + (float)(screenHeight / 2);
                num += num7 * num5;
                num2 += num8 * num5;
            }

            if (mapStyle == 1 && !mapFullscreen)
            {
                if (num13 < num7)
                    num -= (num13 - num7) * num5;

                if (num14 < num8)
                    num2 -= (num14 - num8) * num5;
            }

            num15 = num13 + num15;
            num16 = num14 + num16;
            if (num13 > num7)
                num7 = num13;

            if (num14 > num8)
                num8 = num14;

            if (num15 < num9)
                num9 = num15;

            if (num16 < num10)
                num10 = num16;

            float num39 = (float)textureMaxWidth * num5;
            float num40 = (float)textureMaxHeight * num5;
            float num41 = num;
            float num42 = 0f;

            /*
            for (int k = 0; k <= 4; k++) {
            */
            for (int k = 0; k <= mapTargetX - 1; k++)
            {
                if (!((float)((k + 1) * textureMaxWidth) > num7) || !((float)(k * textureMaxWidth) < num7 + num9))
                    continue;

                for (int l = 0; l <= num6; l++)
                {
                    if ((float)((l + 1) * textureMaxHeight) > num8 && (float)(l * textureMaxHeight) < num8 + num10)
                    {
                        float num43 = num + (float)(int)((float)k * num39);
                        float num44 = num2 + (float)(int)((float)l * num40);
                        float num45 = k * textureMaxWidth;
                        float num46 = l * textureMaxHeight;
                        float num47 = 0f;
                        float num48 = 0f;
                        if (num45 < num7)
                        {
                            num47 = num7 - num45;
                            num43 = num;
                        }
                        else
                        {
                            num43 -= num7 * num5;
                        }

                        if (num46 < num8)
                        {
                            num48 = num8 - num46;
                            num44 = num2;
                        }
                        else
                        {
                            num44 -= num8 * num5;
                        }

                        num43 = num41;
                        float num49 = textureMaxWidth;
                        float num50 = textureMaxHeight;
                        float num51 = (k + 1) * textureMaxWidth;
                        float num52 = (l + 1) * textureMaxHeight;
                        if (num51 >= num9)
                            num49 -= num51 - num9;

                        if (num52 >= num10)
                            num50 -= num52 - num10;

                        num43 += num11;
                        num44 += num12;
                        if (num49 > num47)
                        {
                            Microsoft.Xna.Framework.Rectangle value = new Microsoft.Xna.Framework.Rectangle((int)num47, (int)num48, (int)num49 - (int)num47, (int)num50 - (int)num48);
                            spriteBatch.Draw(Main.instance.mapTarget[k, l], new Vector2(num43, num44), value, new Microsoft.Xna.Framework.Color(b, b, b, b), 0f, default(Vector2), num5, SpriteEffects.None, 0f);
                        }

                        num42 = (float)((int)num49 - (int)num47) * num5;
                    }

                    if (l == num6)
                        num41 += num42;
                }
            }

            if (flag)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, transformMatrix);
            }

            if (!mapFullscreen)
            {
                if (mapStyle == 2)
                {
                    float num53 = (num5 * 0.2f * 2f + 1f) / 3f;
                    if (num53 > 1f)
                        num53 = 1f;

                    num53 *= UIScale;
                    MapIcons.Draw(Vector2.Zero, new Vector2(num - 10f * num5, num2 - 10f * num5), null, num5, num53, ref text);
                    revengeMarker = NPC.RevengeManager.DrawMapIcons(spriteBatch, Vector2.Zero, new Vector2(num - 10f * num5, num2 - 10f * num5), null, num5, num53, ref text);
                    // DrawMiscMapIcons(spriteBatch, Vector2.Zero, new Vector2(num - 10f * num5, num2 - 10f * num5), null, num5, num53, ref text);
                    spriteBatch.End();
                    if (revengeMarker != null)
                    {
                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, uIScaleMatrix);
                        try
                        {
                            revengeMarker.UseMouseOver(spriteBatch, ref text, num53);
                        }
                        catch (Exception e)
                        {
                            TimeLogger.DrawException(e);
                        }

                        spriteBatch.End();
                    }

                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, transformMatrix);
                    try
                    {
                        for (int m = 0; m < 200; m++)
                        {
                            if (npc[m].active && npc[m].townNPC)
                            {
                                int headIndexSafe = TownNPCProfiles.GetHeadIndexSafe(npc[m]);
                                if (headIndexSafe > 0)
                                {
                                    SpriteEffects dir = SpriteEffects.None;
                                    if (npc[m].direction > 0)
                                        dir = SpriteEffects.FlipHorizontally;

                                    float num54 = (npc[m].position.X + (float)(npc[m].width / 2)) / 16f * num5;
                                    float num55 = (npc[m].position.Y + (float)(npc[m].height / 2)) / 16f * num5;
                                    num54 += num;
                                    num55 += num2;
                                    num54 -= 10f * num5;
                                    num55 -= 10f * num5;
                                    // DrawNPCHeadFriendly(npc[m], b, num53, dir, headIndexSafe, num54, num55);
                                }
                            }

                            if (!npc[m].active || npc[m].GetBossHeadTextureIndex() == -1)
                                continue;

                            float bossHeadRotation = npc[m].GetBossHeadRotation();
                            SpriteEffects bossHeadSpriteEffects = npc[m].GetBossHeadSpriteEffects();
                            Vector2 vector = npc[m].Center + new Vector2(0f, npc[m].gfxOffY);
                            if (npc[m].type == 134)
                            {
                                Vector2 center = npc[m].Center;
                                int num56 = 1;
                                int num57 = (int)npc[m].ai[0];
                                while (num56 < 15 && npc[num57].active && npc[num57].type >= 134 && npc[num57].type <= 136)
                                {
                                    num56++;
                                    center += npc[num57].Center;
                                    num57 = (int)npc[num57].ai[0];
                                }

                                center /= (float)num56;
                                vector = center;
                            }

                            int bossHeadTextureIndex = npc[m].GetBossHeadTextureIndex();
                            float num58 = vector.X / 16f * num5;
                            float num59 = vector.Y / 16f * num5;
                            num58 += num;
                            num59 += num2;
                            num58 -= 10f * num5;
                            num59 -= 10f * num5;
                            // DrawNPCHeadBoss(npc[m], b, num53, bossHeadRotation, bossHeadSpriteEffects, bossHeadTextureIndex, num58, num59);
                        }
                    }
                    catch (Exception e2)
                    {
                        TimeLogger.DrawException(e2);
                    }

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, transformMatrix);
                    for (int n = 0; n < 255; n++)
                    {
                        if (player[n].active && !player[n].dead && n != myPlayer && ((!player[myPlayer].hostile && !player[n].hostile) || (player[myPlayer].team == player[n].team && player[n].team != 0) || n == myPlayer))
                        {
                            float num60 = (player[n].position.X + (float)(player[n].width / 2)) / 16f * num5;
                            float num61 = player[n].position.Y / 16f * num5;
                            num60 += num;
                            num61 += num2;
                            num60 -= 6f;
                            num61 -= 2f;
                            num61 -= 2f - num5 / 5f * 2f;
                            num60 -= 10f * num5;
                            num61 -= 10f * num5;
                            Microsoft.Xna.Framework.Color playerHeadBordersColor = GetPlayerHeadBordersColor(player[n]);
                            MapPlayerRenderer.DrawPlayerHead(Camera, player[n], new Vector2(num60, num61), (float)(int)b / 255f, num53, playerHeadBordersColor);
                        }
                    }

                    spriteBatch.End();
                    spriteBatch.Begin();
                }

                if (mapStyle == 1)
                {
                    float num62 = (num5 * 0.25f * 2f + 1f) / 3f;
                    if (num62 > 1f)
                        num62 = 1f;

                    MapIcons.Draw(new Vector2(num13, num14), new Vector2(num3 + num11, num4 + num12), new Microsoft.Xna.Framework.Rectangle(miniMapX, miniMapY, miniMapWidth, miniMapHeight), num5, num62, ref text);
                    revengeMarker = NPC.RevengeManager.DrawMapIcons(spriteBatch, new Vector2(num13, num14), new Vector2(num3 + num11, num4 + num12), new Microsoft.Xna.Framework.Rectangle(miniMapX, miniMapY, miniMapWidth, miniMapHeight), num5, num62, ref text);
                    // DrawMiscMapIcons(spriteBatch, new Vector2(num13, num14), new Vector2(num3 + num11, num4 + num12), new Microsoft.Xna.Framework.Rectangle(miniMapX, miniMapY, miniMapWidth, miniMapHeight), num5, num62, ref text);
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, transformMatrix2);
                    for (int num63 = 0; num63 < 200; num63++)
                    {
                        if (npc[num63].active && npc[num63].townNPC)
                        {
                            int headIndexSafe2 = TownNPCProfiles.GetHeadIndexSafe(npc[num63]);
                            if (headIndexSafe2 > 0)
                            {
                                SpriteEffects dir2 = SpriteEffects.None;
                                if (npc[num63].direction > 0)
                                    dir2 = SpriteEffects.FlipHorizontally;

                                float num64 = ((npc[num63].position.X + (float)npc[num63].width / 2f) / 16f - num13) * num5;
                                float num65 = ((npc[num63].position.Y + npc[num63].gfxOffY + (float)npc[num63].height / 2f) / 16f - num14) * num5;
                                num64 += num3;
                                num65 += num4;
                                num65 -= 2f * num5 / 5f;
                                num64 += num11;
                                num65 += num12;
                                if (num64 > (float)(miniMapX + 12) && num64 < (float)(miniMapX + miniMapWidth - 16) && num65 > (float)(miniMapY + 10) && num65 < (float)(miniMapY + miniMapHeight - 14))
                                {
                                    float num66 = num64 - (float)(TextureAssets.NpcHead[headIndexSafe2].Width() / 2) * num62;
                                    float num67 = num65 - (float)(TextureAssets.NpcHead[headIndexSafe2].Height() / 2) * num62;
                                    float num68 = num66 + (float)TextureAssets.NpcHead[headIndexSafe2].Width() * num62;
                                    float num69 = num67 + (float)TextureAssets.NpcHead[headIndexSafe2].Height() * num62;
                                    if ((float)mouseX >= num66 && (float)mouseX <= num68 && (float)mouseY >= num67 && (float)mouseY <= num69)
                                        text = npc[num63].FullName;

                                    // DrawNPCHeadFriendly(npc[num63], b, num62, dir2, headIndexSafe2, num64, num65);
                                }
                            }
                        }

                        if (!npc[num63].active || npc[num63].GetBossHeadTextureIndex() == -1)
                            continue;

                        float bossHeadRotation2 = npc[num63].GetBossHeadRotation();
                        SpriteEffects bossHeadSpriteEffects2 = npc[num63].GetBossHeadSpriteEffects();
                        Vector2 vector2 = npc[num63].Center + new Vector2(0f, npc[num63].gfxOffY);
                        if (npc[num63].type == 134)
                        {
                            Vector2 center2 = npc[num63].Center;
                            int num70 = 1;
                            int num71 = (int)npc[num63].ai[0];
                            while (num70 < 15 && npc[num71].active && npc[num71].type >= 134 && npc[num71].type <= 136)
                            {
                                num70++;
                                center2 += npc[num71].Center;
                                num71 = (int)npc[num71].ai[0];
                            }

                            center2 /= (float)num70;
                            vector2 = center2;
                        }

                        int bossHeadTextureIndex2 = npc[num63].GetBossHeadTextureIndex();
                        float num72 = (vector2.X / 16f - num13) * num5;
                        float num73 = (vector2.Y / 16f - num14) * num5;
                        num72 += num3;
                        num73 += num4;
                        num73 -= 2f * num5 / 5f;
                        num72 += num11;
                        num73 += num12;
                        if (num72 > (float)(miniMapX + 12) && num72 < (float)(miniMapX + miniMapWidth - 16) && num73 > (float)(miniMapY + 10) && num73 < (float)(miniMapY + miniMapHeight - 14))
                        {
                            float num74 = num72 - (float)(TextureAssets.NpcHeadBoss[bossHeadTextureIndex2].Width() / 2) * num62;
                            float num75 = num73 - (float)(TextureAssets.NpcHeadBoss[bossHeadTextureIndex2].Height() / 2) * num62;
                            float num76 = num74 + (float)TextureAssets.NpcHeadBoss[bossHeadTextureIndex2].Width() * num62;
                            float num77 = num75 + (float)TextureAssets.NpcHeadBoss[bossHeadTextureIndex2].Height() * num62;
                            if ((float)mouseX >= num74 && (float)mouseX <= num76 && (float)mouseY >= num75 && (float)mouseY <= num77)
                                text = npc[num63].GivenOrTypeName;

                            // DrawNPCHeadBoss(npc[num63], b, num62, bossHeadRotation2, bossHeadSpriteEffects2, bossHeadTextureIndex2, num72, num73);
                        }
                    }

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, transformMatrix2);
                    for (int num78 = 0; num78 < 255; num78++)
                    {
                        if (!player[num78].active || ((player[myPlayer].hostile || player[num78].hostile) && (player[myPlayer].team != player[num78].team || player[num78].team == 0) && num78 != myPlayer))
                            continue;

                        float num79 = ((player[num78].position.X + (float)(player[num78].width / 2)) / 16f - num13) * num5;
                        float num80 = ((player[num78].position.Y + player[num78].gfxOffY + (float)(player[num78].height / 2)) / 16f - num14) * num5;
                        num79 += num3;
                        num80 += num4;
                        num79 -= 6f;
                        num80 -= 6f;
                        num80 -= 2f - num5 / 5f * 2f;
                        num79 += num11;
                        num80 += num12;
                        if (!player[num78].dead && num79 > (float)(miniMapX + 6) && num79 < (float)(miniMapX + miniMapWidth - 16) && num80 > (float)(miniMapY + 6) && num80 < (float)(miniMapY + miniMapHeight - 14))
                        {
                            Microsoft.Xna.Framework.Color playerHeadBordersColor2 = GetPlayerHeadBordersColor(player[num78]);
                            MapPlayerRenderer.DrawPlayerHead(Camera, player[num78], new Vector2(num79, num80), (float)(int)b / 255f, num62, playerHeadBordersColor2);
                            if (num78 != myPlayer)
                            {
                                float num81 = num79 + 4f - 14f * num62;
                                float num82 = num80 + 2f - 14f * num62;
                                float num83 = num81 + 28f * num62;
                                float num84 = num82 + 28f * num62;
                                if ((float)mouseX >= num81 && (float)mouseX <= num83 && (float)mouseY >= num82 && (float)mouseY <= num84)
                                    text = player[num78].name;
                            }
                        }

                        if (!player[num78].showLastDeath)
                            continue;

                        num79 = (player[num78].lastDeathPostion.X / 16f - num13) * num5;
                        num80 = (player[num78].lastDeathPostion.Y / 16f - num14) * num5;
                        num79 += num3;
                        num80 += num4;
                        num80 -= 2f - num5 / 5f * 2f;
                        num79 += num11;
                        num80 += num12;
                        if (num79 > (float)(miniMapX + 8) && num79 < (float)(miniMapX + miniMapWidth - 18) && num80 > (float)(miniMapY + 8) && num80 < (float)(miniMapY + miniMapHeight - 16))
                        {
                            spriteBatch.Draw(TextureAssets.MapDeath.Value, new Vector2(num79, num80), new Microsoft.Xna.Framework.Rectangle(0, 0, TextureAssets.MapDeath.Width(), TextureAssets.MapDeath.Height()), Microsoft.Xna.Framework.Color.White, 0f, new Vector2((float)TextureAssets.MapDeath.Width() * 0.5f, (float)TextureAssets.MapDeath.Height() * 0.5f), num62, SpriteEffects.None, 0f);
                            float num85 = num79 + 4f - 14f * num62;
                            float num86 = num80 + 2f - 14f * num62;
                            num85 -= 4f;
                            num86 -= 4f;
                            float num87 = num85 + 28f * num62;
                            float num88 = num86 + 28f * num62;
                            if ((float)mouseX >= num85 && (float)mouseX <= num87 && (float)mouseY >= num86 && (float)mouseY <= num88)
                                num20 = num78;
                        }
                    }

                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, transformMatrix2);
                    MinimapFrameManagerInstance.DrawForeground(spriteBatch);
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, uIScaleMatrix);
                    if (num20 != -1)
                    {
                        TimeSpan timeSpan = DateTime.Now - player[num20].lastDeathTime;
                        text = Language.GetTextValue("Game.PlayerDeathTime", player[num20].name, Lang.LocalizedDuration(timeSpan, abbreviated: false, showAllAvailableUnits: false));
                    }
                    else
                    {
                        revengeMarker?.UseMouseOver(spriteBatch, ref text);
                    }
                }
            }

            if (mapFullscreen)
            {
                int num89 = (int)((0f - num + (float)mouseX) / num5 + num7);
                int num90 = (int)((0f - num2 + (float)mouseY) / num5 + num8);
                bool flag2 = false;
                if ((float)num89 < num7)
                    flag2 = true;

                if ((float)num89 >= num9)
                    flag2 = true;

                if ((float)num90 < num8)
                    flag2 = true;

                if ((float)num90 >= num10)
                    flag2 = true;

                if (!flag2 && Map[num89, num90].Light > 40)
                {
                    int type = Map[num89, num90].Type;
                    int num91 = MapHelper.tileLookup[21];
                    int num92 = MapHelper.tileLookup[441];
                    int num93 = MapHelper.tileOptionCounts[21];
                    int num94 = MapHelper.tileLookup[467];
                    int num95 = MapHelper.tileLookup[468];
                    int num96 = MapHelper.tileOptionCounts[467];
                    int num97 = MapHelper.tileLookup[88];
                    int num98 = MapHelper.tileOptionCounts[88];
                    LocalizedText[] chestType = Lang.chestType;
                    LocalizedText[] chestType2 = Lang.chestType2;
                    if (type >= num91 && type < num91 + num93)
                    {
                        Tile tile = StaticInstance.WorldInstance[num89, num90];
                        if (tile != null)
                        {
                            int num99 = num89;
                            int num100 = num90;
                            if (tile.TileFrameX % 36 != 0)
                                num99--;

                            if (tile.TileFrameY % 36 != 0)
                                num100--;

                            // text = DrawMap_FindChestName(chestType, tile, num99, num100);
                        }
                    }
                    else if (type >= num94 && type < num94 + num96)
                    {
                        Tile tile2 = StaticInstance.WorldInstance[num89, num90];
                        if (tile2 != null)
                        {
                            int num101 = num89;
                            int num102 = num90;
                            if (tile2.TileFrameX % 36 != 0)
                                num101--;

                            if (tile2.TileFrameY % 36 != 0)
                                num102--;

                            // text = DrawMap_FindChestName(chestType2, tile2, num101, num102);
                        }
                    }
                    else if (type >= num92 && type < num92 + num93)
                    {
                        Tile tile3 = StaticInstance.WorldInstance[num89, num90];
                        if (tile3 != null)
                        {
                            int num103 = num89;
                            int num104 = num90;
                            if (tile3.TileFrameX % 36 != 0)
                                num103--;

                            if (tile3.TileFrameY % 36 != 0)
                                num104--;

                            text = chestType[tile3.TileFrameX / 36].Value;
                        }
                    }
                    else if (type >= num95 && type < num95 + num96)
                    {
                        Tile tile4 = StaticInstance.WorldInstance[num89, num90];
                        if (tile4 != null)
                        {
                            int num105 = num89;
                            int num106 = num90;
                            if (tile4.TileFrameX % 36 != 0)
                                num105--;

                            if (tile4.TileFrameY % 36 != 0)
                                num106--;

                            text = chestType2[tile4.TileFrameX / 36].Value;
                        }
                    }
                    else if (type >= num97 && type < num97 + num98)
                    {
                        // Patch note: 'num89' and 'num90' are used below.
                        Tile tile5 = StaticInstance.WorldInstance[num89, num90];
                        if (tile5 != null)
                        {
                            int num107 = num90;
                            int x = num89 - tile5.TileFrameX % 54 / 18;
                            if (tile5.TileFrameY % 36 != 0)
                                num107--;

                            int num108 = Chest.FindChest(x, num107);
                            text = ((num108 < 0) ? Lang.dresserType[0].Value : ((!(chest[num108].name != "")) ? Lang.dresserType[tile5.TileFrameX / 54].Value : (Lang.dresserType[tile5.TileFrameX / 54].Value + ": " + chest[num108].name)));
                        }
                    }
                    else
                    {
                        /*
                        text = Lang.GetMapObjectName(type);
                        */
                        text = Lang._mapLegendCache.FromTile(Map[num89, num90], num89, num90);
                        // Patch note: 'text' is also used in a patch below.
                    }
                }

                float num109 = (num5 * 0.25f * 2f + 1f) / 3f;
                if (num109 > 1f)
                    num109 = 1f;

                num109 = 1f;
                num109 = UIScale;
                revengeMarker = NPC.RevengeManager.DrawMapIcons(spriteBatch, Vector2.Zero, new Vector2(num - 10f * num5, num2 - 10f * num5), null, num5, num109, ref text);
                // DrawMiscMapIcons(spriteBatch, Vector2.Zero, new Vector2(num - 10f * num5, num2 - 10f * num5), null, num5, num109, ref text);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                for (int num110 = 0; num110 < 200; num110++)
                {
                    if (npc[num110].active && npc[num110].townNPC)
                    {
                        int headIndexSafe3 = TownNPCProfiles.GetHeadIndexSafe(npc[num110]);
                        if (headIndexSafe3 > 0)
                        {
                            SpriteEffects dir3 = SpriteEffects.None;
                            if (npc[num110].direction > 0)
                                dir3 = SpriteEffects.FlipHorizontally;

                            float num111 = (npc[num110].position.X + (float)(npc[num110].width / 2)) / 16f * num5;
                            float num112 = (npc[num110].position.Y + npc[num110].gfxOffY + (float)(npc[num110].height / 2)) / 16f * num5;
                            num111 += num;
                            num112 += num2;
                            num111 -= 10f * num5;
                            num112 -= 10f * num5;
                            float num113 = num111 - (float)(TextureAssets.NpcHead[headIndexSafe3].Width() / 2) * num109;
                            float num114 = num112 - (float)(TextureAssets.NpcHead[headIndexSafe3].Height() / 2) * num109;
                            float num115 = num113 + (float)TextureAssets.NpcHead[headIndexSafe3].Width() * num109;
                            float num116 = num114 + (float)TextureAssets.NpcHead[headIndexSafe3].Height() * num109;
                            if ((float)mouseX >= num113 && (float)mouseX <= num115 && (float)mouseY >= num114 && (float)mouseY <= num116)
                                text = npc[num110].FullName;

                            // DrawNPCHeadFriendly(npc[num110], b, num109, dir3, headIndexSafe3, num111, num112);
                        }
                    }

                    if (!npc[num110].active || npc[num110].GetBossHeadTextureIndex() == -1)
                        continue;

                    float bossHeadRotation3 = npc[num110].GetBossHeadRotation();
                    SpriteEffects bossHeadSpriteEffects3 = npc[num110].GetBossHeadSpriteEffects();
                    Vector2 vector3 = npc[num110].Center + new Vector2(0f, npc[num110].gfxOffY);
                    if (npc[num110].type == 134)
                    {
                        Vector2 center3 = npc[num110].Center;
                        int num117 = 1;
                        int num118 = (int)npc[num110].ai[0];
                        while (num117 < 15 && npc[num118].active && npc[num118].type >= 134 && npc[num118].type <= 136)
                        {
                            num117++;
                            center3 += npc[num118].Center;
                            num118 = (int)npc[num118].ai[0];
                        }

                        center3 /= (float)num117;
                        vector3 = center3;
                    }

                    int bossHeadTextureIndex3 = npc[num110].GetBossHeadTextureIndex();
                    float num119 = vector3.X / 16f * num5;
                    float num120 = vector3.Y / 16f * num5;
                    num119 += num;
                    num120 += num2;
                    num119 -= 10f * num5;
                    num120 -= 10f * num5;
                    // DrawNPCHeadBoss(npc[num110], b, num109, bossHeadRotation3, bossHeadSpriteEffects3, bossHeadTextureIndex3, num119, num120);
                    float num121 = num119 - (float)(TextureAssets.NpcHeadBoss[bossHeadTextureIndex3].Width() / 2) * num109;
                    float num122 = num120 - (float)(TextureAssets.NpcHeadBoss[bossHeadTextureIndex3].Height() / 2) * num109;
                    float num123 = num121 + (float)TextureAssets.NpcHeadBoss[bossHeadTextureIndex3].Width() * num109;
                    float num124 = num122 + (float)TextureAssets.NpcHeadBoss[bossHeadTextureIndex3].Height() * num109;
                    if ((float)mouseX >= num121 && (float)mouseX <= num123 && (float)mouseY >= num122 && (float)mouseY <= num124)
                        text = npc[num110].GivenOrTypeName;
                }

                bool flag3 = false;
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                for (int num125 = 0; num125 < 255; num125++)
                {
                    if (player[num125].active && ((!player[myPlayer].hostile && !player[num125].hostile) || (player[myPlayer].team == player[num125].team && player[num125].team != 0) || num125 == myPlayer) && player[num125].showLastDeath/* && DrawPlayerDeathMarker(num, num2, num5, num13, num14, num109, num125)*/)
                        num20 = num125;
                }

                if (num20 != -1)
                {
                    TimeSpan timeSpan2 = DateTime.Now - player[num20].lastDeathTime;
                    text = Language.GetTextValue("Game.PlayerDeathTime", player[num20].name, Lang.LocalizedDuration(timeSpan2, abbreviated: false, showAllAvailableUnits: false));
                }
                else if (revengeMarker != null)
                {
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, uIScaleMatrix);
                    revengeMarker.UseMouseOver(spriteBatch, ref text, num109);
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                }

                for (int num126 = 0; num126 < 255; num126++)
                {
                    if (!player[num126].active || ((player[myPlayer].hostile || player[num126].hostile) && (player[myPlayer].team != player[num126].team || player[num126].team == 0) && num126 != myPlayer))
                        continue;

                    float num127 = ((player[num126].position.X + (float)(player[num126].width / 2)) / 16f - num13) * num5;
                    float num128 = ((player[num126].position.Y + player[num126].gfxOffY + (float)(player[num126].height / 2)) / 16f - num14) * num5;
                    num127 += num;
                    num128 += num2;
                    num127 -= 6f;
                    num128 -= 2f;
                    num128 -= 2f - num5 / 5f * 2f;
                    num127 -= 10f * num5;
                    num128 -= 10f * num5;
                    float num129 = num127 + 4f - 14f * num109;
                    float num130 = num128 + 2f - 14f * num109;
                    float num131 = num129 + 28f * num109;
                    float num132 = num130 + 28f * num109;
                    if (player[num126].dead)
                        continue;

                    Microsoft.Xna.Framework.Color playerHeadBordersColor3 = GetPlayerHeadBordersColor(player[num126]);
                    MapPlayerRenderer.DrawPlayerHead(Camera, player[num126], new Vector2(num127, num128), (float)(int)b / 255f, num109, playerHeadBordersColor3);
                    if (!((float)mouseX >= num129) || !((float)mouseX <= num131) || !((float)mouseY >= num130) || !((float)mouseY <= num132))
                        continue;

                    text = player[num126].name;
                    if (num126 != myPlayer && player[myPlayer].team > 0 && player[myPlayer].team == player[num126].team && netMode == 1 && player[myPlayer].HasUnityPotion() && !flag3 && !cancelWormHole)
                    {
                        flag3 = true;
                        if (!Main.instance.unityMouseOver)
                            // SoundEngine.PlaySound(12);

                        Main.instance.unityMouseOver = true;
                        playerHeadBordersColor3 = OurFavoriteColor;
                        MapPlayerRenderer.DrawPlayerHead(Camera, player[num126], new Vector2(num127, num128), 1f, num109 + 0.5f, playerHeadBordersColor3);
                        text = Language.GetTextValue("Game.TeleportTo", player[num126].name);
                        if (mouseLeft && mouseLeftRelease)
                        {
                            mouseLeftRelease = false;
                            mapFullscreen = false;
                            player[myPlayer].UnityTeleport(player[num126].position);
                            player[myPlayer].TakeUnityPotion();
                        }
                    }
                }

                cancelWormHole = false;
                MapIcons.Draw(Vector2.Zero, new Vector2(num - 10f * num5, num2 - 10f * num5), null, num5, num109, ref text);
                if (!flag3 && Main.instance.unityMouseOver)
                    Main.instance.unityMouseOver = false;

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerStateForCursor, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, UIScaleMatrix);
                PlayerInput.SetZoom_UI();
                int num133 = 10;
                int num134 = screenHeight - 40;
                if (showFrameRate)
                    num134 -= 15;

                int num135 = 0;
                int num136 = 130;
                if (mouseX >= num133 && mouseX <= num133 + 32 && mouseY >= num134 && mouseY <= num134 + 30)
                {
                    num136 = 255;
                    num135 += 4;
                    player[myPlayer].mouseInterface = true;
                    if (mouseLeft && mouseLeftRelease)
                    {
                        // SoundEngine.PlaySound(10);
                        mapFullscreen = false;
                    }
                }

                spriteBatch.Draw(TextureAssets.MapIcon[num135].Value, new Vector2(num133, num134), new Microsoft.Xna.Framework.Rectangle(0, 0, TextureAssets.MapIcon[num135].Width(), TextureAssets.MapIcon[num135].Height()), new Microsoft.Xna.Framework.Color(num136, num136, num136, num136), 0f, default(Vector2), 1f, SpriteEffects.None, 0f);

                SystemLoader.PostDrawFullscreenMap(ref text);

                DrawCursor(DrawThickCursor());
            }

            mouseX = num18;
            mouseY = num19;
            float t = 3f;
            Utils.Swap(ref t, ref inventoryScale);
            if (text != "")
                Main.instance.MouseText(text, 0, 0);

            Utils.Swap(ref t, ref inventoryScale);
            spriteBatch.End();
            try
            {
                if (num13 < num7)
                    num += (num13 - num7) * num5;

                if (num14 < num8)
                    num2 += (num14 - num8) * num5;

                /*if (mapFullscreen && Main.OnPostFullscreenMapDraw != null)
                    Main.OnPostFullscreenMapDraw(new Vector2(num, num2), num5);*/
            }
            catch (Exception e3)
            {
                TimeLogger.DrawException(e3);
            }

            spriteBatch.Begin();
            PlayerInput.SetZoom_Unscaled();
            TimeLogger.DetailedDrawTime(9);
        }

    }
}
