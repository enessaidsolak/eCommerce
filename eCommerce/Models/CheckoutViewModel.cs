using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eCommerce.WEB.Models
{
    public class CheckoutViewModel
    {
        // Kaydedilmiş adresler
        public List<AddressViewModel> SavedAddresses { get; set; } = new List<AddressViewModel>();
        public int SelectedAddressId { get; set; }

        // Fatura alanları
        [Required] public string FirstName { get; set; } = null!;
        [Required] public string LastName { get; set; } = null!;
        public string CompanyName { get; set; } = "";
        [Required] public string AddressLine { get; set; } = null!;
        [Required] public string City { get; set; } = null!;
        [Required] public string Country { get; set; } = null!;
        [Required] public string Postcode { get; set; } = null!;
        [Required] public string Mobile { get; set; } = null!;
        [Required] public string Email { get; set; } = null!;

        public bool CreateAccount { get; set; }
        public bool ShipToDifferentAddress { get; set; }
        public string OrderNotes { get; set; } = "";

        // Sepet
        public List<CartItemModel> CartItems { get; set; } = new List<CartItemModel>();

        // Kargo & Ödeme
        [Required] public string ShippingOption { get; set; } = null!;
        [Required] public string PaymentMethod { get; set; } = null!;

        // Craftgate kredi kartı alanları
        [Required] public string CardHolderName { get; set; } = null!;
        [Required] public string CardNumber { get; set; } = null!;
        [Required] public string ExpireMonth { get; set; } = null!;
        [Required] public string ExpireYear { get; set; } = null!;
        [Required] public string Cvc { get; set; } = null!;

        // Toplamlar
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Total { get; set; }
    }

    public class AddressViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Mobile { get; set; } = "";
        public string AddressLine { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public string Postcode { get; set; } = "";
    }

    public class CartItemModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
