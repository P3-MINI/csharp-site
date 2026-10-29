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

const char* imgf_error_message(int32_t code)
{
    switch (code)
    {
    case IMGF_OK: return "success";
    case IMGF_ERR_NULL_POINTER: return "null pointer";
    case IMGF_ERR_INVALID_ARGUMENT: return "invalid argument";
    case IMGF_ERR_BUFFER_TOO_SMALL: return "pixel buffer is too small";
    case IMGF_ERR_CHANNELS: return "unsupported number of channels";
    case IMGF_ERR_OUT_OF_MEMORY: return "out of memory";
    default: return "unknown error";
    }
}

static size_t pixel_count(const imgf_info* info)
{
    return (size_t)info->width * (size_t)info->height;
}

static int32_t validate(const uint8_t* pixels, size_t length, const imgf_info* info)
{
    if (pixels == NULL || info == NULL)
        return IMGF_ERR_NULL_POINTER;
    if (info->width <= 0 || info->height <= 0)
        return IMGF_ERR_INVALID_ARGUMENT;
    if (info->channels < 1 || info->channels > 4)
        return IMGF_ERR_CHANNELS;
    /* Native code cannot check the size of the buffer - the caller has to pass it */
    if (length < pixel_count(info) * (size_t)info->channels)
        return IMGF_ERR_BUFFER_TOO_SMALL;
    return IMGF_OK;
}

int32_t imgf_invert(uint8_t* pixels, size_t length, const imgf_info* info)
{
    int32_t error = validate(pixels, length, info);
    if (error != IMGF_OK)
        return error;

    size_t count = pixel_count(info) * (size_t)info->channels;
    for (size_t i = 0; i < count; i++)
        pixels[i] = (uint8_t)(255 - pixels[i]);
    return IMGF_OK;
}

int32_t imgf_grayscale(uint8_t* pixels, size_t length, const imgf_info* info)
{
    int32_t error = validate(pixels, length, info);
    if (error != IMGF_OK)
        return error;
    if (info->channels < 3)
        return IMGF_ERR_CHANNELS;

    /* Local copies: a store through uint8_t* may alias *info, which prevents optimizations */
    const size_t count = pixel_count(info), channels = (size_t)info->channels;
    for (size_t i = 0; i < count; i++)
    {
        uint8_t* p = pixels + i * channels;
        /* Luma (ITU-R BT.601) with integer weights: 0.299, 0.587, 0.114 scaled by 256 */
        uint8_t gray = (uint8_t)((77 * p[0] + 150 * p[1] + 29 * p[2]) >> 8);
        p[0] = p[1] = p[2] = gray;
    }
    return IMGF_OK;
}

int32_t imgf_compute_stats(const uint8_t* pixels, size_t length, const imgf_info* info,
                           imgf_stats* stats)
{
    int32_t error = validate(pixels, length, info);
    if (error != IMGF_OK)
        return error;
    if (stats == NULL)
        return IMGF_ERR_NULL_POINTER;

    size_t count = pixel_count(info) * (size_t)info->channels;
    uint8_t min = 255, max = 0;
    uint64_t sum = 0;
    for (size_t i = 0; i < count; i++)
    {
        uint8_t value = pixels[i];
        if (value < min)
            min = value;
        if (value > max)
            max = value;
        sum += value;
    }
    stats->min = min;
    stats->max = max;
    stats->mean = (double)sum / (double)count;
    return IMGF_OK;
}
