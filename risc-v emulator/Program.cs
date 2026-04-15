using System;

class Memory
{
    private byte[] byte_array_ = new byte[65536];

    public uint Read8(int address)
    {
        return byte_array_[address];
    }

    public uint Read16(int address)
    {
        return (uint)(byte_array_[address] |
               (byte_array_[address + 1] << 8));
    }

    public uint Read32(int address)
    {
        if (address > byte_array_.Length - 4) return 0;

        return (uint)(
            byte_array_[address] |
            (byte_array_[address + 1] << 8) |
            (byte_array_[address + 2] << 16) |
            (byte_array_[address + 3] << 24)
        );
    }

    public void Write8(int address, uint value)
    {
        byte_array_[address] = (byte)value;
    }

    public void Write16(int address, uint value)
    {
        byte_array_[address] = (byte)value;
        byte_array_[address + 1] = (byte)(value >> 8);
    }

    public void Write32(int address, uint value)
    {
        byte_array_[address] = (byte)value;
        byte_array_[address + 1] = (byte)(value >> 8);
        byte_array_[address + 2] = (byte)(value >> 16);
        byte_array_[address + 3] = (byte)(value >> 24);
    }
}

class Registers
{
    private uint[] r = new uint[32];

    public uint Read(int i) => i == 0 ? 0 : r[i];

    public void Write(int i, uint v)
    {
        if (i != 0) r[i] = v;
    }
}

class CPU
{
    private Memory mem = new Memory();
    private Registers regs = new Registers();
    private uint pc = 0;

    public uint PC => pc;

    public uint PeekInstruction() => mem.Read32((int)pc);

    private static int SignExtend(uint value, int bits)
    {
        int shift = 32 - bits;
        return ((int)(value << shift)) >> shift;
    }

