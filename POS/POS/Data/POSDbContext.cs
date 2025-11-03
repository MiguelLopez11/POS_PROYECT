using Microsoft.EntityFrameworkCore;

namespace POS.Data
{
    public class POSDbContext: DbContext
    {
        public POSDbContext(DbContextOptions<POSDbContext> options) : base(options)
        {
        }
    }
}
