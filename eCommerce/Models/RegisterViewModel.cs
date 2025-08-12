using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required(ErrorMessage = "İsim zorunludur.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "İsim 2 ile 50 karakter arasında olmalıdır.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Soyisim zorunludur.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Soyisim 2 ile 50 karakter arasında olmalıdır.")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email girin.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Şifre en az 5 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Required(ErrorMessage = "Şifre tekrar zorunludur.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
    public string ConfirmPassword { get; set; }
   
}
