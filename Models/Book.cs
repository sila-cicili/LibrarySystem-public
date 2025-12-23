using System;
using System.Collections.Generic;

namespace LibrarySystem.Models;

public partial class Book
{
    public int BookId { get; set; }

    public string Title { get; set; } = null!;

    public string Author { get; set; } = null!;

    public string? Category { get; set; }

    public int? TotalStock { get; set; }

    public int? CurrentStock { get; set; }

    public DateTime? DateAdded { get; set; }

    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    // 👇 AŞAĞIDAKİ 2 SATIRI EKLEDİK (KİTAP - ŞUBE İLİŞKİSİ) 👇
    
    // 1. Bu kitap hangi şubeye ait? (ID numarası)
    public int? LibraryBranchId { get; set; } 

    // 2. Veritabanı bağlantısı (Entity Framework bunu kullanır)
    public virtual LibraryBranch? LibraryBranch { get; set; }
}