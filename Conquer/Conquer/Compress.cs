using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace Conquer
{
    public class Compress
    {
        public static string rle_encode(string msg)
        {
            string encodedMsg = "";
            char oldLetter = msg[0];
            int count = 1;

            for (int i = 1; i < msg.Length; i++)
            {
                char letter = msg[i];

                if (letter == oldLetter && count < 9)
                {
                    count += 1;
                }
                else
                {
                    encodedMsg += Convert.ToString(count) + oldLetter;
                    oldLetter = letter;
                    count = 1;
                }
            }
            encodedMsg += Convert.ToString(count) + oldLetter;

            return encodedMsg;
        }

        public static string rle_decode(string msg)
        {
            string decodedMsg = "";

            for (int i = 0; i < msg.Length; i += 2)
            {
                char letter = msg[i + 1];
                for (int j = 0; j < msg[i] - '0'; j++)
                {
                    decodedMsg += letter;
                }
            }

            return decodedMsg;
        }

        public static string unary_encode(int n)
        {
            string encodedMsg = "";

            for (int i = 0; i < n; i++)
            {
                encodedMsg += "0";
            }

            return encodedMsg + "1";
        }

        public static int unary_decode(string msg)
        {
            return msg.Length - 1;
        }

        public static string gamma_encode(int n)
        {
            int u = n;
            int i = 0;
            for (; u > 0; i++)
            {
                u /= 2;
            }
            i--;
            string lastBin = "";

            for (int j = 0; j < i; j++)
            {
                lastBin = Convert.ToString(n % 2) + lastBin;
                n /= 2;
            }

            return unary_encode(i) + lastBin;
        }

        public static int gamma_decode(string msg)
        {
            return Convert.ToInt32(msg, 2);
        }

        public static void img_bin(string pathin, string pathout)
        {
            try
            {
                Bitmap img = new Bitmap(pathin);
                Bitmap newImg = new Bitmap(img.Width, img.Height);

                for (int i = 0; i < img.Height; i++)
                {
                    for (int j = 0; j < img.Width; j++)
                    {
                        Color pixelColor = img.GetPixel(j, i);

                        if ((pixelColor.R + pixelColor.G + pixelColor.B) / 3 >= 127)
                            newImg.SetPixel(j, i, Color.White);
                        else
                            newImg.SetPixel(j, i, Color.Black);
                    }
                }

                newImg.Save(pathout);
            }
            catch (Exception e)
            {
                Console.WriteLine("img_bin : File not found...");
                throw;
            }
        }

        private static void GetImgCompressed(Bitmap img, ref string compressedImg)
        {
            //Used in img_compress
            int SameColorPxl = 0;
            Color lastColor = img.GetPixel(0, 0);
            for (int i = 0; i < img.Height; i++)
            {
                for (int j = 0; j < img.Width; j++)
                {
                    if (lastColor == img.GetPixel(j, i))
                        SameColorPxl++;
                    else
                    {
                        compressedImg += gamma_encode(SameColorPxl);
                        SameColorPxl = 1;
                        lastColor = img.GetPixel(j, i);
                    }
                }
            }
            compressedImg += gamma_encode(SameColorPxl);
        }

        private static void SaveImgCompressed(string pathout, string compressedImg)
        {
            //Used in img_compress
            try
            {
                using (StreamWriter file = new StreamWriter(pathout))
                {
                    file.WriteLine(compressedImg);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("SaveImgCompressed : File not found...");
                throw;
            }
        }
        
        public static void img_compress(string pathin, string pathout)
        {
            try
            {
                string compressedImg = "";
                Bitmap img = new Bitmap(pathin);

                compressedImg += gamma_encode(img.Width) + gamma_encode(img.Height);
                if (img.GetPixel(0,0) == Color.FromArgb(0,0,0))
                    compressedImg += "0";
                else
                    compressedImg += "1";
                
                GetImgCompressed(img, ref compressedImg);
                SaveImgCompressed(pathout, compressedImg);
            }
            catch (Exception e)
            {
                Console.WriteLine("img_bin : File not found...");
                throw;
            }
        }

        private static int readANbr(string text, ref int i)
        {
            //Used in img_decompress
            int j = 0;
            string intToDecode = "";
            for (; j < text.Length - i && text[i + j] != '1'; j++)
            {
                intToDecode += text[i + j];
            }
            for (int k = 0; k <= j; k++)
            {
                intToDecode += text[i + j + k];
            }

            i += 2 * j + 1;
            return gamma_decode(intToDecode);
        }

        private static Color GetPxlColor(char pxl)
        {
            //Used in img_decompressed
            if (pxl == '0')
                return Color.Black;
            else
                return Color.White;
        }

        private static void switchColor(ref Color lastColor)
        {
            //Used in img_decompressed
            if (lastColor == Color.White)
                lastColor = Color.Black;
            else
                lastColor = Color.White;
        }

        private static void GetNewPos(int width, int height, ref int x, ref int y)
        {
            //Used in img_decompress
            x++;
            if (x >= width)
            {
                x = 0;
                y++;
            }
            if (y > height)
                Console.Error.WriteLine("img_decompress : content of compressed file doesn't match with its size...");
        }
        
        public static void img_decompress(string path, string pathout)
        {
            try
            {
                using (StreamReader file = new StreamReader(path))
                {
                    string text = file.ReadToEnd();
                    int i = 0;

                    int width = readANbr(text, ref i);
                    int height = readANbr(text, ref i);
                    Bitmap newImg = new Bitmap(width, height);

                    Color lastColor = GetPxlColor(text[i]);
                    i++;

                    int x = 0;
                    int y = 0;
                    for (; i < text.Length - 2;)
                    {
                        int nbPxl = readANbr(text, ref i);
                        for (int j = 0; j < nbPxl; j++)
                        {
                            newImg.SetPixel(x, y, lastColor);
                            GetNewPos(width, height, ref x, ref y);
                        }

                        switchColor(ref lastColor);
                    }
                    
                    newImg.Save(pathout);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("img_decompress : File not found");
                throw;
            }
        }
    }
}