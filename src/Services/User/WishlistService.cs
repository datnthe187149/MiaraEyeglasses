using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.User;

namespace MiaraEyeglasses.Services.User
{
    public interface IWishlistService
    {
        Task<IEnumerable<Wishlist>> GetUserWishlistAsync(int userId);
        Task<bool> AddToWishlistAsync(int userId, int productId);
        Task<bool> RemoveFromWishlistAsync(int userId, int productId);
        Task<bool> ToggleWishlistAsync(int userId, int productId);
    }

    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;

        public WishlistService(IWishlistRepository wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
        }

        public async Task<IEnumerable<Wishlist>> GetUserWishlistAsync(int userId)
        {
            return await _wishlistRepository.GetWishlistByUserIdAsync(userId);
        }

        public async Task<bool> AddToWishlistAsync(int userId, int productId)
        {
            var exists = await _wishlistRepository.IsWishlistedAsync(userId, productId);
            if (exists) return true;

            var wishlist = new Wishlist { UserId = userId, ProductId = productId, CreatedAt = DateTime.UtcNow };
            await _wishlistRepository.AddAsync(wishlist);
            await _wishlistRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromWishlistAsync(int userId, int productId)
        {
            var item = await _wishlistRepository.GetWishlistItemAsync(userId, productId);
            if (item == null) return false;

            _wishlistRepository.Remove(item);
            await _wishlistRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleWishlistAsync(int userId, int productId)
        {
            var isWishlisted = await _wishlistRepository.IsWishlistedAsync(userId, productId);
            if (isWishlisted)
            {
                await RemoveFromWishlistAsync(userId, productId);
                return false; // Now not wishlisted
            }
            else
            {
                await AddToWishlistAsync(userId, productId);
                return true; // Now wishlisted
            }
        }
    }
}

