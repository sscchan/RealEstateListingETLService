using RealEstateListingETLService.Application.Entities;
using RealEstateListingETLService.Application.Services;
using RealEstateListingETLService.Application.Services.Parameters;
using RealEstateListingETLService.Application.Repositories;
using RealEstateListingETLService.Infrastructure.Repositories;
using RealEstateListingETLService.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using CsvHelper;
using System.Globalization;
using System.CommandLine;
using System;
using RealEstateListingETLService.CliApplication;
using System.Dynamic;

namespace RealEstateListingETLService
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Variables intended to be
            PropertySearchCriteria propertySearchCriteria = null;
            string? outputFilePath = null;
            var successfulCommandlineParse = false;

            var hostBuilder = new HostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureHostConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddEnvironmentVariables();
                    configurationBuilder.AddUserSecrets<Program>();
                })
                .ConfigureLogging((logging) =>
                {
                    logging.AddConsole();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient();
                    services.AddTransient<IGeocodingService, MappifyGeocodingService>();
                    services.AddTransient<IGeocodedPropertyListingService, GeocodedPropertyListingService>();
                    services.AddTransient<IPropertyListingService, DomainPropertyListingService>();
                    services.AddTransient<IGeocodedPropertyRepository, GeocodedPropertyLocalFileRepository>();

                    IConfiguration configuration = hostContext.Configuration;
                    services.AddOptions<DomainPropertyListingServiceOptions>().Bind(configuration.GetSection(DomainPropertyListingServiceOptions.DomainPropertyListingService));
                    services.AddOptions<MappifyGeocodingServiceOptions>().Bind(configuration.GetSection(MappifyGeocodingServiceOptions.MappifyGeocodingService));
                });

            var command = CommandLineParser.GetCommand(async (
                    IList<PropertyType> propertyTypes,
                    IList<SuburbNames> suburbNames,
                    uint minimumBedrooms,
                    uint minimumBathrooms,
                    uint minimumCarparks,
                    ulong maximumPrice,
                    FileInfo? databaseStorageFile,
                    FileInfo? outputFile) =>
                {
                    // Set real estate listing search critiera
                    propertySearchCriteria = new PropertySearchCriteria
                    (
                        propertyTypes: propertyTypes,
                        suburbs: suburbNames,
                        minimumNumberOfBedrooms: minimumBedrooms,
                        minimumNumberOfBathrooms: minimumBathrooms,
                        minimumNumberOfCarparks: minimumCarparks,
                        maximumPrice: maximumPrice
                    );


                    // Inject service parameters provided by CLI arguments.
                    outputFilePath = outputFile?.FullName;

                    hostBuilder.ConfigureServices(services =>
                    {
                        services.AddOptions<GeocodedPropertyLocalFileRepositoryOptions>().Configure(
                            (option) => option.LocalFilePath = databaseStorageFile?.FullName ?? Path.Join(Directory.GetCurrentDirectory(), "GeocodedPropertiesStore.json"));

                    });

                    successfulCommandlineParse = true;

                });

            // Parse the CLI arguments with the parser
            await command.InvokeAsync(args);

            // Terminate the application if the command line parsing operation is not successful
            // This include expected scenario where the application is called with "--help"
            if (!successfulCommandlineParse)
            {
                return;
            }

            var host = hostBuilder.Build();


            // CLI application logic
            var propertySearchService = host.Services.GetRequiredService<IGeocodedPropertyListingService>();

            await propertySearchService.UpdateWithNewListingByCriteria(propertySearchCriteria);

            // TODO: 2022-09-03 Opportunities for refactor here, this can be refactored into a CSVWriter Service with a separate GeocodedProperty -> CSV Records mapper definition
            using (var csvFile = File.Open(outputFilePath ?? Path.Join(Directory.GetCurrentDirectory(), "GeocodedPropertiesOutput.csv"), FileMode.Create))
            using (var streamWriter = new StreamWriter(csvFile))
            using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
            {
                var geocodedListing = await propertySearchService.GetListing();

                // The Automapper used by CSVHelper library throws a StackOverflow exception
                // during deserialization of the GeocodedProperty entity.
                // Manual mapping is done here instead.
                var records = new List<dynamic>();
                foreach (var listing in geocodedListing)
                {
                    dynamic record = new ExpandoObject();
                    record.Id = listing.Id;
                    record.Address = $"{listing.Address.StreetAddress}, {listing.Address.Suburb}, {listing.Address.State} {listing.Address.PostCode}";
                    record.Latitude = listing.GeographicCoordinate?.Latitude ?? null;
                    record.Longitude = listing.GeographicCoordinate?.Longitude ?? null;
                    record.BedroomCount = listing.BedroomCount;
                    record.BathroomCount = listing.BathroomCount;
                    record.ParkingSpaceCount = listing.CarparkCount;
                    record.Price = listing.Price;
                    record.OnMarket = listing.OnMarket;
                    record.DetailPageurl = listing.DetailPageAddress?.AbsoluteUri;
                    records.Add(record);
                }
                await csvWriter.WriteRecordsAsync(records);
            }
        }
    }
}