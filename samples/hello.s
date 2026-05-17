.section .text
.global _start

_start:
    li      a7, 64              # syscall: write
    li      a0, 1               # fd: stdout
    la      a1, msg             # buf: address of message
    li      a2, 14              # count: length of "Hello, World!\n"
    ecall

    li      a7, 93              # syscall: exit
    li      a0, 0               # exit code 0
    ecall

msg:
    .ascii  "Hello, World!\n"
