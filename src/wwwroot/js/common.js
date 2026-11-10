// ==========================================================================
// MIARA Eyewear — common.js
// Chức năng: Tiện ích dùng chung (Format tiền, Toast thông báo, Menu, Nút Yêu thích)
// Phân công báo cáo: Dùng chung cả nhóm
// ==========================================================================

// Hàm định dạng số tiền sang chuỗi VNĐ
// Ví dụ: 249000 -> "249.000 ₫"
function formatVND(amount) {
  var numberValue = Number(amount);
  if (isNaN(numberValue)) {
    numberValue = 0;
  }
  return numberValue.toLocaleString('vi-VN') + ' ₫';
}

// Hàm hiển thị hộp thông báo nhanh (Toast) ở góc màn hình
// Tự động tắt sau 2.8 giây
function showToast(msg) {
  var toast = document.getElementById('toast');
  if (!toast) {
    return;
  }

  // Gán nội dung thông báo
  toast.textContent = msg;

  // Thêm class 'show' để CSS hiển thị hộp thông báo
  toast.classList.add('show');

  // Xóa bộ hẹn giờ cũ nếu có để tránh đóng sớm
  if (toast._timer) {
    clearTimeout(toast._timer);
  }

  // Hẹn giờ 2.8 giây sau sẽ tự động gỡ class 'show' để ẩn thông báo
  toast._timer = setTimeout(function () {
    toast.classList.remove('show');
  }, 2800);
}

// Gán các hàm lên đối tượng window để các file JS khác và View Razor có thể gọi trực tiếp
window.formatVND = formatVND;
window.showToast = showToast;
window.Miara = { formatVND: formatVND, showToast: showToast };

// 1. Hiệu ứng xuất hiện mềm mại khi cuộn trang (Reveal Animation)
function initReveals() {
  var elements = document.querySelectorAll('[data-reveal], [data-reveal-group]');
  elements.forEach(function (el) {
    el.classList.add('is-revealed');
  });
}

// 2. Hiệu ứng thanh gạch chân trượt theo menu trên Header (Desktop Navigation)
function initHeaderNav() {
  var nav = document.getElementById('desktop-nav');
  var indicator = document.getElementById('nav-indicator');
  if (!nav || !indicator) {
    return;
  }

  // Hàm tính toán vị trí để di chuyển thanh trượt tới link tương ứng
  function moveIndicator(link) {
    if (!link) {
      indicator.style.opacity = '0';
      return;
    }
    var navRect = nav.getBoundingClientRect();
    var linkRect = link.getBoundingClientRect();
    var x = linkRect.left - navRect.left + (linkRect.width - 24) / 2;
    indicator.style.opacity = '1';
    indicator.style.transform = 'translate3d(' + x.toFixed(1) + 'px, 0, 0)';
  }

  // Đặt thanh trượt vào mục menu đang active hiện tại
  var activeLink = nav.querySelector('a.active');
  moveIndicator(activeLink);

  // Khi di chuột qua các mục menu
  var allLinks = nav.querySelectorAll('a');
  allLinks.forEach(function (link) {
    link.addEventListener('mouseenter', function () {
      moveIndicator(link);
    });
  });

  // Khi chuột rời khỏi thanh menu -> quay về mục đang active
  nav.addEventListener('mouseleave', function () {
    moveIndicator(nav.querySelector('a.active'));
  });

  // Khi thay đổi kích thước cửa sổ trình duyệt -> cập nhật lại vị trí
  window.addEventListener('resize', function () {
    moveIndicator(nav.querySelector('a.active'));
  });
}

