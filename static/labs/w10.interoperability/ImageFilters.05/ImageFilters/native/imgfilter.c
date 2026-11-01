/* We are building the library itself: IMGF_API means "export" (see imgfilter.h) */
#define IMGFILTER_EXPORTS
#include "imgfilter.h"

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

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

/* The layout of this struct is private to the library */
struct imgf_image
{
    imgf_info info;
    uint8_t* pixels;
};

static int32_t live_images = 0;

imgf_image* imgf_image_create(int32_t width, int32_t height, int32_t channels)
{
    if (width <= 0 || height <= 0 || channels < 1 || channels > 4)
        return NULL;

    imgf_image* image = malloc(sizeof *image);
    if (image == NULL)
        return NULL;
    image->info.width = width;
    image->info.height = height;
    image->info.channels = channels;
    image->pixels = calloc(pixel_count(&image->info), (size_t)channels);
    if (image->pixels == NULL)
    {
        free(image);
        return NULL;
    }
    live_images++;
    return image;
}

void imgf_image_destroy(imgf_image* image)
{
    if (image == NULL)
        return;
    free(image->pixels);
    free(image);
    live_images--;
}

imgf_info imgf_image_info(const imgf_image* image)
{
    imgf_info empty = { 0, 0, 0 };
    return image != NULL ? image->info : empty;
}

uint8_t* imgf_image_pixels(imgf_image* image)
{
    return image != NULL ? image->pixels : NULL;
}

int32_t imgf_image_convolve3x3(imgf_image* image, const int32_t kernel[9], int32_t divisor,
                               imgf_progress_fn progress)
{
    if (image == NULL || kernel == NULL)
        return IMGF_ERR_NULL_POINTER;
    if (divisor == 0)
        return IMGF_ERR_INVALID_ARGUMENT;

    const int32_t w = image->info.width, h = image->info.height;
    const size_t c = (size_t)image->info.channels;
    const size_t stride = (size_t)w * c;

    /* Local copy of the kernel: a store through uint8_t* may alias kernel[], which prevents optimizations */
    int32_t k[9];
    memcpy(k, kernel, sizeof k);

    /* Every output pixel needs the original neighbours, so we read from a copy */
    uint8_t* src = malloc(stride * (size_t)h);
    if (src == NULL)
        return IMGF_ERR_OUT_OF_MEMORY;
    memcpy(src, image->pixels, stride * (size_t)h);

    for (int32_t y = 0; y < h; y++)
    {
        /* Neighbouring rows, clamped to the image edges */
        const uint8_t* above = src + (size_t)(y > 0 ? y - 1 : 0) * stride;
        const uint8_t* row = src + (size_t)y * stride;
        const uint8_t* below = src + (size_t)(y < h - 1 ? y + 1 : h - 1) * stride;
        uint8_t* out = image->pixels + (size_t)y * stride;

        for (int32_t x = 0; x < w; x++)
        {
            /* Neighbouring columns (byte offsets), clamped to the image edges */
            size_t left = (size_t)(x > 0 ? x - 1 : 0) * c;
            size_t center = (size_t)x * c;
            size_t right = (size_t)(x < w - 1 ? x + 1 : w - 1) * c;

            for (size_t ch = 0; ch < c; ch++)
            {
                int32_t sum =
                    k[0] * above[left + ch] + k[1] * above[center + ch] + k[2] * above[right + ch] +
                    k[3] * row[left + ch]   + k[4] * row[center + ch]   + k[5] * row[right + ch] +
                    k[6] * below[left + ch] + k[7] * below[center + ch] + k[8] * below[right + ch];
                int32_t value = sum / divisor;
                out[center + ch] = (uint8_t)(value < 0 ? 0 : value > 255 ? 255 : value);
            }
        }

        if (progress != NULL)
            progress(y + 1, h);
    }

    free(src);
    return IMGF_OK;
}

int32_t imgf_live_images(void)
{
    return live_images;
}
