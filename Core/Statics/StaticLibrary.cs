using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Statics
{
    public static class StaticLibrary
    {
        public static List<string[]> LoadCsvFile(string filePath)
        {
            List<string[]> searchList = new List<string[]>();
            using (var reader = new StreamReader(File.OpenRead(filePath)))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    string[] array = line.Split(',');
                    searchList.Add(array);
                }
            }
            return searchList;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        public static Int64 DateTimeToUnixTimeStamp(DateTime date)
        {
            return (Int64)(date.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static bool DateTimeDistTest(DateTime d1, DateTime d2, double nbHours)
        {
            Int64 d1_int = DateTimeToUnixTimeStamp(d1);
            Int64 d2_int = DateTimeToUnixTimeStamp(d2);
            return Math.Abs(d1_int - d2_int) / 3600.0 < nbHours;
        }
        public static List<Type1> GetReversedKeys<Type1, Type2>(this IDictionary<Type1, Type2> x)
        {
            List<Type1> res = new List<Type1> { };
            foreach (Type1 t1 in x.Keys) { res.Add(t1); }
            res.Reverse();
            return res;
        }
    }
}
