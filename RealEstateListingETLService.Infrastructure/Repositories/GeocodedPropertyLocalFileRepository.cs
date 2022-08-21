using System;
using System.IO;
using System.Text.Json;
using RealEstateListingETLService.Application.Entities;
using RealEstateListingETLService.Application.Repositories;
using Microsoft.Extensions.Options;

namespace RealEstateListingETLService.Infrastructure.Repositories
{
    public class GeocodedPropertyLocalFileRepositoryOptions
    {
        public const string GeocodedPropertyLocalFileRepository = "GeocodedPropertyLocalFileRepository";

        /// <summary>
        /// Path to the local file where the Repository read and stores its data
        /// </summary>
        public string LocalFilePath {get; set;} = String.Empty;
    }

    public class GeocodedPropertyLocalFileRepository : IGeocodedPropertyRepository
    {
        private GeocodedPropertyLocalFileRepositoryOptions _repositoryOptions;

        private List<GeocodedProperty>? _cache = null;


        public GeocodedPropertyLocalFileRepository(IOptions<GeocodedPropertyLocalFileRepositoryOptions> geocodedPropertyLocalFileRepositoryOptions)
        {
            _repositoryOptions = geocodedPropertyLocalFileRepositoryOptions.Value;

            InitializeStore();
        }

        public async Task<GeocodedProperty?> FindByAddress(PropertyAddress geocodedPropertyAddress)
        {
            await PopulateCacheIfEmpty();

            return _cache.Where(geocodedProperty => geocodedProperty.Address == geocodedPropertyAddress).FirstOrDefault();
        }

        public async Task<IEnumerable<GeocodedProperty>> GetAll()
        {
            using (FileStream stream = File.OpenRead(_repositoryOptions.LocalFilePath))
            {
                _cache = await JsonSerializer.DeserializeAsync<List<GeocodedProperty>>(stream);
            }

            return _cache;
        }

        public async Task<IEnumerable<GeocodedProperty>> AddOrUpdate(IEnumerable<GeocodedProperty> geocodedProperties)
        {
            await PopulateCacheIfEmpty();

            // Remove old existing entries if it matches any of the supplied geocodedProperties
            foreach (var geocodedProperty in geocodedProperties)
            {
                var existingPropertyEntry = _cache.FirstOrDefault(gp => gp.Id == geocodedProperty.Id);
                if (existingPropertyEntry != null)
                {
                    _cache.Remove(existingPropertyEntry);
                }
            }

            // Add the updated entries
            _cache = (_cache.Concat(geocodedProperties)).ToList();

            // Persist the updated entries
            using (FileStream stream = File.Open(_repositoryOptions.LocalFilePath, FileMode.Create))
            {
                await JsonSerializer.SerializeAsync<List<GeocodedProperty>>(stream, _cache);
            }

            return _cache;
        }

        public async Task SetAllToOffMarket()
        {
            await PopulateCacheIfEmpty();

            foreach (var geocodedProperty in _cache)
            {
                geocodedProperty.SetAsOffMarket();
            }

            // Persist the updated entries
            using (FileStream stream = File.Open(_repositoryOptions.LocalFilePath, FileMode.Create))
            {
                await JsonSerializer.SerializeAsync<List<GeocodedProperty>>(stream, _cache);
            }
        }

        private void InitializeStore()
        {
            // Create local storage file if it doesn't exist
            if (!File.Exists(_repositoryOptions.LocalFilePath))
            {
                using (var file = File.Open(_repositoryOptions.LocalFilePath, FileMode.Create))
                using (StreamWriter sw = new StreamWriter(file))
                {
                    // Empty JSON Array
                    sw.Write("[]");
                }
            }
        }

        private async Task PopulateCacheIfEmpty()
        {
            if (_cache == null)
            {
                await this.GetAll();
            }
        }
    }
}

