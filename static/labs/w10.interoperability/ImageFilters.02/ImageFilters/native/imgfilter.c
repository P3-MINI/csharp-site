/* We are building the library itself: IMGF_API means "export" (see imgfilter.h) */
#define IMGFILTER_EXPORTS
#include "imgfilter.h"

#include <stdio.h>
#include <stdlib.h>

#define IMGF_VERSION "imgfilter 1.0"

#define STRINGIFY_(x) #x
#define STRINGIFY(x) STRINGIFY_(x)

#if defined(__clang__)
#  define IMGF_COMPILER "Clang " __clang_version__
#elif defined(_MSC_VER)
#  define IMGF_COMPILER "MSVC " STRINGIFY(_MSC_VER)
#elif defined(__GNUC__)
#  define IMGF_COMPILER "GCC " __VERSION__
#else
#  define IMGF_COMPILER "unknown compiler"
#endif

#define IMGF_BUILD_INFO_FORMAT "%s, %s, %d-bit"

int32_t imgf_add(int32_t a, int32_t b)
{
    return a + b;
}

const char* imgf_version(void)
{
    /* String literal with static storage duration - owned by the library */
    return IMGF_VERSION;
}

char* imgf_build_info(void)
{
    int bits = (int)(sizeof(void*) * 8);
    /* First call computes the required length, second call formats the text */
    int length = snprintf(NULL, 0, IMGF_BUILD_INFO_FORMAT, IMGF_VERSION, IMGF_COMPILER, bits);
    if (length < 0)
        return NULL;
    char* result = malloc((size_t)length + 1);
    if (result != NULL)
        snprintf(result, (size_t)length + 1, IMGF_BUILD_INFO_FORMAT, IMGF_VERSION, IMGF_COMPILER, bits);
    return result;
}

void imgf_free(void* ptr)
{
    /* Memory must be released by the same allocator (C runtime) that allocated it */
    free(ptr);
}
