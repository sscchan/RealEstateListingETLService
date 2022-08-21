using System;
using RealEstateListingETLService.Application.Entities;
using RealEstateListingETLService.Application.Repositories;
using RealEstateListingETLService.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RealEstateListingETLService.Test.Integration.Infrastructure.Repositories
{
    [TestClass]
    public class GeocodedPropertyRepositoryTests
    {
        private IGeocodedPropertyRepository _geocodedPropertyLocalFileRepository;

        [TestInitialize]
        public void Initialize()
        {
            var repositoryOptions = Options.Create(new GeocodedPropertyLocalFileRepositoryOptions()
            {
                LocalFilePath = Path.Join(Directory.GetCurrentDirectory(), "GeocodedPropertyRepositoryTestsRepositoryFile.json")
            });

            _geocodedPropertyLocalFileRepository = new GeocodedPropertyLocalFileRepository(repositoryOptions);
        }

        [TestMethod]
        public async Task Add_Find_SetOffMarket_GetAll()
        {
            // AddOrUpdate
            var godfreyStProperty = new GeocodedProperty
                (
                    id: "00001",
                    address: new PropertyAddress("13 Godfrey St", "Darlington", "SA", "5047"),
                    geographicCoordinate: new GeographicCoordinate(-35.03131, 138.55655),
                    bedroomCount: 2,
                    bathroomCount: 2,
                    carparkCount: 2,
                    landSize: "455m²",
                    price: "350,000",
                    detailPageAddress: new Uri("https://www.domain.com.au/13-godfrey-street-darlington-sa-5047-2017953596"),
                    onMarket: true
                );

            var kyleStProperty = new GeocodedProperty
                (
                    id: "00007",
                    address: new PropertyAddress("1/5 Kyle St", "Glenside", "SA", "5065"),
                    geographicCoordinate: new GeographicCoordinate(-33.94030, 138.63829),
                    bedroomCount: 3,
                    bathroomCount: 2,
                    carparkCount: 1,
                    landSize: null,
                    price: "AUCTION ON SITE",
                    detailPageAddress: new Uri("https://www.domain.com.au/1-5-kyle-street-glenside-sa-5065-2018018373"),
                    onMarket: false
                );

            var geocodedProperties = new List<GeocodedProperty>()
            {
                godfreyStProperty, kyleStProperty
            };

            await _geocodedPropertyLocalFileRepository.AddOrUpdate(geocodedProperties);

            // FindByAddress
            var findByAddressKyleStResult = await _geocodedPropertyLocalFileRepository.FindByAddress(new PropertyAddress("1/5 Kyle St", "Glenside", "SA", "5065"));
            Assert.AreEqual(kyleStProperty.Id, findByAddressKyleStResult?.Id);

            // SetAllToOffMarket
            Assert.IsTrue(godfreyStProperty.OnMarket);
            await _geocodedPropertyLocalFileRepository.SetAllToOffMarket();
            var findByAddressGodfreyStResult = await _geocodedPropertyLocalFileRepository.FindByAddress(new PropertyAddress("13 Godfrey St", "Darlington", "SA", "5047"));
            Assert.IsFalse(findByAddressKyleStResult?.OnMarket);

            // GetAll
            var getAllResult = await _geocodedPropertyLocalFileRepository.GetAll();
            Assert.IsTrue(getAllResult.Any(geocodedProperty => geocodedProperty.Id == godfreyStProperty.Id));
            Assert.IsTrue(getAllResult.Any(geocodedProperty => geocodedProperty.Id == kyleStProperty.Id));
        }
    }
}

