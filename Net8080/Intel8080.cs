//
// Intel 8080 Microprocessor
//
// 
// Based on code from https://github.com/begoon/i8080-js
//

using System;

//
// TODO: Properly implement HLT
// TODO: Improved comments
//
namespace Net8080
{
    /// <summary>
    /// Intel 8080 (KR580VM80A) microprocessor
    /// </summary>
    public class Intel8080
    {
        // CS-7DF1F90

        const int _carry  = 0x01; // 0
        const int _undef1 = 0x02; // - (1)
        const int _parity = 0x04; // 2
        const int _undef3 = 0x08; // - (0)
        const int _acarry = 0x10; // 4
        const int _intrpt = 0x20; // - (0)
        const int _zero   = 0x40; // 6
        const int _sign   = 0x80; // 7

        const int _reg16_BC = 0;
        const int _reg16_DE = 2;
        const int _reg16_HL = 4;

        const int _reg8_B = 0;
        const int _reg8_C = 1;
        const int _reg8_D = 2;
        const int _reg8_E = 3;
        const int _reg8_H = 4;
        const int _reg8_L = 5;
        const int _reg8_M = 6;
        const int _reg8_A = 7;
        
        /// <summary>
        /// If <c>true</c>, the processor will accept interrupt requests
        /// </summary>
        public bool CanInterrupt { get; private set; }

        /// <summary>
        /// If <c>true</c>, the next call to <c>StepInstruction()</c> will execute
        /// the value passed to <c>RequestInterrupt</c> as an instruction
        /// </summary>
        public bool PendingInterrupt { get; private set; }

        public int StackPointer   { get; private set; }
        public int ProgramCounter { get; set; }
        
        public bool CarryFlag  { get; private set; }
        public bool ParityFlag { get; private set; }
        public bool HCarryFlag { get; private set; }
        public bool ZeroFlag   { get; private set; }
        public bool SignFlag   { get; private set; }
        
        public Int16 REG16_BC { get { return (Int16)getRegister16(_reg16_BC); } set { setRegister16(_reg16_BC, value); } }
        public Int16 REG16_DE { get { return (Int16)getRegister16(_reg16_DE); } set { setRegister16(_reg16_DE, value); } }
        public Int16 REG16_HL { get { return (Int16)getRegister16(_reg16_HL); } set { setRegister16(_reg16_HL, value); } }

        public sbyte REG_B { get { return (sbyte)getRegister8(_reg8_B); } set { setRegister8(_reg8_B, value); } }
        public sbyte REG_C { get { return (sbyte)getRegister8(_reg8_C); } set { setRegister8(_reg8_C, value); } }
        public sbyte REG_D { get { return (sbyte)getRegister8(_reg8_D); } set { setRegister8(_reg8_D, value); } }
        public sbyte REG_E { get { return (sbyte)getRegister8(_reg8_E); } set { setRegister8(_reg8_E, value); } }
        public sbyte REG_H { get { return (sbyte)getRegister8(_reg8_H); } set { setRegister8(_reg8_H, value); } }
        public sbyte REG_L { get { return (sbyte)getRegister8(_reg8_L); } set { setRegister8(_reg8_L, value); } }
        public sbyte REG_M { get { return (sbyte)getRegister8(_reg8_M); } set { setRegister8(_reg8_M, value); } }
        public sbyte REG_A { get { return (sbyte)getRegister8(_reg8_A); } set { setRegister8(_reg8_A, value); } }
        
        public IMemoryBus Memory { get; set; }
        public IInputOutputBus IOBus { get; set; }

        int acc { get { return getRegister8(_reg8_A); } set { setRegister8(_reg8_A, value); } }

        readonly int[] regs = {
        //  b  c  d  e  h  l  m  a
        //  0  1  2  3  4  5  6  7
            0, 0, 0, 0, 0, 0, 0, 0
        };
        
        Int16 interruptVector;

        public Intel8080(IMemoryBus mem, IInputOutputBus iobus)
        {
            Memory = mem;
            IOBus = iobus;
        }
        
        int memoryRead8(int address16)
        {
            return Memory.read(address16 & 0xFFFF) & 0xFF;
        }

