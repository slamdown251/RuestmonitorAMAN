using System.Collections.Generic;

namespace ForceApiCall.DeserializeClasses
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Pagination
    {
        public int offset { get; set; }
        public int count { get; set; }
        public int limit { get; set; }
        public object lastIdentifier { get; set; }
        public object firstIdentifier { get; set; }
        public int total { get; set; }
    }

    public class CapacityGroup
    {
        public string id { get; set; }
        public string number { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string erpContextId { get; set; }
    }

    public class ProductionLine
    {
        public string id { get; set; }
        public string number { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string erpContextId { get; set; }
    }

    public class OperatingState
    {
        public string id { get; set; }
        public string description { get; set; }
        public string code { get; set; }
        public string workplaceStateId { get; set; }
    }

    public class Properties
    {
        public string id { get; set; }
        public string number { get; set; }
        public string description { get; set; }
        public string erpContextId { get; set; }
        public CapacityGroup capacityGroup { get; set; }
        public ProductionLine productionLine { get; set; }
        public OperatingState operatingState { get; set; }
        public string workplaceType { get; set; }
    }

    public class Workplace
    {
        public Properties properties { get; set; }
    }

    public class Embedded
    {
        public List<Workplace> workplaces { get; set; }
    }

    public class Root
    {
        public Pagination pagination { get; set; }
        public Embedded _embedded { get; set; }
    }
}
