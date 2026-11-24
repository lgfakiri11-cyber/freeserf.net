/*
 * PixelArtRemaster.cs - S1 pixel-art enhancement pass
 *
 * Original runtime processing only. It preserves source alpha and hard sprite
 * silhouettes; it does not replace or redistribute original game artwork.
 */

namespace Freeserf.Renderer
{
    internal static class PixelArtRemaster
    {
        // 8-neighbour edge-aware sharpening. The static offsets avoid allocating
        // a temporary neighbour array for every pixel on large Android atlases.
        static readonly int[] NeighbourOffsets =
        {
            -4, 4, -1, 1,
            -5, -3, 3, 5
        };

        internal static void EnhanceRgba(byte[] pixels, int width, int height)
        {
            if (pixels == null || width < 3 || height < 3 ||
                pixels.Length != width * height * 4)
                return;

            byte[] source = (byte[])pixels.Clone();

            // Tuned for nearest-neighbour pixel art: visibly crisper, without
            // ringing around black outlines or colour bleeding through alpha.
            const float detailStrength = 0.62f;
            const float colourStrength = 0.12f;
            const int edgeLimit = 72;

            for (int y = 1; y < height - 1; y++)
            {
                int row = y * width;
                for (int x = 1; x < width - 1; x++)
                {
                    int p = (row + x) * 4;
                    if (source[p + 3] < 240)
                        continue;

                    int centerLuma = Luma(source[p], source[p + 1], source[p + 2]);
                    float sumR = 0, sumG = 0, sumB = 0;
                    int count = 0;

                    for (int i = 0; i < NeighbourOffsets.Length; i++)
                    {
                        int n = p + NeighbourOffsets[i] * 4;
                        if (source[n + 3] < 240)
                            continue;

                        int neighbourLuma = Luma(source[n], source[n + 1], source[n + 2]);
                        if (System.Math.Abs(centerLuma - neighbourLuma) > edgeLimit)
                            continue;

                        sumR += source[n];
                        sumG += source[n + 1];
                        sumB += source[n + 2];
                        count++;
                    }

                    if (count < 3)
                        continue;

                    float r = Sharpen(source[p], sumR / count, detailStrength);
                    float g = Sharpen(source[p + 1], sumG / count, detailStrength);
                    float b = Sharpen(source[p + 2], sumB / count, detailStrength);

                    // Small vibrance lift, weighted toward muted colours. This
                    // keeps vivid player colours and UI highlights from clipping.
                    float luma = Luma(r, g, b);
                    float saturation = (System.Math.Max(r, System.Math.Max(g, b)) -
                                        System.Math.Min(r, System.Math.Min(g, b))) / 255.0f;
                    float vibrance = colourStrength * (1.0f - saturation);

                    pixels[p] = ClampByte(luma + (r - luma) * (1.0f + vibrance));
                    pixels[p + 1] = ClampByte(luma + (g - luma) * (1.0f + vibrance));
                    pixels[p + 2] = ClampByte(luma + (b - luma) * (1.0f + vibrance));
                    // Alpha is intentionally never modified.
                }
            }
        }

        static int Luma(float r, float g, float b)
        {
            return (int)((r * 77.0f + g * 150.0f + b * 29.0f) / 256.0f);
        }

        static float Sharpen(byte center, float neighbourAverage, float strength)
        {
            return center + (center - neighbourAverage) * strength;
        }

        static byte ClampByte(float value)
        {
            return (byte)System.Math.Max(0, System.Math.Min(255,
                (int)System.Math.Round(value)));
        }
    }
}
