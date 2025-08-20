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
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        public string LastName { get; set; } = null!;

        public string CompanyName { get; set; } = "";

        [Required(ErrorMessage = "Adres alanı zorunludur.")]
        public string AddressLine { get; set; } = null!;

        [Required(ErrorMessage = "Şehir alanı zorunludur.")]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "Ülke alanı zorunludur.")]
        public string Country { get; set; } = null!;

        [Required(ErrorMessage = "Posta kodu zorunludur.")]
        public string Postcode { get; set; } = null!;

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        public string Mobile { get; set; } = null!;

        [Required(ErrorMessage = "Email alanı zorunludur.")]
        public string Email { get; set; } = null!;

        public bool CreateAccount { get; set; }
        public bool ShipToDifferentAddress { get; set; }
        public string OrderNotes { get; set; } = "";

        // Sepet
        public List<CartItemModel> CartItems { get; set; } = new List<CartItemModel>();

        // Kargo & Ödeme
        [Required(ErrorMessage = "Kargo seçimi zorunludur.")]
        public string ShippingOption { get; set; } = null!;

        [Required(ErrorMessage = "Ödeme yöntemi zorunludur.")]
        public string PaymentMethod { get; set; } = null!;

        public string CardHolderName { get; set; } = "";
        public string CardNumber { get; set; } = "";
        public string ExpireMonth { get; set; } = "";
        public string ExpireYear { get; set; } = "";
        public string Cvc { get; set; } = "";

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
