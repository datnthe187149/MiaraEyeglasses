// ==========================================================================
// MIARA Eyewear — account.js
// Chức năng: Quản lý tab Hồ sơ / Đổi mật khẩu / Sổ địa chỉ, Lọc đơn hàng & Hủy đơn
// Phân công báo cáo: Thành viên 2 (Khách hàng & Đơn hàng cá nhân)
// ==========================================================================

// 1. Quản lý chuyển đổi giữa các tab thông tin tài khoản (Hồ sơ, Sổ địa chỉ, Đổi mật khẩu)
function initAccountPage() {
  var tabBtns = document.querySelectorAll('[data-account-tab]');
  if (tabBtns.length === 0) {
    return;
  }

  var panelProfile = document.getElementById('account-panel-profile');
  var panelAddresses = document.getElementById('account-panel-addresses');
  var panelPassword = document.getElementById('account-panel-password');

  // Hàm kích hoạt 1 tab và ẩn các tab còn lại
  function activateTab(tabKey) {
    // Cập nhật trạng thái active cho nút tab
    tabBtns.forEach(function (btn) {
      if (btn.getAttribute('data-account-tab') === tabKey) {
        btn.classList.add('active');
      } else {
        btn.classList.remove('active');
      }
    });

    // Ẩn/Hiện panel tương ứng
    if (panelProfile) {
      if (tabKey === 'profile') {
        panelProfile.removeAttribute('hidden');
      } else {
        panelProfile.setAttribute('hidden', '');
      }
    }

    if (panelAddresses) {
      if (tabKey === 'addresses') {
        panelAddresses.removeAttribute('hidden');
      } else {
        panelAddresses.setAttribute('hidden', '');
      }
    }

    if (panelPassword) {
      if (tabKey === 'password') {
        panelPassword.removeAttribute('hidden');
      } else {
        panelPassword.setAttribute('hidden', '');
      }
    }
  }

  // Gán sự kiện click cho từng nút tab
  tabBtns.forEach(function (btn) {
    btn.addEventListener('click', function () {
      var tabKey = btn.getAttribute('data-account-tab');
      activateTab(tabKey);
    });
  });

  // Tự động mở tab Sổ địa chỉ nếu URL trình duyệt có gắn hash #so-dia-chi
  if (window.location.hash === '#so-dia-chi') {
    activateTab('addresses');
  }

  // Bật / tắt form thêm địa chỉ nhận hàng mới
  var btnToggleAddr = document.getElementById('btn-toggle-new-address');
  var btnCancelAddr = document.getElementById('btn-cancel-new-address');
  var addAddrForm = document.getElementById('add-address-form');

  if (btnToggleAddr && addAddrForm) {
    btnToggleAddr.addEventListener('click', function () {
      var isHidden = addAddrForm.hasAttribute('hidden');
      if (isHidden) {
        addAddrForm.removeAttribute('hidden');
      } else {
        addAddrForm.setAttribute('hidden', '');
      }
    });
  }

  if (btnCancelAddr && addAddrForm) {
    btnCancelAddr.addEventListener('click', function () {
      addAddrForm.setAttribute('hidden', '');
    });
  }

  // Bắt sự kiện submit các form cập nhật thông tin
  var accountForms = document.querySelectorAll('#profile-form, #change-password-form, #add-address-form');
  accountForms.forEach(function (f) {
    f.addEventListener('submit', function (e) {
      if (f.getAttribute('action')) {
        // Nếu form có đường dẫn POST lên Controller C# thì cho phép submit bình thường
        return;
      }
      e.preventDefault();
      if (typeof showToast === 'function') {
        showToast('Đã lưu thông tin cập nhật thành công!');
      }
      if (f.id === 'add-address-form') {
        f.setAttribute('hidden', '');
      }
    });
  });
}

// 2. Lọc đơn hàng theo trạng thái và Xử lý Hủy đơn hàng
function initOrdersPage() {
  var filterBtns = document.querySelectorAll('.order-filter-tabs button[data-order-tab]');

  if (filterBtns.length > 0) {
    filterBtns.forEach(function (btn) {
      btn.addEventListener('click', function () {
        // Cập nhật viền nút active
        filterBtns.forEach(function (b) {
          b.classList.remove('active');
        });
        btn.classList.add('active');

        var targetTab = btn.getAttribute('data-order-tab');
        var orderCards = document.querySelectorAll('.order-card');

        // Lọc hiển thị từng thẻ đơn hàng
        orderCards.forEach(function (card) {
          var cardStatus = card.getAttribute('data-order-status');
          if (targetTab === 'all' || cardStatus === targetTab) {
            card.removeAttribute('hidden');
          } else {
            card.setAttribute('hidden', '');
          }
        });
      });
    });
  }

  // Xử lý nút Hủy đơn hàng (Khi đơn ở trạng thái Chờ thanh toán)
  var cancelButtons = document.querySelectorAll('.btn-cancel-unpaid');
  cancelButtons.forEach(function (btn) {
    btn.addEventListener('click', function () {
      var card = btn.closest('.order-card');
      if (!card) {
        return;
      }

      // Cập nhật trạng thái thẻ đơn hàng sang Đã hủy
      card.setAttribute('data-order-status', 'Đã hủy');

      var badge = card.querySelector('.status-badge');
      if (badge) {
        badge.className = 'status-badge status-badge--cancelled';
        badge.textContent = 'Đã hủy';
      }

      var actionWrap = btn.closest('.order-action-btns');
      if (actionWrap) {
        actionWrap.innerHTML = '<span class="locked-cancel-note">Đơn hàng đã được hủy theo yêu cầu</span>';
      }

      if (typeof showToast === 'function') {
        showToast('Đã hủy đơn hàng thành công.');
      }
    });
  });
}

document.addEventListener('DOMContentLoaded', function () {
  initAccountPage();
  initOrdersPage();
});
