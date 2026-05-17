internal class Tests
{
    public static void Run()
    {
        int failed = 0;
        int passed = 0;

        void RunCpu(CPU cpu)
        {
            for (int i = 0; i < 100_000; i++)
            {
                if (cpu.Halted) return;
                cpu.Step();
            }
            Console.WriteLine("FAIL: step limit reached — possible infinite loop");
            failed++;
        }

        void Assert(string name, uint actual, uint expected)
        {
            if (actual == expected)
            {
                Console.WriteLine($"PASS: {name}");
                passed++;
            }
            else
            {
                Console.WriteLine($"FAIL: {name} — expected {expected}, got {actual}");
                failed++;
            }
        }

        CPU cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x40, 0x01,  // ADDI x1, x0, 20
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("ADDI x1=20", cpu.GetReg(1), 20);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x40, 0x01,  // ADDI x1, x0, 20
            0x13, 0x01, 0x50, 0x00,  // ADDI x2, x0, 5
            0xB3, 0x81, 0x20, 0x00,  // ADD  x3, x1, x2
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("ADD x3=25", cpu.GetReg(3), 25);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x13, 0x00, 0x50, 0x00,  // ADDI x0, x0, 5  (write to x0 is discarded)
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("x0 always zero", cpu.GetReg(0), 0);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x40, 0x01,  // ADDI x1, x0, 20
            0x13, 0x01, 0x50, 0x00,  // ADDI x2, x0, 5
            0x33, 0x82, 0x20, 0x40,  // SUB  x4, x1, x2
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("SUB x4=15", cpu.GetReg(4), 15);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0xF0, 0x00,  // ADDI x1, x0, 15
            0x13, 0x01, 0xA0, 0x00,  // ADDI x2, x0, 10
            0x33, 0xF1, 0x20, 0x00,  // AND  x2, x1, x2
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("AND 15&10=10", cpu.GetReg(2), 10);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x50, 0x00,  // ADDI x1, x0, 5
            0x13, 0x01, 0xA0, 0x00,  // ADDI x2, x0, 10
            0x33, 0xE1, 0x20, 0x00,  // OR   x2, x1, x2
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("OR 5|10=15", cpu.GetReg(2), 15);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0xF0, 0x00,  // ADDI x1, x0, 15
            0x13, 0x01, 0xA0, 0x00,  // ADDI x2, x0, 10
            0x33, 0xC1, 0x20, 0x00,  // XOR  x2, x1, x2
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("XOR 15^10=5", cpu.GetReg(2), 5);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0xB7, 0x11, 0x00, 0x00,  // LUI x3, 1  (x3 = 0x1000)
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("LUI x3=0x1000", cpu.GetReg(3), 0x1000);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x50, 0x00,  // ADDI x1, x0, 5
            0x13, 0x01, 0x50, 0x00,  // ADDI x2, x0, 5
            0x63, 0x84, 0x20, 0x00,  // BEQ  x1, x2, +8  (taken)
            0x93, 0x00, 0x10, 0x00,  // ADDI x1, x0, 1   (skipped)
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("BEQ taken x1 still 5", cpu.GetReg(1), 5);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x70, 0x02,  // ADDI x1, x0, 39
            0x23, 0x20, 0x10, 0x00,  // SW   x1, 0(x0)
            0x03, 0x21, 0x00, 0x00,  // LW   x2, 0(x0)
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("SW/LW x2=39", cpu.GetReg(2), 39);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x6F, 0x00, 0x80, 0x00,  // JAL  x0, +8
            0x93, 0x00, 0x10, 0x00,  // ADDI x1, x0, 1  (skipped)
            0x93, 0x00, 0x20, 0x00,  // ADDI x1, x0, 2
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("JAL skips x1=2", cpu.GetReg(1), 2);

        // ADDI negative immediate
        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0xF0, 0xFF,  // ADDI x1, x0, -1
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("ADDI x1=-1", cpu.GetReg(1), 0xFFFFFFFF);

        // SLLI
        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x10, 0x00,  // ADDI x1, x0, 1
            0x13, 0x91, 0x40, 0x00,  // SLLI x2, x1, 4
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("SLLI x2=16", cpu.GetReg(2), 16);

        // SRLI
        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x00, 0x01,  // ADDI x1, x0, 16
            0x13, 0xD1, 0x20, 0x00,  // SRLI x2, x1, 2
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("SRLI x2=4", cpu.GetReg(2), 4);

        // SRAI (arithmetic shift preserves sign)
        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x80, 0xFF,  // ADDI x1, x0, -8
            0x13, 0xD1, 0x20, 0x40,  // SRAI x2, x1, 2
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("SRAI x2=-2", cpu.GetReg(2), 0xFFFFFFFE);

        // SLT signed
        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0xF0, 0xFF,  // ADDI x1, x0, -1
            0x13, 0x01, 0x10, 0x00,  // ADDI x2, x0, 1
            0xB3, 0xA1, 0x20, 0x00,  // SLT x3, x1, x2
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("SLT -1<1=1", cpu.GetReg(3), 1);

        // SLTU unsigned
        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0xF0, 0xFF,  // ADDI x1, x0, -1  (=0xFFFFFFFF unsigned)
            0x13, 0x01, 0x10, 0x00,  // ADDI x2, x0, 1
            0xB3, 0x31, 0x11, 0x00,  // SLTU x3, x2, x1
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("SLTU 1<0xFFFFFFFF=1", cpu.GetReg(3), 1);

        // BNE taken
        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x50, 0x00,  // ADDI x1, x0, 5
            0x13, 0x01, 0x60, 0x00,  // ADDI x2, x0, 6
            0x63, 0x94, 0x20, 0x00,  // BNE x1, x2, +8  (taken)
            0x93, 0x00, 0x30, 0x06,  // ADDI x1, x0, 99 (skipped)
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("BNE taken x1 still 5", cpu.GetReg(1), 5);

        // BLT taken
        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x30, 0x00,  // ADDI x1, x0, 3
            0x13, 0x01, 0x70, 0x00,  // ADDI x2, x0, 7
            0x63, 0xC4, 0x20, 0x00,  // BLT x1, x2, +8  (taken: 3 < 7)
            0x93, 0x00, 0x30, 0x06,  // ADDI x1, x0, 99 (skipped)
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("BLT taken x1 still 3", cpu.GetReg(1), 3);

        // BGE taken
        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x70, 0x00,  // ADDI x1, x0, 7
            0x13, 0x01, 0x30, 0x00,  // ADDI x2, x0, 3
            0x63, 0xD4, 0x20, 0x00,  // BGE x1, x2, +8  (taken: 7 >= 3)
            0x93, 0x00, 0x30, 0x06,  // ADDI x1, x0, 99 (skipped)
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("BGE taken x1 still 7", cpu.GetReg(1), 7);

        // BLTU taken (unsigned)
        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0xF0, 0xFF,  // ADDI x1, x0, -1  (=0xFFFFFFFF unsigned)
            0x13, 0x01, 0x10, 0x00,  // ADDI x2, x0, 1
            0x63, 0x64, 0x11, 0x00,  // BLTU x2, x1, +8  (taken: 1 < 0xFFFFFFFF)
            0x13, 0x01, 0x30, 0x06,  // ADDI x2, x0, 99  (skipped)
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("BLTU taken x2 still 1", cpu.GetReg(2), 1);

        // SB + LBU (store byte, load zero-extended)
        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x80, 0x0C,  // ADDI x1, x0, 200
            0x23, 0x00, 0x10, 0x00,  // SB x1, 0(x0)
            0x03, 0x41, 0x00, 0x00,  // LBU x2, 0(x0)
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("SB/LBU x2=200", cpu.GetReg(2), 200);

        // SB + LB (store byte, load sign-extended)
        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0xF0, 0xFF,  // ADDI x1, x0, -1  (byte=0xFF)
            0x23, 0x00, 0x10, 0x00,  // SB x1, 0(x0)
            0x03, 0x01, 0x00, 0x00,  // LB x2, 0(x0)
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("SB/LB x2=-1", cpu.GetReg(2), 0xFFFFFFFF);

        // AUIPC
        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x97, 0x10, 0x00, 0x00,  // AUIPC x1, 1  (x1 = PC(0) + 0x1000)
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("AUIPC x1=0x1000", cpu.GetReg(1), 0x1000);

        // JALR
        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0xC0, 0x00,  // ADDI x1, x0, 12
            0x67, 0x81, 0x00, 0x00,  // JALR x2, x1, 0  (jump to 12, x2=8)
            0x93, 0x01, 0x10, 0x00,  // ADDI x3, x0, 1  (skipped)
            0x73, 0x00, 0x10, 0x00,  // EBREAK
        });
        RunCpu(cpu);
        Assert("JALR x2=8", cpu.GetReg(2), 8);
        Assert("JALR x3 skipped=0", cpu.GetReg(3), 0);

        Console.WriteLine($"\n{passed} passed, {failed} failed.");
    }
}