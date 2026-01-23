#include "pattern.h"
#include <math.h>
#include <stdlib.h>
#include <time.h>

pattern_t* pattern_init(int width, int height)
{
    pattern_t* p = (pattern_t*)malloc(sizeof(pattern_t) +
                                      sizeof(color_t) * width * height);
    if (p == NULL)
        return p;
    p->height = height;
    p->width = width;
}

void pattern_populate(pattern_t* pattern, point_t* points, int point_count)
{
    srand(time(NULL));
    for (int y = 0; y < pattern->height; y++)
        for (int x = 0; x < pattern->width; x++)
        {
            point_t p = {x * 1.0 / pattern->width, y * 1.0 / pattern->height};

            int id1 = rand() % point_count;
            point_t dp1 = {p.x - points[id1].x, p.y - points[id1].y};
            double score1 = dp1.x * dp1.x + dp1.y * dp1.y;

            pattern->values[x + y * pattern->width].r =
                (unsigned char)(fmax(1 - score1 * 64, 0) * 255);
            pattern->values[x + y * pattern->width].g =
                (unsigned char)(fmax(0.75 - score1 * 32, 0) * 255);
            pattern->values[x + y * pattern->width].b =
                (unsigned char)(fmax(0.75 - score1 * 32, 0) * 255);
        }
}
void pattern_enstripen(pattern_t* pattern, stripe_settings_t* settings)
{
    if (settings->slope == -1)
        return;
    for (int y = 0; y < pattern->height; y++)
        for (int x = 0; x < pattern->width; x++)
        {
            point_t p = {x * 1.0 / pattern->width, y * 1.0 / pattern->height};

            double f =
                fmod(p.x + p.y * settings->slope,
                     (settings->stripe_a_width + settings->stripe_b_width) *
                         (1 + settings->slope));
            color_t* c =
                (f < settings->stripe_a_width * (1 + settings->slope)) ? &settings->a : &settings->b;
            pattern->values[x + y * pattern->width].r = c->r;
            pattern->values[x + y * pattern->width].g = c->g;
            pattern->values[x + y * pattern->width].b = c->b;
        }
}
void pattern_destroy(pattern_t* pattern) { free(pattern); }
