using System;
using RealEstateListingETLService.Application.Entities;
using RealEstateListingETLService.Application.Services.Parameters;

namespace RealEstateListingETLService.Application.Services
{
    public interface IGeocodedPropertyListingService
    {
        public Task<IEnumerable<GeocodedProperty>> GetListing();

        public Task UpdateWithNewListingByCriteria(PropertySearchCriteria propertySearchCriteria);
    }
}

