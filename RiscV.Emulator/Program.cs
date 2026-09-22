using System;

/// <summary>Byte-addressable little-endian memory. Out-of-bounds accesses are ignored.</summary>
class Memory
{
    private byte[] byte_array_;
    public bool Silent = false;

    /// <summary>Allocates memory of the given size in bytes.</summary>
    public Memory(int size = 65536)
    {
        byte_array_ = new byte[size];
    }

    private bool InBounds(int address, int width)
        => address >= 0 && address <= byte_array_.Length - width;

    /// <summary>Reads a byte from the given address</summary>
    public uint Read8(int address)
    {
        if (!InBounds(address, 1))
        {
            if (!Silent) Console.WriteLine($"Warning: out-of-bounds read at address 0x{address:X8}");
            return 0;
        }
        return byte_array_[address];
    }

    /// <summary>Reads an unsigned 16-bit little-endian halfword from the given address.</summary>
    public uint Read16(int address)
    {
        if (!InBounds(address, 2))
        {
            if (!Silent) Console.WriteLine($"Warning: out-of-bounds read at address 0x{address:X8}");
            return 0;
        }
        return (uint)(byte_array_[address] | (byte_array_[address + 1] << 8));
    }

    /// <summary>Reads an unsigned 32-bit little-endian word from the given address.</summary>
    public uint Read32(int address)
    {
        if (!InBounds(address, 4))
        {
            if (!Silent) Console.WriteLine($"Warning: out-of-bounds read at address 0x{address:X8}");
            return 0;
        }
        return (uint)(
            byte_array_[address] |
            (byte_array_[address + 1] << 8) |
            (byte_array_[address + 2] << 16) |
            (byte_array_[address + 3] << 24)
        );
    }

    /// <summary>Writes a byte to the given address.</summary>
    public void Write8(int address, uint value)
    {
        if (!InBounds(address, 1))
        {
            if (!Silent) Console.WriteLine($"Warning: out-of-bounds write at address 0x{address:X8}");
            return;
        }
        byte_array_[address] = (byte)value;
    }

    /// <summary>Writes as a little-endian halfword.</summary>
    public void Write16(int address, uint value)
    {
        if (!InBounds(address, 2))
        {
            if (!Silent) Console.WriteLine($"Warning: out-of-bounds write at address 0x{address:X8}");
            return;
        }
        byte_array_[address] = (byte)value;
        byte_array_[address + 1] = (byte)(value >> 8);
    }

    /// <summary>Writes as a little-endian word to the given address.</summary>
    public void Write32(int address, uint value)
    {
        if (!InBounds(address, 4))
        {
            if (!Silent) Console.WriteLine($"Warning: out-of-bounds write at address 0x{address:X8}");
            return;
        }
        byte_array_[address] = (byte)value;
        byte_array_[address + 1] = (byte)(value >> 8);
        byte_array_[address + 2] = (byte)(value >> 16);
        byte_array_[address + 3] = (byte)(value >> 24);
    }
}

/// <summary>The 32 general-purpose RISC-V integer registers. x0 is hardwired to zero.</summary>
class Registers
{
    private uint[] r = new uint[32];

    /// <summary>Returns the value of register.</summary>
    public uint Read(int i) => i == 0 ? 0 : r[i];

    /// <summary>Writes to register. Writes to x0 are silently discarded.</summary>
    public void Write(int i, uint v)
    {
        if (i != 0) r[i] = v;
    }
}

/// <summary>RV32I CPU. Executes one instruction per call.</summary>
class CPU
{
    private Memory mem;
    private Registers regs = new Registers();
    private uint pc = 0;
    public bool Debug = false;
    public bool Halted { get; private set; } = false;

    private bool _silent = false;
    public bool Silent
    {
        get => _silent;
        set { _silent = value; mem.Silent = value; }
    }

