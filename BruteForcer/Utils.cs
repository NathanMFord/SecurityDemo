using System;
using System.Collections.Generic;

namespace BruteForcer {
    public static class Utils {
        private static Random rng = new Random();
        
        // https://stackoverflow.com/questions/273313
        public static void Shuffle<T>(this IList<T> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                T val = list[k];
                list[k] = list[n];
                list[n] = val;
            }
        }
    }
}