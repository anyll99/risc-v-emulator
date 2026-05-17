.section .text
.global _start

# Computes the 10th Fibonacci number iteratively.
# Result (55) will be visible in register a0 on the register dump.

_start:
    li      t0, 0               # a = fib(0) = 0
    li      t1, 1               # b = fib(1) = 1
    li      t2, 10              # counter = 10

loop:
    beqz    t2, done            # if counter == 0, done
    add     t3, t0, t1          # next = a + b
    mv      t0, t1              # a = b
    mv      t1, t3              # b = next
    addi    t2, t2, -1          # counter--
    j       loop

done:
    mv      a0, t0              # a0 = fib(10) = 55
    li      a7, 93              # syscall: exit
    ecall
