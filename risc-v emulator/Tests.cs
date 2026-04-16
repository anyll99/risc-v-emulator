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

        CPU cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[] { 0x93, 0x00, 0x40, 0x01, 0x73, 0x00, 0x10, 0x00 });
        while (!cpu.Halted) cpu.Step();
        Assert("ADDI x1=20", cpu.GetReg(1), 20);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x40, 0x01,
            0x13, 0x01, 0x50, 0x00,
            0xB3, 0x81, 0x20, 0x00,
            0x73, 0x00, 0x10, 0x00,
        });
        while (!cpu.Halted) cpu.Step();
        Assert("ADD x3=25", cpu.GetReg(3), 25);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[] { 0x13, 0x00, 0x50, 0x00, 0x73, 0x00, 0x10, 0x00 });
        while (!cpu.Halted) cpu.Step();
        Assert("x0 always zero", cpu.GetReg(0), 0);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x40, 0x01,
            0x13, 0x01, 0x50, 0x00,
            0x33, 0x82, 0x20, 0x40,
            0x73, 0x00, 0x10, 0x00,
        });
        while (!cpu.Halted) cpu.Step();
        Assert("SUB x4=15", cpu.GetReg(4), 15);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0xF0, 0x00,
            0x13, 0x01, 0xA0, 0x00,
            0x33, 0xF1, 0x20, 0x00,
            0x73, 0x00, 0x10, 0x00,
        });
        while (!cpu.Halted) cpu.Step();
        Assert("AND 15&10=10", cpu.GetReg(2), 10);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x50, 0x00,
            0x13, 0x01, 0xA0, 0x00,
            0x33, 0xE1, 0x20, 0x00,
            0x73, 0x00, 0x10, 0x00,
        });
        while (!cpu.Halted) cpu.Step();
        Assert("OR 5|10=15", cpu.GetReg(2), 15);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0xF0, 0x00,
            0x13, 0x01, 0xA0, 0x00,
            0x33, 0xC1, 0x20, 0x00,
            0x73, 0x00, 0x10, 0x00,
        });
        while (!cpu.Halted) cpu.Step();
        Assert("XOR 15^10=5", cpu.GetReg(2), 5);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0xB7, 0x11, 0x00, 0x00,
            0x73, 0x00, 0x10, 0x00,
        });
        while (!cpu.Halted) cpu.Step();
        Assert("LUI x3=0x1000", cpu.GetReg(3), 0x1000);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x50, 0x00,
            0x13, 0x01, 0x50, 0x00,
            0x63, 0x84, 0x20, 0x00,
            0x93, 0x00, 0x10, 0x00,
            0x73, 0x00, 0x10, 0x00,
        });
        while (!cpu.Halted) cpu.Step();
        Assert("BEQ taken x1 still 5", cpu.GetReg(1), 5);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x93, 0x00, 0x70, 0x02,
            0x23, 0x20, 0x10, 0x00,
            0x03, 0x21, 0x00, 0x00,
            0x73, 0x00, 0x10, 0x00,
        });
        while (!cpu.Halted) cpu.Step();
        Assert("SW/LW x2=39", cpu.GetReg(2), 39);

        cpu = new CPU();
        cpu.Silent = true;
        cpu.LoadProgram(new byte[]
        {
            0x6F, 0x00, 0x80, 0x00,
            0x93, 0x00, 0x10, 0x00,
            0x93, 0x00, 0x20, 0x00,
            0x73, 0x00, 0x10, 0x00,
        });
        while (!cpu.Halted) cpu.Step();
        Assert("JAL skips x1=2", cpu.GetReg(1), 2);

        Console.WriteLine($"\n{passed} passed, {failed} failed.");
    }
}