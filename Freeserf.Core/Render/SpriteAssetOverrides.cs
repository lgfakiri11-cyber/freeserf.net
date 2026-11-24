/*
 * SpriteAssetOverrides.cs
 *
 * Built-in original pixel-art replacements for map nature sprites, with optional
 * external PNG overrides. Generated sprites retain the source canvas dimensions
 * so the original placement metadata and animation timing remain valid.
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

        public static Sprite Apply(Data.Resource resource, uint index, Sprite original)
        {
            if (original == null) return null;
            string key = resource + "/" + index.ToString("D3");
            if (Cache.TryGetValue(key, out var cached)) return cached;

            // External artwork takes precedence. It is resampled to the native
            // canvas instead of being silently rejected for a different size.
            string file = Path.Combine(Root, resource.ToString().ToLowerInvariant(), index.ToString("D3") + ".png");
            if (File.Exists(file))
            {
                try
                {
                    using (var loaded = Sprite.CreateFromFile(file))
                    {
                        var fitted = Fit(loaded, original.Width, original.Height);
                        Cache[key] = fitted;
                        return fitted;
                    }
                }
                catch { /* Keep the game bootable if an optional PNG is invalid. */ }
            }

            // First native remaster pass: all 24 animated tree frames/types.
            if (resource == Data.Resource.MapObject && index < 24)
            {
                var generated = NatureSprite(index, original.Width, original.Height);
                Cache[key] = generated;
                return generated;
            }

            Missing.Add(key);
            return original;
        }

        private static Sprite Fit(Sprite source, uint width, uint height)
        {
            if (source.Width == width && source.Height == height)
                return Sprite.CreateFromData(width, height, source.GetData());

            var pixels = new Sprite.Color[width * height];
            var src = source.GetData();
            for (int y = 0; y < (int)height; y++)
            {
                int sy = Math.Min((int)source.Height - 1, y * (int)source.Height / (int)height);
                for (int x = 0; x < (int)width; x++)
                {
                    int sx = Math.Min((int)source.Width - 1, x * (int)source.Width / (int)width);
                    int p = (sy * (int)source.Width + sx) * 4;
                    pixels[y * (int)width + x] = new Sprite.Color { Blue=src[p], Green=src[p+1], Red=src[p+2], Alpha=src[p+3] };
                }
            }
            return Sprite.CreateFromPixels(width, height, pixels);
        }

        private static Sprite NatureSprite(uint index, uint width, uint height)
        {
            int w = (int)width, h = (int)height;
            var px = new Sprite.Color[w * h];
            var rng = new Random((int)index * 7919 + 17);
            int variant = (int)(index / 8);
            int frame = (int)(index % 8);
            int cx = w / 2;
            int ground = Math.Max(1, h - Math.Max(2, h / 9));
            int crownW = Math.Max(3, w * (variant == 1 ? 42 : 36) / 100);
            int crownH = Math.Max(3, h * (variant == 1 ? 58 : 55) / 100);
            int crownY = Math.Max(0, h * 5 / 100);

            if (variant == 2) // palm / broad tropical tree
            {
                Line(px,w,h,cx,ground,cx + w/18,crownY+crownH/2,new Sprite.Color{Red=105,Green=65,Blue=34,Alpha=255},Math.Max(1,w/18));
                for(int b=0;b<9;b++)
                {
                    double a=(-Math.PI/2)+(b-4)*0.34;
                    int ex=cx+(int)(Math.Cos(a)*crownW*1.05), ey=crownY+crownH/2+(int)(Math.Sin(a)*crownH*0.55);
                    Line(px,w,h,cx,crownY+crownH/2,ex,ey,new Sprite.Color{Red=38,Green=125,Blue=43,Alpha=255},Math.Max(1,w/30));
                    for(int k=0;k<5;k++){int lx=cx+(ex-cx)*k/5,ly=crownY+crownH/2+(ey-(crownY+crownH/2))*k/5; Dot(px,w,h,lx,ly,new Sprite.Color{Red=78,Green=170,Blue=47,Alpha=255});}
                }
            }
            else
            {
                // trunk and branching structure
                var trunk = new Sprite.Color{Red=105,Green=63,Blue=35,Alpha=255};
                Line(px,w,h,cx,ground,cx + (frame%3)-1,crownY+crownH/2,trunk,Math.Max(2,w/12));
                Line(px,w,h,cx,crownY+crownH*3/4,cx-crownW/2,crownY+crownH/2,new Sprite.Color{Red=122,Green=76,Blue=40,Alpha=255},Math.Max(1,w/24));
                Line(px,w,h,cx,crownY+crownH*3/4,cx+crownW/2,crownY+crownH/2,new Sprite.Color{Red=122,Green=76,Blue=40,Alpha=255},Math.Max(1,w/24));

                int clusters = variant==0 ? 42 : 58;
                for(int n=0;n<clusters;n++)
                {
                    double a=rng.NextDouble()*Math.PI*2;
                    double r=Math.Sqrt(rng.NextDouble());
                    int x=cx+(int)(Math.Cos(a)*crownW*r);
                    int y=crownY+crownH/2+(int)(Math.Sin(a)*crownH*0.48*r);
                    int rad=Math.Max(1,Math.Min(w,h)/18+rng.Next(Math.Max(1,Math.Min(w,h)/25+1)));
                    var baseCol = variant==1
                        ? new Sprite.Color{Red=variant==1?42:(byte)36,Green=variant==1?115:(byte)95,Blue=35,Alpha=255}
                        : new Sprite.Color{Red=36,Green=variant==0?105:(byte)82,Blue=31,Alpha=255};
                    if(variant==1) baseCol=new Sprite.Color{Red=52,Green=105,Blue=35,Alpha=255};
                    if(variant==2) baseCol=new Sprite.Color{Red=175,Green=104,Blue=35,Alpha=255};
                    int shade=rng.Next(-22,28);
                    var col=new Sprite.Color{Red=Clamp(baseCol.Red+shade),Green=Clamp(baseCol.Green+shade),Blue=Clamp(baseCol.Blue+shade),Alpha=255};
                    Ellipse(px,w,h,x,y,rad,Math.Max(1,rad*3/4),col);
                    if(rng.Next(3)==0) Dot(px,w,h,x-rad/3,y-rad/3,new Sprite.Color{Red=150,Green=variant==2?120:(byte)190,Blue=55,Alpha=255});
                }
                // a few crisp highlights and small fruit accents
                for(int n=0;n<Math.Max(3,w/5);n++)
                {
                    int x=cx+rng.Next(-crownW,crownW+1), y=crownY+crownH/2+rng.Next(-crownH/2,crownH/2+1);
                    if(Inside(x,y,w,h) && px[y*w+x].Alpha!=0)
                        Dot(px,w,h,x,y,new Sprite.Color{Red=variant==2?235:(byte)135,Green=variant==2?125:(byte)195,Blue=variant==2?30:(byte)62,Alpha=255});
                }
            }

            // ground contact shadow and small root/grass accents
            Ellipse(px,w,h,cx,ground,Math.Max(2,w/5),Math.Max(1,h/22),new Sprite.Color{Red=37,Green=48,Blue=31,Alpha=150});
            for(int i=0;i<Math.Max(4,w/3);i++)
            {
                int x=cx+rng.Next(-Math.Max(2,w/5),Math.Max(3,w/5));
                int y=ground-rng.Next(0,Math.Max(1,h/18));
                Dot(px,w,h,x,y,new Sprite.Color{Red=74,Green=124,Blue=43,Alpha=255});
            }
            return Sprite.CreateFromPixels(width,height,px);
        }

        private static byte Clamp(int v) => (byte)Math.Max(0,Math.Min(255,v));
        private static bool Inside(int x,int y,int w,int h) => x>=0 && y>=0 && x<w && y<h;
        private static void Dot(Sprite.Color[] p,int w,int h,int x,int y,Sprite.Color c){if(Inside(x,y,w,h))p[y*w+x]=c;}
        private static void Ellipse(Sprite.Color[] p,int w,int h,int cx,int cy,int rx,int ry,Sprite.Color c)
        {
            if(rx<1)rx=1;if(ry<1)ry=1;
            for(int y=-ry;y<=ry;y++)for(int x=-rx;x<=rx;x++)
                if((x*x)/(double)(rx*rx)+(y*y)/(double)(ry*ry)<=1.0)Dot(p,w,h,cx+x,cy+y,c);
        }
        private static void Line(Sprite.Color[] p,int w,int h,int x0,int y0,int x1,int y1,Sprite.Color c,int thick)
        {
            int dx=Math.Abs(x1-x0),sx=x0<x1?1:-1,dy=-Math.Abs(y1-y0),sy=y0<y1?1:-1,err=dx+dy;
            while(true){for(int oy=-thick/2;oy<=thick/2;oy++)for(int ox=-thick/2;ox<=thick/2;ox++)Dot(p,w,h,x0+ox,y0+oy,c);if(x0==x1&&y0==y1)break;int e2=2*err;if(e2>=dy){err+=dy;x0+=sx;}if(e2<=dx){err+=dx;y0+=sy;}}
        }
    }
}
