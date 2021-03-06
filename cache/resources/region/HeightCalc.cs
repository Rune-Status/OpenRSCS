﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenRSCS.cache.resources.region {
    public class HeightCalc {

        private const int JAGEX_CIRCULAR_ANGLE = 2048;
        private const double ANGULAR_RATIO = 360D/JAGEX_CIRCULAR_ANGLE;
        private const double JAGEX_RADIAN = (Math.PI / 180) * ANGULAR_RATIO;
        private static readonly int[] SIN = new int[JAGEX_CIRCULAR_ANGLE];
        private static readonly int[] COS = new int[JAGEX_CIRCULAR_ANGLE];

        static HeightCalc() {
            for(int i = 0; i < JAGEX_CIRCULAR_ANGLE; i++) {
                SIN[i] = (int)(65536.0D*Math.Sin((double)i*JAGEX_RADIAN));
                COS[i] = (int)(65536.0D*Math.Cos((double)i*JAGEX_RADIAN));
            }
        }

        public static int calculate(int baseX, int baseY, int x, int y) {
            int xc = (baseX >> 3) + 932638 + x;
            int yc = (baseY >> 3) + 556190 + y;
            int n = interpolateNoise(xc + '\ub135', yc + 91923, 4) - 128 + (interpolateNoise(10294 + xc, yc + '\u93bd', 2) - 128 >> 1) + (interpolateNoise(xc, yc, 1) - 128 >> 2);
            n = 35 + (int)((double)n*0.3D);
            if(n >= 10) {
                if(n > 60) {
                    n = 60;
                }
            } else {
                n = 10;
            }
            return n;
        }

        private static int interpolateNoise(int x, int y, int frequency) {
            int intX = x/frequency;
            int fracX = x & frequency - 1;
            int intY = y/frequency;
            int fracY = y & frequency - 1;
            int v1 = smoothedNoise1(intX, intY);
            int v2 = smoothedNoise1(intX + 1, intY);
            int v3 = smoothedNoise1(intX, intY + 1);
            int v4 = smoothedNoise1(1 + intX, 1 + intY);
            int i1 = interpolate(v1, v2, fracX, frequency);
            int i2 = interpolate(v3, v4, fracX, frequency);
            return interpolate(i1, i2, fracY, frequency);
        }

        private static int smoothedNoise1(int x, int y) {
            int corners = noise(x - 1, y - 1) + noise(x + 1, y - 1) + noise(x - 1, 1 + y) + noise(x + 1, y + 1);
            int sides = noise(x - 1, y) + noise(1 + x, y) + noise(x, y - 1) + noise(x, 1 + y);
            int center = noise(x, y);
            return center/4 + sides/8 + corners/16;
        }

        private static int noise(int x, int y) {
            int n = x + y*57;
            n ^= n << 13;
            return ((n*(n*n*15731 + 789221) + 1376312589) & int.MaxValue) >> 19 & 255;
        }

        private static int interpolate(int a, int b, int x, int y) {
            int f = 65536 - COS[1024*x/y] >> 1;
            return (f*b >> 16) + (a*(65536 - f) >> 16);
        }

    }
}
