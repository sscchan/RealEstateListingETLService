using System;
using System.Diagnostics;
using System.Net;
using RealEstateListingETLService.Application.Entities;

namespace RealEstateListingETLService.Application.Services.Dtos
{
    public class PropertyListingSearchResultDto
    {
        public string Id { get; private set; }
        public PropertySearchResultAddressDto? Address { get; private set; }

        public uint? BedroomCount { get; private set; }
        public float? BathroomCount { get; private set; }
        public uint? CarparkCount { get; private set; }

        public string? LandSize { get; private set; }
        public string? Price { get; private set; }

        public Uri? DetailPageAddress { get; private set; }

        public PropertyListingSearchResultDto(string id, PropertySearchResultAddressDto? address, uint? bedroomCount, float? bathroomCount, uint? carparkCount, string? landSize, string? price, Uri? detailPageAddress)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Address = address;
            BedroomCount = bedroomCount;
            BathroomCount = bathroomCount;
            CarparkCount = carparkCount;
            LandSize = landSize;
            Price = price;
            DetailPageAddress = detailPageAddress;
        }
    }

    public class PropertySearchResultAddressDto
    {
        public string? StreetAddress { get; private set; }
        public string? Suburb { get; private set; }
        public string? State { get; private set; }
        public string? Postcode { get; private set; }

        public PropertySearchResultAddressDto(string? streetAddress, string? suburb, string? state, string? postcode)
        {
            StreetAddress = streetAddress;
            Suburb = suburb;
            State = state;
            Postcode = postcode;
        }
    }
}
