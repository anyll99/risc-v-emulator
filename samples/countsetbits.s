.section .text
.global _start

_start:
  li t0, 0b10110101   # I set the input as 181 which has 5 set bits
  li t1, 0
  li t2, 32

loop:
  andi t3, t0, 1
  add t1, t1, t3
  srli t0, t0, 1
  addi t2, t2, -1
  bne t2, x0, loop

  li a7, 93
  li a0, 0
  ecall