// 3. Xử lý các tương tác trên Header: Tìm kiếm, Menu điện thoại, Chuông thông báo
function initHeaderActions() {
  var btnSearch = document.getElementById('search-toggle-btn');
  var searchRegion = document.getElementById('header-search-region');
  var btnSearchClose = document.getElementById('header-search-close');
  var searchInput = document.getElementById('header-search');

  // Bật / tắt khung tìm kiếm trên Header
  if (btnSearch && searchRegion) {
    btnSearch.addEventListener('click', function () {
      var isHidden = searchRegion.hasAttribute('hidden');
      if (isHidden) {
        searchRegion.removeAttribute('hidden');
        btnSearch.setAttribute('aria-expanded', 'true');
        if (searchInput) {
          searchInput.focus();
        }
      } else {
        searchRegion.setAttribute('hidden', '');
        btnSearch.setAttribute('aria-expanded', 'false');
      }
    });
  }

  if (btnSearchClose && searchRegion) {
    btnSearchClose.addEventListener('click', function () {
      searchRegion.setAttribute('hidden', '');
      if (btnSearch) {
        btnSearch.setAttribute('aria-expanded', 'false');
      }
    });
  }

  // Mở / đóng Menu dành cho thiết bị di động (Mobile Menu)
  var btnMobile = document.getElementById('mobile-menu-btn');
  var mobileNav = document.getElementById('mobile-nav');
  var iconOpen = document.getElementById('mobile-menu-icon-open');
  var iconClose = document.getElementById('mobile-menu-icon-close');

  if (btnMobile && mobileNav) {
    btnMobile.addEventListener('click', function () {
      var expanded = btnMobile.getAttribute('aria-expanded') === 'true';
      if (expanded) {
        btnMobile.setAttribute('aria-expanded', 'false');
        mobileNav.setAttribute('hidden', '');
        if (iconOpen) iconOpen.removeAttribute('hidden');
        if (iconClose) iconClose.setAttribute('hidden', '');
      } else {
        btnMobile.setAttribute('aria-expanded', 'true');
        mobileNav.removeAttribute('hidden');
        if (iconOpen) iconOpen.setAttribute('hidden', '');
        if (iconClose) iconClose.removeAttribute('hidden');
      }
    });
  }

  // Mở / đóng hộp thông báo chuông (Notification Dropdown)
  var btnNotif = document.getElementById('notif-toggle-btn');
  var notifPanel = document.getElementById('notif-panel');
  var btnMarkRead = document.getElementById('notif-mark-read-btn');
  var notifCount = document.getElementById('notif-count-dot');

  if (btnNotif && notifPanel) {
    btnNotif.addEventListener('click', function (e) {
      e.stopPropagation();
      var isHidden = notifPanel.hasAttribute('hidden');
      if (isHidden) {
        notifPanel.removeAttribute('hidden');
        btnNotif.setAttribute('aria-expanded', 'true');
      } else {
        notifPanel.setAttribute('hidden', '');
        btnNotif.setAttribute('aria-expanded', 'false');
      }
    });

    // Bấm chuột ra ngoài hộp thông báo thì tự động đóng lại
    document.addEventListener('click', function (e) {
      if (!notifPanel.contains(e.target) && !btnNotif.contains(e.target)) {
        notifPanel.setAttribute('hidden', '');
        btnNotif.setAttribute('aria-expanded', 'false');
      }
    });
  }

  // Đánh dấu đã đọc tất cả thông báo
  if (btnMarkRead) {
    btnMarkRead.addEventListener('click', function () {
      var unreadItems = document.querySelectorAll('.notif-item.is-unread');
      unreadItems.forEach(function (item) {
        item.classList.remove('is-unread');
      });
      if (notifCount) {
        notifCount.textContent = '0';
      }
      showToast('Đã đánh dấu đọc tất cả thông báo.');
    });
  }
}

// 4. Xử lý nút trái tim Yêu thích (Wishlist Toggle)
function initFavorites() {
  document.addEventListener('click', function (e) {
    var favBtn = e.target.closest('.favorite-button, .favorite-detail');
    if (!favBtn) {
      return;
    }
    e.preventDefault();

    // Đảo trạng thái class 'is-saved'
    var isSaved = favBtn.classList.toggle('is-saved');
    favBtn.setAttribute('aria-pressed', String(isSaved));

    // Tô màu icon trái tim
    var svg = favBtn.querySelector('svg');
    if (svg) {
      if (isSaved) {
        svg.setAttribute('fill', 'currentColor');
      } else {
        svg.setAttribute('fill', 'none');
      }
    }

    // Hiển thị thông báo Toast
    if (isSaved) {
      showToast('Đã thêm vào danh sách yêu thích.');
    } else {
      showToast('Đã bỏ khỏi danh sách yêu thích.');
    }
  });
}

// Khi tài liệu HTML tải xong thì khởi tạo các chức năng
document.addEventListener('DOMContentLoaded', function () {
  initReveals();
  initHeaderNav();
  initHeaderActions();
  initFavorites();
});
