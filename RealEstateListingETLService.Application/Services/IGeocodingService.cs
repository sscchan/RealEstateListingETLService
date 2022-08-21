using System;
using RealEstateListingETLService.Application.Services.Dtos;

namespace RealEstateListingETLService.Application.Services
{
    public interface IGeocodingService
    {
        public Task<GeocodingCoordinateResultDto> Geocode(string streetAddress, string suburb, string state, string postCode);
    }
}
