using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models
{
    public class Users
    {
        [Key]
        // SQL tarafında 'UserId' olan ismi 'Id' olarak değiştirdik, kodda da 'Id' olmalı.
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçersiz e-posta formatı.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        // Şablona göre: Admin veya User rolleri için
        [Required]
        public string Role { get; set; } = "User";

        // İlişkiler (Navigation Properties)
        // Bir kullanıcının birden fazla oturumu ve liderlik tablosu kaydı olabilir.
        public List<Sessions>? Sessions { get; set; } = new();
        public List<Leaderboard>? LeaderboardEntries { get; set; } = new();
    }
}