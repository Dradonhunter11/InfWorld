using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace InfWorld.CoreLoader
{
    internal static partial class CoreLoaderHandler
    {
        private static void ModifyWorldIO(TypeDefinition type, AssemblyDefinition terraria)
        {
            type.IsPublic = true;

            foreach (var methodDefinition in type.Methods)
            {
                methodDefinition.IsPublic = true;
            }

        }
    }
}
