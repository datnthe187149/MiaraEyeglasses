using Microsoft.EntityFrameworkCore;
using MiaraEyeglasses.Data;
using MiaraEyeglasses.Models.Entities;
using MiaraEyeglasses.Models.ViewModels;
using MiaraEyeglasses.Repositories.Order;

namespace MiaraEyeglasses.Services.Order
{
    public interface ICartService
    {
        Task<CartViewModel> GetUserCartAsync(int userId);
        Task<bool> AddToCartAsync(int userId, AddToCartRequest request);
        Task<bool> UpdateQuantityAsync(int userId, int cartItemId, int quantity);
        Task<bool> ToggleSelectAsync(int userId, int cartItemId);
        Task<bool> RemoveCartItemAsync(int userId, int cartItemId);
        Task<decimal> ApplyCouponAsync(int userId, string couponCode);
    }

    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly EyeglassesDbContext _context;

        public CartService(ICartRepository cartRepository, ICouponRepository couponRepository, EyeglassesDbContext context)
        {
            _cartRepository = cartRepository;
            _couponRepository = couponRepository;
            _context = context;
        }

        public async Task<CartViewModel> GetUserCartAsync(int userId)
        {
            var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId);
            var viewModel = new CartViewModel();

            foreach (var item in cartItems)
            {
                var primaryImage = item.Variant.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                                   ?? item.Variant.Product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                                   ?? "/images/products/p1.png";

                viewModel.Items.Add(new CartItemViewModel
                {
                    CartItemId = item.CartItemId,
                    VariantId = item.VariantId,
                    ProductId = item.Variant.ProductId,
                    ProductName = item.Variant.Product.ProductName,
                    Sku = item.Variant.Sku,
                    ColorName = item.Variant.ColorName,
                    SizeLabel = item.Variant.SizeLabel,
                    ImageUrl = primaryImage,
                    FramePrice = item.Variant.Product.Price,
                    LensOptionId = item.LensOptionId,
                    LensName = item.LensOption.LensName,
                    LensPrice = item.LensOption.ExtraPrice,
                    Quantity = item.Quantity,
                    IsSelected = item.IsSelected,
                    Prescription = new PrescriptionInputModel
                    {
                        RightSph = item.RightSph,
                        RightCyl = item.RightCyl,
                        RightAxis = item.RightAxis,
                        LeftSph = item.LeftSph,
                        LeftCyl = item.LeftCyl,
                        LeftAxis = item.LeftAxis,
                        PD = item.PD,
                        PrescriptionImg = item.PrescriptionImg,
                        Note = item.Note
                    }
                });
            }

            return viewModel;
        }

        public async Task<bool> AddToCartAsync(int userId, AddToCartRequest request)
        {
            var variant = await _context.ProductVariants
                                        .Include(v => v.Product)
                                        .FirstOrDefaultAsync(v => v.VariantId == request.VariantId && v.IsActive);
            if (variant == null || variant.StockQty < request.Quantity)
                return false;

            var existingItem = await _cartRepository.GetCartItemAsync(userId, request.VariantId, request.LensOptionId);
            if (existingItem != null)
            {
                existingItem.Quantity = Math.Min(existingItem.Quantity + request.Quantity, 20);
                if (request.Prescription != null)
                {
                    existingItem.RightSph = request.Prescription.RightSph;
                    existingItem.RightCyl = request.Prescription.RightCyl;
                    existingItem.RightAxis = request.Prescription.RightAxis;
                    existingItem.LeftSph = request.Prescription.LeftSph;
                    existingItem.LeftCyl = request.Prescription.LeftCyl;
                    existingItem.LeftAxis = request.Prescription.LeftAxis;
                    existingItem.PD = request.Prescription.PD;
                    existingItem.PrescriptionImg = request.Prescription.PrescriptionImg;
                    existingItem.Note = request.Prescription.Note;
                }
                _cartRepository.Update(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    UserId = userId,
                    VariantId = request.VariantId,
                    LensOptionId = request.LensOptionId,
                    Quantity = request.Quantity,
                    IsSelected = true,
                    RightSph = request.Prescription?.RightSph,
                    RightCyl = request.Prescription?.RightCyl,
                    RightAxis = request.Prescription?.RightAxis,
                    LeftSph = request.Prescription?.LeftSph,
                    LeftCyl = request.Prescription?.LeftCyl,
                    LeftAxis = request.Prescription?.LeftAxis,
                    PD = request.Prescription?.PD,
                    PrescriptionImg = request.Prescription?.PrescriptionImg,
                    Note = request.Prescription?.Note,
                    CreatedAt = DateTime.UtcNow
                };
                await _cartRepository.AddAsync(newItem);
            }

            await _cartRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateQuantityAsync(int userId, int cartItemId, int quantity)
        {
            var item = await _cartRepository.GetByIdAsync(cartItemId);
            if (item == null || item.UserId != userId)
                return false;

            if (quantity <= 0)
            {
                _cartRepository.Remove(item);
            }
            else
            {
                item.Quantity = Math.Min(quantity, 20);
                _cartRepository.Update(item);
            }

            await _cartRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleSelectAsync(int userId, int cartItemId)
        {
            var item = await _cartRepository.GetByIdAsync(cartItemId);
            if (item == null || item.UserId != userId)
                return false;

            item.IsSelected = !item.IsSelected;
            _cartRepository.Update(item);
            await _cartRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveCartItemAsync(int userId, int cartItemId)
        {
            var item = await _cartRepository.GetByIdAsync(cartItemId);
            if (item == null || item.UserId != userId)
                return false;

            _cartRepository.Remove(item);
            await _cartRepository.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> ApplyCouponAsync(int userId, string couponCode)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
                return 0;

            var cart = await GetUserCartAsync(userId);
            var subtotal = cart.Subtotal;
            if (subtotal <= 0)
                return 0;

            var coupon = await _couponRepository.GetValidCouponAsync(couponCode.Trim().ToUpper(), subtotal);
            if (coupon == null)
                return 0;

            decimal discount = 0;
            if (coupon.DiscountType == "AMOUNT")
            {
                discount = coupon.DiscountValue;
            }
            else if (coupon.DiscountType == "PERCENT")
            {
                discount = Math.Round(subtotal * coupon.DiscountValue / 100, 0);
                if (coupon.MaxDiscount.HasValue && discount > coupon.MaxDiscount.Value)
                    discount = coupon.MaxDiscount.Value;
            }

            return Math.Min(discount, subtotal);
        }
    }
}

