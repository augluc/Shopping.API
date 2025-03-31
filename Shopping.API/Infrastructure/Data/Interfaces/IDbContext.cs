using System.Data;

namespace Shopping.API.Infrastructure.Data.Interfaces
{
    public interface IDbContext
    {
        IDbConnection CreateConnection();
    }
}
