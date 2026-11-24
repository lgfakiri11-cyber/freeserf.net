/*
 * SpriteAssetOverrides.cs
 *
 * Optional external PNG overrides for original game sprites.
 * Files are loaded only when their dimensions exactly match the source sprite.
 * Original sprite metadata (offsets/baselines) remains authoritative in DataSource.
 */
using Freeserf.Data;
using Data = Freeserf.Data.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace Freeserf.Render
{
    internal static class SpriteAssetOverrides
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> Missing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static string Root
        {
            get
            {
                var configured = Environment.GetEnvironmentVariable("FREESERF_GRAPHICS_DIR");
                return string.IsNullOrWhiteSpace(configured)
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Freeserf", "graphics")
                    : configured;
            }
        }

        // Naming: <root>/<resource>/<index:D3>.png, e.g. map_object/012.png.
        public static Sprite Apply(Data.Resource resource, uint index, Sprite original)
        {
            if (original == null)
                return null;

            string key = resource + "/" + index.ToString("D3");
            if (Cache.TryGetValue(key, out var cached))
                return cached;
            if (Missing.Contains(key))
                return original;

            string file = Path.Combine(Root, resource.ToString().ToLowerInvariant(), index.ToString("D3") + ".png");
            if (!File.Exists(file))
            {
                Missing.Add(key);
                return original;
            }

            try
            {
                var replacement = Sprite.CreateFromFile(file);
                if (replacement.Width != original.Width || replacement.Height != original.Height)
                {
                    replacement.Dispose();
                    Missing.Add(key);
                    return original;
                }

                Cache.Add(key, replacement);
                return replacement;
            }
            catch
            {
                // A malformed or unsupported override must never prevent the game from starting.
                Missing.Add(key);
                return original;
            }
        }
    }
}
