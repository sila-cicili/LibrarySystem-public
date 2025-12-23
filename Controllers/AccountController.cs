using Microsoft.AspNetCore.Mvc;
using LibrarySystem.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly KütüphaneeContext _context;

        public AccountController(KütüphaneeContext context)
        {
            _context = context;
        }

        // ==========================================
        // 👇 YENİ EKLENEN KISIM: ÜYE OL (REGISTER) 👇
        // ==========================================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Bu kullanıcı adı zaten var mı kontrol et
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Bu kullanıcı adı zaten kullanılıyor.");
                    return View(model);
                }

                // 2. Yeni Kullanıcı Oluştur
                var newUser = new User
                {
                    Username = model.Username,
                    // Senin Login kodunda veritabanındaki adının 'PasswordHash' olduğunu gördüm:
                    PasswordHash = model.Password, 
                    Role = "student" // ⚠️ Varsayılan olarak öğrenci yapıyoruz
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // 3. Başarılı mesajı ver ve Giriş sayfasına gönder
                TempData["Message"] = "Kayıt başarılı! Lütfen giriş yapınız.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // ==========================================
        // 👆 YENİ EKLENEN KISIM BİTTİ 👆
        // ==========================================


        // --- ESKİ KODLARIN AYNEN DURUYOR ---

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Veritabanında bu kullanıcı var mı?
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password);

            if (user != null)
            {
                // Kullanıcı bulundu, kimlik kartını (Cookie) hazırlayalım
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username ?? ""),
                    new Claim(ClaimTypes.Role, user.Role ?? "") // Rolü buraya yüklüyoruz
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Giriş yap
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                // Admin ise farklı yere, öğrenci ise farklı yere yönlendirebilirsin (Şimdilik Home/Index)
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}