    private const uint OP_R_TYPE = 0x33;
    private const uint OP_I_TYPE = 0x13;
    private const uint OP_LOAD = 0x03;
    private const uint OP_STORE = 0x23;
    private const uint OP_BRANCH = 0x63;
    private const uint OP_LUI = 0x37;
    private const uint OP_AUIPC = 0x17;
    private const uint OP_JAL = 0x6F;
    private const uint OP_JALR = 0x67;
    private const uint OP_SYSTEM = 0x73;

    /// <summary>Creates a CPU with its own memory.</summary>
    public CPU(int memorySize = 65536)
    {
        mem = new Memory(memorySize);
    }

    public uint PC => pc;

    public uint GetReg(int i) => regs.Read(i);

    public uint PeekInstruction() => mem.Read32((int)pc);

    private static int SignExtend(uint value, int bits)
    {
        int shift = 32 - bits;
        return ((int)(value << shift)) >> shift;
    }

    /// <summary>Fetches, decodes, and executes one instruction. Does nothing if halted.</summary>
    public void Step()
    {
        if (Halted) return;

        uint inst = mem.Read32((int)pc);

        // bits 0-6: selects the instruction format (R, I, load, store, branch, etc.)
        uint opcode = inst & 0x7F;
        
        // bits 7-11: destination register
        uint rd = (inst >> 7) & 0x1F;

        // bits 12-14: selects the operation within an opcode (ADD vs XOR)
        uint funct3 = (inst >> 12) & 0x7;

        // bits 15-19: first source register
        uint rs1 = (inst >> 15) & 0x1F;

        // bits 20-24: second source register
        uint rs2 = (inst >> 20) & 0x1F;
        
        // bits 25-31: further distinguishes operations (ADD vs SUB, SRL vs SRA)
        uint funct7 = (inst >> 25) & 0x7F;

        uint oldPc = pc;
        pc += 4;

        switch (opcode)
        {
            // ================= R TYPE =================
            case OP_R_TYPE:
                {
                    uint a = regs.Read((int)rs1);
                    uint b = regs.Read((int)rs2);

                    switch (funct3)
                    {
                        // ADD / SUB (addition or subtraction)
                        case 0x0:
                            regs.Write((int)rd, funct7 == 0x20 ? a - b : a + b);
                            break;

                        // SLL (shift left logical)
                        case 0x1:
                            regs.Write((int)rd, a << (int)(b & 0x1F));
                            break;

                        //SLT (set less than)
                        case 0x2:
                            regs.Write((int)rd, (uint)((int)a < (int)b ? 1 : 0));
                            break;

                        // SLTU (set less than unsigned)
                        case 0x3:
                            regs.Write((int)rd, a < b ? 1u : 0u);
                            break;

                        // XOR
                        case 0x4:
                            regs.Write((int)rd, a ^ b);
                            break;

                        // SRL / SRA (shift right logical, shift right arithmetic)
                        case 0x5:
                            regs.Write((int)rd,
                                funct7 == 0x20
                                    ? (uint)((int)a >> (int)(b & 0x1F))
                                    : a >> (int)(b & 0x1F));
                            break;

                        // OR
                        case 0x6:
                            regs.Write((int)rd, a | b);
                            break;
                        
                        // AND
                        case 0x7:
                            regs.Write((int)rd, a & b);
                            break;
                    }
                    break;
                }

            // ================= I TYPE =================
            case OP_I_TYPE:
                {
                    uint a = regs.Read((int)rs1);
                    int imm = SignExtend(inst >> 20, 12);
                    int shamt = (int)((inst >> 20) & 0x1F);

                    switch (funct3)
                    {
                        // ADDI(add immediate)
                        case 0x0:
                            regs.Write((int)rd, (uint)((int)a + imm));
                            break;
                        
                        // SLLI (shift left logical immediate)
                        case 0x1:
                            regs.Write((int)rd, a << shamt);
                            break;

                        // SLTI (set less than immediate)
                        case 0x2:
                            regs.Write((int)rd, (uint)((int)a < imm ? 1 : 0));
                            break;

                        // SLTIU (set less than immediate unsigned)
                        case 0x3:
                            regs.Write((int)rd, a < (uint)imm ? 1u : 0u);
                            break;

                        // XORI (xor immediate)
                        case 0x4:
                            regs.Write((int)rd, a ^ (uint)imm);
                            break;

                        // SRLI / SRAI (shift right logical immediate, shift right arithmetic immediate)
                        case 0x5:
                            regs.Write((int)rd,
                                funct7 == 0x20
                                    ? (uint)((int)a >> shamt)
                                    : a >> shamt);
                            break;

                        // ORI (or immediate)
                        case 0x6:
                            regs.Write((int)rd, a | (uint)imm);
                            break;

                        // ANDI (and immediate)
                        case 0x7:
                            regs.Write((int)rd, a & (uint)imm);
                            break;
                    }
                    break;
                }

            // ================= LOAD =================
            case OP_LOAD:
                {
                    int imm = SignExtend(inst >> 20, 12);
                    uint addr = (uint)((int)regs.Read((int)rs1) + imm);

                    switch (funct3)
                    {
                        // LB (load byte)
                        case 0x0:
                            regs.Write((int)rd, (uint)(sbyte)mem.Read8((int)addr));
                            break;

                        // LH (load halfword)
                        case 0x1:
                            regs.Write((int)rd, (uint)(short)mem.Read16((int)addr));
                            break;

                        // LW (load word)
                        case 0x2:
                            regs.Write((int)rd, mem.Read32((int)addr));
                            break;

                        // LBU (load byte unsigned)
                        case 0x4:
                            regs.Write((int)rd, mem.Read8((int)addr));
                            break;

                        // LHU (load halfword unsigned)
                        case 0x5:
                            regs.Write((int)rd, mem.Read16((int)addr));
                            break;
                    }
                    break;
                }

            // ================= STORE =================
            case OP_STORE:
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
                        // SB (store byte)
                        case 0x0:
                            mem.Write8((int)addr, val);
                            break;

                        // SH (store halfword)
                        case 0x1:
                            mem.Write16((int)addr, val);
                            break;

                        // SW (store word)
                        case 0x2:
                            mem.Write32((int)addr, val);
                            break;
                    }
                    break;
                }

