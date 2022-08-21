using System;
namespace RealEstateListingETLService.Application.Services.Dtos
{
    public class GeocodingCoordinateResultDto
    {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double Confidence { get; private set; }

        public GeocodingCoordinateResultDto(double latitude, double longitude, double confidence)
        {
            Latitude = latitude;
            Longitude = longitude;
            Confidence = confidence;
        }
    }
}

