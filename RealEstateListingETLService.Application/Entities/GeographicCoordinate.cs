using System;
using System.Diagnostics.Metrics;
using System.IO;
using System.Reflection.Emit;
using RealEstateListingETLService.Application.Entities.Library;

namespace RealEstateListingETLService.Application.Entities
{
    public class GeographicCoordinate : ValueObject
    {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        public GeographicCoordinate(double latitude, double longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Latitude;
            yield return Longitude;
        }
    }
}