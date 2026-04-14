using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.Text;

namespace Logis.Application.Orders.Contracts
{
    public sealed class CreateOrderRequest
    {

        [Required(ErrorMessage = "Pickup Address Is Required.")]
        
        public required AddressDto PickupAddress { get; init; }


        [Required(ErrorMessage ="Delivery Address Is Required.")]
        public required AddressDto DeliveryAddress { get; init; }


        [Required(ErrorMessage = "Currency Code Is Required.")]
        [StringLength(3,MinimumLength =3,ErrorMessage = "Currency Code Must Be Exactly 3 Chars.")]
        [RegularExpression("^[A-Z]{3}",ErrorMessage ="Currency Code Must Be UpperCase ISO-4217 (e.g. USD, EGP)")]
        public required string CurrencyCode { get; init; }



        [Range(0.1,2000,ErrorMessage ="WeightKg Must Be Between 0.1 & 2000.")]
        public required decimal WeightKg { get; init; }



        [Range(0.01,10_000_000, ErrorMessage ="Declared Value Must Be Positive.")]
        public required decimal DeclaredValue { get; init; }



        [StringLength(500, ErrorMessage = "Note Can't Exceed 500 Chars")]
        public string? Note { get; init; }



        [Required(ErrorMessage ="Order Must Have Atleast One Item.")]
        [MinLength(1,ErrorMessage ="Add Atleast One Item.")]
        public required List<CreateOrderItemDto> Items { get; init; }
    }

    public sealed class CreateOrderItemDto
    {

        [Required(ErrorMessage = "SKU is required.")]
        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters.")]
        public required string Sku { get; init; }



        [Required(ErrorMessage = "Item name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Item name must be between 2 and 100 characters.")]
        public required string Name { get; init; }



        [Range(1, 10_000, ErrorMessage = "Quantity must be between 1 and 10,000.")]
        public required int Quantity { get; init; }



        [Range(0.01, 1_000_000, ErrorMessage = "Unit price must be positive.")]
        public required decimal UnitPrice { get; init; }
    }
    public sealed class AddressDto
    {
        public required string ContactName { get; init; }
        public required string Phone { get; init; }
        public required string Line1 { get; init; }
        public string? Line2 { get; init; }
        public required string City { get; init; }
        public string? State { get; init; }
        public required string PostalCode { get; init; }
        public required string CountryCode { get; init; }
    }
}
