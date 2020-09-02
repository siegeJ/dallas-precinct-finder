using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PrecinctFinder
{
    using PrecinctMap = Dictionary<int, List<AddressData>>;

    public class PrecinctFinder
    {
        static void Init(bool abWaitForInput = true)
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

            WaitForInput();
        }

        static void WaitForInput()
        {
            if (!Debugger.IsAttached)
            {
                Console.ReadLine();
            }
        }

        static HttpClient _httpClient = new HttpClient();

        static void Main(string[] args)
        {
            Init();

            var defaultListSize = 16;
            var dirList = new List<string>(defaultListSize);
            var filePaths = new List<String>(defaultListSize);

            foreach (var str in args)
            {
                if (File.Exists(str))
                {
                    filePaths.Add(str);
                }
                else if (Directory.Exists(str))
                {
                    dirList.Add(str);
                }
            }
            
            dirList.Add(Directory.GetCurrentDirectory());

            foreach(var dir in dirList)
            {
                if (Directory.Exists(dir))
                {
                    Console.WriteLine($"Searching {dir} for .txt files");

                    filePaths.AddRange(Directory.GetFiles(dir, "*.txt").Where(f => !f.Contains("PRECINCTS")).ToList());
                }
            }

            Console.WriteLine($"Found {filePaths.Count} Files:");

            if (filePaths.Count == 0)
            {
                Console.WriteLine($"No files found.");
            }

            foreach (var filePath in filePaths)
            {
                Console.WriteLine(filePath);
            }

            Console.WriteLine($"Press any key to continue...");
            WaitForInput();

            //Put this up here so we arent recreating it. Whatevs.

            //IDK just make something up
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.135 Safari/537.36");

            foreach (var filePath in filePaths)
            {
                Console.WriteLine($"Getting precincts in file {filePath}");

                //New lines with the precinct number
                var failedParses = new Dictionary<int, List<string>>();
                var addressDataByPrecinct = new PrecinctMap();
                var fileLines = File.ReadLines(filePath);

                var curZipcode = 0;
                foreach (var fileLine in fileLines)
                {
                    //blank
                    if (string.IsNullOrWhiteSpace(fileLine))
                    {
                        continue;
                    }

                    var addressLineTrimmed = fileLine.Trim();

                    //Zipcode line 
                    if (Regex.IsMatch(addressLineTrimmed, @"(^\d{5}$)|(^\d{9}$)|(^\d{5}-\d{4}$)"))
                    {
                        if (Int32.TryParse(addressLineTrimmed, out curZipcode))
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Getting precincts for zipcode {curZipcode}");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            Console.WriteLine($"Could not parse zipcode from { addressLineTrimmed }");
                        }

                        continue;
                    }

                    if (curZipcode != 0)
                    {
                        Console.WriteLine($"Getting precinct for {addressLineTrimmed}");

                        try
                        {
                            var queriedAddress = AddressData.QueryForAddressData(addressLineTrimmed, curZipcode);
                            if (queriedAddress != null)
                            {
                                var addrList = Utils.GetOrCreateListInDict(addressDataByPrecinct, queriedAddress.PrecinctNumber);
                                addrList.Add(queriedAddress);
                            }
                            else
                            {
                                var failedParseList = Utils.GetOrCreateListInDict(failedParses, curZipcode);
                                failedParseList.Add(addressLineTrimmed);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Failed to get precinct for {addressLineTrimmed}: {ex}");
                            Console.ForegroundColor = ConsoleColor.White;

                            if (Debugger.IsAttached)
                            {
                                throw ex;
                            }
                        }

                    }

                }
       
                var oldFileName = filePath.Replace(".txt", "");
                var newFileName = oldFileName + "_PRECINCTS.txt";
                Console.WriteLine($"Creating new file {newFileName} with precinct names");

                Utils.WriteFile(newFileName, addressDataByPrecinct, failedParses);
            }

            Console.WriteLine($"Finished, Press any key to exit...");
            Console.ReadLine();
        }

        public static async Task<PrecinctResponse> GetPrecinct(AddressData aAddressData)
        {
            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("PRECINCT_FINDER_ADDRESS_NUMBER", aAddressData.StreetNumber.ToString()),
                new KeyValuePair<string, string>("PRECINCT_FINDER_STREET_NAME", aAddressData.StreetName),
                new KeyValuePair<string, string>("PRECINCT_FINDER_ZIPCODE", aAddressData.ZipCode.ToString()),
                new KeyValuePair<string, string>("lang", "en"),
            };

            //postData.Add(new KeyValuePair<string, string>("PRECINCT_FINDER_PRE_DIRECTION", null));

            if (!string.IsNullOrEmpty(aAddressData.BuildingID))
            {
                postData.Add(new KeyValuePair<string, string>("PRECINCT_FINDER_APARTMENT_NUMBER", aAddressData.BuildingID));
            }

            if (!string.IsNullOrEmpty(aAddressData.City))
            {
                postData.Add(new KeyValuePair<string, string>("PRECINCT_FINDER_CITY", null));
            }

            using var content = new FormUrlEncodedContent(postData);
            content.Headers.Clear();
            content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            HttpResponseMessage response = await _httpClient.PostAsync("https://www.dallascountyvotes.org/ce/mobile/seam/resource/rest/precinct/findstreet", content);

            var responseString = await response.Content.ReadAsStringAsync();

            var precinctResponse = JsonConvert.DeserializeObject<PrecinctResponse>(responseString);
            return precinctResponse;
        }
    }
}
