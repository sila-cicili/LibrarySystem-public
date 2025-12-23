using Microsoft.AspNetCore.Mvc;
using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Yetki kontrolleri için
using NetTopologySuite.Geometries;      // Harita (Point) işlemleri için

namespace LibrarySystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibraryApiController : ControllerBase
    {
        private readonly KütüphaneeContext _context;

        public LibraryApiController(KütüphaneeContext context)
        {
            _context = context;
        }

        // GET: api/LibraryApi
        // Tüm kütüphaneleri haritada göstermek için listeler
        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            var branches = await _context.LibraryBranches
                .Select(b => new 
                {
                    b.Id,
                    b.Name,
                    b.Address,
                    Lat = b.Location.Y, // Y = Enlem (Latitude)
                    Lng = b.Location.X  // X = Boylam (Longitude)
                })
                .ToListAsync();

            return Ok(branches);
        }

        // POST: api/LibraryApi
        // Yeni şube ekleme (Sadece Admin)
        [HttpPost]
        [Authorize(Roles = "admin")] 
        public async Task<IActionResult> AddBranch([FromBody] BranchDto data)
        {
            if (data == null || string.IsNullOrEmpty(data.Name))
                return BadRequest("Şube adı boş olamaz.");

            // Harita koordinatını oluştur (SRID 4326 = GPS standardı)
            var location = new Point(data.Lng, data.Lat) { SRID = 4326 };

            var newBranch = new LibraryBranch
            {
                Name = data.Name,
                Address = data.Address,
                Location = location
            };

            _context.LibraryBranches.Add(newBranch);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Başarıyla eklendi!" });
        }

        // DELETE: api/LibraryApi/5
        // Şube silme (Sadece Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            var branch = await _context.LibraryBranches.FindAsync(id);
            if (branch == null)
            {
                return NotFound("Kütüphane bulunamadı.");
            }

            _context.LibraryBranches.Remove(branch);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kütüphane başarıyla silindi." });
        }
    }

    // Frontend ile veri alışverişi için kullanılan model
    public class BranchDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}