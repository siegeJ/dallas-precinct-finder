using System.Collections.Generic;

namespace PrecinctFinder
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class CategoryType
    {
        public int id { get; set; }
        public string name { get; set; }
        public int sortOrder { get; set; }
    }

    public class Category
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool display { get; set; }
        public bool displayIsLocked { get; set; }
        public string fileName { get; set; }
        public string pluginVersion { get; set; }
        public string description { get; set; }
        public bool isDeployableAsPlugin { get; set; }
        public CategoryType categoryType { get; set; }
    }

    public class PageName
    {
        public string pageName { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Category category { get; set; }
        public bool active { get; set; }
    }

    public class PageTextName
    {
        public string pageTextName { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public PageName pageName { get; set; }
    }

    public class PRECINCTFINDERSTREETNODATAMSG
    {
        public string pageTextValue { get; set; }
        public string language { get; set; }
        public bool rendered { get; set; }
        public string modified { get; set; }
        public PageTextName pageTextName { get; set; }
    }

    public class CategoryType2
    {
        public int id { get; set; }
        public string name { get; set; }
        public int sortOrder { get; set; }
    }

    public class Category2
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool display { get; set; }
        public bool displayIsLocked { get; set; }
        public string fileName { get; set; }
        public string pluginVersion { get; set; }
        public string description { get; set; }
        public bool isDeployableAsPlugin { get; set; }
        public CategoryType2 categoryType { get; set; }
    }

    public class PageName2
    {
        public string pageName { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Category2 category { get; set; }
        public bool active { get; set; }
    }

    public class PageTextName2
    {
        public string pageTextName { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public PageName2 pageName { get; set; }
    }

    public class PRECINCTFINDERSTREETCRUMB
    {
        public string pageTextValue { get; set; }
        public string language { get; set; }
        public bool rendered { get; set; }
        public string modified { get; set; }
        public PageTextName2 pageTextName { get; set; }
    }

    public class CategoryType3
    {
        public int id { get; set; }
        public string name { get; set; }
        public int sortOrder { get; set; }
    }

    public class Category3
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool display { get; set; }
        public bool displayIsLocked { get; set; }
        public string fileName { get; set; }
        public string pluginVersion { get; set; }
        public string description { get; set; }
        public bool isDeployableAsPlugin { get; set; }
        public CategoryType3 categoryType { get; set; }
    }

    public class PageName3
    {
        public string pageName { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Category3 category { get; set; }
        public bool active { get; set; }
    }

    public class PageTextName3
    {
        public string pageTextName { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public PageName3 pageName { get; set; }
    }

    public class PRECINCTFINDERSTREETH1
    {
        public string pageTextValue { get; set; }
        public string language { get; set; }
        public bool rendered { get; set; }
        public string modified { get; set; }
        public PageTextName3 pageTextName { get; set; }
    }

    public class PageTexts
    {
        public PRECINCTFINDERSTREETNODATAMSG PRECINCT_FINDER_STREET_NODATA_MSG { get; set; }
        public PRECINCTFINDERSTREETCRUMB PRECINCT_FINDER_STREET_CRUMB { get; set; }
        public PRECINCTFINDERSTREETH1 PRECINCT_FINDER_STREET_H1 { get; set; }
    }

    public class PageLabelName
    {
        public string pageLabelName { get; set; }
        public string title { get; set; }
        public string description { get; set; }
    }

    public class PageLabel
    {
        public string pageLabelValue { get; set; }
        public string language { get; set; }
        public bool rendered { get; set; }
        public int labelOrder { get; set; }
        public string modified { get; set; }
        public PageLabelName pageLabelName { get; set; }
    }

    public class Street
    {
        public string address { get; set; }
        public string predir { get; set; }
        public string street { get; set; }
        public string type { get; set; }
        public string postdir { get; set; }
        public string city { get; set; }
        public string zipcode { get; set; }
        public int precinct { get; set; }
        public string precinctname { get; set; }
    }

    public class PrecinctResponse
    {
        public string pageName { get; set; }
        public PageTexts pageTexts { get; set; }
        public List<PageLabel> pageLabels { get; set; }
        public List<Street> streets { get; set; }
    }
}
