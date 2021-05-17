using System;

namespace MyStreamingApp.Utils.Extensions
{
    public static class StringExtensions
    {
        public static bool IsTwin(this string a, string b)
        {
            if (string.IsNullOrEmpty(a)) throw new ArgumentNullException("a is null");
            if (string.IsNullOrEmpty(b)) throw new ArgumentNullException("b is null");

            char[] first = a.ToLower().ToCharArray();
            char[] second = b.ToLower().ToCharArray();

            if (first.Length != second.Length)
                return false;

            Array.Sort(first);
            Array.Sort(second);

            return new string(first).Equals(new string(second));

            //for (int i = 0; i < first.Length; i++)
            //{
            //    if (first[i] != second[i])
            //    {
            //        return false;
            //    }
            //}
            //return true;
        }
    }
}
