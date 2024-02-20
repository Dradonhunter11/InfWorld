using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfiniteWorldLibrary.WorldGenerator.BiomeGenerator;

namespace InfWorld.Registeries
{
    class BiomeRegistery
    {
        internal static Dictionary<int, Biome> BiomeList;


        public static void Register(Biome biome)
        {
            BiomeList.Add(BiomeList.Count, biome);
        }

        //public static readonly Biome PURITY_BIOME = Register(BiomeFactory.MakePurity());
        //public static readonly Biome CORRUPT_BIOME = Register(BiomeFactory.MakeCorruption());
        //public static readonly Biome CRIMSON_BIOME = Register(BiomeFactory.MakeCrimson());
        //public static readonly Biome SNOW_BIOME = Register(BiomeFactory.MakeSnow());
        //public static readonly Biome JUNGLE_BIOME = Register(BiomeFactory.MakeIHateThisBiome());
    }
}
