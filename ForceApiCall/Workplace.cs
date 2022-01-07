using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ForceApiCall
{
    public class Workplace
    {
        [Key]
        public int PrimaryId { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public string TargetSetupTime { get; set; }
        public List<SetupData> Setups { get; set; }
    }

    public class SetupData
    {
        [Key]
        public int PrimaryId { get; set; }
        public DateTime setupStartTs { get; set; }
        public DateTime setupEndTs { get; set; }
        public string setupDuration { get; set; }
        public string setupTimeDeviationAbs { get; set; }
        public string setupTimeDeviationRel { get; set; }
        public DateTime targetStartDate { get; set; }
        public string setupRate { get; set; }

        public int WorkplaceId { get; set; }
        public Workplace Workplace { get; set; }
    }
}
