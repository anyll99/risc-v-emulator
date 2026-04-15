# risc-v emulator

A lightweight, low-level **RV32I (Base Integer Instruction Set)** interpreter written in C#.

Simulates the core architecture of a 32-bit RISC-V CPU, which fetches, decodes and executes instructions in a virtual environment.

## Key Features

* **Complete RV32I Execution:**
  Implements the base integer instruction set, including arithmetic, logical, memory, and control flow operations
  
* **Virtual Register File:**
  Implements 32 general-purpose registers with hardware accurate 'x0' zero-register logic.
  
* **Simulated Memory:**
  A 64 KB byte-addressable memory system supporting 8, 16 and 32 bit Load/Store operations.
  
* **Instruction Categories:**
  * **R-Type:** `ADD`, `SUB`, `AND`, `OR`, `XOR`, shifts, comparisons
  * **I-Type:** Immediate arithmetic, loads, shifts
  * **S-Type:** Store instructions
  * **B-Type** Conditional Branching (`BEQ`, `BNE`, etc.)
  * **U-Type** `LUI`, `AUIPC`
  * **J-Type** `JAL`, `JALR`
 
 * **Instruction Categories:**
  * Basic `ECALL` support (exit, write)
  * `EBREAK` handling for debugging

 * **Debug mode**
  * Step-by-step execution tracking
  * Instruction-level logging 

## Internal Architecture

The emulator is built around three core modules:

1. **The Memory Controller:** Manages a 65,536-byte array, handling the bit-shifting required to read/write across byte-aligned boundaries.
2. **The Register File:** A storage array that enforces RISC-V rules, such as ensuring 'x0' always remains 0.
3. **The Execution Engine:** A cycle-accurate interpreter that decodes raw machine code into operations using bitwise shifting and masking.

 ## How it works?

 The CPU follows the standart Fetch-Decode-Execute cycle:
 1. **Fetch** Pulls a 32-bit instruction from memory.
 2. **Decode** Breaks the instruction into opcode, registers and immediates.
 3. **Execute** Performs the logic and updates the CPU state (PC, Registers or Memory).
