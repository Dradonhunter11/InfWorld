using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfiniteWorldLibrary.World.Region;
using InfiniteWorldLibrary.WorldGenerator.ChunkGenerator;
using Terraria.ModLoader;
using Terraria.Social.Base;

namespace InfWorld.Commands
{
    internal class ForceGenerate : ModCommand
    {
        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (args.Length == 0)
            {
                var player = caller.Player;

                var chunkPosition = player.position.X / 16 / Chunk.ChunkWidth;
                var chunkId = (uint)Math.Floor(chunkPosition);
                ChunkGeneratorV2.ForceGenerate(chunkId);
            }
            else if (args.Length == 1)
            {
                if (uint.TryParse(args[0], out var chunkId))
                {
                    ChunkGeneratorV2.ForceGenerate(chunkId);
                }
            }
        }

        public override string Command => "forcegen";
        public override CommandType Type => CommandType.Chat;
    }
}
