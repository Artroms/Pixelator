using System;
using System.Drawing;

namespace Pixelator
{
    static class ColorExtension
    {
        public static double DeltaE(Color a, Color b)
        {
            return ColorFormulas.DoFullCompare(a.R, a.G, a.B, b.R, b.G, b.B);
        }

        public static double DeltaHSL(Color a, Color b)
        {
            double h = (int)Math.Round(Math.Abs(a.GetHue() - b.GetHue()));
            double s = (int)Math.Round(Math.Abs(a.GetSaturation() - b.GetSaturation()) * 255);
            double l = (int)Math.Round(Math.Abs(a.GetBrightness() - b.GetBrightness()) * 255);
            return (h + s + l) / 3;
        }

        public static double DeltaHue(Color a, Color b)
        {
            return (Math.Abs(a.GetHue() - b.GetHue()));
        }

        public static double Delta(this Color a, Color b)
        {
            double dr = Math.Abs(a.R - b.R);
            double dg = Math.Abs(a.G - b.G);
            double db = Math.Abs(a.B - b.B);
            return (dr + db + dg) / 3;
        }

        public static double DeltaLinear(this Color a, Color b)
        {
            double dr = Math.Abs(a.R - b.R);
            double dg = Math.Abs(a.G - b.G);
            double db = Math.Abs(a.B - b.B);
            return (dr + db + dg);
        }

        public static double DeltaEuclidean(this Color a, Color b)
        {
            double dr = Math.Pow(a.R - b.R, 2);
            double dg = Math.Pow(a.G - b.G, 2);
            double db = Math.Pow(a.B - b.B, 2);
            return Math.Sqrt(dr + db + dg);
        }

        public static double DeltaBrightnes(this Color a, Color b)
        {
            return Math.Abs(a.GetBrightness() - b.GetBrightness());
        }

        public static Color Avarage(Color[] colors)
        {
            long r = 0;
            long g = 0;
            long b = 0;
            for (int i = 0; i < colors.Length; i++)
            {
                r += colors[i].R;
                g += colors[i].G;
                b += colors[i].B;
            }
            r /= colors.Length;
            g /= colors.Length;
            b /= colors.Length;
            return Color.FromArgb((int)r, (int)g, (int)b);
        }

        public static Color SquareAvarage(Color[] colors)
        {
            double r = 0;
            double g = 0;
            double b = 0;
            for (int i = 0; i < colors.Length; i++)
            {
                r += colors[i].R * colors[i].R;
                g += colors[i].G * colors[i].G;
                b += colors[i].B * colors[i].B;
            }
            r /= colors.Length;
            g /= colors.Length;
            b /= colors.Length;

            r = Math.Sqrt(r);
            g = Math.Sqrt(g);
            b = Math.Sqrt(b);
            return Color.FromArgb((int)r, (int)g, (int)b);
        }

        public static Color Nearest(this Color c, Color[] colors, Func<Color,Color, double> solver)
        {
            var delta = double.MaxValue;
            Color nearest = c;
            for (int i = 0; i < colors.Length; i++)
            {
                if (solver(c, colors[i]) < delta)
                {
                    delta = solver(c, colors[i]);
                    nearest = colors[i];
                }
            }
            return nearest;
        }

        public static double Luminosity(this Color c)
        {
            return Math.Sqrt(.241 * c.R + .691 * c.G + .068 * c.B);
        }

        public static double DeltaLuminocity(this Color c1, Color c2)
        {
            return Math.Abs(c1.Luminosity() - c2.Luminosity());
        }

        private struct ColorFormulas
        {
            public double X;
            public double Y;
            public double Z;

            public double CieL;
            public double CieA;
            public double CieB;

            public ColorFormulas(int RVal, int GVal, int BVal)
            {
                double R = Convert.ToDouble(RVal) / 255.0;       //R from 0 to 255
                double G = Convert.ToDouble(GVal) / 255.0;       //G from 0 to 255
                double B = Convert.ToDouble(BVal) / 255.0;       //B from 0 to 255

                if (R > 0.04045)
                {
                    R = Math.Pow(((R + 0.055) / 1.055), 2.4);
                }
                else
                {
                    R = R / 12.92;
                }
                if (G > 0.04045)
                {
                    G = Math.Pow(((G + 0.055) / 1.055), 2.4);
                }
                else
                {
                    G = G / 12.92;
                }
                if (B > 0.04045)
                {
                    B = Math.Pow(((B + 0.055) / 1.055), 2.4);
                }
                else
                {
                    B = B / 12.92;
                }

                R = R * 100;
                G = G * 100;
                B = B * 100;

                //Observer. = 2°, Illuminant = D65
                X = R * 0.4124 + G * 0.3576 + B * 0.1805;
                Y = R * 0.2126 + G * 0.7152 + B * 0.0722;
                Z = R * 0.0193 + G * 0.1192 + B * 0.9505;


                double ref_X = 95.047;
                double ref_Y = 100.000;
                double ref_Z = 108.883;

                double var_X = X / ref_X;         // Observer= 2°, Illuminant= D65
                double var_Y = Y / ref_Y;
                double var_Z = Z / ref_Z;

                if (var_X > 0.008856)
                {
                    var_X = Math.Pow(var_X, (1 / 3.0));
                }
                else
                {
                    var_X = (7.787 * var_X) + (16 / 116.0);
                }
                if (var_Y > 0.008856)
                {
                    var_Y = Math.Pow(var_Y, (1 / 3.0));
                }
                else
                {
                    var_Y = (7.787 * var_Y) + (16 / 116.0);
                }
                if (var_Z > 0.008856)
                {
                    var_Z = Math.Pow(var_Z, (1 / 3.0));
                }
                else
                {
                    var_Z = (7.787 * var_Z) + (16 / 116.0);
                }

                CieL = (116 * var_Y) - 16;
                CieA = 500 * (var_X - var_Y);
                CieB = 200 * (var_Y - var_Z);
            }

            ///
            /// The smaller the number returned by this, the closer the colors are
            ///
            ///
            /// 
            public int CompareTo(ColorFormulas oComparisionColor)
            {
                // Based upon the Delta-E (1976) formula at easyrgb.com (http://www.easyrgb.com/index.php?X=DELT&H=03#text3)
                double DeltaE = Math.Sqrt(Math.Pow((CieL - oComparisionColor.CieL), 2) + Math.Pow((CieA - oComparisionColor.CieA), 2) + Math.Pow((CieB - oComparisionColor.CieB), 2));
                return Convert.ToInt16(Math.Round(DeltaE));
            }

            public static int DoFullCompare(int R1, int G1, int B1, int R2, int G2, int B2)
            {
                ColorFormulas oColor1 = new ColorFormulas(R1, G1, B1);
                ColorFormulas oColor2 = new ColorFormulas(R2, G2, B2);
                return oColor1.CompareTo(oColor2);
            }


        }
    }
}
