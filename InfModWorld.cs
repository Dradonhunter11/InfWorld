using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InfiniteWorldLibrary;
using InfiniteWorldLibrary.World;
using InfiniteWorldLibrary.World.Region;
using InfiniteWorldLibrary.WorldGenerator.ChunkGenerator;
using InfWorld.CoreLoader;
using InfWorld.Patching.ILPatches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace InfWorld
{
    class InfModWorld : ModSystem
    {
        public bool ThreadRunning = false;

        public override void Load()
        {
            InfWorld.Instance = (InfWorld)Mod;
            CoreLoaderHandler.PatchAssembly();
            Patching.Detours.Detours.Load();
        }

        public override void OnWorldLoad()
        {
            ChunkGeneratorV2.Create();
            base.OnWorldLoad();
        }

        public override void PreUpdateWorld()
        {
            if(Main.netMode != NetmodeID.Server)
                //InfWorld.Map.PreRender((int)(Main.LocalPlayer.position.X / 16f / Chunk.ChunkWidth), (int)(Main.LocalPlayer.position.Y / 16f / Chunk.ChunkHeight));
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (Main.netMode == NetmodeID.SinglePlayer && !ThreadRunning)
            {
                /*Thread thread = new Thread(() =>
                {
                    while (ThreadRunning)
                    {
                        // InfWorld.Tile.Update(Main.LocalPlayer);

                        Thread.Sleep(50);
                        if (Main.graphics.GraphicsDevice.IsDisposed)
                        {
                            ThreadRunning = false;
                        }
                    }
                });
                ThreadRunning = true;
                thread.Start();*/
            }
        }


        public override void UpdateUI(GameTime gameTime)
        {
            if (!Main.CanUpdateGameplay)
            {
                Main.LocalPlayer.position = new Vector2(5000, 3000);
                
                Main.ToggleGameplayUpdates(true);
            }
            base.UpdateUI(gameTime);
        }

        public override void PostDrawTiles()
        {
            //InfWorld.Map.Draw();
        }
    }
    
    
}