    public void Step()
    {
        uint inst = mem.Read32((int)pc);

        uint opcode = inst & 0x7F;
        uint rd = (inst >> 7) & 0x1F;
        uint funct3 = (inst >> 12) & 0x7;
        uint rs1 = (inst >> 15) & 0x1F;
        uint rs2 = (inst >> 20) & 0x1F;
        uint funct7 = (inst >> 25) & 0x7F;

        uint oldPc = pc;
        pc += 4;

        switch (opcode)
        {
            // ================= R TYPE =================
            case 0x33:
            {
                uint a = regs.Read((int)rs1);
                uint b = regs.Read((int)rs2);

                switch (funct3)
                {
                    case 0x0:
                        regs.Write((int)rd, funct7 == 0x20 ? a - b : a + b);
                        break;

                    case 0x1:
                        regs.Write((int)rd, a << (int)(b & 0x1F));
                        break;

                    case 0x2:
                        regs.Write((int)rd, (uint)((int)a < (int)b ? 1 : 0));
                        break;

                    case 0x3:
                        regs.Write((int)rd, a < b ? 1u : 0u);
                        break;

                    case 0x4:
                        regs.Write((int)rd, a ^ b);
                        break;

                    case 0x5:
                        regs.Write((int)rd,
                            funct7 == 0x20
                                ? (uint)((int)a >> (int)(b & 0x1F))
                                : a >> (int)(b & 0x1F));
                        break;

                    case 0x6:
                        regs.Write((int)rd, a | b);
                        break;

                    case 0x7:
                        regs.Write((int)rd, a & b);
                        break;
                }
                break;
            }

            // ================= I TYPE =================
            case 0x13:
            {
                uint a = regs.Read((int)rs1);
                int imm = SignExtend(inst >> 20, 12);

                int shamt = (int)((inst >> 20) & 0x1F);

                switch (funct3)
                {
                    case 0x0:
                        regs.Write((int)rd, (uint)((int)a + imm));
                        break;

                    case 0x1:
                        regs.Write((int)rd, a << shamt);
                        break;

                    case 0x2:
                        regs.Write((int)rd, (uint)((int)a < imm ? 1 : 0));
                        break;

                    case 0x3:
                        regs.Write((int)rd, a < (uint)imm ? 1u : 0u);
                        break;

                    case 0x4:
                        regs.Write((int)rd, a ^ (uint)imm);
                        break;

                    case 0x5:
                        regs.Write((int)rd,
                            funct7 == 0x20
                                ? (uint)((int)a >> shamt)
                                : a >> shamt);
                        break;

                    case 0x6:
                        regs.Write((int)rd, a | (uint)imm);
                        break;

                    case 0x7:
                        regs.Write((int)rd, a & (uint)imm);
                        break;
                }
                break;
            }

            // ================= LOAD =================
            case 0x03:
            {
                int imm = SignExtend(inst >> 20, 12);
                uint addr = (uint)((int)regs.Read((int)rs1) + imm);

                switch (funct3)
                {
                    case 0x0:
                        regs.Write((int)rd, (uint)(sbyte)mem.Read8((int)addr));
                        break;

                    case 0x1:
                        regs.Write((int)rd, (uint)(short)mem.Read16((int)addr));
                        break;

                    case 0x2:
                        regs.Write((int)rd, mem.Read32((int)addr));
                        break;

                    case 0x4:
                        regs.Write((int)rd, mem.Read8((int)addr));
                        break;

                    case 0x5:
                        regs.Write((int)rd, mem.Read16((int)addr));
                        break;
                }
                break;
            }

            // ================= STORE =================
            case 0x23:
            {
                int imm =
                    SignExtend(
                        ((inst >> 25) << 5) | ((inst >> 7) & 0x1F),
                        12
                    );

                uint addr = (uint)((int)regs.Read((int)rs1) + imm);
                uint val = regs.Read((int)rs2);

                switch (funct3)
                {
                    case 0x0:
                        mem.Write8((int)addr, val);
                        break;
                    case 0x1:
                        mem.Write16((int)addr, val);
                        break;
                    case 0x2:
                        mem.Write32((int)addr, val);
                        break;
                }
                break;
            }

            // ================= BRANCH =================
            case 0x63:
            {
                uint a = regs.Read((int)rs1);
                uint b = regs.Read((int)rs2);

                bool take = funct3 switch
                {
                    0x0 => a == b,
                    0x1 => a != b,
                    0x4 => (int)a < (int)b,
                    0x5 => (int)a >= (int)b,
                    0x6 => a < b,
                    0x7 => a >= b,
                    _ => false
                };

                int imm =
                    SignExtend(
                        ((inst >> 31) << 12) |
                        ((inst >> 7) & 0x1) << 11 |
                        ((inst >> 25) & 0x3F) << 5 |
                        ((inst >> 8) & 0xF) << 1,
                        13
                    );

                if (take)
                    pc = (uint)((int)oldPc + imm);

                break;
            }

            // ================= LUI =================
            case 0x37:
                regs.Write((int)rd, inst & 0xFFFFF000);
                break;

            // ================= AUIPC =================
            case 0x17:
                regs.Write((int)rd, (uint)((int)oldPc + (int)(inst & 0xFFFFF000)));
                break;

            // ================= JAL =================
            case 0x6F:
            {
                int imm =
                    SignExtend(
                        ((inst >> 31) << 20) |
                        ((inst >> 12) & 0xFF) << 12 |
                        ((inst >> 20) & 0x1) << 11 |
                        ((inst >> 21) & 0x3FF) << 1,
                        21
                    );

                regs.Write((int)rd, pc);
                pc = (uint)((int)oldPc + imm);
                break;
            }

            // ================= JALR =================
            case 0x67:
            {
                int imm = SignExtend(inst >> 20, 12);
                uint target = (uint)((int)regs.Read((int)rs1) + imm) & ~1u;

                regs.Write((int)rd, pc);
                pc = target;
                break;
            }

            default:
                Console.WriteLine($"Unknown opcode: {opcode:X}");
                break;
        }
        Console.WriteLine(
    $"PC={oldPc:X}, opcode={opcode:X}, rd={rd}, rs1={rs1}, rs2={rs2}, funct3={funct3}, funct7={funct7}"
);
    }

    public void DumpRegisters()
    {
        Console.WriteLine($"PC: 0x{pc:X8}");
        for (int i = 0; i < 32; i++)
        {
            Console.Write($"x{i}: {regs.Read(i),-10}");
            if ((i + 1) % 4 == 0) Console.WriteLine();
        }
        Console.WriteLine("----------------------------------");
    }

    public void LoadProgram(byte[] program)
    {
        for (int i = 0; i < program.Length; i++)
            mem.Write8(i, program[i]);
    }
}

class Program
{
    static void Main()
    {
        CPU cpu = new CPU();

        byte[] program = 
        {
            0x93, 0x00, 0x00, 0xFF, // addi x1, x0, 20
            0x13, 0x01, 0x20, 0x00, // addi x2, x0, 5
            0x33, 0xD1, 0x20, 0x00,
            0xB3, 0x52, 0x21, 0x40
        };
        cpu.LoadProgram(program);

        Console.WriteLine($"Loaded {program.Length} bytes");

        for (int i = 0; i < 3; i++)
        {
            if (cpu.PeekInstruction() == 0) break;
            cpu.Step();
            cpu.DumpRegisters();
        }

        Console.WriteLine("Done");
        Console.ReadLine();
    }
}