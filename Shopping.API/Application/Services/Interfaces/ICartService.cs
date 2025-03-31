using Shopping.API.Domain.Models;
using Shopping.API.Domain.Models.Request;

namespace Shopping.API.Application.Services.Interfaces
{
    public interface ICartService
    {
        Task<Cart?> GetCartByIdAsync(int id);
        Task<Cart> CreateCartAsync(CartRequest shoppingCartRequest);
        Task<bool> DeleteCartAsync(int id);
        Task<Product?> GetProductByIdAsync(int id);
        Task<decimal> GetCartTotalAsync(int cartId);
        Task<Product> AddProductToCartAsync(int cartId, Product product);
        Task<bool> UpdateProductInCartAsync(int productId, Product product);
        Task<bool> RemoveProductFromCartAsync(int productId);
        Task<bool> ApplyDiscountAsync(int cartId, decimal discountPercentage);
    }
}
