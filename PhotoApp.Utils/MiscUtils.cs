using System;
using System.Collections.Generic;

namespace PhotoApp.Utils
{
    public class MiscUtils
    {
        /// <summary>
        /// Break a List<T/> into multiple chunks. The <paramref name="list="/> is cleared out and the items are moved into the returned chunks
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list to be chunked</param>
        /// <param name="chunkSize">The size of each chunk</param>
        /// <returns>A list of chunks</returns>
        public static List<List<T>> BreakIntoChunks<T>(List<T> list, int chunkSize)
        {
            if (chunkSize <= 0)
            {
                throw new ArgumentException("Chunk size must be greater than 0");
            }
            List<List<T>> retVal = new List<List<T>>();
            while (list.Count > 0)
            {
                int count = list.Count > chunkSize ? chunkSize : list.Count;
                retVal.Add(list.GetRange(0, count));
                list.RemoveRange(0, count);
            }
            return retVal;
        }
    }
}