/*
 * PixelArtRemaster.cs - Conservative pixel-art detail pass
 *
 * This is an original runtime enhancement for freeserf.net. It does not
 * replace or redistribute any of the original game artwork.
 */

namespace Freeserf.Renderer
{
    internal static class PixelArtRemaster
    {
        // Edge-preserving micro-contrast. Neighbours across transparent gaps
        // and strong colour boundaries are ignored to protect sprite outlines.
        internal static void EnhanceRgba(byte[] pixels, int width, int height)
        {
            if (pixels == null || width < 3 || height < 3 ||
                pixels.Length != width * height * 4)
                return;

            byte[] source = (byte[])pixels.Clone();
            const float strength = 0.20f;
            const int luminanceLimit = 36;

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int p = (y * width + x) * 4;
                    if (source[p + 3] < 240)
                        continue;

                    int[] neighbours =
                    {
                        p - 4, p + 4, p - width * 4, p + width * 4
                    };

                    float sumR = 0, sumG = 0, sumB = 0;
                    int count = 0;
                    int centerLuma = Luma(source[p], source[p + 1], source[p + 2]);

                    for (int i = 0; i < neighbours.Length; i++)
                    {
                        int n = neighbours[i];
                        if (source[n + 3] < 240)
                            continue;

                        int neighbourLuma = Luma(source[n], source[n + 1], source[n + 2]);
                        if (System.Math.Abs(centerLuma - neighbourLuma) > luminanceLimit)
                            continue;

                        sumR += source[n];
                        sumG += source[n + 1];
                        sumB += source[n + 2];
                        count++;
                    }

                    if (count < 2)
                        continue;

                    pixels[p] = Adjust(source[p], sumR / count, strength);
                    pixels[p + 1] = Adjust(source[p + 1], sumG / count, strength);
                    pixels[p + 2] = Adjust(source[p + 2], sumB / count, strength);
                }
            }
        }

        static int Luma(byte r, byte g, byte b)
        {
            return (r * 77 + g * 150 + b * 29) >> 8;
        }

        static byte Adjust(byte center, float neighbourAverage, float strength)
        {
            int value = (int)System.Math.Round(center + (center - neighbourAverage) * strength);
            return (byte)System.Math.Max(0, System.Math.Min(255, value));
        }
    }
}
