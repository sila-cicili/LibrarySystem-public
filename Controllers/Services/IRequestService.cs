using LibrarySystem.Models;

namespace LibrarySystem.Services
{
    public interface IRequestService
    {
        // Yönetici için bekleyen talepler
        Task<List<Request>> BekleyenTalepleriGetir();
        
        // Yönetici Onay/Red
        Task TalebiOnayla(int requestId);
        Task TalebiReddet(int requestId);

        // Öğrenci için Talep Oluşturma
        Task TalepOlustur(string username, int bookId);

        // 👇 YENİ: Öğrencinin kendi taleplerini görmesi için
        Task<List<Request>> KullaniciTalepleriniGetir(string username);
    }
}