// ==========================================================================
// MIARA Eyewear — staff.js
// Chức năng: Cổng Nhân viên (Staff Portal)
//             Xử lý đơn hàng, Cập nhật tồn kho, Kiểm duyệt đánh giá khách hàng
// Phân công báo cáo: Thành viên 3 (Vận hành & Nghiệp vụ nhân viên)
// ==========================================================================

function initStaffPortal() {
  // 1. Chuyển đổi giữa các tab làm việc của nhân viên
  var staffBtns = document.querySelectorAll('[data-staff-tab]');
  var panelOrders = document.getElementById('staff-panel-orders');
  var panelInventory = document.getElementById('staff-panel-inventory');
  var panelReviews = document.getElementById('staff-panel-reviews');

  if (staffBtns.length > 0) {
    staffBtns.forEach(function (btn) {
      btn.addEventListener('click', function () {
        var tabKey = btn.getAttribute('data-staff-tab');

        // Đổi trạng thái active cho các nút tab
        staffBtns.forEach(function (b) {
          if (b === btn) {
            b.classList.add('active');
          } else {
            b.classList.remove('active');
          }
        });

        // Ẩn/Hiện panel tương ứng
        if (panelOrders) {
          if (tabKey === 'orders') {
            panelOrders.removeAttribute('hidden');
          } else {
            panelOrders.setAttribute('hidden', '');
          }
        }

        if (panelInventory) {
          if (tabKey === 'inventory') {
            panelInventory.removeAttribute('hidden');
          } else {
            panelInventory.setAttribute('hidden', '');
          }
        }

        if (panelReviews) {
          if (tabKey === 'reviews') {
            panelReviews.removeAttribute('hidden');
          } else {
            panelReviews.setAttribute('hidden', '');
          }
        }
      });
    });
  }

  // 2. Cập nhật trạng thái xử lý đơn hàng từ Dropdown
  var statusSelects = document.querySelectorAll('.status-select-control');
  statusSelects.forEach(function (sel) {
    sel.addEventListener('change', function () {
      var row = sel.closest('tr');
      var badge = null;
      if (row) {
        badge = row.querySelector('.status-cell-badge');
      }

      if (badge) {
        badge.textContent = sel.value;

        // Chọn class màu sắc phù hợp bằng if-else rõ ràng
        var badgeColor = 'status-badge--processing';
        if (sel.value === 'Hoàn thành') {
          badgeColor = 'status-badge--completed';
        } else if (sel.value === 'Trả hàng' || sel.value === 'Đã hủy') {
          badgeColor = 'status-badge--cancelled';
        }

        badge.className = 'status-badge status-cell-badge ' + badgeColor;
      }

      if (typeof showToast === 'function') {
        showToast('Đã cập nhật trạng thái đơn sang: ' + sel.value);
      }
    });
  });

  // 3. Nút Lưu số lượng tồn kho sản phẩm
  var saveStockBtns = document.querySelectorAll('.btn-save-stock');
  saveStockBtns.forEach(function (btn) {
    btn.addEventListener('click', function () {
      if (typeof showToast === 'function') {
        showToast('Đã cập nhật số lượng tồn kho thành công.');
      }
    });
  });

  // 4. Bật / tắt duyệt hiển thị đánh giá của khách hàng
  var reviewBtns = document.querySelectorAll('.btn-toggle-review');
  reviewBtns.forEach(function (btn) {
    btn.addEventListener('click', function () {
      var row = btn.closest('tr');
      var badge = null;
      if (row) {
        badge = row.querySelector('.review-status-badge');
      }

      if (!badge) {
        return;
      }

      var currentText = badge.textContent.trim();
      var isApproved = (currentText === 'Đã duyệt' || currentText === 'Đã duyệt hiển thị');

      if (isApproved) {
        // Đổi sang Ẩn
        badge.textContent = 'Đã ẩn';
        badge.className = 'status-badge status-badge--cancelled review-status-badge';
        btn.textContent = 'Duyệt hiển thị';
        if (typeof showToast === 'function') {
          showToast('Đã ẩn đánh giá khỏi trang sản phẩm.');
        }
      } else {
        // Đổi sang Duyệt
        badge.textContent = 'Đã duyệt hiển thị';
        badge.className = 'status-badge status-badge--completed review-status-badge';
        btn.textContent = 'Ẩn đánh giá';
        if (typeof showToast === 'function') {
          showToast('Đã duyệt hiển thị đánh giá.');
        }
      }
    });
  });
}

document.addEventListener('DOMContentLoaded', function () {
  initStaffPortal();
});
