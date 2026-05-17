.section .text
.global _start

# Sums integers from 1 to 10.
# Result (55) will be visible in register a0 on the register dump.

_start:
    li      t0, 1               # i = 1
    li      t1, 10              # limit = 10
    li      a0, 0               # sum = 0

loop:
    add     a0, a0, t0          # sum += i
    addi    t0, t0, 1           # i++
    ble     t0, t1, loop        # if i <= 10, repeat

    li      a7, 93              # syscall: exit
    ecall
