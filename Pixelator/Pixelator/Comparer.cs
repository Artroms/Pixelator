using System;
using System.Drawing;
using System.Collections.Concurrent;

namespace Pixelator
{
    public class Comparer
    {
        public Color key;
        public ConcurrentDictionary<Color, byte> values = new ConcurrentDictionary<Color, byte>();

        public static Comparer Nearest(Color c, Comparer[] colors, Func<Color, Color, double> solver)
        {
            var delta = double.MaxValue;
            Comparer nearest = null;
            for (int i = 0; i < colors.Length; i++)
            {
                if (solver(c, colors[i].key) < delta)
                {
                    delta = solver(c, colors[i].key);
                    nearest = colors[i];
                }
            }
            return nearest;
        }

        public static Comparer Farthest(Color c, Comparer[] colors, Func<Color, Color, double> solver)
        {
            var delta = double.MinValue;
            Comparer nearest = null;
            for (int i = 0; i < colors.Length; i++)
            {
                if (solver(c, colors[i].key) > delta)
                {
                    delta = solver(c, colors[i].key);
                    nearest = colors[i];
                }
            }
            return nearest;
        }
    }
}
