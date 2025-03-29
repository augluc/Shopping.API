using Shopping.API.Models;
using Shopping.API.Models.Request;
using System.Threading.Tasks;

namespace Shopping.API.Repositories.Interfaces
{
    public interface ICartRepository
    {
        // Shopping Cart Operations
        Task<ShoppingCart?> GetByIdAsync(int cartId);
        Task<ShoppingCart> CreateAsync(ShoppingCartRequest shoppingCartRequest);
        Task<bool> DeleteAsync(int cartId); // Returns true/false indicating success/failure

        // Product Operations
        Task<Product?> GetProductByIdAsync(int productId); // Nullable return type for missing product
        Task AddProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product); // Returns true if updated successfully
        Task<bool> RemoveProductAsync(int productId); // Returns true if product was removed successfully
    }
}
