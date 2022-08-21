using System;
using RealEstateListingETLService.Application.Entities;
using System.CommandLine;

namespace RealEstateListingETLService.CliApplication
{
    public class CommandLineParser
    {
        public static RootCommand GetCommand(Action<IList<PropertyType>, IList<SuburbNames>, uint, uint, uint, ulong, FileInfo?, FileInfo?> commandHandler)
        {
            var rootCommand = new RootCommand("Application for retriving and geocoding Australian real estate property listing.");

            var propertyTypesOption = new Option<IList<PropertyType>>
                (
                    name: "--property-types",
                    description: "The property types to be included in the search.",
                    getDefaultValue: () => new List<PropertyType> { PropertyType.House, PropertyType.Townhouse }
                );

            var suburbNamesOption = new Option<IList<SuburbNames>>
                (
                    name: "--surburbs",
                    description: "The property types to be included in the search.",
                    getDefaultValue: () => (SuburbNames[])Enum.GetValues(typeof(SuburbNames))
                );

            var minimumBedroomOption = new Option<uint>
                (
                    name: "--minimum-bedrooms",
                    description: "The minimum number of bedrooms required.",
                    getDefaultValue: () => 0
                );

            var minimumBathroomOption = new Option<uint>
                (
                    name: "--minimum-bathrooms",
                    description: "The minimum number of bathrooms required.",
                    getDefaultValue: () => 0
                );

            var minimumCarparkOption = new Option<uint>
                (
                    name: "--minimum-carparks",
                    description: "The minimum number of car parking space required.",
                    getDefaultValue: () => 0
                );

            var maximumPriceOption = new Option<ulong>
                (
                    name: "--maximum-price",
                    description: "The maximum price of listing.",
                    getDefaultValue: () => 10000000
                );

            var storageFileOption = new Option<FileInfo?>
                (
                    name: "--storage-file",
                    description: "The path to where the internal data storage file are located. Default: ./geocoded_property_database.json"
                );

            var outputFileOption = new Option<FileInfo?>
                (
                    name: "--csv-out-file",
                    description: "The path to where the execution results CSV file are to be saved. Default: ./geocoded_property_output.csv"
                );

            rootCommand.Add(propertyTypesOption);
            rootCommand.Add(suburbNamesOption);
            rootCommand.Add(minimumBedroomOption);
            rootCommand.Add(minimumBathroomOption);
            rootCommand.Add(minimumCarparkOption);
            rootCommand.Add(maximumPriceOption);
            rootCommand.Add(storageFileOption);
            rootCommand.Add(outputFileOption);

            rootCommand.SetHandler(
                commandHandler, 
                propertyTypesOption,
                suburbNamesOption,
                minimumBedroomOption,
                minimumBathroomOption,
                minimumCarparkOption,
                maximumPriceOption,
                storageFileOption,
                outputFileOption);

            return rootCommand;
        }
    }
}

