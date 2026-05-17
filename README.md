# RISC-V Emulator

A lightweight RV32I (Base Integer Instruction Set) emulator written in C#. It simulates a 32-bit RISC-V CPU by fetching, decoding, and executing instructions from a flat binary file.

## Usage

```
dotnet run <path_to_binary.bin>
```

Example:
```
dotnet run program.bin
```

To run the built-in test suite:
```
dotnet run --test
```

To produce a binary from a RISC-V assembly file, you need the [riscv-none-elf-gcc toolchain](https://github.com/xpack-binutils/riscv-none-elf-gcc/releases). Then run:

```bash
riscv-none-elf-gcc -march=rv32i -mabi=ilp32 -nostdlib -static -T link.ld program.s -o program.elf
riscv-none-elf-objcopy -O binary program.elf program.bin
```

## Features

**Instruction set — full RV32I coverage:**
- R-Type: `ADD`, `SUB`, `AND`, `OR`, `XOR`, `SLL`, `SRL`, `SRA`, `SLT`, `SLTU`
- I-Type: `ADDI`, `ANDI`, `ORI`, `XORI`, `SLTI`, `SLTIU`, `SLLI`, `SRLI`, `SRAI`
- Load: `LB`, `LH`, `LW`, `LBU`, `LHU`
- S-Type: `SB`, `SH`, `SW`
- B-Type: `BEQ`, `BNE`, `BLT`, `BGE`, `BLTU`, `BGEU`
- U-Type: `LUI`, `AUIPC`
- J-Type: `JAL`, `JALR`
- System: `ECALL` (exit, write), `EBREAK`

**Memory:** 64 KB byte-addressable memory with 8, 16, and 32-bit load/store support. Out-of-bounds accesses print a warning instead of crashing.

**Registers:** 32 general-purpose registers with hardware-accurate `x0` zero-register enforcement. After execution, only non-zero registers are printed to keep the output clean:

```
PC: 0x00000014
----------------------------------
x1   20           (0x00000014)
x2   5            (0x00000005)
x3   25           (0x00000019)
x4   15           (0x0000000F)
----------------------------------
```

**Debug mode:** Set `cpu.Debug = true` to enable step-by-step instruction logging.

**Silent mode:** Set `cpu.Silent = true` to suppress all CPU output, useful for running tests programmatically.

**Safety:** Execution is capped at 1,000,000 instructions to prevent infinite loops.

## Architecture

The emulator is built around three classes:

- `Memory` — manages a 65,536-byte array with bounds-checked read/write operations
- `Registers` — a 32-entry register file that enforces the `x0 = 0` rule
- `CPU` — the execution engine that runs the fetch-decode-execute cycle

The CPU follows the standard fetch-decode-execute cycle:

1. **Fetch** — reads a 32-bit instruction from memory at the current PC
2. **Decode** — extracts the opcode, registers, and immediates using bitwise masking
3. **Execute** — performs the operation and updates the PC, registers, or memory

## Sample Programs

The `samples/` folder contains example RISC-V assembly programs you can build and run:

| File | Description | Expected result |
|---|---|---|
| `hello.s` | Prints "Hello, World!" to stdout | `Hello, World!` printed |
| `sum.s` | Sums integers from 1 to 10 | `a0 = 55` in register dump |
| `fibonacci.s` | Computes the 10th Fibonacci number | `a0 = 55` in register dump |

To build all samples, run the build script from inside the `samples/` folder:

```powershell
cd samples
.\build.ps1
```

Then run a binary from the project directory:

```
dotnet run ..\samples\hello.bin
dotnet run ..\samples\sum.bin
dotnet run ..\samples\fibonacci.bin
```

## Testing

The emulator includes a built-in test suite covering all major RV32I instruction types. Run it with:

```
dotnet run --test
```

Expected output:
```
PASS: ADDI x1=20
PASS: ADD x3=25
PASS: x0 always zero
PASS: SUB x4=15
PASS: AND 15&10=10
PASS: OR 5|10=15
PASS: XOR 15^10=5
PASS: LUI x3=0x1000
PASS: BEQ taken x1 still 5
PASS: SW/LW x2=39
PASS: JAL skips x1=2
PASS: ADDI x1=-1
PASS: SLLI x2=16
PASS: SRLI x2=4
PASS: SRAI x2=-2
PASS: SLT -1<1=1
PASS: SLTU 1<0xFFFFFFFF=1
PASS: BNE taken x1 still 5
PASS: BLT taken x1 still 3
PASS: BGE taken x1 still 7
PASS: BLTU taken x2 still 1
PASS: SB/LBU x2=200
PASS: SB/LB x2=-1
PASS: AUIPC x1=0x1000
PASS: JALR x2=8
PASS: JALR x3 skipped=0

26 passed, 0 failed.
```

## Requirements

- [.NET SDK](https://dotnet.microsoft.com/download) (6.0 or later)
- [xpack-riscv-none-elf-gcc](https://github.com/xpack-binutils/riscv-none-elf-gcc/releases) to assemble `.s` files into binaries
