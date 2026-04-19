using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;

namespace QuizApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        // Veritabanı bağlantısını içeri alıyoruz (Dependency Injection)
        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            // 1. Girdi Kontrolü
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email and password are required.";
                return View();
            }

            // 2. Kullanıcıyı Sorgula
            // Senin formun Email istediği için Email sütununa bakıyoruz
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // 3. Session (Oturum) Kaydı - Rapor için kritik
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username ?? "");
                HttpContext.Session.SetString("Role", user.Role ?? "User");

                // Giriş başarılı, Quiz listesine yönlendir
                return RedirectToAction("Index", "Quiz");
            }

            // 4. Hata Mesajı
            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Users user)
        {
            if (ModelState.IsValid)
            {
                // Şablona göre yeni kayıtlar her zaman "User" rolüyle başlar
                user.Role = "User";

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Login));
            }
            return View(user);
        }

        // Logout İşlemi
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}