#ifndef IMGFILTER_H
#define IMGFILTER_H

#include <stddef.h>
#include <stdint.h>

/*
 * IMGF_API marks functions exported from the shared library.
 * - MSVC: symbols are hidden unless marked with __declspec(dllexport).
 *   IMGFILTER_EXPORTS is defined only while building the DLL itself (in imgfilter.c);
 *   C/C++ code using the DLL gets __declspec(dllimport).
 * - GCC/Clang: we compile with -fvisibility=hidden, so only symbols marked
 *   with visibility("default") are exported.
 */
#if defined(_WIN32)
#  if defined(IMGFILTER_EXPORTS)
#    define IMGF_API __declspec(dllexport)
#  else
#    define IMGF_API __declspec(dllimport)
#  endif
#else
#  define IMGF_API __attribute__((visibility("default")))
#endif

#ifdef __cplusplus
extern "C" {
#endif

/* Error codes returned by the filter functions */
#define IMGF_OK                    0
#define IMGF_ERR_NULL_POINTER     -1
#define IMGF_ERR_INVALID_ARGUMENT -2
#define IMGF_ERR_BUFFER_TOO_SMALL -3
#define IMGF_ERR_CHANNELS         -4
#define IMGF_ERR_OUT_OF_MEMORY    -5

/* Basic functions */
IMGF_API int32_t imgf_add(int32_t a, int32_t b);
/* Returns a static string - the caller must NOT free it */
IMGF_API const char* imgf_version(void);
/* Returns a newly allocated string - the caller must free it with imgf_free */
IMGF_API char* imgf_build_info(void);
IMGF_API void imgf_free(void* ptr);
/* Returns a static description of an error code */
IMGF_API const char* imgf_error_message(int32_t code);

/* Image description: pixels are stored row by row, `channels` bytes per pixel */
typedef struct imgf_info
{
    int32_t width;
    int32_t height;
    int32_t channels;
} imgf_info;

typedef struct imgf_stats
{
    uint8_t min;
    uint8_t max;
    double mean;
} imgf_stats;

/* Filters working on a pixel buffer owned by the caller */
IMGF_API int32_t imgf_invert(uint8_t* pixels, size_t length, const imgf_info* info);
IMGF_API int32_t imgf_grayscale(uint8_t* pixels, size_t length, const imgf_info* info);
IMGF_API int32_t imgf_compute_stats(const uint8_t* pixels, size_t length, const imgf_info* info,
                                    imgf_stats* stats);

#ifdef __cplusplus
}
#endif

#endif /* IMGFILTER_H */
