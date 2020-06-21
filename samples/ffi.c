#include <stdio.h>
#include <stdint.h>
#include <stdbool.h>

void print_int32(int32_t i) {
    printf("%d\n", i);
}

void print_bool(bool b) {
    printf("%s\n", b ? "true" : "false");
}
