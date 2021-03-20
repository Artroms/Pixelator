using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace Pixelator
{
    class PixelArtConverter
    {
        public static List<Comparer> GetComparersByAvarage(List<Color> colors, int step)
        {
            step = Math.Clamp(step, 2, 128);
            step = step - (step % 2);
            bool[,,] cube = new bool[256, 256, 256];
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    for (int k = 0; k < 256; k++)
                    {
                        cube[i, j, k] = false;
                    }
                }
            }

            for (int i = 0; i < colors.Count; i++)
            {
                var c = colors[i];
                cube[c.R, c.G, c.B] = true;
            }

            List<Comparer> avarageColors = new List<Comparer>();

            for (int i = 0; i < 256; i += step)
            {
                for (int j = 0; j < 256; j += step)
                {
                    for (int k = 0; k < 256; k += step)
                    {
                        Comparer comp = new Comparer();
                        List<Color> avarage = new List<Color>();
                        Parallel.For(0, step, (l) =>
                        {
                            for (int m = 0; m < step; m++)
                            {
                                for (int n = 0; n < step; n++)
                                {
                                    if (cube[i + l, j + m, k + n] == true)
                                    {
                                        comp.values.TryAdd(Color.FromArgb(i + l, j + m, k + n), default);
                                    }
                                }
                            }
                        });
                        if (comp.values.Count != 0)
                        {
                            comp.key = ColorExtension.SquareAvarage(comp.values.Keys.ToArray());
                            avarageColors.Add(comp);
                        }
                    }
                }
            }
            return avarageColors.ToList();
        }

        public static List<Comparer> GetComparersByKMeans(Color[] colors, int count, Func<Color, Color, double> solver)
        {
            List<Comparer> centroids = new List<Comparer>();
            Random rnd = new Random();
            for (int i = 0; i < count; i++)
            {
                var centroid = new Comparer();
                centroid.key = colors[rnd.Next(0, colors.Length)];
                centroids.Add(centroid);
            }
            for (int i = 0; i < colors.Length; i++)
            {
                Comparer.Nearest(colors[i], centroids.ToArray(), solver).values.TryAdd(colors[i], default);
            }
            return KMeansComparers(centroids, solver);
        }

        public static List<Comparer> GetComparersByKMeansFor(List<Color> colors, int count, Func<Color, Color, double> solver)
        {
            List<Comparer> centroids = new List<Comparer>();
            Random rnd = new Random();
            for (int i = 0; i < count; i++)
            {
                var centroid = new Comparer();
                centroid.key = colors[rnd.Next(0, colors.Count)];
                centroids.Add(centroid);
            }
            for (int i = 0; i < colors.Count; i++)
            {
                Comparer.Nearest(colors[i], centroids.ToArray(), solver).values.TryAdd(colors[i], default);
            }
            bool changed = true;
            int k = 0;
            while (changed && k < 100)
            {
                changed = KMeansComparersFor(centroids, solver);
                k++;
            }
            return centroids;
        }

        private static List<Comparer> KMeansComparers(List<Comparer> centroids, Func<Color, Color, double> solver)
        {
            var colors = new List<Color>();
            for (int i = 0; i < centroids.Count; i++)
            {
                colors.AddRange(centroids[i].values.Keys);
                centroids[i].values.Clear();
            }
            for (int i = 0; i < colors.Count; i++)
            {
                Comparer.Nearest(colors[i], centroids.ToArray(), solver).values.TryAdd(colors[i], default);
            }
            colors.Clear();
            colors.TrimExcess();
            bool changed = false;
            for (int i = 0; i < centroids.Count; i++)
            {
                Color before = centroids[i].key;
                centroids[i].key = ColorExtension.SquareAvarage(centroids[i].values.Keys.ToArray());
                if (before != centroids[i].key)
                    changed = true;
            }
            if (!changed)
            {
                return centroids;
            }
            return KMeansComparers(centroids, solver);
        }

        private static bool KMeansComparersFor(List<Comparer> centroids, Func<Color, Color, double> solver)
        {
            var colors = new List<Color>();
            for (int i = 0; i < centroids.Count; i++)
            {
                colors.AddRange(centroids[i].values.Keys);
                centroids[i].values.Clear();
            }
            Parallel.For(0, colors.Count, (i) =>
            {
                Comparer.Nearest(colors[i], centroids.ToArray(), solver).values.TryAdd(colors[i], default);
            });
            bool changed = false;
            Parallel.For(0, centroids.Count, (i) =>
            {
                Color before = centroids[i].key;
                if(!centroids[i].values.IsEmpty)
                    centroids[i].key = ColorExtension.SquareAvarage(centroids[i].values.Keys.ToArray());
                if (before != centroids[i].key)
                    changed = true;
            });
            if (!changed)
            {
                return false;
            }
            return true;
        }

        public static List<Comparer> GetComparersByMedianCut(List<Color> colors, int step)
        {
            colors = colors.ToList();
            List<Comparer> pallete = new List<Comparer>();
            ComparerSeparation(colors, step, pallete);
            return pallete;
        }

        private static void ComparerSeparation(List<Color> colors, int iter, List<Comparer> pallete)
        {
            double rmin = double.MaxValue;
            double rmax = double.MinValue;
            double gmin = double.MaxValue;
            double gmax = double.MinValue;
            double bmin = double.MaxValue;
            double bmax = double.MinValue;
            for (int i = 0; i < colors.Count; i++)
            {
                rmin = colors[i].R < rmin ? colors[i].R : rmin;
                rmax = colors[i].R > rmax ? colors[i].R : rmax;
                gmin = colors[i].G < gmin ? colors[i].G : gmin;
                gmax = colors[i].G > gmax ? colors[i].G : gmax;
                bmin = colors[i].B < bmin ? colors[i].B : bmin;
                bmax = colors[i].B > bmax ? colors[i].B : bmax;
            }
            double r = rmax - rmin;
            double g = gmax - gmin;
            double b = bmax - bmin;
            if (r > g && r > b)
                colors.Sort(delegate (Color c1, Color c2)
                {
                    return c1.R.CompareTo(c2.R);
                });
            else if (g > r && g > b)
                colors.Sort(delegate (Color c1, Color c2)
                {
                    return c1.G.CompareTo(c2.G);
                });
            else
                colors.Sort(delegate (Color c1, Color c2)
                {
                    return c1.B.CompareTo(c2.B);
                });
            iter--;
            if (iter == 0)
            {
                Comparer comp = new Comparer();
                foreach (var item in colors)
                {
                    comp.values.TryAdd(item, default);
                }
                comp.key = ColorExtension.SquareAvarage(comp.values.Keys.ToArray());
                pallete.Add(comp);
                return;
            }
            else
            {
                var down = colors.GetRange(0, colors.Count / 2);
                var up = colors.GetRange(colors.Count / 2, colors.Count / 2);
                ComparerSeparation(up, iter, pallete);
                ComparerSeparation(down, iter, pallete);
            }
        }

        public static void VisualizeColors(List<Color> colors, string folder, string name)
        {
            Bitmap b = new Bitmap(colors.Count * 32, 32);
            for (int i = 0; i < b.Height; i++)
            {
                for (int j = 0; j < b.Width; j++)
                {
                    b.SetPixel(j, i, colors[j / 32]);
                }
            }
            Directory.CreateDirectory(folder);
            string newPath = folder + @"\" + name + ".png";
            b.Save(newPath);
        }

        public static void VisualizeColors(Color[] colors, string folder, string name)
        {
            Bitmap b = new Bitmap(colors.Length * 32, 32);
            for (int i = 0; i < b.Height; i++)
            {
                for (int j = 0; j < b.Width; j++)
                {
                    b.SetPixel(j, i, colors[j / 32]);
                }
            }
            Directory.CreateDirectory(folder);
            string newPath = folder + @"\" + name + ".png";
            b.Save(newPath);
        }
    }
}
