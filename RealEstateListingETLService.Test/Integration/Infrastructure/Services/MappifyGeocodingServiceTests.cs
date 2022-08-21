using System;
using RealEstateListingETLService.Application.Services;
using RealEstateListingETLService.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace RealEstateListingETLService.Test.Integration.Infrastructure.Services
{
    [TestClass]
    public class MappifyGeocodingServiceTests
    {
        private HttpClient _httpClient;
        private IGeocodingService _geocodingService;

        [TestInitialize]
        public void Initialize()
        {
            var mappifyGeocodingServiceOptions = Options.Create(new MappifyGeocodingServiceOptions
            {
                ApiEndpoint = Environment.GetEnvironmentVariable($"{MappifyGeocodingServiceOptions.MappifyGeocodingService}__ApiEndpoint") ?? "https://mappify.io/api/rpc/address/geocode/",
                ApiKey = Environment.GetEnvironmentVariable($"{MappifyGeocodingServiceOptions.MappifyGeocodingService}__ApiKey") ?? throw new ArgumentNullException("MappifyGeocodingService API Key required for Integration Test.")
            });

            _httpClient = new HttpClient();
            _geocodingService = new MappifyGeocodingService(mappifyGeocodingServiceOptions, _httpClient);
        }

        [TestMethod]
        public async Task Geocode()
        {
            var result = await _geocodingService.Geocode
                (
                    streetAddress: "11 Kopoola Cr",
                    suburb: "gillies Plains",
                    state: "SA",
                    postCode: "5086"
                );

            Assert.IsNotNull(result);
            // Verify that the Geogarphical Coordinates are somewhere around Adelaide
            // The Lat/Long check ranges are intentionally large to verify Geocoding results returned
            // is "around Adelaide, in South Australia" since the focus of the test is in external service
            // connectivity, not the returned results' accuracy.
            Assert.IsTrue(136.48 <= result.Longitude && result.Longitude <= 140.13);
            Assert.IsTrue(-36.185 <= result.Latitude && result.Latitude <= -32.79);
        }

    }
}