            // ================= BRANCH =================
            case OP_BRANCH:
                {
                    uint a = regs.Read((int)rs1);
                    uint b = regs.Read((int)rs2);

                    bool take = funct3 switch
                    {
                        // BEQ (branch if equal)
                        0x0 => a == b,
                        // BNE (branch if not equal)
                        0x1 => a != b,
                        // BLT (branch if less than)
                        0x4 => (int)a < (int)b,
                        // BGE (branch if greater or equal)
                        0x5 => (int)a >= (int)b,
                        // BLTU (branch if less than unsigned)
                        0x6 => a < b,
                        // BGEU (branch if greater or equal unsigned)
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
            case OP_LUI:
                // LUI (load upper immediate)
                regs.Write((int)rd, inst & 0xFFFFF000);
                break;

            // ================= AUIPC =================
            case OP_AUIPC:
                // AUIPC (add upper immediate to PC)
                regs.Write((int)rd, oldPc + (inst & 0xFFFFF000));
                break;

            // ================= JAL =================
            case OP_JAL:
                {
                    int imm =
                        SignExtend(
                            ((inst >> 31) << 20) |
                            ((inst >> 12) & 0xFF) << 12 |
                            ((inst >> 20) & 0x1) << 11 |
                            ((inst >> 21) & 0x3FF) << 1,
                            21
                        );

                    // (jump and link)
                    regs.Write((int)rd, pc);
                    pc = (uint)((int)oldPc + imm);
                    break;
                }

            // ================= JALR =================
            case OP_JALR:
                {
                    int imm = SignExtend(inst >> 20, 12);
                    uint target = (uint)((int)regs.Read((int)rs1) + imm) & ~1u;
                    // (jump and link register)

                    regs.Write((int)rd, pc);
                    pc = target;
                    break;
                }

            case OP_SYSTEM:
                {
                    uint funct12 = inst >> 20;

                    switch (funct12)
                    {
                        // ECALL (environment call) asks the environment to do something, chosen by a7
                        case 0x000:
                            HandleEcall();
                            break;

                        // EBREAK (environment break) halts the cpu
                        case 0x001:
                            if (!Silent) Console.WriteLine("EBREAK hit - halting.");
                            Halted = true;
                            break;

                        default:
                            if (!Silent) Console.WriteLine($"Unhandled SYSTEM funct12: 0x{funct12:X3}");
                            break;
                    }
                    break;
                }

            default:
                if (!Silent) Console.WriteLine($"Unknown opcode: 0x{opcode:X2} at PC=0x{oldPc:X8}");
                Halted = true;
                break;
        }

        if (Debug)
        {
            Console.WriteLine(
                $"PC=0x{oldPc:X8} op=0x{opcode:X2} rd={rd,-2} " +
                $"rs1={rs1,-2} rs2={rs2,-2} f3={funct3} f7=0x{funct7:X2}"
            );
        }
    }

