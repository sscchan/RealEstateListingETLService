using System;
using System.Text.RegularExpressions;
using RealEstateListingETLService.Application.Entities;
using RealEstateListingETLService.Application.Services;
using RealEstateListingETLService.Application.Services.Dtos;
using RealEstateListingETLService.Application.Services.Parameters;
using RealEstateListingETLService.Infrastructure.Extensions;
using RealEstateListingETLService.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace RealEstateListingETLService.Infrastructure.Repositories
{
    public class DomainPropertyListingServiceOptions
    {
        public const string DomainPropertyListingService = "DomainPropertyListingService";

        /// <summary>
        /// The endpoint for the "For Sale" listing
        /// </summary>
        public string ForSaleListingEndpoint { get; set; } = String.Empty;
    }

    public class DomainPropertyListingService : IPropertyListingService
    {
        private readonly DomainPropertyListingServiceOptions _options;

        public DomainPropertyListingService(IOptions<DomainPropertyListingServiceOptions> domainPropertyListingServiceOptions)
        {
            _options = domainPropertyListingServiceOptions.Value;
        }

        public async Task<IEnumerable<PropertyListingSearchResultDto>> SearchProperties(PropertySearchCriteria searchCriteria)
        {
            // Playwright (as of 2022-08-21) does not seem to obey BrowserContext global timeout settings.
            // This constant value is used to set individual timeouts on page load navigations.
            const int RESULT_PAGE_LOAD_TIMEOUT = 300000;

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new ()
            {
                Headless = false,
                Timeout = RESULT_PAGE_LOAD_TIMEOUT
            });
            var page = await browser.NewPageAsync();


            // Order of QueryParams deliberate set to emulate the website's order
            var initialSearchUrl =
                _options.ForSaleListingEndpoint +
                "?" +
                $"suburb={ToDomainSuburbQueryString(searchCriteria.Suburbs)}&" +
                $"ptype={ToDomainPropertyTypeQueryString(searchCriteria.PropertyTypes)}&" +
                $"bedrooms={searchCriteria.MinimumNumberOfBedrooms}-any&" +
                $"bathrooms={searchCriteria.MinimumNumberOfBathrooms}-any&" +
                $"price=0-{searchCriteria.MaximumPrice}&" +
                "excludeunderoffer=1&" +
                $"carspaces={searchCriteria.MinimumNumberOfCarparks}-any";


            await page.GotoAsync(initialSearchUrl, new () { Timeout = RESULT_PAGE_LOAD_TIMEOUT });


            const int MAXIMUM_NUMBER_OF_RESULT_PAGES_TO_CRAWL = 100;
            var propertySearchResultDaos = new List<PropertyListingSearchResultDto>();
            var resultPageCount = 0;
            do
            {
                await Task.Delay(2000);

                var resultEntriesCount = await page.Locator("li[data-testid^='listing-']").CountAsync();
                for (int i = 0; i < resultEntriesCount; i++)
                {
                    var resultEntry = page.Locator(@"li[data-testid^='listing-']").Nth(i);

                    var listingId = (await resultEntry.GetAttributeAsync("data-testid"))?.Replace("listing-", "");

                    var detailPageUrl = await resultEntry.Locator("a.address").GetAttributeAsync("href")!;
                    var address = await resultEntry.Locator(@"h2[data-testid='address-wrapper']").TextContentAsync();
                    // Replace the non-breaking white space (unicode 160) with a standard whitespace
                    address = address?.Replace(" ", " ");
               
                    var features = resultEntry.Locator("span[data-testid='property-features-text-container']");
                    const string featureTextSelector = "span[data-testid='property-features-text']";

                    var bedroomFeature = features.Filter(new() { Has = page.Locator(featureTextSelector, new() { HasTextString = "Beds" }) });
                    var bedroomCount = await bedroomFeature.CountAsync() != 0 ? (await bedroomFeature.TextContentAsync())?.Replace(" Beds", "") : null;

                    var bathroomFeature = features.Filter(new() { Has = page.Locator(featureTextSelector, new() { HasTextString = "Baths" }) });
                    var bathroomCount = await bathroomFeature.CountAsync() != 0 ? (await bathroomFeature.TextContentAsync())?.Replace(" Baths", "") : null;

                    var carparkFeature = features.Filter(new() { Has = page.Locator(featureTextSelector, new() { HasTextString = "Parking" }) });
                    var carparkCount = await carparkFeature.CountAsync() != 0 ? (await carparkFeature.TextContentAsync())?.Replace(" Parking", "") : null;

                    var landSize = resultEntry.Locator("span[data-testid='property-features-text-container']", new() { HasTextString = "m²" });
                    var landSizeAmount = await landSize.CountAsync() != 0 ? await landSize.TextContentAsync() : null;

                    var price = resultEntry.Locator(@"p[data-testid='listing-card-price']");
                    var priceAmount = await price.CountAsync() != 0 ? (await price.TextContentAsync())?.Trim() : null;


                    // Address strings are in the form of
                    // <Street Number> <Street Name>, <SUBURB> <STATE CODE> <Postcode>
                    // e.g."42 Ferguson Avenue, MYRTLE BANK SA 5064"
                    var addressRegex = new Regex(@"(.*?), (.*) (\w{2,3}) (\d{4})", RegexOptions.IgnoreCase);
                    var match = addressRegex.Match(address);
                    PropertySearchResultAddressDto addressResultDto;

                    // Correctly regex matched groups consist of [0] string that maches + 4 groups
                    if (match.Groups.Count == 5)
                    {
                        addressResultDto = new PropertySearchResultAddressDto
                        (
                            streetAddress: match.Groups[1].Value,
                            suburb: match.Groups[2].Value,
                            state: match.Groups[3].Value,
                            postcode: match.Groups[4].Value
                        );
                    }
                    else
                    {
                        addressResultDto = new PropertySearchResultAddressDto
                        (
                            streetAddress: address,
                            suburb: null,
                            state: null,
                            postcode: null
                        );
                    }

                    // Convert bedroom, bathroom and carkparking space counts into numberical values
                    uint? bedroomCountNumberical = null;
                    if (bedroomCount == null)
                    {
                        bedroomCountNumberical = null;
                    } else
                    {
                        if (uint.TryParse(bedroomCount, out var bedroomCountParsed))
                        {
                            bedroomCountNumberical = bedroomCountParsed;
                        }
                    }

                    float? bathroomCountNumerical = null;
                    if (bathroomCount == null)
                    {
                        bathroomCountNumerical = null;
                    }
                    else
                    {
                        if (float.TryParse(bathroomCount, out var bathroomCountParsed))
                        {
                            bathroomCountNumerical = bathroomCountParsed;
                        }
                    }

                    uint? carparkCountNumberical = null;
                    if (carparkCount == null)
                    {
                        carparkCountNumberical = null;
                    }
                    else
                    {
                        if (uint.TryParse(carparkCount, out var carparkCountParsed))
                        {
                            carparkCountNumberical = carparkCountParsed;
                        }
                    }

                    propertySearchResultDaos.Add(new PropertyListingSearchResultDto(
                        id: listingId,
                        address: addressResultDto,
                        bedroomCount: bedroomCountNumberical,
                        bathroomCount: bathroomCountNumerical,
                        carparkCount: carparkCountNumberical,
                        landSize: landSizeAmount,
                        price: priceAmount,
                        detailPageAddress: new Uri(detailPageUrl)
                        ));
                }

                // Navigate to the next results page until completion
                // Select the two (back and forward) pagination navigation.
                // The second (last) button is the "forward" button.
                // The button is "disabled" if no more pages are available
                var moreResultPagesAvailable = await page.Locator(@"[data-testid=paginator-navigation-button]").Last.IsEnabledAsync();
                if (moreResultPagesAvailable)
                {
                    await page.Locator(@"[data-testid=paginator-navigation-button]").Last.ClickAsync(new () { Timeout = RESULT_PAGE_LOAD_TIMEOUT });
                }
                else
                {
                    break;
                }

                resultPageCount++;

            } while (resultPageCount < MAXIMUM_NUMBER_OF_RESULT_PAGES_TO_CRAWL);

            return propertySearchResultDaos;
        }

        private static readonly Dictionary<SuburbNames, string> SuburbNamesToQueryStringValue = new()
        {
            { SuburbNames.Leabrook, "leabrook-sa-5068" },
            { SuburbNames.ToorakGardens, "toorak-gardens-sa-5065" },
            { SuburbNames.Erindale, "erindale-sa-5066" },
            { SuburbNames.HazelwoodPark, "hazelwood-park-sa-5066" },
            { SuburbNames.Dulwich, "dulwich-sa-5065" },
            { SuburbNames.LindenPark, "linden-park-sa-5065" },
            { SuburbNames.Glenside, "glenside-sa-5065" },
            { SuburbNames.Frewville, "frewville-sa-5063" },
            { SuburbNames.Glenunga, "glenunga-sa-5064" },
            { SuburbNames.StGeorges, "st-georges-sa-5064" },
            { SuburbNames.Eastwood, "eastwood-sa-5063" },
            { SuburbNames.Unley, "unley-sa-5061" },
            { SuburbNames.Parkside, "parkside-sa-5063" },
            { SuburbNames.Fullarton, "fullarton-sa-5063" },
            { SuburbNames.MyrtleBank, "myrtle-bank-sa-5064" },
            { SuburbNames.GlenOsmond, "glen-osmond-sa-5064" },
            { SuburbNames.Beaumont, "beaumont-sa-5066" },
            { SuburbNames.MountOsmond, "mount-osmond-sa-5064" },
            { SuburbNames.LeawoodGardens, "leawood-gardens-sa-5150" },
        };

        private string ToDomainSuburbQueryString(IEnumerable<SuburbNames> suburbNames)
        {
            return string.Join(',', suburbNames.Select((suburbName) => SuburbNamesToQueryStringValue[suburbName]));
        }

        private static readonly Dictionary<PropertyType, List<string>> PropertyTypeToQueryStringValues = new()
        {
            { PropertyType.House, new() { "house", "duplex", "free-standing", "new-home-designs", "new-house-land", "semi-detached", "terrace", "villa" } },
            { PropertyType.Townhouse, new() { "town-house", "block-of-units" } },
            { PropertyType.Apartment, new() { "apartment-unit-flat", "new-apartments", "pent0house", "studio" } },
            { PropertyType.Land, new() { "vacant-land", "development-site", "new-land" } }
        };

        private string ToDomainPropertyTypeQueryString(IEnumerable<PropertyType> propertyTypes)
        {
            return string.Join(',', propertyTypes.SelectMany(propertyType => PropertyTypeToQueryStringValues[propertyType]));
        }

    }
}

