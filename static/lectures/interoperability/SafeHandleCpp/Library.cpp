#include "Library.h"
#include <cstring>
#include <print>

const char *CreateString()
{
    std::println("Creating a C string");
    const char str[] = "C string";
    size_t length = std::strlen(str);
    char* dup = new char[length + 1];
    std::strcpy(dup, str);
    return dup;
}

void PrintString(const char *str)
{
    std::println("The C string is: '{}'", str);
}

void DestroyString(const char *str)
{
    std::println("Destroying a C string");
    delete[] str;
}
