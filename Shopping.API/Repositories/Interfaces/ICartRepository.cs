using Shopping.API.Models;
using Shopping.API.Models.Request;
using System.Threading.Tasks;

namespace Shopping.API.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<ShoppingCart?> GetByIdAsync(int cartId);
        Task<ShoppingCart> CreateAsync(ShoppingCartRequest shoppingCartRequest);
        Task<bool> DeleteAsync(int cartId);
        Task<Product?> GetProductByIdAsync(int productId);
        Task AddProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> RemoveProductAsync(int productId);
    }
}
