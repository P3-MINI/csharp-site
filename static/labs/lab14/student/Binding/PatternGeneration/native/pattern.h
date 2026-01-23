#include "color.h"
#include "point.h"

#ifdef _WIN32
#ifdef LIBRARY_EXPORT
#define LIBRARY __declspec(dllexport)
#else
#define LIBRARY __declspec(dllimport)
#endif
#else
#define LIBRARY
#endif

typedef struct pattern
{
    int width, height;
    color_t values[];
} pattern_t;

typedef struct stripe_settings
{
    color_t a, b;
    double stripe_a_width, stripe_b_width;
    double slope;
} stripe_settings_t;

LIBRARY pattern_t* pattern_init(int width, int height);
LIBRARY void pattern_populate(pattern_t* pattern, point_t* points,
                              int point_count);
LIBRARY void pattern_enstripen(pattern_t* pattern, stripe_settings_t* settings);
LIBRARY void pattern_destroy(pattern_t* pattern);
