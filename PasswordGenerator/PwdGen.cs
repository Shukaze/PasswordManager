using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PasswordGenerator
{
    public sealed class PwdGen
    {
        public string LowerCharacters { get; set; } = "abcdefghijklmnopqrstuvwxyz";
        public string UpperCharacters { get; set; } = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public string Symbols { get; set; } = "!@$()=+-,:.";
        public string Digits { get; set; } = "0123456789";
        public int Length { get; set; } = 14;
        public int MinSymbols { get; set; } = 1;
        public int MinLowerCharacters { get; set; } = 1;
        public int MinUpperCharacters { get; set; } = 1;
        public int MinDigits { get; set; } = 1;

        public string Generate()
        {   
            StringBuilder sb = new StringBuilder();
            if (MinLowerCharacters > 0)
            {
                sb.Append(LowerCharacters);
            }
            if (MinUpperCharacters > 0)
            {
                sb.Append(UpperCharacters);
            }
            if (MinSymbols > 0)
            {
                sb.Append(Symbols);
            }
            if (MinDigits > 0)
            {
                sb.Append(Digits);
            }
            string all = sb.ToString();
            using (var rng = new RNGCryptoServiceProvider())//cryptographic Random Number Generator
            {
                List<int> numbers = new List<int>();
                for (int idx = 0; idx < Length; idx++)
                {
                    numbers.Add(idx); // 0 => 0, 1 => 1, и т.д.
                }
                List<int> positions = new List<int>();
                while (numbers.Count > 0)
                {
                    var nidx = numbers.Count == 1 ? 0 : Next(rng, numbers.Count);
                    positions.Add(numbers[nidx]);
                    numbers.RemoveAt(nidx);
                }
                char[] pwd = new char[Length];
                int drawidx = 0;
                Draw(rng, pwd, ref drawidx, MinLowerCharacters, LowerCharacters, Length, positions);
                Draw(rng, pwd, ref drawidx, MinUpperCharacters, UpperCharacters, Length, positions);
                Draw(rng, pwd, ref drawidx, MinSymbols, Symbols, Length, positions);
                Draw(rng, pwd, ref drawidx, MinDigits, Digits, Length, positions);
                Draw(rng, pwd, ref drawidx, Length - drawidx, all, Length, positions);
                string ret = new string(pwd);
                Array.Clear(pwd, 0, pwd.Length);
                return ret;
            }
        }

        private void Draw(RNGCryptoServiceProvider rng,
                            char[] pwd,
                            ref int drawidx,
                            int drawcnt,
                            string symbols,
                            int pwdlen,
                            List<int> positions)
        {
            if (symbols.Length > 0 && drawcnt > 0)
            {
                for (int cnt = 0; cnt < drawcnt && drawidx < pwdlen; cnt++)
                {
                    pwd[positions[drawidx++]] = symbols[Next(rng, symbols.Length)];
                }
            }
        }

        private int Next(RNGCryptoServiceProvider rng, int upper_limit)
        {
            if (upper_limit <= 0)
            {
                throw new ArgumentException($"Invalid upper limit {upper_limit}.");
            }
            if (upper_limit == 1)
            {
                return 0;
            }
            return (int)(Next(rng) % (uint)upper_limit);
        }

        private uint Next(RNGCryptoServiceProvider rng)
        {
            byte[] randomNumber = new byte[4];
            rng.GetBytes(randomNumber);
            return BitConverter.ToUInt32(randomNumber, 0);
        }
    }
}