        void memoryWrite8(int address16, int w8)
        {
            Memory.write(address16 & 0xFFFF, w8 & 0xFF);
        }

        int memoryRead16(int address16)
        {
            return memoryRead8(address16) | (memoryRead8(address16 + 1) << 8);
        }

        void memoryWrite16(int address16, int w16)
        {
            memoryWrite8(address16, w16 & 0xFF);
            memoryWrite8(address16 + 1, w16 >> 8);
        }

        int getRegister8(int register)
        {
            return register != 6 ? regs[register] : memoryRead8(getRegister16(_reg16_HL));
        }

        void setRegister8(int register, int w8)
        {
            w8 &= 0xFF;
            if (register != 6)
                regs[register] = w8;
            else
                memoryWrite8(getRegister16(_reg16_HL), w8);
        }

        int getRegister16(int register)
        {
            return register != 6 ? (regs[register] << 8) | regs[register + 1] : StackPointer;
        }

        void setRegister16(int register, int w16)
        {
            if (register != 6)
            {
                setRegister8(register, w16 >> 8);
                setRegister8(register + 1, w16 & 0xFF);
            }
            else
                StackPointer = w16;
        }

        int stackPop()
        {
            var top = memoryRead16(StackPointer);
            StackPointer = (StackPointer + 2) & 0xFFFF;
            return top;
        }

        void stackPush(int value16)
        {
            StackPointer = (StackPointer - 2) & 0xFFFF;
            memoryWrite16(StackPointer, value16);
        }

        public int GetPSW()
        {
            var f = 0;
            if (SignFlag)   f |= _sign; else f &= ~_sign;
            if (ZeroFlag)   f |= _zero; else f &= ~_zero;
            if (HCarryFlag) f |= _acarry; else f &= ~_acarry;
            if (ParityFlag) f |= _parity; else f &= ~_parity;
            if (CarryFlag)  f |= _carry; else f &= ~_carry;
            f |=  _undef1; // undef1 is always 1.
            f &= ~_undef3; // undef3 is always 0.
            f &= ~_intrpt; // undef5 is always 0.
            return f;
        }

        void setPSW(int w8)
        {
            SignFlag   = (w8 & _sign) >= 1;
            ZeroFlag   = (w8 & _zero) >= 1;
            HCarryFlag = (w8 & _acarry) >= 1;
            ParityFlag = (w8 & _parity) >= 1;
            CarryFlag  = (w8 & _carry) >= 1;
        }

        int fetchProgramByte()
        {
            var v = memoryRead8(ProgramCounter);
            ProgramCounter = (ProgramCounter + 1) & 0xFFFF;
            return v;
        }

        int fetchProgramWord()
        {
            return fetchProgramByte() | (fetchProgramByte() << 8);
        }
        
        void add(int immediate8, bool carry)
        {
            var w16 = acc + immediate8 + ((carry) ? 1 : 0);
            var index = ((acc & 0x88) >> 1) | ((immediate8 & 0x88) >> 2) | ((w16 & 0x88) >> 3);
            acc = w16 & 0xFF;
            SignFlag   = (acc & 0x80) != 0;
            ZeroFlag   = (acc == 0);
            HCarryFlag = halfCarryLookup[index & 0x7];
            ParityFlag = paritylookup[acc];
            CarryFlag  = (w16 & 0x0100) != 0;
        }

        void subtract(int immediate8, bool carry)
        {
            var w16 = (acc - immediate8 - ((carry) ? 1 : 0)) & 0xFFFF;
            var index = ((acc & 0x88) >> 1) | ((immediate8 & 0x88) >> 2) | ((w16 & 0x88) >> 3);
            acc = w16 & 0xFF;
            SignFlag   = (acc & 0x80) != 0;
            ZeroFlag   = (acc == 0);
            HCarryFlag = !subHalfCarryLookup[index & 0x7];
            ParityFlag = paritylookup[acc];
            CarryFlag  = (w16 & 0x0100) != 0;
        }

        void compare(int immediate8)
        {
            // Preform a subtraction and ignore the result
            var atemp = acc;
            subtract(immediate8, false);
            acc = atemp;
        }

