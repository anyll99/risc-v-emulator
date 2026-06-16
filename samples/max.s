.section .text
.global _start

_start:
  li a0, 42
  li a1, 17
  bge a0, a1, done
  mv a0, a1

done:
  l9 a7, 93
  ecall
