using System;
using RealEstateListingETLService.Application.Entities;
using RealEstateListingETLService.Application.Repositories;
using RealEstateListingETLService.Application.Services;
using RealEstateListingETLService.Application.Services.Parameters;
using RealEstateListingETLService.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

namespace RealEstateListingETLService.Test.Integration.Infrastructure.Services
{
    [TestClass]
    public class DomainPropertyListingServiceTests
    {
        private IPropertyListingService _propertyListingService;

        [TestInitialize]
        public void Initialize()
        {
            _propertyListingService = new DomainPropertyListingService(Options.Create(
                new DomainPropertyListingServiceOptions()
                {
                    ForSaleListingEndpoint = Environment.GetEnvironmentVariable("DomainPropertyListingService__ForSaleListingEndpoint") ?? throw new ArgumentNullException("DomainPropertyListingsService Endpoint configuration required for Integration Test.")
                }));
        }

        [TestMethod]
        [Timeout(15 * 60 * 1000)]
        public async Task SearchProperties()
        {
            // Arrange
            var searchCriteria = new PropertySearchCriteria
                (
                    propertyTypes : new List<PropertyType>
                    {
                        PropertyType.House,
                        PropertyType.Townhouse,
                    },
                    suburbs : (SuburbNames[])Enum.GetValues(typeof(SuburbNames)),
                    minimumNumberOfBedrooms: 3,
                    minimumNumberOfBathrooms: 2,
                    minimumNumberOfCarparks: 1,
                    maximumPrice: 1000000
                );


            var propertySearchResultDao = await _propertyListingService.SearchProperties(searchCriteria);
        }
      
    }
}

