using System;
using RealEstateListingETLService.Application.Entities.Library;

namespace RealEstateListingETLService.Application.Entities
{
    public class GeocodedProperty
    {
        public string Id { get; private set; }
        public PropertyAddress Address { get; private set; }
        public GeographicCoordinate? GeographicCoordinate { get; private set; }

        public uint? BedroomCount { get; private set; }
        public float? BathroomCount { get; private set; }
        public uint? CarparkCount { get; private set; }

        public string? LandSize { get; private set; }
        public string? Price { get; private set; }

        public Uri? DetailPageAddress { get; private set; }
        public bool OnMarket { get; private set; }

        public GeocodedProperty(
            string id,
            PropertyAddress address,
            GeographicCoordinate geographicCoordinate,
            uint? bedroomCount,
            float? bathroomCount,
            uint? carparkCount,
            string? landSize,
            string? price,
            Uri? detailPageAddress,
            bool onMarket)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Address = address;
            GeographicCoordinate = geographicCoordinate;
            BedroomCount = bedroomCount;
            BathroomCount = bathroomCount;
            CarparkCount = carparkCount;
            LandSize = landSize;
            Price = price;
            DetailPageAddress = detailPageAddress;
            OnMarket = onMarket;
        }

        public void SetAsOffMarket()
        {
            OnMarket = false;
        }
    }
}

