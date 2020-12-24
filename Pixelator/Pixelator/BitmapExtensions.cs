using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Linq;

namespace Pixelator
{
    static class BitmapExtension
    {
        public static Bitmap Clone(this Bitmap b)
        {
            Bitmap colorMap = new Bitmap(b.Width, b.Height);
            for (int i = 0; i < colorMap.Height; i++)
            {
                for (int j = 0; j < colorMap.Width; j++)
                {
                    colorMap.SetPixel(j, i, b.GetPixel(j, i));
                }
            }
            return colorMap;
        }

        public static List<Color> GetColorList(this Bitmap b)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            List<Color> colors = new List<Color>();
            for (int i = 0; i < b.Height; i++)
            {
                for (int j = 0; j < b.Width; j++)
                {
                    colors.Add(b.GetPixel(j, i));
                }
            }
            sw.LogAndReset("GetColorList");
            return colors;
        }

        public static unsafe List<Color> UnsafeGetColorList(this Bitmap b)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            BitmapData imageData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte* scan0 = (byte*)imageData.Scan0.ToPointer();
            int stride = imageData.Stride;
            System.Collections.Concurrent.ConcurrentQueue<Color> queue = new System.Collections.Concurrent.ConcurrentQueue<Color>();

            for (int y = 0; y < imageData.Height; y++)
            {
                byte* row = scan0 + (y * stride);

                Parallel.For(0, imageData.Width, (x) =>
                {
                    int bIndex = x * 3;
                    int gIndex = bIndex + 1;
                    int rIndex = bIndex + 2;

                    int pixelR = row[rIndex];
                    int pixelG = row[gIndex];
                    int pixelB = row[bIndex];
                    var c = Color.FromArgb(pixelR, pixelG, pixelB);
                    queue.Enqueue(c);
                });
            }
            b.UnlockBits(imageData);
            var c = queue.ToList();
            sw.LogAndReset("UnsafeGetColorList");
            return c;
        }

        public static Color[] GetColorArray(this Bitmap b)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            List<Color> colors = new List<Color>();
            for (int i = 0; i < b.Height; i++)
            {
                for (int j = 0; j < b.Width; j++)
                {
                    colors.Add(b.GetPixel(j, i));
                }
            }
            var c = colors.ToArray();
            colors.Clear();
            colors.TrimExcess();
            colors = null;
            sw.LogAndReset("GetColorArray");
            return c;
        }

        public static HashSet<Color> GetColorSet(this Bitmap b)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            HashSet<Color> colors = new HashSet<Color>();
            for (int i = 0; i < b.Height; i++)
            {
                for (int j = 0; j < b.Width; j++)
                {
                    colors.Add(b.GetPixel(j, i));
                }
            }
            sw.LogAndReset("GetColorSet");
            return colors;
        }

        public static Bitmap Resize(this Bitmap b, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(b, 0, 0, width, height);
            }
            return result;
        }

        public static Bitmap Scale(this Bitmap b, double scaleX, double scaleY)
        {
            Bitmap result = new Bitmap((int)(b.Width * scaleX), (int)(b.Height * scaleY));
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(b, 0, 0, (int)(result.Width), (int)(result.Height));
            }
            return result;
        }

        public static void ColorWithComparer(this Bitmap b, List<Comparer> comparisons, Func<Color, Color, double> deltaSolver)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int i = 0; i < b.Height; i++)
            {
                for (int j = 0; j < b.Width; j++)
                {
                    var c = b.GetPixel(j, i);
                    Color nearest = c;
                    double delta = int.MaxValue;
                    for (int k = 0; k < comparisons.Count; k++)
                    {
                        if (c.A == 0)
                            break;
                        if (comparisons[k].values.Keys.Contains(c))
                        {
                            b.SetPixel(j, i, comparisons[k].key);
                            break;
                        }
                        var curDelta = deltaSolver(comparisons[k].key, c);
                        if (curDelta < delta)
                        {
                            delta = curDelta;
                            b.SetPixel(j, i, comparisons[k].key);
                        }
                    }
                }
            }
            sw.LogAndReset("ColorWithComparer");
        }

        public static unsafe void UnsafeColorWithComparer(this Bitmap b, List<Comparer> comparisons, Func<Color, Color, double> deltaSolver)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            BitmapData imageData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int bytesPerPixel = 3;
            byte* scan0 = (byte*)imageData.Scan0.ToPointer();
            int stride = imageData.Stride;

            for (int y = 0; y < imageData.Height; y++)
            {
                byte* row = scan0 + (y * stride);

                Parallel.For(0, imageData.Width, (x) =>
                {
                    int bIndex = x * bytesPerPixel;
                    int gIndex = bIndex + 1;
                    int rIndex = bIndex + 2;

                    int pixelR = row[rIndex];
                    int pixelG = row[gIndex];
                    int pixelB = row[bIndex];

                    var c = Color.FromArgb(pixelR, pixelG, pixelB);

                    Color nearest = c;
                    double delta = int.MaxValue;
                    for (int k = 0; k < comparisons.Count; k++)
                    {
                        if (c.A == 0)
                            break;
                        if (comparisons[k].values.ContainsKey(c))
                        {
                            row[rIndex] = comparisons[k].key.R;
                            row[gIndex] = comparisons[k].key.G;
                            row[bIndex] = comparisons[k].key.B;
                            break;
                        }
                        var curDelta = deltaSolver(comparisons[k].key, c);
                        if (curDelta < delta)
                        {
                            delta = curDelta;
                            row[rIndex] = comparisons[k].key.R;
                            row[gIndex] = comparisons[k].key.G;
                            row[bIndex] = comparisons[k].key.B;
                        }
                    }
                });
            }
            b.UnlockBits(imageData);
            sw.LogAndReset("UnsafeColorWithComparer");
        }

        public static void SaveWithName(this Bitmap b, string folder, string name)
        {
            Directory.CreateDirectory(folder);
            string newPath = folder + @"\" + name + ".png";
            Console.WriteLine(newPath);
            b.Save(newPath);
        }

        public static Bitmap BitmapWithPallete(Bitmap b, Color[] mostPopular, Func<Color, Color, double> deltaSolver)
        {
            Bitmap newB = new Bitmap(b.Width, b.Height);
            for (int i = 0; i < b.Height; i++)
            {
                for (int j = 0; j < b.Width; j++)
                {
                    var c = b.GetPixel(j, i);
                    Color nearest = c;
                    double delta = int.MaxValue;
                    for (int k = 0; k < mostPopular.Length; k++)
                    {
                        if (c.A == 0)
                            break;
                        var curDelta = deltaSolver(mostPopular[k], c);
                        if (curDelta < delta)
                        {
                            delta = curDelta;
                            nearest = mostPopular[k];
                        }
                    }
                    newB.SetPixel(j, i, nearest);
                }
            }
            return newB;
        }
    }
}