    private void HandleEcall()
    {
        uint syscall = regs.Read(17);

        switch (syscall)
        {
            // exit, halts with exit code a0
            case 93:
                uint exitCode = regs.Read(10);
                if (!Silent) Console.WriteLine($"ECALL exit({exitCode})");
                Halted = true;
                break;

            // write, prints a2 bytes starting at address a1 to fd a0
            case 64:
                {
                    uint fd = regs.Read(10);
                    uint buf = regs.Read(11);
                    uint count = regs.Read(12);

                    if (fd == 1 || fd == 2)
                    {
                        for (uint i = 0; i < count; ++i)
                            Console.Write((char)mem.Read8((int)(buf + i)));
                    }

                    regs.Write(10, count);
                    break;
                }

            default:
                if (!Silent) Console.WriteLine($"Unhandled ECALL syscall={syscall}");
                break;
        }
    }

    /// <summary>Prints PC and all non-zero registers to stdout.</summary>
    public void DumpRegisters()
    {
        Console.WriteLine($"PC: 0x{pc:X8}");
        Console.WriteLine("----------------------------------");
        bool anyNonZero = false;
        for (int i = 1; i < 32; i++)
        {
            uint val = regs.Read(i);
            if (val != 0)
            {
                Console.WriteLine($"x{i,-2}  {val,-12} (0x{val:X8})");
                anyNonZero = true;
            }
        }
        if (!anyNonZero)
            Console.WriteLine("all registers are zero");
        Console.WriteLine("----------------------------------");
    }

    /// <summary>Copies program into memory starting at load address.</summary>
    public void LoadProgram(byte[] program, uint loadAddress = 0)
    {
        for (int i = 0; i < program.Length; i++)
            mem.Write8((int)(loadAddress + i), program[i]);
    }
}

class RiscVProgram
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: dotnet run <path_to_assembly.bin>");
            Console.WriteLine("       dotnet run --test");
            return;
        }

        if (args[0] == "--test")
        {
            Tests.Run();
            return;
        }

        string path = args[0];

        if (!File.Exists(path))
        {
            Console.WriteLine($"Error: file '{path}' not found.");
            return;
        }

        byte[] program;
        try
        {
            program = File.ReadAllBytes(path);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
            return;
        }

        if (program.Length == 0)
        {
            Console.WriteLine("Error: binary file is empty.");
            return;
        }

        CPU cpu = new CPU();
        cpu.LoadProgram(program);

        const int maxSteps = 1_000_000;
        int steps = 0;

        while (!cpu.Halted && steps < maxSteps)
        {
            cpu.Step();
            steps++;
        }

        if (!cpu.Halted)
        {
            Console.WriteLine("Error: execution limit reached, possible infinite loop.");
            Console.WriteLine($"Last PC: 0x{cpu.PC:X8}");
        }

        cpu.DumpRegisters();
    }
}