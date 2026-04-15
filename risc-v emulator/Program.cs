
 class Memory()
{
    private byte[] byte_array_ = new byte[65536];

    public uint Read8(int address)
    {
        return (uint)byte_array_[address];
    }

    public uint Read16(int address)
    {
        return (uint)byte_array_[address] |
               (uint)byte_array_[address + 1] << 8;
    }

    public uint Read32(int address)
    {
        return (uint)byte_array_[address] |
                (uint)byte_array_[address + 1] << 8 |
                (uint)byte_array_[address + 2] << 16 |
                (uint)byte_array_[address + 3] << 24;

    }

    public void Write8(int address, uint value)
    {
        byte_array_[address] = (byte)value;
        return;
    }

    public void Write16(int address, uint value)
    {
        byte_array_[address] = (byte)value;
        byte_array_[address + 1] = (byte)(value >> 8);
        return;
    }

    public void Write32(int address, uint value)
    {
        byte_array_[address] = (byte)value;
        byte_array_[address + 1] = (byte)(value >> 8);
        byte_array_[address + 2] = (byte)(value >> 16);
        byte_array_[address + 3] = (byte)(value >> 24);
        return;
    }

} 

class Registers()
{
    private uint[] register_array_ = new uint[32];

    public uint Read(int index)
    {
        if (index == 0)
        {
            return 0;
        }
        else
        {
            return register_array_[index];
        }
    }

    public void Write(int index, uint value)
    {
        if (index == 0)
        {
            return;
        }

        else
        {
            register_array_[index] = value;
        }
    }

}

class CPU
{
    private Memory mem;
    private Registers regs;
    private uint pc;

    public CPU()
    {
        mem = new Memory();
        regs = new Registers();
        pc = 0;
    }

