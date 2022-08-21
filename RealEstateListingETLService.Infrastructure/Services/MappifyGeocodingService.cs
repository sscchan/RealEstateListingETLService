using System;
using System.Net.Http.Json;
using RealEstateListingETLService.Application.Services;
using RealEstateListingETLService.Application.Services.Dtos;
using Microsoft.Extensions.Options;

namespace RealEstateListingETLService.Infrastructure.Services
{
    public class MappifyGeocodingServiceOptions
    {
        public const string MappifyGeocodingService = "MappifyGeocodingService";

        public string ApiEndpoint { get; set; } = String.Empty;
        public string ApiKey { get; set; } = String.Empty;
    }

    public class MappifyGeocodingService : IGeocodingService
    {
        private readonly MappifyGeocodingServiceOptions _options;
        private readonly HttpClient _httpClient;

        public MappifyGeocodingService(IOptions<MappifyGeocodingServiceOptions> mappifyGeocodingServiceOptions, HttpClient httpClient)
        {
            _options = mappifyGeocodingServiceOptions.Value;
            _httpClient = httpClient;
        }

        public async Task<GeocodingCoordinateResultDto> Geocode(string streetAddress, string suburb, string state, string postCode)
        {
            var requestPayload = new MappifyGeocodeApiRequestDto(
                streetAddress: streetAddress,
                suburb: suburb,
                state: state,
                postCode: postCode,
                apiKey: _options.ApiKey);

            var response = await _httpClient.PostAsync(_options.ApiEndpoint, JsonContent.Create(requestPayload));

            var responsePayload = await response.Content.ReadFromJsonAsync<MappifyGeocodeApiResponseDto>();

            return new GeocodingCoordinateResultDto
                (
                    latitude: responsePayload.result.Location.Lat,
                    longitude: responsePayload.result.Location.Lon,
                    confidence: responsePayload.Confidence
                );
        }
    }

    internal class MappifyGeocodeApiRequestDto
    {
        public string StreetAddress { get; private set; }
        public string Suburb { get; private set; }
        public string State { get; private set; }
        public string PostCode { get; private set; }
        public string ApiKey { get; private set; }

        public MappifyGeocodeApiRequestDto(string streetAddress, string suburb, string state, string postCode, string apiKey)
        {
            StreetAddress = streetAddress ?? throw new ArgumentNullException(nameof(streetAddress));
            Suburb = suburb;
            State = state;
            PostCode = postCode;
            ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }
    }

    // POCO representing the shape of Mappify Geocode API's response
    // See: https://mappify.io/docs/#api-Geocoding-PostApiRpcAddressGeocode
    internal class MappifyGeocodeApiResponseDto
    {
        public string Type { get; set; }
        public MappifyGeocodeApiResponseResultDto result { get; set; }
        public float Confidence { get; set; }
    }

    internal class MappifyGeocodeApiResponseResultDto
    {
        public string BuildingName { get; set; }
        public int? NumberFirst { get; set; }
        public int? NumberLast { get; set; }
        public string StreetName { get; set; }
        public string StreetType { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string PostCode { get; set; }
        public string StreetAddress { get; set; }
        public MappifyGeocodeApiResponseResultLocationDto Location { get; set; }
    }

    internal class MappifyGeocodeApiResponseResultLocationDto
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
}
 