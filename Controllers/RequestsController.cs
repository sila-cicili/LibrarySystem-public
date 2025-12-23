using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LibrarySystem.Services;

namespace LibrarySystem.Controllers
{
    [Authorize] // Giriş yapan herkes (Öğrenci/Admin) erişebilir
    public class RequestsController : Controller
    {
        private readonly IRequestService _requestService;

        public RequestsController(IRequestService requestService)
        {
            _requestService = requestService;
        }

        // 👇 Talep Et Butonuna Basınca Çalışan Metod
        public async Task<IActionResult> Create(int bookId)
        {
            var username = User.Identity?.Name;
            if (username == null) return RedirectToAction("Login", "Account");

            await _requestService.TalepOlustur(username, bookId);

            TempData["Message"] = "Talebiniz alındı! Durumunu 'Taleplerim' sayfasından takip edebilirsiniz.";
            return RedirectToAction("Index", "Books");
        }

        // 👇 TALEPLERİM SAYFASI (Yeni Eklenen)
        public async Task<IActionResult> MyRequests()
        {
            var username = User.Identity?.Name;
            if (username == null) return RedirectToAction("Login", "Account");

            var myRequests = await _requestService.KullaniciTalepleriniGetir(username);
            return View(myRequests);
        }

        // --- YÖNETİCİ İŞLEMLERİ ---

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var requests = await _requestService.BekleyenTalepleriGetir();
            return View(requests);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Approve(int id)
        {
            await _requestService.TalebiOnayla(id);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Reject(int id)
        {
            await _requestService.TalebiReddet(id);
            return RedirectToAction(nameof(Index));
        }
    }
}