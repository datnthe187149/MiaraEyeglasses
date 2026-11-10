// ==========================================================================
// MIARA Eyewear — cart.js
// Chức năng: Giỏ hàng, Tính lại tổng tiền khi tăng/giảm (+)(-), Xóa món, Áp dụng Voucher
// Phân công báo cáo: Thành viên 2 (Giỏ hàng & Đơn hàng)
// ==========================================================================

function initCartPage() {
  var container = document.getElementById('cart-items-container');
  if (!container) {
    return;
  }

  var subtotalEl = document.getElementById('cart-subtotal-val');
  var discountEl = document.getElementById('cart-discount-val');
  var grandTotalEl = document.getElementById('cart-grandtotal-val');
  var couponInput = document.getElementById('coupon-code-input');
  var applyCouponBtn = document.getElementById('apply-coupon-btn');
  var couponMsg = document.getElementById('coupon-msg');
  var discountAmount = 50000;

  // Hàm phụ trợ định dạng tiền VNĐ
  function formatMoney(amount) {
    if (typeof formatVND === 'function') {
      return formatVND(amount);
    }
    return Number(amount).toLocaleString('vi-VN') + ' ₫';
  }

  // Hàm tính lại tổng tiền cả giỏ hàng khi có thay đổi số lượng hoặc xóa món
  function recalcCart() {
    var subtotal = 0;
    var items = container.querySelectorAll('.cart-item');

    items.forEach(function (item) {
      // Đọc đơn giá của dòng sản phẩm
      var unitPrice = 0;
      var priceAttr = item.getAttribute('data-unit-price');
      if (priceAttr) {
        unitPrice = Number(priceAttr);
      }

      // Đọc số lượng hiện tại
      var qtyEl = item.querySelector('.qty-val');
      var qty = 1;
      if (qtyEl && qtyEl.textContent) {
        qty = Number(qtyEl.textContent);
      }

      // Thành tiền của từng dòng = Đơn giá * Số lượng
      var lineTotal = unitPrice * qty;
      var lineDisplay = item.querySelector('.line-total-display');
      if (lineDisplay) {
        lineDisplay.textContent = formatMoney(lineTotal);
      }

      // Cộng dồn vào tạm tính
      subtotal = subtotal + lineTotal;
    });

    // Tính mức giảm giá: chỉ giảm khi giỏ hàng có tiền
    var currentDiscount = 0;
    if (subtotal > 0) {
      currentDiscount = discountAmount;
    }

    // Tổng tiền thanh toán cuối cùng = Tạm tính - Giảm giá
    var grandTotal = subtotal - currentDiscount;
    if (grandTotal < 0) {
      grandTotal = 0;
    }

    // Hiển thị số tiền lên giao diện
    if (subtotalEl) {
      subtotalEl.textContent = formatMoney(subtotal);
    }
    if (discountEl) {
      discountEl.textContent = '-' + formatMoney(currentDiscount);
    }
    if (grandTotalEl) {
      grandTotalEl.textContent = formatMoney(grandTotal);
    }
  }

  // Lắng nghe sự kiện click trên toàn bộ giỏ hàng
  container.addEventListener('click', function (e) {
    var decBtn = e.target.closest('.qty-dec');
    var incBtn = e.target.closest('.qty-inc');

    // 1. Nếu bấm nút Giảm (-) hoặc Tăng (+) số lượng
    if (decBtn || incBtn) {
      var item = e.target.closest('.cart-item');
      if (!item) {
        return;
      }

      var qtyEl = item.querySelector('.qty-val');
      var currentQty = 1;
      if (qtyEl && qtyEl.textContent) {
        currentQty = Number(qtyEl.textContent);
      }

      var nextQty = currentQty;
      if (decBtn) {
        // Giảm số lượng nhưng không cho nhỏ hơn 1
        nextQty = currentQty - 1;
        if (nextQty < 1) {
          nextQty = 1;
        }
      } else if (incBtn) {
        // Tăng số lượng tối đa 10 cái
        nextQty = currentQty + 1;
        if (nextQty > 10) {
          nextQty = 10;
        }
      }

      if (qtyEl) {
        qtyEl.textContent = String(nextQty);
      }

      // Tính lại tổng tiền sau khi đổi số lượng
      recalcCart();
      return;
    }

    // 2. Nếu bấm nút Xóa món khỏi giỏ hàng
    var removeBtn = e.target.closest('.cart-remove-btn');
    if (removeBtn) {
      var row = removeBtn.closest('.cart-item');
      if (row) {
        row.remove();
        recalcCart();
        if (typeof showToast === 'function') {
          showToast('Đã xóa sản phẩm khỏi giỏ hàng.');
        }
      }
    }
  });

  // 3. Xử lý khi bấm nút Áp dụng mã Voucher giảm giá
  if (applyCouponBtn && couponInput) {
    applyCouponBtn.addEventListener('click', function () {
      var code = couponInput.value.trim().toUpperCase();

      if (code === 'MIARA50K') {
        discountAmount = 50000;
        if (couponMsg) {
          couponMsg.textContent = 'Đã áp dụng mã MIARA50K (-50.000 ₫)';
          couponMsg.style.color = '#1e7e46';
        }
      } else if (code === 'MIARA10') {
        discountAmount = 84800;
        if (couponMsg) {
          couponMsg.textContent = 'Đã áp dụng mã MIARA10 (-10%)';
          couponMsg.style.color = '#1e7e46';
        }
      } else {
        discountAmount = 0;
        if (couponMsg) {
          couponMsg.textContent = 'Mã giảm giá không hợp lệ hoặc đã hết hạn.';
          couponMsg.style.color = '#b42318';
        }
      }

      // Cập nhật lại tổng tiền sau khi áp dụng mã
      recalcCart();
    });
  }
}

document.addEventListener('DOMContentLoaded', function () {
  initCartPage();
});
