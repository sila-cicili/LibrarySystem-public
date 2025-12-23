using Microsoft.EntityFrameworkCore;
using LibrarySystem.Models;

namespace LibrarySystem.Services
{
    public class BookService : IBookService
    {
        private readonly KütüphaneeContext _context;

        public BookService(KütüphaneeContext context)
        {
            _context = context;
        }

        // 👇 GÜNCELLENEN METOD (İmzayı IBookService ile aynı yaptık)
        public async Task<List<Book>> TumKitaplariGetir(string aramaKelimesi, int? branchId = null)
        {
            // Kitapları çekerken Şube bilgisini de (LibraryBranch) yanına al
            var books = _context.Books
                .Include(b => b.LibraryBranch) 
                .AsQueryable();

            // 1. Eğer şube ID geldiyse, sadece o şubedekileri filtrele
            if (branchId.HasValue)
            {
                books = books.Where(b => b.LibraryBranchId == branchId.Value);
            }

            // 2. Arama kelimesi varsa ona göre de filtrele
            if (!string.IsNullOrEmpty(aramaKelimesi))
            {
                books = books.Where(s => s.Title.Contains(aramaKelimesi) || s.Author.Contains(aramaKelimesi));
            }

            return await books.ToListAsync();
        }

        // --- Diğer metodlar (Aynı kalıyor) ---

        public async Task<Book?> KitapGetirIdIle(int? id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task YeniKitapEkle(Book book)
        {
            _context.Add(book);
            await _context.SaveChangesAsync();
        }

        public async Task KitapGuncelle(Book book)
        {
            _context.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task KitapSil(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
        }
    }
}