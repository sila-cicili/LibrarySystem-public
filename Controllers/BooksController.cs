using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // ⚠️ Dropdown (SelectList) için şart
using Microsoft.EntityFrameworkCore;        // ⚠️ Include metodu için şart
using LibrarySystem.Models;
using LibrarySystem.Services;
using Microsoft.AspNetCore.Authorization;

namespace LibrarySystem.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly KütüphaneeContext _context; // Şube listesini çekmek için gerekli

        public BooksController(IBookService bookService, KütüphaneeContext context)
        {
            _bookService = bookService;
            _context = context;
        }

        // 1. KİTAPLARI LİSTELEME (Arama ve Şube Filtreleme)
        public async Task<IActionResult> Index(string searchString, int? branchId)
        {
            // Seçili şubeyi View'a gönderelim (Filtre temizle butonu için)
            ViewBag.CurrentBranchId = branchId;

            // Servisten kitapları çek
            var books = await _bookService.TumKitaplariGetir(searchString, branchId);
            return View(books);
        }

        // 2. KİTAP DETAYI (Şube Bilgisi Dahil)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // Kitabı çekerken Şube bilgisini de (LibraryBranch) dahil ediyoruz
            var book = await _context.Books
                .Include(b => b.LibraryBranch)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null) return NotFound();

            return View(book);
        }

        // 3. YENİ KİTAP EKLEME SAYFASI (Admin)
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            // Şube listesini Dropdown için View'a gönder
            ViewData["LibraryBranchId"] = new SelectList(_context.LibraryBranches, "Id", "Name");
            return View();
        }

        // 4. YENİ KİTAP EKLEME İŞLEMİ (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([Bind("BookId,Title,Author,Category,TotalStock,CurrentStock,DateAdded,LibraryBranchId")] Book book)
        {
            if (ModelState.IsValid)
            {
                await _bookService.YeniKitapEkle(book);
                return RedirectToAction(nameof(Index));
            }
            // Hata olursa listeyi tekrar doldur
            ViewData["LibraryBranchId"] = new SelectList(_context.LibraryBranches, "Id", "Name", book.LibraryBranchId);
            return View(book);
        }

        // 5. KİTAP DÜZENLEME SAYFASI (Admin) - Şube Seçmeli
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _bookService.KitapGetirIdIle(id);
            if (book == null) return NotFound();

            // 👇 Şube listesini Dropdown'a doldur (Seçili olan şube ile birlikte)
            ViewData["LibraryBranchId"] = new SelectList(_context.LibraryBranches, "Id", "Name", book.LibraryBranchId);
            
            return View(book);
        }

        // 6. KİTAP DÜZENLEME İŞLEMİ (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, [Bind("BookId,Title,Author,Category,TotalStock,CurrentStock,DateAdded,LibraryBranchId")] Book book)
        {
            if (id != book.BookId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _bookService.KitapGuncelle(book);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _bookService.KitapGetirIdIle(book.BookId) == null) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            // Hata olursa listeyi tekrar doldur
            ViewData["LibraryBranchId"] = new SelectList(_context.LibraryBranches, "Id", "Name", book.LibraryBranchId);
            return View(book);
        }

        // 7. SİLME SAYFASI (Admin)
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.LibraryBranch)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null) return NotFound();

            return View(book);
        }

        // 8. SİLME İŞLEMİ (Admin)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _bookService.KitapSil(id);
            return RedirectToAction(nameof(Index));
        }
    }
}