using Newtonsoft.Json;
using System.Collections.Generic;

namespace ForceApiCall
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Pagination
    {
        public int offset { get; set; }
        public int count { get; set; }
        public int limit { get; set; }
        public int total { get; set; }
        public object lastIdentifier { get; set; }
        public object firstIdentifier { get; set; }
    }

    public class Properties
    {
        public string orderId { get; set; }
        public string operationId { get; set; }
        public string workplaceId { get; set; }
        public string workplaceGroup { get; set; }
        public string materialId { get; set; }
        public string setupStartTs { get; set; }
        public string setupEndTs { get; set; }
        public string setupDuration { get; set; }
        public object persDuration { get; set; }
        public string realTimePerStrokeSec { get; set; }
        public string targetQuantity { get; set; }
        public string targetSetupTime { get; set; }
        public string targetStartDate { get; set; }
        public string phaseText { get; set; }
        public string phaseMnemonic { get; set; }
        public string statusText { get; set; }
        public string statusMnemonic { get; set; }
        public string statusTime { get; set; }
        public string tbSetupDuration { get; set; }
        public string setupTimeDeviationAbs { get; set; }
        public string setupTimeDeviationRel { get; set; }
        public string setupRate { get; set; }
        public object priority { get; set; }
        public string unit { get; set; }
    }

    public class AVODetailsRuestenWSAPLTT
    {
        public Properties properties { get; set; }
    }

    public class Embedded
    {
        [JsonProperty("AVODetailsRuesten-WSAPL-TT")]
        public List<AVODetailsRuestenWSAPLTT> AVODetailsSetupJVFC { get; set; }
    }

    public class Root
    {
        public Pagination pagination { get; set; }
        public Embedded _embedded { get; set; }
    }


}
