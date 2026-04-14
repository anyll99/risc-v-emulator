
 class Memory()
{
    private byte[] byte_array_ = new byte[65536];

    public uint Read32(int address)
    {
        return  (uint)byte_array_[address]           |
                (uint)byte_array_[address + 1] << 8  |
                (uint)byte_array_[address + 2] << 16 |
                (uint)byte_array_[address + 3] << 24;

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
            case 0x33:
                uint val1 = regs.Read((int)rs1);
                uint val2 = regs.Read((int)rs2);
                
                if (funct3 == 0x0)
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

                else if (funct3 == 0x7)
                {
                    regs.Write((int)rd, val1 & val2);
                }

                else if (funct3 == 0x6)
                {
                    regs.Write((int)rd, val1 | val2);
                }
                break;

            case 0x13:
                uint i_val1 = regs.Read((int)rs1);
                int imm = ((int)instruction) >> 20;

                if (funct3 == 0x0)
                {
                    regs.Write((int)rd, (uint)(i_val1 + imm));
                }

                break;

            default:
                Console.WriteLine("Unkown Opcode: " + opcode.ToString("X"));
                break;
        }

    }
}