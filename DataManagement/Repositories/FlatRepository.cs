using Application.Interfaces.Repository;
using DataManagement.Models;

namespace DataManagement.Repositories
{
    public class FlatRepository : IFlatRepository
    {
        private readonly RentFinderDbContext _context;

        public FlatRepository(RentFinderDbContext context)
        {
            _context = context;
        }
        
        public void CreateFlat()
        {
            throw new NotImplementedException();
        }
    }
}