        void and(int immediate8)
        {
            HCarryFlag = ((acc | immediate8) & 0x08) != 0;
            acc &= immediate8;
            SignFlag   = (acc & 0x80) != 0;
            ZeroFlag   = (acc == 0);
            ParityFlag = paritylookup[acc];
            CarryFlag  = false;
        }

        void xor(int immediate8)
        {
            acc ^= immediate8;
            SignFlag   = (acc & 0x80) != 0;
            ZeroFlag   = (acc == 0);
            HCarryFlag = false;
            ParityFlag = paritylookup[acc];
            CarryFlag  = false;
        }

        void or(int immediate8)
        {
            acc |= immediate8;
            SignFlag   = (acc & 0x80) != 0;
            ZeroFlag   = (acc == 0);
            HCarryFlag = false;
            ParityFlag = paritylookup[acc];
            CarryFlag  = false;
        }

        void call(int address16)
        {
            stackPush(ProgramCounter);
            ProgramCounter = address16;
        }

        void jcond(int cond, int address16)
        {
            var flags = new[] { (ZeroFlag) ? 1 : 0, (CarryFlag) ? 1 : 0, (ParityFlag) ? 1 : 0, (SignFlag) ? 1 : 0 };
            var direction = ((cond & 0x08) != 0) ? 1 : 0;
            ProgramCounter = flags[(cond >> 4) & 0x03] == direction ? address16 : ProgramCounter;
        }

        /// <summary>
        /// If <c>CanInterrupt</c> is <c>true</c>, an interrupt is raised that will
        /// be executed on the next call to <c>StepInstruction()</c>
        /// </summary>
        /// <param name="vector">Instruction to execute</param>
        public bool RequestInterrupt(Int16 vector)
        {
            if (CanInterrupt)
            {
                interruptVector = vector;
                PendingInterrupt = true;
            }
            return PendingInterrupt;
        }

        /// <summary>
        /// Execute the byte at the program counter or service a pending interrupt
        /// </summary>
        public int StepInstruction()
        {
            if (PendingInterrupt)
            {
                PendingInterrupt = false;
                return DoInstruction(interruptVector);
            }
            return DoInstruction(fetchProgramByte());
        }
        
