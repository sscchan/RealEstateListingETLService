using System;
using RealEstateListingETLService.Application.Entities;

namespace RealEstateListingETLService.Application.Repositories
{
    public interface IGeocodedPropertyRepository
    {
        public Task<GeocodedProperty?> FindByAddress(PropertyAddress address);

        /// <summary>
        /// Returns all GeocodedProperty entries in the repository
        /// </summary>
        public Task<IEnumerable<GeocodedProperty>> GetAll();

        public Task<IEnumerable<GeocodedProperty>> AddOrUpdate(IEnumerable<GeocodedProperty> geocodedProperties);

        /// <summary>
        /// Set all of the entries' in the repository's "OnMarket" field to false
        /// </summary>
        public Task SetAllToOffMarket();
    }
}

