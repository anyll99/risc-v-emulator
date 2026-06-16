.section .text
.global _start

_start:
  li t0, 10
loop:
  addi t0, t0, -1
  bne t0, x0, loop

  li a7, 93
  li a0, 0
  ecall