        int DoInstruction(Int32 opcode)
        {
            switch (opcode)
            {
                // NOP
                //
                case 0x00: // NOP
                case 0x08: // NOP (undocumented)
                case 0x10: // NOP (undocumented)
                case 0x18: // NOP (undocumented)
                case 0x20: // NOP (undocumented)
                case 0x28: // NOP (undocumented)
                case 0x30: // NOP (undocumented)
                case 0x38: // NOP (undocumented)
                return 4;

                // LXI [data16]
                //
                case 0x01: // LXI B  [data16]
                case 0x11: // LXI D  [data16]
                case 0x21: // LXI H  [data16]
                case 0x31: // LXI SP [data16]
                {
                    setRegister16(opcode >> 3, fetchProgramWord());
                    return 10;
                }

                // STAX
                //
                case 0x02: // STAX B
                case 0x12: // STAX D        
                {
                    memoryWrite8(getRegister16(opcode >> 3), acc);
                    return 7;
                }

                // INX [reg16]
                //
                case 0x03: // INX BC
                case 0x13: // INX DE
                case 0x23: // INX HL
                case 0x33: // INX SP
                {
                    var register16 = opcode >> 3;
                    setRegister16(register16, (getRegister16(register16) + 1) & 0xFFFF);
                    return 5;
                }

                // INR [reg8]
                // Increment [reg8] by 1
                case 0x04: // INR B
                case 0x0C: // INR C
                case 0x14: // INR D
                case 0x1C: // INR E
                case 0x24: // INR H
                case 0x2C: // INR L
                case 0x34: // INR M
                case 0x3C: // INR A
                {
                    var register = opcode >> 3;
                    var val = getRegister8(register);
                    val = (val + 1) & 0xFF;
                    setRegister8(register, val);
                    SignFlag = (val & 0x80) != 0;
                    ZeroFlag = (val == 0);
                    HCarryFlag = (val & 0x0F) == 0;
                    ParityFlag = paritylookup[val];
                    return opcode != 0x34 ? 5 : 10;
                }

                // DCR [reg8]
                // Decrement [reg8] by 1
                case 0x05: // DCR B
                case 0x0D: // DCR C
                case 0x15: // DCR D
                case 0x1D: // DCR E
                case 0x25: // DCR H
                case 0x2D: // DCR L
                case 0x35: // DCR M
                case 0x3D: // DCR A
                {
                    var register = opcode >> 3;
                    var val = getRegister8(register);
                    val = (val - 1) & 0xFF;
                    setRegister8(register, val);
                    SignFlag = (val & 0x80) != 0;
                    ZeroFlag = (val == 0);
                    HCarryFlag = (val & 0x0F) != 0x0F;
                    ParityFlag = paritylookup[val];
                    return opcode != 0x35 ? 5 : 10;
                }

                // MVI [reg8] [data8]
                // Move immediate value [data8] to register [reg8]
                case 0x06: // MVI B [data8]
                case 0x0E: // MVI C [data8]
                case 0x16: // MVI D [data8]
                case 0x1E: // MVI E [data8]
                case 0x26: // MVI H [data8]
                case 0x2E: // MVI L [data8]
                case 0x36: // MVI M [data8]
                case 0x3E: // MVI A [data8]
                {
                    setRegister8(opcode >> 3, fetchProgramByte());
                    return opcode != 0x36 ? 7 : 10;
                }

                // RLC
                //
                case 0x07: // RLC
                {
                    CarryFlag = ((acc & 0x80) != 0);
                    acc = ((acc << 1) & 0xFF) | ((CarryFlag) ? 1 : 0);
                    return 4;
                }

                // DAD [reg16]
                //
                case 0x09: // DAD BC
                case 0x19: // DAD DE
                case 0x29: // DAD HL
                case 0x39: // DAD SP
                {
                    var hl = getRegister16(_reg16_HL) + getRegister16((opcode & 0x30) >> 3);
                    CarryFlag = (hl & 0x10000) != 0;
                    setRegister8(_reg8_H, hl >> 8);
                    setRegister8(_reg8_L, hl & 0xFF);
                    return 10;
                }

                // LDAX
                //
                case 0x0A: // LDAX BC
                case 0x1A: // LDAX DE
                {
                    acc = memoryRead8(getRegister16((opcode & 0x10) >> 3));
                    return 7;
                }

                // DCX [reg16]
                //
                case 0x0B: // DCX BE
                case 0x1B: // DCX DL
                case 0x2B: // DCX HL
                case 0x3B: // DCX SP
                {
                    var register16 = (opcode & 0x30) >> 3;
                    setRegister16(register16, (getRegister16(register16) - 1) & 0xFFFF);
                    return 5;
                }

                // RRC
                //
                case 0x0F: // RRC
                {
                    CarryFlag = (acc & 0x01) == 1;
                    acc = (acc >> 1) | (((CarryFlag) ? 1 : 0) << 7);
                    return 4;
                }

                // RAL
                //
                case 0x17: // RAL
                {
                    var w8 = (CarryFlag) ? 1 : 0;
                    CarryFlag = ((acc & 0x80) != 0);
                    acc = (acc << 1) | w8;
                    return 4;
                }

                // RAR
                //
                case 0x1F:  // RAR
                {
                    var w8 = (CarryFlag) ? 1 : 0;
                    CarryFlag = (acc & 0x01) == 1;
                    acc = (acc >> 1) | (w8 << 7);
                    return 4;
                }

                // SHLD
                //
                case 0x22: // SHLD [addr16]
                {
                    var address16 = fetchProgramWord();
                    memoryWrite8(address16, getRegister8(_reg8_L));
                    memoryWrite8(address16 + 1, getRegister8(_reg8_H));
                    return 16;
                }

                // DAA
                //
                case 0x27: // DAA
                {
                    var carry = CarryFlag;
                    var add1 = 0;
                    if (HCarryFlag || (acc & 0x0F) > 9) add1 = 0x06;
                    if (CarryFlag || (acc >> 4) > 9 || ((acc >> 4) >= 9 && (acc & 0xF) > 9))
                    {
                        add1 |= 0x60;
                        carry = true;
                    }
                    add(add1, false);
                    ParityFlag = paritylookup[acc];
                    CarryFlag = carry;
                    return 4;
                }

                // LDHL
                //
                case 0x2A: // LDHL [addr16]
                {
                    var address16 = fetchProgramWord();
                    setRegister8(_reg8_L, memoryRead8(address16));
                    setRegister8(_reg8_H, memoryRead8(address16 + 1));
                    return 16;
                }

                // CMA
                //
                case 0x2F: // CMA
                {
                    acc = acc ^ 0xFF;
                    return 4;
                }

                // STA
                //
                case 0x32: // STA [addr8]
                {
                    memoryWrite8(fetchProgramWord(), acc);
                    return 13;
                }

                // STC
                //
                case 0x37: // STC
                {
                    CarryFlag = true;
                    return 4;
                }

                // LDA
                //
                case 0x3A: // LDA [addr8]
                {
                    acc = memoryRead8(fetchProgramWord());
                    return 13;
                }

                // CMC
                //
                case 0x3F: // CMC
                {
                    CarryFlag = !CarryFlag;
                    return 4;
                }

                case 0x40: // MOV B B
                case 0x41: // MOV B C
                case 0x42: // MOV B D
                case 0x43: // MOV B E
                case 0x44: // MOV B H
                case 0x45: // MOV B L
                case 0x46: // MOV B M
                case 0x47: // MOV B A

                case 0x48: // MOV C B
                case 0x49: // MOV C C
                case 0x4A: // MOV C D
                case 0x4B: // MOV C E
                case 0x4C: // MOV C H
                case 0x4D: // MOV C L
                case 0x4E: // MOV C M
                case 0x4F: // MOV C A

                case 0x50: // MOV D B
                case 0x51: // MOV D C
                case 0x52: // MOV D D
                case 0x53: // MOV D E
                case 0x54: // MOV D H
                case 0x55: // MOV D L
                case 0x56: // MOV D M
                case 0x57: // MOV D A

                case 0x58: // MOV E B
                case 0x59: // MOV E C
                case 0x5A: // MOV E D
                case 0x5B: // MOV E E
                case 0x5C: // MOV E H
                case 0x5D: // MOV E L
                case 0x5E: // MOV E M
                case 0x5F: // MOV E A

                case 0x60: // MOV H B
                case 0x61: // MOV H C
                case 0x62: // MOV H D
                case 0x63: // MOV H E
                case 0x64: // MOV H H
                case 0x65: // MOV H L
                case 0x66: // MOV H M
                case 0x67: // MOV H A

                case 0x68: // MOV L B
                case 0x69: // MOV L C
                case 0x6A: // MOV L D
                case 0x6B: // MOV L E
                case 0x6C: // MOV L H
                case 0x6D: // MOV L L
                case 0x6E: // MOV L M
                case 0x6F: // MOV L A

                case 0x70: // MOV M B
                case 0x71: // MOV M C
                case 0x72: // MOV M D
                case 0x73: // MOV M E
                case 0x74: // MOV M H
                case 0x75: // MOV M L
                case 0x77: // MOV M A

                case 0x78: // MOV A B
                case 0x79: // MOV A C
                case 0x7A: // MOV A D
                case 0x7B: // MOV A E
                case 0x7C: // MOV A H
                case 0x7D: // MOV A L
                case 0x7E: // MOV A M
                case 0x7F: // MOV A A
                {
                    var src = opcode & 7;
                    var dst = (opcode >> 3) & 7;
                    setRegister8(dst, getRegister8(src));
                    return (src == 6 || dst == 6 ? 7 : 5);
                }

                // HLT
                //
                case 0x76: // HLT
                {
                    ProgramCounter = (ProgramCounter - 1) & 0xffff;
                    return 4;
                }

                case 0x80: // ADD B
                case 0x81: // ADD C
                case 0x82: // ADD D
                case 0x83: // ADD E
                case 0x84: // ADD H
                case 0x85: // ADD L
                case 0x86: // ADD M
                case 0x87: // ADD A

                case 0x88: // ADC B
                case 0x89: // ADC C
                case 0x8A: // ADC D
                case 0x8B: // ADC E
                case 0x8C: // ADC H
                case 0x8D: // ADC L
                case 0x8E: // ADC M
                case 0x8F: // ADC A
                {
                    var r = opcode & 0x07;
                    add(getRegister8(r), (opcode & 0x08) >= 1 && CarryFlag);
                    return (r != 6 ? 4 : 7);
                }

                case 0x90: // SUB B
                case 0x91: // SUB C
                case 0x92: // SUB D
                case 0x93: // SUB E
                case 0x94: // SUB H
                case 0x95: // SUB L
                case 0x96: // SUB M
                case 0x97: // SUB A

                case 0x98: // SBB B
                case 0x99: // SBB C
                case 0x9A: // SBB D
                case 0x9B: // SBB E
                case 0x9C: // SBB H
                case 0x9D: // SBB L
                case 0x9E: // SBB M
                case 0x9F: // SBB A
                {
                    var r = opcode & 0x07;
                    subtract(getRegister8(r), (opcode & 0x08) >= 1 && CarryFlag);
                    return (r != 6 ? 4 : 7);
                }

                // ANA [reg8]
                //
                case 0xA0: // ANA B
                case 0xA1: // ANA C
                case 0xA2: // ANA D
                case 0xA3: // ANA E
                case 0xA4: // ANA H
                case 0xA5: // ANA L
                case 0xA6: // ANA M
                case 0xA7: // ANA A
                {
                    var r = opcode & 0x07;
                    and(getRegister8(r));
                    return (r != 6 ? 4 : 7);
                }

                // XRA [reg8]
                //
                case 0xA8: // XRA B 
                case 0xA9: // XRA C 
                case 0xAA: // XRA D 
                case 0xAB: // XRA E 
                case 0xAC: // XRA H 
                case 0xAD: // XRA L 
                case 0xAE: // XRA M 
                case 0xAF: // XRA A 
                {
                    var r = opcode & 0x07;
                    xor(getRegister8(r));
                    return (r != 6 ? 4 : 7);
                }

                // ORA [reg8]
                // Logical OR with Acc
                case 0xB0: // ORA B 
                case 0xB1: // ORA C 
                case 0xB2: // ORA D 
                case 0xB3: // ORA E 
                case 0xB4: // ORA H 
                case 0xB5: // ORA L 
                case 0xB6: // ORA M 
                case 0xB7: // ORA A 
                {
                    var r = opcode & 0x07;
                    or(getRegister8(r));
                    return (r != 6 ? 4 : 7);
                }

                // CMP [reg8]
                // Compare [reg8] with Acc
                case 0xB8: // CMP B
                case 0xB9: // CMP C
                case 0xBA: // CMP D 
                case 0xBB: // CMP E 
                case 0xBC: // CMP H 
                case 0xBD: // CMP L 
                case 0xBE: // CMP M 
                case 0xBF: // CMP A 
                {
                    var r = opcode & 0x07;
                    compare(getRegister8(r));
                    return (r != 6 ? 4 : 7);
                }

                // Rcond
                // Return conditionally from a subroutine
                case 0xC0: // RNZ 
                case 0xC8: // RZ 
                case 0xD0: // RNC 
                case 0xD8: // RC 
                case 0xE0: // RPO 
                case 0xE8: // RPE 
                case 0xF0: // RP 
                case 0xF8: // RM 
                {
                    var flags = new[] { (ZeroFlag) ? 1 : 0, (CarryFlag) ? 1 : 0, (ParityFlag) ? 1 : 0, (SignFlag) ? 1 : 0 };
                    var r1 = (opcode >> 4) & 0x03;
                    var direction = ((opcode & 0x08) != 0) ? 1 : 0;
                    if (flags[r1] != direction) return 5;
                    ProgramCounter = stackPop();
                    return 11;
                }

                // POP
                //
                case 0xC1: // POP BC 
                case 0xD1: // POP DE 
                case 0xE1: // POP HL 
                case 0xF1: // POP PSW 
                {
                    var register16 = (opcode & 0x30) >> 3;
                    var top = stackPop();
                    if (register16 != 6)
                    {
                        setRegister16(register16, top);
                    }
                    else
                    {
                        acc = top >> 8;
                        setPSW(top & 0xFF);
                    }
                    return 11;
                }

                // Jcond [addr16]
                // Jump conditionally to [addr16]
                case 0xC2: // JNZ [addr16]
                case 0xCA: // JZ  [addr16]
                case 0xD2: // JNC [addr16] 
                case 0xDA: // JC  [addr16] 
                case 0xE2: // JPO [addr16] 
                case 0xEA: // JPE [addr16] 
                case 0xF2: // JP  [addr16] 
                case 0xFA: // JM  [addr16] 
                {
                    jcond(opcode, fetchProgramWord());
                    return 10;
                }

                // JMP [addr16]
                // Jump unconditionally to [addr16]
                case 0xC3: // JMP [addr16]
                case 0xCB: // JMP [addr16] (undocumented)
                {
                    ProgramCounter = fetchProgramWord();
                    return 10;
                }

                // Ccond [addr16]
                //
                case 0xC4: // CNZ [addr16] 
                case 0xCC: // CZ  [addr16] 
                case 0xD4: // CNC [addr16] 
                case 0xDC: // CC  [addr16] 
                case 0xE4: // CPO [addr16] 
                case 0xEC: // CPE [addr16] 
                case 0xF4: // CP  [addr16] 
                case 0xFC: // EM  [addr16]
                {
                    var flags = new[] { (ZeroFlag) ? 1 : 0, (CarryFlag) ? 1 : 0, (ParityFlag) ? 1 : 0, (SignFlag) ? 1 : 0 };
                    var r1 = (opcode >> 4) & 0x03;
                    var direction = ((opcode & 0x08) != 0) ? 1 : 0;
                    var w16 = fetchProgramWord();
                    if (flags[r1] != direction) return 11;
                    call(w16);
                    return 17;
                }

                // PUSH [BC | DE | HL | PSW]
                //
                case 0xC5: // PUSH BC
                case 0xD5: // PUSH DE
                case 0xE5: // PUSH HL
                case 0xF5: // PUSH PSW 
                {
                    var register16 = (opcode & 0x30) >> 3;
                    var w16 = register16 != 6 ? getRegister16(register16) : (acc << 8) | GetPSW();
                    stackPush(w16);
                    return 11;
                }

                // ADI [data8]
                //
                case 0xC6: // ADI [data8]
                {
                    add(fetchProgramByte(), false);
                    return 7;
                }

                // RST # (0 - 7)
                //
                case 0xC7: // RST 0 (0x00)
                case 0xCF: // RST 1 (0x08)
                case 0xD7: // RST 2 (0x10)
                case 0xDF: // RST 3 (0x18)
                case 0xE7: // RST 4 (0x20)
                case 0xEF: // RST 5 (0x28)
                case 0xF7: // RST 5 (0x30)
                case 0xFF: // RST 7 (0x38)
                {
                    stackPush(ProgramCounter);
                    ProgramCounter = (opcode & 0x38);// * 8;
                    return 11;
                }

                // RET
                //
                case 0xC9: // RET 
                case 0xD9: // RET (undocumented)
                {
                    ProgramCounter = stackPop();
                    return 10;
                }

                // CALL [addr16]
                //
                case 0xCD: // CALL [addr16]
                case 0xDD: // CALL [addr16] (undocumented)
                case 0xED: // CALL [addr16] (undocumented)
                case 0xFD: // CALL [addr16] (undocumented)
                {
                    call(fetchProgramWord());
                    return 17;
                }

                // ACI [data8]
                //
                case 0xCE: // ACI [data8]
                {
                    add(fetchProgramByte(), CarryFlag);
                    return 7;
                }

                // OUT [port8]
                //
                case 0xD3: // OUT [port8]
                {
                    IOBus.output(fetchProgramByte(), acc);
                    return 10;
                }

                // SUI [data8]
                //
                case 0xD6: // SUI [data8]
                {
                    subtract(fetchProgramByte(), false);
                    return 7;
                }

                // IN [port8]
                //
                case 0xDB: // IN [port8]
                {
                    acc = IOBus.input(fetchProgramByte());
                    return 10;
                }

                // SBI [data8]
                //
                case 0xDE: // SBI [data8]
                {
                    subtract(fetchProgramByte(), CarryFlag);
                    return 7;
                }

                // XTHL
                //
                case 0xE3: // XTHL
                {
                    var w16 = memoryRead16(StackPointer);
                    memoryWrite16(StackPointer, getRegister16(_reg16_HL));
                    setRegister8(_reg8_L, w16 & 0xFF);
                    setRegister8(_reg8_H, w16 >> 8);
                    return 18;
                }

                // ANI [data8]
                //
                case 0xE6: // ANI [data8]
                {
                    and(fetchProgramByte());
                    return 7;
                }

                // PCHL
                //
                case 0xE9: // PCHL
                {
                    ProgramCounter = getRegister16(_reg16_HL);
                    return 5;
                }

                // XCHG
                //
                case 0xEB: // XCHG
                {
                    var w8 = getRegister8(_reg8_L);
                    setRegister8(_reg8_L, getRegister8(_reg8_E));
                    setRegister8(_reg8_E, w8);
                    w8 = getRegister8(_reg8_H);
                    setRegister8(_reg8_H, getRegister8(_reg8_D));
                    setRegister8(_reg8_D, w8);
                    return 4;
                }

                // XRI [data8]
                //
                case 0xEE: // XRI [data8]
                {
                    xor(fetchProgramByte());
                    return 7;
                }

                // EI / DI
                //
                case 0xF3: // DI
                case 0xFB: // EI
                {
                    CanInterrupt = (opcode & 0x08) != 0;
                    IOBus.interrupt(CanInterrupt);
                    return 4;
                }

                // ORI [data8]
                //
                case 0xF6: // ORI [data8]
                {
                    or(fetchProgramByte());
                    return 7;
                }

                // SPHL
                //
                case 0xF9: // SPHL
                {
                    StackPointer = getRegister16(_reg16_HL);
                    return 5;
                }

                // CPI [data8]
                //
                case 0xFE: // CPI [data8]
                {
                    compare(fetchProgramByte());
                    return 7;
                }

                default: throw new Exception("Unhandled opcode " + opcode.ToString("X2")); // Impossible?!
            }
        }
        
