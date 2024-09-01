using Microsoft.EntityFrameworkCore;
using MoviesService.Models;

namespace MoviesService.Data
{
    public class SerialRepo : ISerialRepo
    {
        private readonly AppDbContext _context;

        public SerialRepo(AppDbContext context)
        {
            _context = context;
        }

        public void CreateSerial(Serial serial)
        {
            ArgumentNullException.ThrowIfNull(serial);

            _context.Serials.Add(serial);
        }

        public IEnumerable<Serial> GetAllSerials()
        {
            return _context.Serials.ToList();
        }

        public Serial? GetSerialById(int id)
        {
            return _context.Serials.FirstOrDefault(p => p.Id == id);
        }

        public bool SaveChanges()
        {
            return _context.SaveChanges() >= 0;
        }
    }
}
