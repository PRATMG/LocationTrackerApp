using System;
using SQLite;

namespace LocationTrackerApp.Models
{
    public class LocationEntry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // store in UTC; format later for display
        public DateTime TimestampUtc { get; set; }
    }
}
