using System.Data;

namespace Shopping.API.Data.Interfaces
{
    public interface IDbContext
    {
        IDbConnection CreateConnection();
    }
}
