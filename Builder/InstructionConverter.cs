using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSOpCode = System.Reflection.Emit.OpCode;
using CSOpCodes = System.Reflection.Emit.OpCodes;
using MonoCecilOpCode = Mono.Cecil.Cil.OpCodes;

namespace InfWorld.Builder
{
    class InstructionConverter
    {
        public Dictionary<string, CSOpCode> OpCode = new Dictionary<string, CSOpCode>()
        {
            [MonoCecilOpCode.Add.Name] = CSOpCodes.Add,
            [MonoCecilOpCode.Add_Ovf.Name] = CSOpCodes.Add_Ovf,
            [MonoCecilOpCode.Add_Ovf_Un.Name] = CSOpCodes.Add_Ovf_Un,
            [MonoCecilOpCode.And.Name] = CSOpCodes.And,
            [MonoCecilOpCode.Arglist.Name] = CSOpCodes.Arglist,
            [MonoCecilOpCode.Beq.Name] = CSOpCodes.Beq,
            [MonoCecilOpCode.Beq_S.Name] = CSOpCodes.Beq_S,
            [MonoCecilOpCode.Bge.Name] = CSOpCodes.Bge,
            [MonoCecilOpCode.Bge_S.Name] = CSOpCodes.Bge_S,
            [MonoCecilOpCode.Bge_Un.Name] = CSOpCodes.Bge_Un,
            [MonoCecilOpCode.Bge_Un_S.Name] = CSOpCodes.Bge_Un_S,
            [MonoCecilOpCode.Bgt.Name] = CSOpCodes.Bgt,
            [MonoCecilOpCode.Bgt_S.Name] = CSOpCodes.Bgt_S,
            [MonoCecilOpCode.Bgt_Un.Name] = CSOpCodes.Bgt_Un,
            [MonoCecilOpCode.Bgt_Un_S.Name] = CSOpCodes.Bgt_Un_S,
            [MonoCecilOpCode.Ble.Name] = CSOpCodes.Ble,
            [MonoCecilOpCode.Ble_S.Name] = CSOpCodes.Ble_S,
            [MonoCecilOpCode.Ble_Un.Name] = CSOpCodes.Ble_Un,
            [MonoCecilOpCode.Ble_Un_S.Name] = CSOpCodes.Ble_Un_S,
            [MonoCecilOpCode.Blt.Name] = CSOpCodes.Blt,
            [MonoCecilOpCode.Blt_S.Name] = CSOpCodes.Blt_S,
            [MonoCecilOpCode.Blt_Un.Name] = CSOpCodes.Blt_Un,
            [MonoCecilOpCode.Blt_Un_S.Name] = CSOpCodes.Blt_Un_S,
            [MonoCecilOpCode.Bne_Un.Name] = CSOpCodes.Bne_Un,
            [MonoCecilOpCode.Bne_Un_S.Name] = CSOpCodes.Bne_Un_S,
            [MonoCecilOpCode.Box.Name] = CSOpCodes.Box,
            [MonoCecilOpCode.Br.Name] = CSOpCodes.Br,
            [MonoCecilOpCode.Br_S.Name] = CSOpCodes.Br_S,
            [MonoCecilOpCode.Break.Name] = CSOpCodes.Break,
            [MonoCecilOpCode.Brfalse.Name] = CSOpCodes.Brfalse,
            [MonoCecilOpCode.Brfalse_S.Name] = CSOpCodes.Brfalse_S,
            [MonoCecilOpCode.Brtrue.Name] = CSOpCodes.Brtrue,
            [MonoCecilOpCode.Brtrue_S.Name] = CSOpCodes.Brtrue_S,
        };
    }
}
