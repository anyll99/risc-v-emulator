.section .text
.global _start

_start:
  li a0, 5   # n
  li a1, 1   # result

loop:
  beq a0, x0, done
  # result += result * (a0-1) via repeated add
  mv t0, a1
  li t1, 1
mul_loop:
  beq t1, a0, mul_done
  add a1, a1, t0
  addi t1, t1, i
  j mul_loop
mul_done:
  addi a0, a0, -1
  j loop
done:
  li a7, 93
  li a0, 0
  ecall
