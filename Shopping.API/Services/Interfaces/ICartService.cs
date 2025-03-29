using Shopping.API.Models.Request;
using Shopping.API.Models;

namespace Shopping.API.Services.Interfaces
{
    public interface ICartService
    {
        Task<ShoppingCart?> GetCartByIdAsync(int id);
        Task<ShoppingCart> CreateCartAsync(ShoppingCartRequest shoppingCartRequest);
        Task<bool> DeleteCartAsync(int id);
        Task<Product?> GetProductByIdAsync(int id);
        Task<Product> AddProductToCartAsync(int cartId, Product product);
        Task<bool> UpdateProductInCartAsync(int productId, Product product);
        Task<bool> RemoveProductFromCartAsync(int productId);
    }
}
