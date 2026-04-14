using System;
using System.Collections.Generic;
using System.Text;

namespace Logis.Domain.ValueObjects
{
    public sealed class Address
    {
        public string ContactName { get; private set; } = string.Empty;
        public string Phone { get; private set; } = string.Empty;
        public string Line1 { get; private set; } = string.Empty;
        public string? Line2 { get; private set; }
        public string City { get; private set; } = string.Empty;
        public string? State { get; private set; }
        public string PostalCode { get; private set; } = string.Empty;
        public string CountryCode { get; private set; } = string.Empty;

        private Address() { }

        public Address(string contactName, string phone, string line1, string? line2, string city, string? state, string postalCode, string countryCode)
        {
            ContactName = contactName.Trim();
            Phone = phone.Trim();
            Line1 = line1.Trim();
            Line2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
            City = city.Trim();
            State = string.IsNullOrWhiteSpace(state) ? null : state.Trim();
            PostalCode = postalCode.Trim();
            CountryCode = countryCode.Trim().ToUpperInvariant();
        }
    }
}
