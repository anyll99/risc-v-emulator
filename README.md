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

To produce a binary from a RISC-V assembly file, you need the [riscv-none-elf-gcc toolchain](https://github.com/xpack-binutils/riscv-none-elf-gcc/releases). Then run:

```bash
riscv-none-elf-as -march=rv32i -mabi=ilp32 -o program.o program.s
riscv-none-elf-objcopy -O binary program.o program.bin
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

**Memory:** 64 KB byte-addressable memory with 8, 16, and 32-bit load/store support.

**Registers:** 32 general-purpose registers with hardware-accurate `x0` zero-register enforcement.

**Debug mode:** Set `cpu.Debug = true` to enable step-by-step instruction logging.

## Architecture

The emulator is built around three classes:

- `Memory` — manages a 65,536-byte array with bounds-checked read/write operations
- `Registers` — a 32-entry register file that enforces the `x0 = 0` rule
- `CPU` — the execution engine that runs the fetch-decode-execute cycle

The CPU follows the standard fetch-decode-execute cycle:

1. **Fetch** — reads a 32-bit instruction from memory at the current PC
2. **Decode** — extracts the opcode, registers, and immediates using bitwise masking
3. **Execute** — performs the operation and updates the PC, registers, or memory

## Requirements

- [.NET SDK](https://dotnet.microsoft.com/download) (6.0 or later)
- [xpack-riscv-none-elf-gcc](https://github.com/xpack-binutils/riscv-none-elf-gcc/releases) to assemble `.s` files into binaries
