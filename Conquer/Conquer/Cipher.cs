using System;
using System.Collections.Generic;

namespace Conquer
{
    public class Cipher
    {

        public static string xor_cipher(string msg, char key)
        {
            string str = "";
            foreach (char letter in msg)
            {
                str += (char) (letter ^ key);
            }

            return str;
        }

        public static char ror(char c, int n)
        {
            int bin = Convert.ToInt32(c);
            char newC;

            n = modulo(n, 16);

            newC = (char) (bin << (16 - n) | bin >> n);
            return newC;
        }

        public static string ror_cipher(string msg, int n)
        {
            string str = "";
            foreach (char letter in msg)
            {
                str += ror(letter, n);
            }

            return str;
        }

        private static int modulo(int i, int n)
        {
            //Used in rotn and ror
            int sol = i % n;
            if (sol < 0)
                sol += n;

            return sol;
        }

        public static string rotn(string msg, int n)
        {
            string str = "";
            foreach (char letter in msg)
            {
                char add;
                if (letter >= 97 && letter <= 122)
                    add = (char) (modulo ((letter - 'a' + n), 26) + 'a');
                else
                {
                    if (letter >= 65 && letter <= 90)
                        add = (char) (modulo((letter - 'A' + n), 26) + 'A');
                    else
                        add = letter;
                }

                str += add;
            }
            return str;
        }

        private static int isPresent(char letter, List<char> key)
        {
            // Used in freq
            int index = 0;
            for (; index < key.Count && letter != key[index]; index++) ;

            if (index < key.Count)
                return index;
            return -1;
        }

        public static char[] freq(string msg)
        {
            List<int> key = new List<int>();
            List<char> value = new List<char>();

            for (int i = 0; i < msg.Length; i++)
            {
                char letter = Char.ToLower(msg[i]);
                if (letter >= 'a' && letter <= 'z')
                {
                    int index = isPresent(letter, value);

                    if (index == -1)
                    {
                        value.Add(letter);
                        key.Add(1);
                    }
                    else
                        key[index] += 1;
                }
            }

            int[] key2 = key.ToArray();
            char[] value2 = value.ToArray();
            Array.Sort(key2, value2);
            return value2;
        }

        public static string vigenere_encode(string msg, string key)
        {
            string RealMsg = msg.ToUpper();
            string RealKey = key.ToUpper();

            string encodedSentence = "";
            int nbLetter = 0;

            for (int i = 0; i < RealMsg.Length; i++)
            {
                char letter = RealMsg[i];
                if (letter >= 'A' && letter <= 'Z')
                {
                    encodedSentence += (char) ((letter - 'A' + RealKey[nbLetter % key.Length] - 'A') % 26 + 'A');
                    nbLetter++;
                }
                else
                    encodedSentence += letter;
            }

            return encodedSentence;
        }

        public static string vigenere_decode(string msg, string key)
        {
            string decryptionKey = "";
            string realKey = key.ToUpper();

            for (int i = 0; i < realKey.Length; i++)
            {
                decryptionKey += (char) (modulo ((26 - realKey[i] - 'A'), 26) + 'A');
            }

            return vigenere_encode(msg, decryptionKey);
        }
    }
}