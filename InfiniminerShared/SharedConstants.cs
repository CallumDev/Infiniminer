using System;
using System.Numerics;
using LibreLancer;
using LibreLancer.Graphics;

namespace Infiniminer
{
    public class Defines
    {
        public static ReadOnlySpan<byte> AppSignature => "INFINIMINER\b"u8;
        public const string INFINIMINER_VERSION = "v1.6";
        public const int GROUND_LEVEL = 8;

        /*public const string deathByLava = "HAD AN UNFORTUNATE SMELTING ACCIDENT!";
        public const string deathByElec = "WAS LIT UP!";//"GOT TOO CLOSE TO THE POWER LINES!";
        public const string deathByExpl = "WAS KILLED BY AN EXPLOSION!";//SAW A BRIGHT FLASH";
        public const string deathByFall = "HAD A QUICK MEET WITH GRAVITY!";//SOLID GROUND!";
        public const string deathByMiss = "WAS KILLED BY MISADVENTURE!";
        public const string deathBySuic = "HAS COMMITED PIXELCIDE!";*/

        public const string deathByLava = "WAS INCINERATED BY LAVA!";
        public const string deathByElec = "WAS ELECTROCUTED!";
        public const string deathByExpl = "WAS KILLED IN AN EXPLOSION!";
        public const string deathByFall = "WAS KILLED BY GRAVITY!";
        public const string deathByMiss = "WAS KILLED BY MISADVENTURE!";
        public const string deathBySuic = "HAS COMMITED PIXELCIDE!";
        
        static Color4 RGB(int r, int g, int b) => new Color4((byte) r, (byte) g, (byte) g, 255);
        
        public static Color4 IM_BLUE = RGB(80, 150, 255);
        public static Color4 IM_RED = RGB(222, 24, 24);

        public static Color4[] BLUE_SHADES = { //Darkest to lightest
                                                RGB(0,28,57),
                                                RGB(0,58,117),
                                                RGB(0,91,183),
                                                RGB(0,107,215),
                                                RGB(0,127,255)
                                            };
        public static Color4[] generateShades(Color4 col)
        {
            Color4 temp = col;//new Color(80, 150, 255);
            //How do we get temp from that to BLUE_SHADES?
            //First let's order which colours are most dominant
            int[] dominance = new int[]{ 0, 1, 2 }; //2 most dominant
            byte[] values = new byte[]{(byte)(temp.R * 255), (byte)(temp.B * 255), (byte)(temp.G * 255)};

            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int i = 0; i < 2; i++)
                {
                    if (values[i] > values[i + 1])
                    {
                        byte onesec = values[i + 1];
                        values[i + 1] = values[i];
                        values[i] = onesec;
                        int onesec2 = dominance[i + 1];
                        dominance[i + 1] = dominance[i];
                        dominance[i] = onesec2;
                        changed = true;
                    }
                }
            }

            //...Well, first I guess we remove the least dominant colour - threshold of 80, decrease of 30?
            float min = (temp.R < temp.G ? temp.R : temp.G);
            min = min < temp.B ? min : temp.B;
            float max = (temp.R > temp.G ? temp.R : temp.G);
            max = max > temp.B ? max : temp.B;

            min *= 255;
            max *= 255;
            temp.R *= 255;
            temp.G *= 255;
            temp.B *= 255;
            //byte adjusted = (byte)(min - 80 < 0 ? 0 : min - 30); //The -30 is intentional
            //temp = new Color(temp.R == min ? adjusted : temp.R, temp.G == min ? adjusted : temp.G, temp.B == min ? adjusted : temp.B);

