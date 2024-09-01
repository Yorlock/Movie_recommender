using MoviesService.Models;

namespace MoviesService.Data
{
    public interface ISerialRepo
    {
        bool SaveChanges();

        IEnumerable<Serial> GetAllSerials();

        Serial? GetSerialById(int id);

        void CreateSerial(Serial serial);
    }
}
