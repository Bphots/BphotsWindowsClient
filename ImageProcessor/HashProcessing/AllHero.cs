using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageProcessor.HashProcessing
{
    public class AllHero
    {
        public static List<EachHero> HeroInfo = new List<EachHero>();
        
        private static int GetDec(char a, char b)
        {
            int x1, x2;
            if (a >= 'a')
                x1 = a - 'a' + 10;
            else x1 = a - '0';
            if (b >= 'a')
                x2 = b - 'a' + 10;
            else x2 = b - '0';
            var s = Convert.ToString(x1 ^ x2, 2);
            var count = s.Count(c => c == '1');

            return count;
        }

        private static int GetDifference(string a, string b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException();

            var count = 0;
            for (var i = 0; i < a.Length; i++)
                count += GetDec(a[i], b[i]);

            return count;
        }

        public Tuple<int, double> GetAns(string dhash)
        {
            var disList = new List<int>();

            var minid = 0;
            double mindis = int.MaxValue;
            foreach (var eachhero in HeroInfo)
            {
                var sumn = 0;
                double dis = 0;
                var count = eachhero.DHash.Count;
                if (count > 50) count = 50;

                for (var i = 0; i < count; i++)
                {
                    disList.Add(GetDifference(dhash, eachhero.DHash[i]));
                }

                disList.Sort();

                for (var i = 0; i < count / 5; i++)
                    sumn += disList[i];

                dis = 1.0 * sumn / (count / 5.0);

                if (dis < mindis)
                {
                    minid = eachhero.Id;
                    mindis = dis;
                }

                disList.Clear();
            }

            if (mindis < 30)
                return new Tuple<int, double>(minid, mindis);

            return new Tuple<int, double>(0, 0);
        }
    }
}