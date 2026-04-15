# risc-v emulator

A lightweight, low-level **RV32I (Base Integer Instruction Set)** interpreter written in C#.

Simulates the core architecture of a 32-bit RISC-V CPU, which fetches, decodes and executes instructions in a virtual environment.

## Key Features

* **Complete RV32I Execution:** Supports the fundamental 32-bit instruction set.
* **Virtual Register File:** Implements 32 general-purpose registers with hardware accurate 'x0' zero-register logic.
* **Simulated Memory:** A 64 KB byte-addressable memory system supporting 8, 16 and 32 bit Load/Store operations.
* **Instruction Pipeline logic:**
  * **R-Type:** Arithmetic and logical operations such as (ADD, SUB, AND, OR, etc.)
  * **I-Type:** Immediate math and Memory Loads
  * **S-Type:** Memory Stores.
  * **B-Type** Conditional Branching (loops and if-statements)

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

## Current Status: Work in progress (4/14/2026)
- [x] Memory & Register Implementation
- [x] R-Type & I-Type Math
- [x] Load/Store Instructions
- [ ] Branching Logic (B-Type)
- [ ] Jump Instructions (J-Type)
