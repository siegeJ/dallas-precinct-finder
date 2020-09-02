using System;
using System.Collections.Generic;
using System.IO;

namespace PrecinctFinder
{
    using PrecinctMap = Dictionary<int, List<AddressData>>;

    public static class Utils
    {
        public static List<T> GetOrCreateListInDict<Key, T>(Dictionary<Key, List<T>> aDictionary, Key aKey)
        {
            List<T> returnList;
            if (!aDictionary.TryGetValue(aKey, out returnList))
            {
                returnList = new List<T>(64);
                aDictionary[aKey] = returnList;
            }

            return returnList;
        }

        public static void WriteFile(string aFileName, PrecinctMap aPrecinctMap, Dictionary<int, List<string>> aFailedParsesByZip)
        {
            var linesToWrite = new List<string>(512);

            foreach (var kvp in aPrecinctMap)
            {
                linesToWrite.Add($"PRECINCT {kvp.Key}");

                kvp.Value.Sort();

                foreach (var house in kvp.Value)
                {
                    linesToWrite.Add($"\t { house.AddrLineOne }");
                }
            }

            if (aFailedParsesByZip.Count > 0)
            {
                linesToWrite.Add("UNKNOWN PRECINCTS");

                foreach (var kvp in aFailedParsesByZip)
                {
                    linesToWrite.Add(kvp.Key.ToString());

                    foreach (var failedVal in kvp.Value)
                    {
                        linesToWrite.Add($"\t {failedVal}");
                    }
                }
            }

            WriteFile(aFileName, linesToWrite);
        }

        private static void WriteFile(string fileName, IEnumerable<string> lines)
        {
            using var tw = new StreamWriter(fileName);

            foreach (String s in lines)
            {
                tw.WriteLine(s);
            }
        }
    }
}
