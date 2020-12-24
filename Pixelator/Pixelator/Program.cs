using System;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Linq;

namespace Pixelator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Write full path to file, including file extension");
            var path = Console.ReadLine();
            var folder = System.IO.Path.GetDirectoryName(path);
            Console.WriteLine("Write new name for file");
            var name = Console.ReadLine();
            int colorsCount = 5;


            Mat output = new Mat(path);
            Cv2.FastNlMeansDenoisingColored(output, output, 5, 30);
            Cv2.MedianBlur(output, output, 3);
            Cv2.MedianBlur(output, output, 3);

            var b = BitmapConverter.ToBitmap(output);
            b = b.Scale(0.1,0.1);
            var c = b.GetColorSet().ToList();

            Console.WriteLine("Write count of colors");
            int.TryParse(Console.ReadLine(), out colorsCount);
            colorsCount = Math.Clamp(colorsCount, 2, c.Count);

            var comp = PixelArtConverter.GetComparersByKMeansFor(c, 7, ColorExtension.DeltaE);
            b.UnsafeColorWithComparer(comp, ColorExtension.DeltaE);
            b.SaveWithName(folder, name);
            System.Diagnostics.Process.Start(@"cmd.exe ", @"/c " + folder + @"\" + name + ".png");

        }
    }
}
