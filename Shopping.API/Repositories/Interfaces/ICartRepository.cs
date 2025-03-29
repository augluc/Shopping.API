using Shopping.API.Models;
using Shopping.API.Models.Request;
using System.Threading.Tasks;

namespace Shopping.API.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetByIdAsync(int cartId);
        Task<Cart> CreateAsync(CartRequest cartRequest);
        Task<bool> DeleteAsync(int cartId);
        Task<Product?> GetProductByIdAsync(int productId);
        Task AddProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> RemoveProductAsync(int productId);
    }
}
