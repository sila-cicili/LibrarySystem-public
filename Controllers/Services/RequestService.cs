using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Services
{
    public class RequestService : IRequestService
    {
        private readonly KütüphaneeContext _context;

        public RequestService(KütüphaneeContext context)
        {
            _context = context;
        }

        // 1. Bekleyen Talepler (Yönetici)
        public async Task<List<Request>> BekleyenTalepleriGetir()
        {
            return await _context.Requests
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.Status == "Pending")
                .ToListAsync();
        }

        // 2. Onayla
        public async Task TalebiOnayla(int requestId)
        {
            var request = await _context.Requests.FindAsync(requestId);
            if (request != null)
            {
                request.Status = "Approved";
                await _context.SaveChangesAsync();
            }
        }

        // 3. Reddet
        public async Task TalebiReddet(int requestId)
        {
            var request = await _context.Requests.FindAsync(requestId);
            if (request != null)
            {
                request.Status = "Rejected";
                await _context.SaveChangesAsync();
            }
        }

        // 4. Talep Oluştur (Öğrenci)
        public async Task TalepOlustur(string username, int bookId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user != null)
            {
                var newRequest = new Request
                {
                    UserId = user.UserId, // Modelinde ID adı neyse onu kullan (user.Id veya user.UserId)
                    BookId = bookId,
                    RequestDate = DateTime.Now,
                    Status = "Pending"
                };
                _context.Requests.Add(newRequest);
                await _context.SaveChangesAsync();
            }
        }

        // 👇 5. KULLANICI TALEPLERİNİ GETİR (Yeni Eklenen Kısım)
        public async Task<List<Request>> KullaniciTalepleriniGetir(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return new List<Request>();

            return await _context.Requests
                .Include(r => r.Book) // Kitap adını görmek için Include şart
                .Where(r => r.UserId == user.UserId) // Sadece bu kullanıcınınkiler
                .OrderByDescending(r => r.RequestDate) // En yeni en üstte
                .ToListAsync();
        }
    }
}