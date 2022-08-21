using System;
using RealEstateListingETLService.Application.Entities.Library;

namespace RealEstateListingETLService.Application.Entities
{
    public class PropertyAddress : ValueObject
    {
        public string? StreetAddress { get; private set; }
        public string? Suburb { get; private set; }
        public string? State { get; private set; }
        public string? PostCode { get; private set; }

        public PropertyAddress(string? streetAddress, string? suburb, string? state, string? postCode)
        {
            StreetAddress = streetAddress;
            Suburb = suburb;
            State = state;
            PostCode = postCode;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return StreetAddress;
            yield return Suburb;
            yield return State;
            yield return PostCode;
        }
    }
}
