using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Pixelator
{
    class Program
    {
        static void Main(string[] args)
        {
            Test();
        }

        private static void Test()
        {
            Console.WriteLine("Write full path to file, including file extension");
            var path = Console.ReadLine();
            var folder = System.IO.Path.GetDirectoryName(path);
            Console.WriteLine("Write new name for file");
            var name = Console.ReadLine();
            int colorsCount = 5;
            int maxColorsCount = 10000;
            int minColorsCount = 2;

            Console.WriteLine("Write count of colors");
            int.TryParse(Console.ReadLine(), out colorsCount);
            colorsCount = Math.Clamp(colorsCount, minColorsCount, maxColorsCount);

            Pixelate(path, name, colorsCount);
        }

        private static void Pixelate(string sourcePath, string resultName)
        {
            var folder = Path.GetDirectoryName(sourcePath);
            int colorsCount = 5;

            var b = new Bitmap(sourcePath);
            b = b.Scale(0.1, 0.1);
            var c = b.GetColorSet().ToList();

            var comp = PixelArtConverter.GetComparersByKMeansFor(c, colorsCount, ColorExtension.DeltaE);
            b.UnsafeColorWithComparer(comp, ColorExtension.DeltaE);
            b.SaveWithName(folder, resultName);
        }

        private static void Pixelate(string sourcePath, string resultName, int colorsCount)
        {
            var folder = Path.GetDirectoryName(sourcePath);

            var b = new Bitmap(sourcePath);
            b = b.Scale(0.1, 0.1);
            var c = b.GetColorSet().ToList();

            var comp = PixelArtConverter.GetComparersByKMeansFor(c, colorsCount, ColorExtension.DeltaE);
            b.UnsafeColorWithComparer(comp, ColorExtension.DeltaE);
            b.SaveWithName(folder, resultName);
        }

        private static void Pixelate(string sourcePath, string resultName, List<Color> palette)
        {
            var folder = Path.GetDirectoryName(sourcePath);

            var b = new Bitmap(sourcePath);
            b = b.Scale(0.1, 0.1);
            var c = b.GetColorSet().ToList();

            b.UnsafeColorWithPalette(palette, ColorExtension.DeltaE);
            b.SaveWithName(folder, resultName);
        }
    }
}
