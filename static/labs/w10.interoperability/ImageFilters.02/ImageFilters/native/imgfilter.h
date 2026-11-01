#ifndef IMGFILTER_H
#define IMGFILTER_H

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

/* Basic functions */
IMGF_API int32_t imgf_add(int32_t a, int32_t b);
/* Returns a static string - the caller must NOT free it */
IMGF_API const char* imgf_version(void);
/* Returns a newly allocated string - the caller must free it with imgf_free */
IMGF_API char* imgf_build_info(void);
IMGF_API void imgf_free(void* ptr);

#ifdef __cplusplus
}
#endif

#endif /* IMGFILTER_H */
