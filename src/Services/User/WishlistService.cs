using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Repositories.User;

namespace MiaraEyeglasses.Services.User
{
    // Interface dịch vụ quản lý mẫu kính yêu thích
    public interface IWishlistService
    {
        Task<IEnumerable<Wishlist>> GetUserWishlistAsync(int userId);
        // Thêm mới dữ liệu: AddToWishlist
        Task<bool> AddToWishlistAsync(int userId, int productId);
        // Xóa dữ liệu: RemoveFromWishlist
        Task<bool> RemoveFromWishlistAsync(int userId, int productId);
        // Đổi trạng thái kích hoạt: ToggleWishlist
        Task<bool> ToggleWishlistAsync(int userId, int productId);
    }

    // Service thêm hoặc xóa sản phẩm khỏi danh sách yêu thích
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;

        // Khởi tạo đối tượng WishlistService kèm nạp các phụ thuộc cần thiết
        public WishlistService(IWishlistRepository wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
        }

        public async Task<IEnumerable<Wishlist>> GetUserWishlistAsync(int userId)
        {
            return await _wishlistRepository.GetWishlistByUserIdAsync(userId);
        }

        // Thêm mới dữ liệu: AddToWishlist
        public async Task<bool> AddToWishlistAsync(int userId, int productId)
        {
            var exists = await _wishlistRepository.IsWishlistedAsync(userId, productId);
            if (exists) return true;

            var wishlist = new Wishlist { UserId = userId, ProductId = productId, CreatedAt = DateTime.UtcNow };
            await _wishlistRepository.AddAsync(wishlist);
            await _wishlistRepository.SaveChangesAsync();
            return true;
        }

        // Xóa dữ liệu: RemoveFromWishlist
        public async Task<bool> RemoveFromWishlistAsync(int userId, int productId)
        {
            var item = await _wishlistRepository.GetWishlistItemAsync(userId, productId);
            if (item == null) return false;

            _wishlistRepository.Remove(item);
            await _wishlistRepository.SaveChangesAsync();
            return true;
        }

        // Đổi trạng thái kích hoạt: ToggleWishlist
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