    public void Step()
    {
        uint instruction = mem.Read32((int)pc);


        uint opcode = instruction & 0x7F;
        uint rd = (instruction >> 7) & 0x1F;
        uint funct3 = (instruction >> 12) & 0x7;
        uint rs1 = (instruction >> 15) & 0x1F;
        uint rs2 = (instruction >> 20) & 0x1F;
        uint funct7 = (instruction >> 25) & 0x7F;

        pc += 4;

        switch (opcode)
        {
            case 0x33: //R-Type operations
                uint val1 = regs.Read((int)rs1);
                uint val2 = regs.Read((int)rs2);
                
                if (funct3 == 0x0) //Addition or subtraction
                {
                    if (funct7 == 0x0)
                    {
                        regs.Write((int)rd, val1 + val2);
                    }
                    else if (funct7 == 0x20)
                    {
                        regs.Write((int)rd, val1 - val2);
                    }
                }

                else if (funct3 == 0x1) //Shift left logical
                {
                    int shiftAmount = (int)(val2 & 0x1F);
                    regs.Write((int)rd, val1 << shiftAmount);
                }

                else if (funct3 == 0x2) //Set less than(signed comparison)
                {
                    int s1 = (int)val1;
                    int s2 = (int)val2;
                    regs.Write((int)rd, s1 < s2 ? 1u : 0u);
                }

                else if (funct3 == 0x3) //Set less than (unsigned comparison)
                {
                    regs.Write((int)rd, val1 < val2 ? 1u : 0u);
                }

                else if (funct3 == 0x4) //XOR
                {
                    regs.Write((int)rd, val1 ^ val2);
                }

                else if (funct3 == 0x5) //Shift right Logical/Arithmetic
                {
                    int shiftAmount = (int)(val2 & 0x1F);

                    if (funct7 == 0x00)
                    {
                        regs.Write((int)rd, val1 >> shiftAmount);
                    }

                    else if (funct7 == 0x20)
                    {
                        int signedVal1 = (int)val1;
                        regs.Write((int)rd, (uint)(signedVal1 >> shiftAmount));
                    }
                }

                else if (funct3 == 0x6) //OR
                {
                    regs.Write((int)rd, val1 | val2);
                }

                else if (funct3 == 0x7) //AND
                {
                    regs.Write((int)rd, val1 & val2);
                }
                break;

            case 0x13: //I-Type operations
                uint i_val1 = regs.Read((int)rs1);
                int imm = ((int)instruction) >> 20;

                if (funct3 == 0x0) //ADDI
                {
                    regs.Write((int)rd, (uint)(i_val1 + imm));
                }

                else if (funct3 == 0x1) // SLLI
                {
                    int shamt = (int)rs2;
                    regs.Write((int)rd, i_val1 << shamt);
                }

                else if (funct3 == 0x2) // SLTI
                {
                    regs.Write((int)rd, (int)i_val1 < imm ? 1u : 0u);
                }

                else if (funct3 == 0x3) // SLTIU
                {
                    regs.Write((int)rd, (uint)i_val1 < imm ? 1u : 0u);
                }

                else if (funct3 == 0x4) // XORI
                {
                    regs.Write((int)rd, i_val1 ^ (uint)imm);
                }

                else if (funct3 == 0x5) // SRLI / SRAI
                {
                    int shamt = (int)rs2;

                    if (funct7 == 0x00)
                    {
                        regs.Write((int)rd, i_val1 >> shamt);
                    }

                    else if (funct7 == 0x20)
                    {
                        regs.Write((int)rd, (uint)((int)i_val1 >> shamt));
                    }
                }

                else if (funct3 == 0x6) //ORI
                {
                    regs.Write((int)rd, i_val1 | (uint)imm);
                }

                else if (funct3 == 0x7) //ANDI
                {
                    regs.Write((int)rd, i_val1 & (uint)imm);
                }

                break;

            case 0x03: //I-Type Loads

                int load_imm = ((int)instruction) >> 20;
                uint load_addr = regs.Read((int)rs1) + (uint)load_imm;

                if (funct3 == 0x0) // LB
                {
                    sbyte val8 = (sbyte)mem.Read8((int)load_addr);
                    regs.Write((int)rd, (uint)((int)val8));
                }

                else if (funct3 == 0x1) // LH
                {
                    short val16 = (short)mem.Read16((int)load_addr);
                    regs.Write((int)rd, (uint)((int)val16));
                }

                else if (funct3 == 0x2) // LW
                {
                    uint val32 = mem.Read32((int)load_addr);
                    regs.Write((int)rd, val32);
                }

                else if (funct3 == 0x4) // LBU
                {
                    regs.Write((int)rd, mem.Read8((int)load_addr));
                }

                else if (funct3 == 0x5) // LHU
                {
                    regs.Write((int)rd, mem.Read16((int)load_addr));
                }
                break;

            case 0x23: // S-Type Stores

                int imm_s = ((int)(instruction & 0xFE000000) >> 20 | (int)(instruction >> 7) & 0x1F);
                uint store_addr = regs.Read((int)rs1) + (uint)imm_s;
                uint val_to_store = regs.Read((int)rs2);

                if (funct3 == 0x0) // SB
                {
                    mem.Write8((int)store_addr, val_to_store);
                }

                else if (funct3 == 0x1) // SH
                {
                    mem.Write16((int)store_addr, val_to_store);
                }

                else if (funct3 == 0x2) // SW
                {
                    mem.Write32((int)store_addr, val_to_store);
                }

                break;

            case 0x63:

                int imm_b = ((int)(instruction & 0x80000000) >> 19) |
                            ((int)(instruction & 0x00000080) << 4) |
                            ((int)(instruction & 0x7E000000) >> 20) |
                            ((int)(instruction & 0x00000F00) >> 7);

                uint b_val1 = regs.Read((int)rs1);
                uint b_val2 = regs.Read((int)rs2);
                bool take_branch = false;

                if (funct3 == 0x0) // BEQ
                {
                    take_branch = (b_val1 == b_val2);
                }

                else if (funct3 == 0x1) // BNE
                {
                    take_branch = (b_val1 != b_val2);
                }

                else if (funct3 == 0x4) // BLT
                {
                    take_branch = ((int)b_val1 < b_val2);
                }

                else if (funct3 == 0x5) // BGE
                {
                    take_branch = ((int)b_val1 == (int)b_val2);
                }

                else if (funct3 == 0x6) // BLTU
                {
                    take_branch = (b_val1 < b_val2);
                }

                else if (funct3 == 0x7) // BGEU
                {
                    take_branch = (b_val1 >= b_val2);
                }

                if (take_branch)
                {
                    pc = (uint)((int)pc - 4 + imm_b);
                }

                break;


            default:
                Console.WriteLine("Unkown Opcode: " + opcode.ToString("X"));
                break;
        }

    }
}