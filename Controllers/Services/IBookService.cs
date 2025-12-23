using LibrarySystem.Models;

namespace LibrarySystem.Services
{
    public interface IBookService
    {
        // 👇 BURASI DEĞİŞTİ:
        // Parantez içine 'int? branchId = null' ekledik.
        // Bu sayede hem arama yapabiliriz hem de şube seçebiliriz.
        Task<List<Book>> TumKitaplariGetir(string aramaKelimesi, int? branchId = null);

        // Diğerleri aynı kalıyor
        Task<Book?> KitapGetirIdIle(int? id);

        Task YeniKitapEkle(Book book);

        Task KitapGuncelle(Book book);

        Task KitapSil(int id);
    }
}