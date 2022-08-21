using System;
using RealEstateListingETLService.Application.Entities;

namespace RealEstateListingETLService.Application.Services.Parameters
{
    public class PropertySearchCriteria
    {
        public IEnumerable<PropertyType> PropertyTypes { get; private set; }

        public IEnumerable<SuburbNames> Suburbs { get; private set; }
        public uint MinimumNumberOfBedrooms { get; private set; }
        public uint MinimumNumberOfBathrooms { get; private set; }
        public uint MinimumNumberOfCarparks { get; private set; }

        public ulong MaximumPrice { get; private set; }

        public PropertySearchCriteria(
            IEnumerable<PropertyType> propertyTypes,
            IEnumerable<SuburbNames> suburbs,
            uint minimumNumberOfBedrooms,
            uint minimumNumberOfBathrooms,
            uint minimumNumberOfCarparks,
            ulong maximumPrice)
        {
            PropertyTypes = propertyTypes ?? throw new ArgumentNullException(nameof(propertyTypes));
            Suburbs = suburbs ?? throw new ArgumentNullException(nameof(suburbs));
            MinimumNumberOfBedrooms = minimumNumberOfBedrooms;
            MinimumNumberOfBathrooms = minimumNumberOfBathrooms;
            MinimumNumberOfCarparks = minimumNumberOfCarparks;
            MaximumPrice = maximumPrice;
        }
    }
}

