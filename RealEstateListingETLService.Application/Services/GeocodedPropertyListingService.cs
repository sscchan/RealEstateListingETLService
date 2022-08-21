using System;
using RealEstateListingETLService.Application.Entities;
using RealEstateListingETLService.Application.Services.Parameters;
using RealEstateListingETLService.Application.Repositories;


namespace RealEstateListingETLService.Application.Services
{
    public class GeocodedPropertyListingService : IGeocodedPropertyListingService
    {
        private readonly IGeocodedPropertyRepository _geocodedPropertyRepository;
        private readonly IPropertyListingService _propertyListingService;
        private readonly IGeocodingService _geocodingService;

        public GeocodedPropertyListingService(
            IGeocodedPropertyRepository geocodedPropertyRepository,
            IPropertyListingService propertyListingService,
            IGeocodingService geocodingService)
        {
            _geocodedPropertyRepository = geocodedPropertyRepository;
            _propertyListingService = propertyListingService;
            _geocodingService = geocodingService;
        }

        public async Task<IEnumerable<GeocodedProperty>> GetListing()
        {
            return await _geocodedPropertyRepository.GetAll();
        }

        public async Task UpdateWithNewListingByCriteria(PropertySearchCriteria propertySearchCriteria)
        {
            // Retrive current (non-geocoded) property listing
            var listedProperties = await _propertyListingService.SearchProperties(propertySearchCriteria);

            // Geocode current (non-geocoded) property listing
            var geocodedListedProperties = new List<GeocodedProperty>();
            foreach (var listedProperty in listedProperties)
            {
                // Only geocode address if the repository does not already contain an entry by that address
                var existingGeocodedProperty = await _geocodedPropertyRepository.FindByAddress(new PropertyAddress
                    (
                        streetAddress: listedProperty.Address?.StreetAddress,
                        suburb: listedProperty.Address?.Suburb,
                        state: listedProperty.Address?.State,
                        postCode: listedProperty.Address?.Postcode
                    ));

                GeographicCoordinate? listedPropertyGeographicCoordinate;
                if (existingGeocodedProperty != null && existingGeocodedProperty.GeographicCoordinate != null)
                {
                    listedPropertyGeographicCoordinate = existingGeocodedProperty.GeographicCoordinate;
                }
                else
                {
                    var geocodingResult = await _geocodingService.Geocode
                        (
                            streetAddress: listedProperty.Address?.StreetAddress,
                            suburb: listedProperty.Address?.Suburb,
                            state: listedProperty.Address?.State,
                            postCode: listedProperty.Address?.Postcode
                        );

                    listedPropertyGeographicCoordinate = new GeographicCoordinate
                        (
                            latitude: geocodingResult.Latitude,
                            longitude: geocodingResult.Longitude
                        );
                }

                geocodedListedProperties.Add(new GeocodedProperty
                    (
                        id: listedProperty.Id,
                        address: new PropertyAddress(
                            streetAddress: listedProperty.Address?.StreetAddress,
                            suburb: listedProperty.Address?.Suburb,
                            state: listedProperty.Address?.State,
                            postCode: listedProperty.Address?.Postcode),
                        geographicCoordinate: listedPropertyGeographicCoordinate,
                        bedroomCount: listedProperty.BedroomCount,
                        bathroomCount: listedProperty.BathroomCount,
                        carparkCount: listedProperty.CarparkCount,
                        landSize: listedProperty.LandSize,
                        price: listedProperty.Price,
                        detailPageAddress: listedProperty.DetailPageAddress,
                        onMarket: true
                    ));
            }

            // Reset all geocoded properties to "off-market", as the newly searched
            // and geocoded listing will be the only ones on market
            await _geocodedPropertyRepository.SetAllToOffMarket();
            await _geocodedPropertyRepository.AddOrUpdate(geocodedListedProperties);
        }
    }
}

