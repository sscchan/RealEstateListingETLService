using System;
using RealEstateListingETLService.Application.Services.Dtos;
using RealEstateListingETLService.Application.Services.Parameters;

namespace RealEstateListingETLService.Application.Services
{
    public interface IPropertyListingService
    {
        public Task<IEnumerable<PropertyListingSearchResultDto>> SearchProperties(PropertySearchCriteria propertySearchCriteria);
    }
}

