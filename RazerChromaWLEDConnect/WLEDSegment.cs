// Licensed to the Chroma Control Contributors under one or more agreements.
// The Chroma Control Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChromaBroadcastSampleApplication
{
    internal class WLEDSegment
    {
        public int[][] i;
        public byte[] b;
        public WLEDSegment(float len, int[] color1, int[] color2, int[] color3, int[] color4)
        {
            i = new int[(int)len][];
            b = new byte[(int)len * 4 + 2];

            // TODO: Speed this up if all the same color
            b[0] = 1;
            b[1] = 1;

            var ledsPerColor = Math.Ceiling(len / 3);
            for (int led = 0; led < len; led++)
            {
                int[] diffColor = new int[3];
                if (led < ledsPerColor)
                {
                    var rDiff = (color2[0] - color1[0]) / ledsPerColor;
                    var gDiff = (color2[1] - color1[1]) / ledsPerColor;
                    var bDiff = (color2[2] - color1[2]) / ledsPerColor;

                    // determine how many times we need to add the diff

                    diffColor[0] = color1[0] + (int)Math.Round(rDiff * led);
                    diffColor[1] = color1[1] + (int)Math.Round(gDiff * led);
                    diffColor[2] = color1[2] + (int)Math.Round(bDiff * led);

                    i[led] = diffColor;
                }
                else if (led < ledsPerColor * 2)
                {
                    var rDiff = (color3[0] - color2[0]) / ledsPerColor;
                    var gDiff = (color3[1] - color2[1]) / ledsPerColor;
                    var bDiff = (color3[2] - color2[2]) / ledsPerColor;

                    // determine how many times we need to add the diff
                    diffColor[0] = color2[0] + (int)Math.Round(rDiff * (led - ledsPerColor));
                    diffColor[1] = color2[1] + (int)Math.Round(gDiff * (led - ledsPerColor));
                    diffColor[2] = color2[2] + (int)Math.Round(bDiff * (led - ledsPerColor));

                    i[led] = diffColor;
                }
                else if (led < ledsPerColor * 3)
                {
                    var rDiff = (color4[0] - color3[0]) / ledsPerColor;
                    var gDiff = (color4[1] - color3[1]) / ledsPerColor;
                    var bDiff = (color4[2] - color3[2]) / ledsPerColor;

                    // determine how many times we need to add the diff
                    diffColor[0] = color3[0] + (int)Math.Round(rDiff * (led - ledsPerColor * 2));
                    diffColor[1] = color3[1] + (int)Math.Round(gDiff * (led - ledsPerColor * 2));
                    diffColor[2] = color3[2] + (int)Math.Round(bDiff * (led - ledsPerColor * 2));

                    i[led] = diffColor;
                }
                else
                {
                    i[led] = color4;
                }

                b[2 + led * 4] = (byte)led;
                b[3 + led * 4] = (byte)diffColor[0];
                b[4 + led * 4] = (byte)diffColor[1];
                b[5 + led * 4] = (byte)diffColor[2];
            }
        }
    }
}