            //Ok, now we have the least dominant colour removed or at the very least subdued
            //Now we need to scale the remaining colours
            //The input colour for blue is closest to the brightest blue colour, so we'll use that for comparison
            //Thus it's 255/255, 215,255, etc. for dominant
            //And 127/150, 107/150, etc. for second most dominant
            //What's the scaled diff from the first dominant to second dominant though?
            //{ 2, 2, 2, 2, 2 } !
            //What are the dominant percentages for the original colour?
            //{ .313, .588, 1 }
            //Huh...
            //And the dominant percentages for the adjusted color to the second is 2
            //Seems accurate enough. Might as well give this a try!
            float[] dominantMultipliers = new float[] { 0.223529f, 0.4588235f, 0.717647f, 0.843137f, 1f };
            float[] secondDominantMultipliers = new float[] { 0.1866667f, 0.3866667f, 0.6066667f, 0.713333f, 0.8466667f };
            //float[] multipliers = new float[] { 1.197477f, 1.1866124f, 1.1829346f, 1.181968f, 1.1811f };
            float[] dominantPercentages = new float[] { temp.R/max, temp.G/max, temp.B/max };
            float[] leastDominantMultipliers = new float[] { 1f, 1f, 1f, 1f, 1f }; //No info to compare yet
            float[][] multipliers = new float[][] { dominantMultipliers, secondDominantMultipliers, leastDominantMultipliers };

            //Everything set? Then let's calculate!
            Color4[] shades = new Color4[5];
            //Loop through each shade, going from darkest to lightest
            for (int i = 0; i < dominantMultipliers.Length; i++)
            {
                //Go through the colour channels
                shades[i] = RGB(
                    (byte) (temp.R *
                            (multipliers[0][i] /
                             dominantPercentages[0])), //(byte)(multipliers[dominance[0]][i] * temp.R);
                    (byte) (temp.G *
                            (multipliers[0][i] /
                             dominantPercentages[1])), //(byte)(multipliers[dominance[1]][i] * temp.G);
                    (byte) (temp.B *
                            (multipliers[0][i] /
                             dominantPercentages[2])) //(byte)(multipliers[dominance[2]][i] * temp.B);
                ); 
            }
            return shades;
        }

        public static void generateShadedTexture(Color4 col, Texture2D bsprite, ref Texture2D target)
        {
            //System.Console.WriteLine("Input colour: " + col.ToString());
            try
            {
                Color4[] shades = generateShades(col);
                /*System.Console.WriteLine("Output colours: " + shades[0]);
                System.Console.WriteLine("\t" + shades[1]);
                System.Console.WriteLine("\t" + shades[2]);
                System.Console.WriteLine("\t" + shades[3]);
                System.Console.WriteLine("\t" + shades[4]);*/
                uint[] colArray = new uint[bsprite.Width * bsprite.Height];
                bsprite.GetData<uint>(colArray);
                for (int c = 0; c < colArray.Length; c++)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (ColorsEqual(Color4.FromRgba(colArray[c]), BLUE_SHADES[i]))
                        {
                            colArray[c] = (uint)shades[i].ToAbgr();
                            break;
                        }
                    }
                }
                target.SetData<uint>(colArray);
            }
            catch (System.Exception e)
            {
                System.Console.OpenStandardError();
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine(e.StackTrace);
                System.Console.Error.Close();
            }
            //System.Console.WriteLine("Finished texture shading!");
        }

        static bool ColorsEqual(Color4 a, Color4 b)
        {
            var aR = (byte) (a.R * 255);
            var aG = (byte) (a.G * 255);
            var aB = (byte) (a.B * 255);

            var bR = (byte) (b.R * 255);
            var bG = (byte) (b.G * 255);
            var bB = (byte) (b.B * 255);

            return Math.Abs(aR - bR) < 3 &&
                   Math.Abs(aG - bG) < 3 &&
                   Math.Abs(aB - bB) < 3;
        }

        public static string Sanitize(string input)
        {
            string output = "";
            for (int i = 0; i < input.Length; i++)
            {
                char c = (char)input[i];
                if (c >= 32 && c <= 126)
                    output += c;
            }
            return output;
        }
    }
}