        readonly bool[] paritylookup = {
            true , false, false, true , false, true , true , false, false, true , true , false, true , false, false, true ,
            false, true , true , false, true , false, false, true , true , false, false, true , false, true , true , false,
            false, true , true , false, true , false, false, true , true , false, false, true , false, true , true , false,
            true , false, false, true , false, true , true , false, false, true , true , false, true , false, false, true ,
            false, true , true , false, true , false, false, true , true , false, false, true , false, true , true , false,
            true , false, false, true , false, true , true , false, false, true , true , false, true , false, false, true ,
            true , false, false, true , false, true , true , false, false, true , true , false, true , false, false, true ,
            false, true , true , false, true , false, false, true , true , false, false, true , false, true , true , false,
            false, true , true , false, true , false, false, true , true , false, false, true , false, true , true , false,
            true , false, false, true , false, true , true , false, false, true , true , false, true , false, false, true ,
            true , false, false, true , false, true , true , false, false, true , true , false, true , false, false, true ,
            false, true , true , false, true , false, false, true , true , false, false, true , false, true , true , false,
            true , false, false, true , false, true , true , false, false, true , true , false, true , false, false, true ,
            false, true , true , false, true , false, false, true , true , false, false, true , false, true , true , false,
            false, true , true , false, true , false, false, true , true , false, false, true , false, true , true , false,
            true , false, false, true , false, true , true , false, false, true , true , false, true , false, false, true ,
        };

        readonly bool[] halfCarryLookup    = { false, false, true, false, true, false, true, true };
        readonly bool[] subHalfCarryLookup = { false, true, true, true, false, false, false, true };

    }
}
