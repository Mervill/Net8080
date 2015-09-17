using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net8080
{
	public enum Opcode
	{
		NOP		= 0x00, /* nop */
		UNDOC_NOP0 = 0x08, /* nop, undocumented */
		UNDOC_NOP1 = 0x10, /* nop, undocumented */
		UNDOC_NOP2 = 0x18, /* nop, undocumented */
		UNDOC_NOP3 = 0x20, /* nop, undocumented */
		UNDOC_NOP4 = 0x28, /* nop, undocumented */
		UNDOC_NOP5 = 0x30, /* nop, undocumented */
		UNDOC_NOP6 = 0x38, /* nop, undocumented */

		LXI_B	= 0x01, /* lxi b,	data16 */
		LXI_D	= 0x11, /* lxi d,	data16 */
		LXI_H	= 0x21, /* lxi h,	data16 */
		LXI_SP	= 0x31, /* lxi sp,	data16 */

		STAX_B	= 0x02, /* stax b */
		STAX_D	= 0x12, /* stax d */

		INX_B	= 0x03, /* inx b	*/
		INX_D	= 0x13, /* inx d	*/
		INX_H	= 0x23, /* inx h	*/
		INX_SP	= 0x33, /* inx sp	*/

		INR_B	= 0x04, /* inr b */
		INR_C	= 0x0C, /* inr c */
		INR_D	= 0x14, /* inr d */
		INR_E	= 0x1C, /* inr e */
		INR_H	= 0x24, /* inr h */
		INR_L	= 0x2C, /* inr l */
		INR_M	= 0x34, /* inr m */
		INR_A	= 0x3C, /* inr a */

		DCR_B	= 0x05, /* dcr b */
		DCR_C	= 0x0D, /* dcr c */
		DCR_D	= 0x15, /* dcr d */
		DCR_E	= 0x1D, /* dcr e */
		DCR_H	= 0x25, /* dcr h */
		DCR_L	= 0x2D, /* dcr l */
		DCR_M	= 0x35, /* dcr m */
		DCR_A	= 0x3D, /* dcr a */

		MVI_B	= 0x06, /* mvi b, data8 */
		MVI_C	= 0x0E, /* mvi c, data8 */
		MVI_D	= 0x16, /* mvi d, data8 */
		MVI_E	= 0x1E, /* mvi e, data8 */
		MVI_H	= 0x26, /* mvi h, data8 */
		MVI_L	= 0x2E, /* mvi l, data8 */
		MVI_M	= 0x36, /* mvi m, data8 */
		MVI_A	= 0x3E, /* mvi a, data8 */

		RLC		= 0x07, /* rlc */

		DAD_B	= 0x09, /* dad b	*/
		DAD_D	= 0x19, /* dad d	*/
		DAD_HL	= 0x29, /* dad hl	*/
		DAD_SP	= 0x39, /* dad sp	*/

		LDAX_B	= 0x0A, /* ldax b	*/
		LDAX_D	= 0x1A, /* ldax d	*/

		DCX_B	= 0x0B, /* dcx b	*/
		DCX_D	= 0x1B, /* dcx d	*/
		DCX_H	= 0x2B, /* dcx h	*/
		DCX_SP	= 0x3B, /* dcx sp	*/

		RRC		= 0x0F, /* rrc			*/
		RAL		= 0x17, /* ral			*/
		RAR		= 0x1F, /* rar			*/
		SHLD	= 0x22, /* shld addr	*/
		DAA		= 0x27, /* daa			*/
		LHLD	= 0x2A, /* ldhl addr	*/
		CMA		= 0x2F, /* cma			*/
		STA		= 0x32, /* sta	addr	*/
		STC		= 0x37, /* stc			*/
		LDA		= 0x3A, /* lda	addr	*/
		CMC		= 0x3F, /* cmc			*/

		MOV_B_B = 0x40, /* mov b, b */
		MOV_B_C = 0x41, /* mov b, c */
		MOV_B_D = 0x42, /* mov b, d */
		MOV_B_E = 0x43, /* mov b, e */
		MOV_B_H = 0x44, /* mov b, h */
		MOV_B_L = 0x45, /* mov b, l */
		MOV_B_M = 0x46, /* mov b, m */
		MOV_B_A = 0x47, /* mov b, a */
		MOV_C_B = 0x48, /* mov c, b */
		MOV_C_C = 0x49, /* mov c, c */
		MOV_C_D = 0x4A, /* mov c, d */
		MOV_C_E = 0x4B, /* mov c, e */
		MOV_C_H = 0x4C, /* mov c, h */
		MOV_C_L = 0x4D, /* mov c, l */
		MOV_C_M = 0x4E, /* mov c, m */
		MOV_C_A = 0x4F, /* mov c, a */
		MOV_D_B = 0x50, /* mov d, b */
		MOV_D_C = 0x51, /* mov d, c */
		MOV_D_D = 0x52, /* mov d, d */
		MOV_D_E = 0x53, /* mov d, e */
		MOV_D_H = 0x54, /* mov d, h */
		MOV_D_L = 0x55, /* mov d, l */
		MOV_D_M = 0x56, /* mov d, m */
		MOV_D_A = 0x57, /* mov d, a */
		MOV_E_B = 0x58, /* mov e, b */
		MOV_E_C = 0x59, /* mov e, c */
		MOV_E_D = 0x5A, /* mov e, d */
		MOV_E_E = 0x5B, /* mov e, e */
		MOV_E_H = 0x5C, /* mov e, h */
		MOV_E_L = 0x5D, /* mov e, l */
		MOV_E_M = 0x5E, /* mov e, m */
		MOV_E_A = 0x5F, /* mov e, a */
		MOV_H_B = 0x60, /* mov h, b */
		MOV_H_C = 0x61, /* mov h, c */
		MOV_H_D = 0x62, /* mov h, d */
		MOV_H_E = 0x63, /* mov h, e */
		MOV_H_H = 0x64, /* mov h, h */
		MOV_H_L = 0x65, /* mov h, l */
		MOV_H_M = 0x66, /* mov h, m */
		MOV_H_A = 0x67, /* mov h, a */
		MOV_L_B = 0x68, /* mov l, b */
		MOV_L_C = 0x69, /* mov l, c */
		MOV_L_D = 0x6A, /* mov l, d */
		MOV_L_E = 0x6B, /* mov l, e */
		MOV_L_H = 0x6C, /* mov l, h */
		MOV_L_L = 0x6D, /* mov l, l */
		MOV_L_M = 0x6E, /* mov l, m */
		MOV_L_A = 0x6F, /* mov l, a */
		MOV_M_B = 0x70, /* mov m, b */
		MOV_M_C = 0x71, /* mov m, c */
		MOV_M_D = 0x72, /* mov m, d */
		MOV_M_E = 0x73, /* mov m, e */
		MOV_M_H = 0x74, /* mov m, h */
		MOV_M_L = 0x75, /* mov m, l */
		MOV_M_A = 0x77, /* mov m, a */
		MOV_A_B = 0x78, /* mov a, b */
		MOV_A_C = 0x79, /* mov a, c */
		MOV_A_D = 0x7A, /* mov a, d */
		MOV_A_E = 0x7B, /* mov a, e */
		MOV_A_H = 0x7C, /* mov a, h */
		MOV_A_L = 0x7D, /* mov a, l */
		MOV_A_M = 0x7E, /* mov a, m */
		MOV_A_A = 0x7F, /* mov a, a */

		HLT		= 0x76, /* hlt */

		ADD_B	= 0x80, /* add b */
		ADD_C	= 0x81, /* add c */
		ADD_D	= 0x82, /* add d */
		ADD_E	= 0x83, /* add e */
		ADD_H	= 0x84, /* add h */
		ADD_L	= 0x85, /* add l */
		ADD_M	= 0x86, /* add m */
		ADD_A	= 0x87, /* add a */

		ADC_B	= 0x88, /* adc b */
		ADC_C	= 0x89, /* adc c */
		ADC_D	= 0x8A, /* adc d */
		ADC_E	= 0x8B, /* adc e */
		ADC_H	= 0x8C, /* adc h */
		ADC_L	= 0x8D, /* adc l */
		ADC_M	= 0x8E, /* adc m */
		ADC_A	= 0x8F, /* adc a */

		SUB_B	= 0x90, /* sub b */
		SUB_C	= 0x91, /* sub c */
		SUB_D	= 0x92, /* sub d */
		SUB_E	= 0x93, /* sub e */
		SUB_H	= 0x94, /* sub h */
		SUB_L	= 0x95, /* sub l */
		SUB_M	= 0x96, /* sub m */
		SUB_A	= 0x97, /* sub a */

		SBB_B	= 0x98, /* sbb b */
		SBB_C	= 0x99, /* sbb c */
		SBB_D	= 0x9A, /* sbb d */
		SBB_E	= 0x9B, /* sbb e */
		SBB_H	= 0x9C, /* sbb h */
		SBB_L	= 0x9D, /* sbb l */
		SBB_M	= 0x9E, /* sbb m */
		SBB_A	= 0x9F, /* sbb a */

		ANA_B	= 0xA0, /* ana b */
		ANA_C	= 0xA1, /* ana c */
		ANA_D	= 0xA2, /* ana d */
		ANA_E	= 0xA3, /* ana e */
		ANA_H	= 0xA4, /* ana h */
		ANA_L	= 0xA5, /* ana l */
		ANA_M	= 0xA6, /* ana m */
		ANA_A	= 0xA7, /* ana a */

		XRA_B	= 0xA8, /* xra b */
		XRA_C	= 0xA9, /* xra c */
		XRA_D	= 0xAA, /* xra d */
		XRA_E	= 0xAB, /* xra e */
		XRA_H	= 0xAC, /* xra h */
		XRA_L	= 0xAD, /* xra l */
		XRA_M	= 0xAE, /* xra m */
		XRA_A	= 0xAF, /* xra a */

		ORA_B	= 0xB0, /* ora b */
		ORA_C	= 0xB1, /* ora c */
		ORA_D	= 0xB2, /* ora d */
		ORA_E	= 0xB3, /* ora e */
		ORA_H	= 0xB4, /* ora h */
		ORA_L	= 0xB5, /* ora l */
		ORA_M	= 0xB6, /* ora m */
		ORA_A	= 0xB7, /* ora a */

		CMP_B	= 0xB8, /* cmp b */
		CMP_C	= 0xB9, /* cmp c */
		CMP_D	= 0xBA, /* cmp d */
		CMP_E	= 0xBB, /* cmp e */
		CMP_H	= 0xBC, /* cmp h */
		CMP_L	= 0xBD, /* cmp l */
		CMP_M	= 0xBE, /* cmp m */
		CMP_A	= 0xBF, /* cmp a */

		RNZ		= 0xC0, /* rnz 	*/
		RZ		= 0xC8, /* rz 	*/
		RNC		= 0xD0, /* rnc 	*/
		RC		= 0xD8, /* rc 	*/
		RPO		= 0xE0, /* rpo 	*/
		RPE		= 0xE8, /* rpe 	*/
		RP		= 0xF0, /* rp 	*/
		RM		= 0xF8, /* rm 	*/

		POP_B	= 0xC1, /* pop b	*/
		POP_D	= 0xD1, /* pop d	*/
		POP_H	= 0xE1, /* pop h	*/
		POP_PSW = 0xF1, /* pop psw	*/

		JNZ		= 0xC2, /* jnz	addr */
		JZ		= 0xCA, /* jz	addr */
		JNC		= 0xD2, /* jnc	addr */
		JC		= 0xDA, /* jc	addr */
		JPO		= 0xE2, /* jpo	addr */
		JPE		= 0xEA, /* jpe	addr */
		JP		= 0xF2, /* jp	addr */
		JM		= 0xFA, /* jm	addr */
		JMP		= 0xC3, /* jmp	addr */
		UNDOC_JMP = 0xCB, /* jmp addr, undocumented */

		CNZ		= 0xC4, /* cnz	addr */
		CZ		= 0xCC, /* cz	addr */
		CNC		= 0xD4, /* cnc	addr */
		CC		= 0xDC, /* cc	addr */
		CPO		= 0xE4, /* cpo	addr */
		CPE		= 0xEC, /* cpe	addr */
		CP		= 0xF4, /* cp	addr */
		CM		= 0xFC, /* cm	addr */

		PUSH_B	= 0xC5, /* push b */
		PUSH_D	= 0xD5, /* push d */
		PUSH_H	= 0xE5, /* push h */
		PUSH_PSW = 0xF5, /* push psw */

		ADI		= 0xC6, /* adi data8 */

		RST_0	= 0xC7, /* rst 0 */
		RST_1	= 0xCF, /* rst 1 */
		RST_2	= 0xD7, /* rst 2 */
		RST_3	= 0xDF, /* rst 3 */
		RST_4	= 0xE7, /* rst 4 */
		RST_5	= 0xEF, /* rst 5 */
		RST_6	= 0xF7, /* rst 6 */
		RST_7	= 0xFF, /* rst 7 */

		RET			= 0xC9, /* ret */
		UNDOC_RET	= 0xD9, /* ret, undocumented */

		CALL		= 0xCD, /* call addr */
		UNDOC_CALL0 = 0xDD, /* call, undocumented */
		UNDOC_CALL1 = 0xED, /* call, undocumented */
		UNDOC_CALL2 = 0xFD, /* call, undocumented */

		ACI		= 0xCE, /* aci	data8	*/
		OUT		= 0xD3, /* out	port8	*/
		SUI		= 0xD6, /* sui	data8	*/
		IN		= 0xDB, /* in	port8	*/
		SBI		= 0xDE, /* sbi	data8	*/
		XTHL	= 0xE3, /* xthl			*/
		ANI		= 0xE6, /* ani	data8	*/
		PCHL	= 0xE9, /* pchl 		*/
		XCHG	= 0xEB, /* xchg 		*/
		XRI		= 0xEE, /* xri	data8	*/
		DI		= 0xF3, /* di 			*/
		EI		= 0xFB, /* ei 			*/
		ORI		= 0xF6, /* ori	data8	*/
		SPHL	= 0xF9, /* sphl 		*/
		CPI		= 0xFE, /* cpi	data8	*/
	}

    public enum OpcodeMnemonic
    {
        NOP,
        LXI,
        STAX,
        INX,
        INR,
        DCR,
        MVI,
        RLC,
        DAD,
        LDAX,
        DCX,
        RRC,
        RAL,
        RAR,
        SHLD,
        DAA,
        LHLD,
        CMA,
        STA,
        STC,
        LDA,
        CMC,
        MOV,
        HLT,
        ADD,
        ADC,
        SUB,
        SBB,
        ANA,
        XRA,
        ORA,
        CMP,
        RNZ,
        RZ,
        RNC,
        RC,
        RPO,
        RPE,
        RP,
        RM,
        POP,
        JNZ,
        JZ,
        JNC,
        JC,
        JPO,
        JPE,
        JP,
        JM,
        JMP,
        CNZ,
        CZ,
        CNC,
        CC,
        CPO,
        CPE,
        CP,
        CM,
        PUSH,
        ADI,
        RST,
        RET,
        CALL,
        ACI,
        OUT,
        SUI,
        IN,
        SBI,
        XTHL,
        ANI,
        PCHL,
        XCHG,
        XRI,
        DI,
        EI,
        ORI,
        SPHL,
        CPI,
    }

	public static class OpcodeExtentions
	{
        public static int DataLength(this OpcodeMnemonic code)
        {
            switch (code)
            {
            default: return 1;
            case OpcodeMnemonic.MVI:
            case OpcodeMnemonic.ADI:
            case OpcodeMnemonic.ACI:
            case OpcodeMnemonic.SUI:
            case OpcodeMnemonic.SBI:
            case OpcodeMnemonic.ANI:
            case OpcodeMnemonic.ORI:
            case OpcodeMnemonic.XRI:
            case OpcodeMnemonic.CPI:
            case OpcodeMnemonic.IN:
            case OpcodeMnemonic.OUT:
                return 2;
            case OpcodeMnemonic.LXI:
            case OpcodeMnemonic.LDA:
            case OpcodeMnemonic.STA:
            case OpcodeMnemonic.LHLD:
            case OpcodeMnemonic.SHLD:
            case OpcodeMnemonic.JNZ:
            case OpcodeMnemonic.JZ:
            case OpcodeMnemonic.JNC:
            case OpcodeMnemonic.JC:
            case OpcodeMnemonic.JPO:
            case OpcodeMnemonic.JPE:
            case OpcodeMnemonic.JP:
            case OpcodeMnemonic.JM:
            case OpcodeMnemonic.JMP:
            case OpcodeMnemonic.CALL:
            case OpcodeMnemonic.CNZ:
            case OpcodeMnemonic.CZ:
            case OpcodeMnemonic.CNC:
            case OpcodeMnemonic.CC:
            case OpcodeMnemonic.CPO:
            case OpcodeMnemonic.CPE:
            case OpcodeMnemonic.CP:
            case OpcodeMnemonic.CM:
                return 3;
            }
        }

        public static int DataLength(this Opcode code)
		{
			switch(code)
			{
			default: return 1;
			case Opcode.MVI_B:
			case Opcode.MVI_C:
			case Opcode.MVI_D:
			case Opcode.MVI_E:
			case Opcode.MVI_H:
			case Opcode.MVI_L:
			case Opcode.MVI_M:
			case Opcode.MVI_A:
			case Opcode.ADI:
			case Opcode.ACI:
			case Opcode.SUI:
			case Opcode.SBI:
			case Opcode.ANI:
			case Opcode.ORI:
			case Opcode.XRI:
			case Opcode.CPI:
			case Opcode.IN:
			case Opcode.OUT:
				return 2;
			case Opcode.LXI_B:
			case Opcode.LXI_D:
			case Opcode.LXI_H:
			case Opcode.LXI_SP:
			case Opcode.LDA:
			case Opcode.STA:
			case Opcode.LHLD:
			case Opcode.SHLD:
			case Opcode.JNZ:	
			case Opcode.JZ:
			case Opcode.JNC:
			case Opcode.JC:
			case Opcode.JPO:
			case Opcode.JPE:	
			case Opcode.JP:
			case Opcode.JM:
			case Opcode.JMP:
			case Opcode.UNDOC_JMP:
			case Opcode.CALL:
			case Opcode.UNDOC_CALL0:
			case Opcode.UNDOC_CALL1:
			case Opcode.UNDOC_CALL2:
			case Opcode.CNZ:
			case Opcode.CZ:
			case Opcode.CNC:
			case Opcode.CC:
			case Opcode.CPO:
			case Opcode.CPE:
			case Opcode.CP:
			case Opcode.CM:
				return 3;
			}
		}

	}

}
