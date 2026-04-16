

internal class Tests
{
    public static void Run()
    {
        int failed = 0;
        int passed = 0;


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

        // Test  1: addi x1, x0, 20 -> x1 should be 20
        CPU cpu = new CPU();
        cpu.LoadProgram(new byte[] { 0x93, 0x00, 0x40, 0x01, 0x73, 0x00, 0x10, 0x00 });
        while (!cpu.Halted) cpu.Step();
        Assert("ADDI x1=20", cpu.GetReg(1), 20);

        // Test 2: add x3 = x1 + x2 (20 + 5 = 25)
        cpu = new CPU();
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x40, 0x01,  // addi x1, x0, 20
            0x13, 0x01, 0x50, 0x00,  // addi x2, x0, 5
            0xB3, 0x81, 0x20, 0x00,  // add  x3, x1, x2
            0x73, 0x00, 0x10, 0x00,  // ebreak
        });
        while (!cpu.Halted) cpu.Step();
        Assert("ADD x3=25", cpu.GetReg(3), 25);

        // Test 3: x0 should always be 0
        cpu = new CPU();
        cpu.LoadProgram(new byte[] { 0x13, 0x00, 0x50, 0x00, 0x73, 0x00, 0x10, 0x00 });
        while (!cpu.Halted) cpu.Step();
        Assert("x0 always zero", cpu.GetReg(0), 0);



        // SUB
        cpu = new CPU();
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x40, 0x01,  // addi x1, x0, 20
            0x13, 0x01, 0x50, 0x00,  // addi x2, x0, 5
            0x33, 0x82, 0x20, 0x40,  // sub  x4, x1, x2
            0x73, 0x00, 0x10, 0x00,  // ebreak
        });
        while (!cpu.Halted) cpu.Step();
        Assert("SUB x4=15", cpu.GetReg(4), 15);

        // AND
        cpu = new CPU();
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0xF0, 0x00,  // addi x1, x0, 15  (0b1111)
            0x13, 0x01, 0xA0, 0x00,  // addi x2, x0, 10  (0b1010)
            0x33, 0xF1, 0x20, 0x00,  // and  x2, x1, x2
            0x73, 0x00, 0x10, 0x00,  // ebreak
        });
        while (!cpu.Halted) cpu.Step();
        Assert("AND 15&10=10", cpu.GetReg(2), 10);

        // OR
        cpu = new CPU();
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x50, 0x00,  // addi x1, x0, 5   (0b0101)
            0x13, 0x01, 0xA0, 0x00,  // addi x2, x0, 10  (0b1010)
            0x33, 0xE1, 0x20, 0x00,  // or   x2, x1, x2
            0x73, 0x00, 0x10, 0x00,  // ebreak
        });
        while (!cpu.Halted) cpu.Step();
        Assert("OR 5|10=15", cpu.GetReg(2), 15);

        // XOR
        cpu = new CPU();
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0xF0, 0x00,  // addi x1, x0, 15  (0b1111)
            0x13, 0x01, 0xA0, 0x00,  // addi x2, x0, 10  (0b1010)
            0x33, 0xC1, 0x20, 0x00,  // xor  x2, x1, x2
            0x73, 0x00, 0x10, 0x00,  // ebreak
        });
        while (!cpu.Halted) cpu.Step();
        Assert("XOR 15^10=5", cpu.GetReg(2), 5);

        // LUI
        cpu = new CPU();
        cpu.LoadProgram(new byte[]
        {
            0xB7, 0x11, 0x00, 0x00,  // lui x3, 1  (x3 = 0x1000)
            0x73, 0x00, 0x10, 0x00,  // ebreak
        });
        while (!cpu.Halted) cpu.Step();
        Assert("LUI x3=0x1000", cpu.GetReg(3), 0x1000);

        // BEQ taken
        cpu = new CPU();
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x50, 0x00,  // addi x1, x0, 5
            0x13, 0x01, 0x50, 0x00,  // addi x2, x0, 5
            0x63, 0x84, 0x20, 0x00,  // beq  x1, x2, +8  (skip next)
            0x93, 0x00, 0x10, 0x00,  // addi x1, x0, 1  (should be skipped)
            0x73, 0x00, 0x10, 0x00,  // ebreak
        });
        while (!cpu.Halted) cpu.Step();
        Assert("BEQ taken x1 still 5", cpu.GetReg(1), 5);

        // Store and load
        cpu = new CPU();
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x70, 0x02,  // addi x1, x0, 39
            0x23, 0x20, 0x10, 0x00,  // sw   x1, 0(x0)   store 39 at address 0
            0x03, 0x21, 0x00, 0x00,  // lw   x2, 0(x0)   load from address 0
            0x73, 0x00, 0x10, 0x00,  // ebreak
        });
        while (!cpu.Halted) cpu.Step();
        Assert("SW/LW x2=39", cpu.GetReg(2), 39);

        // JAL
        cpu = new CPU();
        cpu.LoadProgram(new byte[]
        {
            0x6F, 0x00, 0x80, 0x00,  // jal x0, +8  (jump over next instruction)
            0x93, 0x00, 0x10, 0x00,  // addi x1, x0, 1  (should be skipped)
            0x93, 0x00, 0x20, 0x00,  // addi x1, x0, 2
            0x73, 0x00, 0x10, 0x00,  // ebreak
        });
        while (!cpu.Halted) cpu.Step();
        Assert("JAL skips x1=2", cpu.GetReg(1), 2);

        Console.WriteLine($"\n{passed} passed, {failed} failed.");
    }
}
