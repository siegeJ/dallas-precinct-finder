using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PrecinctFinder
{
    class PrecinctFinder
    {
        private static List<string> CommonAddressAbb = new List<string>()
        {
            "AVE", "BLVD", "CTR", "CIR","CT","DR","EXPY","HTS","HWY","IS","JCT","LK","LN","MTN","PKWY","PL","PLZ","RDG","RD","SQ","ST","STA","TER","TPKE","VLY","WAY"
        };

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Dallas County Precinct Finder");
            Console.WriteLine("By: CJ Stankovich https://github.com/siegeJ");
            Console.ForegroundColor = ConsoleColor.White;


            Console.WriteLine($"*****************");
            Console.WriteLine($"Place .txt files with addresses in same directory as this executable");
            Console.WriteLine($"Format like so with zipcode followed by addresses on a new line...");
            Console.WriteLine($"For best results, remove everything after the street name");
            Console.WriteLine($"*****************");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"75041");
            Console.WriteLine("1001 Towngate");
            Console.WriteLine("1003 Towngate");
            Console.WriteLine("1005 Towngate");
            Console.WriteLine($"75048");
            Console.WriteLine("2001 Riverchase");
            Console.WriteLine("2003 Riverchase");
            Console.WriteLine("2005 Riverchase");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine($"Press any key to continue...");
            Console.ReadLine();

            var filePaths = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.txt").Where(f => !f.Contains("PRECINCTS")).ToList();

            Console.WriteLine($"Found {filePaths.Count} Files:");

            if (filePaths.Count == 0)
            {
                Console.WriteLine($"No files found in {Directory.GetCurrentDirectory()}");
            }

            foreach (var filePath in filePaths)
            {
                Console.WriteLine(filePath);
            }

            Console.WriteLine($"Press any key to continue...");
            Console.ReadLine();

            //Put this up here so we arent recreating it. Whatevs.
            using var httpClient = new HttpClient();
            //IDK just make something up
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.135 Safari/537.36");

            foreach (var filePath in filePaths)
            {
                Console.WriteLine($"Getting precincts in file {filePath}");

                //New lines with the precinct number
                List<string> linesWithPrecincts = new List<string>();

                var fileLines = File.ReadLines(filePath);

                string zipcode = "";
                foreach (var fileLine in fileLines)
                {
                    //blank
                    if (string.IsNullOrEmpty(fileLine))
                    {
                        continue;
                    }

                    var addressLineTrimmed = fileLine.Trim();

                    //Zipcode line 
                    if (Regex.IsMatch(addressLineTrimmed, @"(^\d{5}$)|(^\d{9}$)|(^\d{5}-\d{4}$)"))
                    {
                        zipcode = addressLineTrimmed;

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Getting precincts for zipcode {zipcode}");
                        Console.ForegroundColor = ConsoleColor.White;

                        linesWithPrecincts.Add(zipcode);
                        continue;
                    }

                    if (!string.IsNullOrEmpty(zipcode))
                    {
                        Console.WriteLine($"Getting precinct for {addressLineTrimmed}");

                        try
                        {
                            List<string> seperatedAddress = addressLineTrimmed.Split(' ').ToList();

                            //Some assumptions here, assume first is always number, last is always Ln, Street, Blvd, etc.
                            seperatedAddress.RemoveAll(c => CommonAddressAbb.Contains(c.ToUpper()));
                            var addressNumber = seperatedAddress[0];

                            string streetName = "";
                            for (int i = 1; i < seperatedAddress.Count; i++)
                            {
                                streetName += seperatedAddress[i] + " ";
                            }

                            //Maybe make this all async tasks later but I'm afraid of DDOSing the server :)
                            var precinctResponse = GetPrecinct(addressNumber, streetName.Trim(), zipcode, httpClient).GetAwaiter().GetResult();
                            linesWithPrecincts.Add($"{addressLineTrimmed} {precinctResponse.streets.FirstOrDefault()?.precinctname}");
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Failed to get precinct for {addressLineTrimmed}: {ex}");
                            Console.ForegroundColor = ConsoleColor.White;
                            linesWithPrecincts.Add($"{addressLineTrimmed} ERROR");
                        }

                
                    }

                }

       
                var oldFileName = filePath.Replace(".txt", "");
                var newFileName = oldFileName + "_PRECINCTS.txt";
                Console.WriteLine($"Creating new file {newFileName} with precinct names");
                WriteFile(newFileName, linesWithPrecincts);

            }

            Console.WriteLine($"Finished, Press any key to exit...");
            Console.ReadLine();


        }


        private static void WriteFile(string fileName, IEnumerable<string> lines)
        {
            using TextWriter tw = new StreamWriter(fileName);

            foreach (String s in lines)
            {
                tw.WriteLine(s);
            }
        }

        private static async Task<PrecinctResponse> GetPrecinct(string addressNumber, string streetName, string zipCode, HttpClient httpClient)
        {
            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("PRECINCT_FINDER_ADDRESS_NUMBER", addressNumber));
            //postData.Add(new KeyValuePair<string, string>("PRECINCT_FINDER_PRE_DIRECTION", null));
            postData.Add(new KeyValuePair<string, string>("PRECINCT_FINDER_STREET_NAME", streetName));
            //postData.Add(new KeyValuePair<string, string>("PRECINCT_FINDER_APARTMENT_NUMBER", null));
            //postData.Add(new KeyValuePair<string, string>("PRECINCT_FINDER_CITY", null));
            postData.Add(new KeyValuePair<string, string>("PRECINCT_FINDER_ZIPCODE", zipCode));
            postData.Add(new KeyValuePair<string, string>("lang", "en"));


            using var content = new FormUrlEncodedContent(postData);
            content.Headers.Clear();
            content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            HttpResponseMessage response = await httpClient.PostAsync("https://www.dallascountyvotes.org/ce/mobile/seam/resource/rest/precinct/findstreet", content);

            var responseString = await response.Content.ReadAsStringAsync();

            var precinctResponse = JsonConvert.DeserializeObject<PrecinctResponse>(responseString);

            return precinctResponse;
        }
    }
}
