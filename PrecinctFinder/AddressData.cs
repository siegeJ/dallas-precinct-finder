
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrecinctFinder
{
    public class AddressData : IComparable<AddressData>
    {
        private static List<string> CommonStreetTypeAbbv = new List<string>()
        {
            "CIR", "AVE", "BLVD", "CTR","CT","DR","EXPY","HTS","HWY","IS","JCT","LK","LN","MTN","PKWY","PL","PLZ","RDG","RD","SQ","ST","STA","TER","TPKE","VLY","WAY"
        };

        // It's easier to split on the common address if there's a preceeding space before it.
        // Addresses are almost always in the following format: "123 Jenkins Dr, Apt #420, Happytown TX, 71717"
        // Because of this, if we compare against somethign like " DR" instead of "dr", we can provide an easier place
        // to split the string and extract things like Addr line 2s.
        // Went with a second list to not pollute any expected usages of the first.
        static readonly List<string> _CommonAddressAbb_WithSpace = null;

        static AddressData()
        {
            _CommonAddressAbb_WithSpace = new List<string>(CommonStreetTypeAbbv.Count);

            foreach(var str in CommonStreetTypeAbbv)
            {
                _CommonAddressAbb_WithSpace.Add($" {str}");
            }
        }

        public enum BuildingType
        {
            Single_Household,
            MultiFamily_Complex,
            BT_COUNT
        }

        public BuildingType Type { get; private set; }

        public int StreetNumber { get; private set; }
        public int PrecinctNumber { get; private set; }
        public string StreetName { get; private set; }
        public string StreetType { get; private set; }
        public string City { get; private set; }

        // Something like an Apt #69 or Ste. 220. Many different flavors of this, so use a string over a numeric
        public string BuildingID { get; private set; }
        public int ZipCode { get; private set; }

        public string AddrLineOne { get { return $"{StreetNumber} {StreetName} {StreetType}"; }}
        public string AddrLineTwo
        {
            get
            {
                if (Type == BuildingType.Single_Household)
                {
                    return string.Empty;
                }
                else
                {
                    return BuildingID;
                }
            }
        }

        public override string ToString() { return $"{AddrLineOne} {AddrLineTwo}, {City}, {ZipCode}"; }

        public int CompareTo(AddressData aRhs)
        {
            if (City != aRhs.City)
            {
                return string.Compare(City, aRhs.City);
            }

            if (PrecinctNumber != aRhs.PrecinctNumber)
            {
                return PrecinctNumber.CompareTo(aRhs.PrecinctNumber);
            }

            if (ZipCode != aRhs.ZipCode)
            {
                return ZipCode.CompareTo(aRhs.ZipCode);
            }

            if (StreetName != aRhs.StreetName)
            {
                return string.Compare(StreetName, aRhs.StreetName);
            }

            if (StreetNumber != aRhs.StreetNumber)
            {
                return StreetNumber.CompareTo(aRhs.StreetNumber);
            }

            return string.Compare(BuildingID, aRhs.BuildingID);
        }

        public static AddressData QueryForAddressData(string aRawData, int aZipCode)
        {
            if (aZipCode <= 0)
            {
                return null;
            }

            AddressData returnVal = null;

            var streetnumberIdx = aRawData.IndexOf(' ');

            var streetNumberSubstring = aRawData.Substring(0, streetnumberIdx).Trim();
            int streetNumber;
            if (streetnumberIdx > 0  && int.TryParse(streetNumberSubstring, out streetNumber))
            {
                var streetName = string.Empty;
                int idxOfStreetType = -1;

                // Try to split out Addr line 1 from any 2 that may exist.
                // Use the Street Type for this.
                foreach (var addrType in _CommonAddressAbb_WithSpace)
                {
                    idxOfStreetType = aRawData.IndexOf(addrType, StringComparison.CurrentCultureIgnoreCase);
                    if (idxOfStreetType > 0)
                    {
                        streetName = aRawData.Substring(streetnumberIdx, idxOfStreetType - addrType.Length + 1).Trim();
                        break;
                    }
                }

                if (idxOfStreetType != -1)
                {
                    var addrQuery = new AddressData
                    {
                        ZipCode = aZipCode,
                        StreetName = streetName,
                        StreetNumber = streetNumber
                    };
                    //Maybe make this all async tasks later but I'm afraid of DDOSing the server :)
                    var precinctResponse = PrecinctFinder.GetPrecinct(addrQuery).GetAwaiter().GetResult();
                    var precinctString = precinctResponse.streets.Count > 0 ? precinctResponse.streets.First().precinctname.Replace("PRECINCT ", "") : string.Empty;

                    int precinctNumber;
                    if (int.TryParse(precinctString, out precinctNumber))
                    {
                        var buildingResult = precinctResponse.streets[0];

                        // Success!
                        // TODO: use an init ctor instead of creating more garbage.
                        returnVal = new AddressData()
                        {
                            ZipCode = aZipCode,
                            PrecinctNumber = precinctNumber,
                            StreetNumber = streetNumber,
                            StreetName = buildingResult.street,
                            StreetType = buildingResult.type,
                            City = buildingResult.city,
                        };
                    }

                }
            }

            return returnVal;
        }
    }
}
