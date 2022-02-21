using System;

namespace LabaLINQ
{
    public record WeatherEvent
    {
        public string EventId { get; set; }
        public Type Type { get; set; }
        public Severity Severity { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string TimeZone { get; set; }
        public string AirportCode { get; set; }
        public string LocationLat{ get; set; }
        public string LocationLng{ get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }
}