// ==========================================================================
// MIARA Eyewear — admin.js
// Chức năng: Cổng Quản trị (Admin Portal)
//             Tab quản trị, Form CRUD, Hoàn tiền VNPay, Lọc người dùng, Khóa tài khoản, Preview ảnh
// Phân công báo cáo: Thành viên 3 (Quản trị hệ thống & Cấu hình)
// ==========================================================================

function initAdminPortal() {
  // 1. Chuyển đổi giữa các tab Quản trị (Sản phẩm, Danh mục, Thương hiệu, Người dùng)
  var adminBtns = document.querySelectorAll('[data-admin-tab]');
  if (adminBtns.length > 0) {
    var allPanels = document.querySelectorAll('.admin-tab-panel');

    adminBtns.forEach(function (btn) {
      btn.addEventListener('click', function () {
        var tabKey = btn.getAttribute('data-admin-tab');

        // Đổi trạng thái active cho các nút tab
        adminBtns.forEach(function (b) {
          if (b === btn) {
            b.classList.add('active');
          } else {
            b.classList.remove('active');
          }
        });

        // Ẩn/Hiện panel tương ứng
        allPanels.forEach(function (panel) {
          if (panel.id === 'admin-panel-' + tabKey) {
            panel.removeAttribute('hidden');
          } else {
            panel.setAttribute('hidden', '');
          }
        });
      });
    });
  }

  // 2. Nút "+ Thêm mới" mở nhanh form nhập liệu
  var toggleFormBtns = document.querySelectorAll('[data-toggle-form]');
  toggleFormBtns.forEach(function (btn) {
    btn.addEventListener('click', function () {
      var formId = btn.getAttribute('data-toggle-form');
      var form = document.getElementById(formId);
      if (!form) {
        return;
      }

      form.removeAttribute('hidden');
      form.reset();

      // Tự động focus vào ô nhập liệu đầu tiên
      var firstInput = form.querySelector('input[type="text"], input[type="email"]');
      if (firstInput) {
        firstInput.focus();
      }
      form.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    });
  });

  // 3. Nút "Sửa" trên từng dòng bảng -> Đưa thông tin lên form để chỉnh sửa
  var editRowBtns = document.querySelectorAll('.btn-edit-row');
  editRowBtns.forEach(function (btn) {
    btn.addEventListener('click', function () {
      var formId = btn.getAttribute('data-target-form');
      var editName = btn.getAttribute('data-edit-name');
      if (!editName) {
        editName = '';
      }

      var form = document.getElementById(formId);
      if (!form) {
        return;
      }

      form.removeAttribute('hidden');
      var firstInput = form.querySelector('input[type="text"], input[type="email"]');
      if (firstInput) {
        firstInput.value = editName;
        firstInput.focus();
      }
      form.scrollIntoView({ behavior: 'smooth', block: 'nearest' });

      if (typeof showToast === 'function') {
        showToast('Đang tải thông tin "' + editName + '" lên form để chỉnh sửa.');
      }
    });
  });

  // 4. Xử lý gửi các form CRUD
  var crudForms = document.querySelectorAll('.crud-form');
  crudForms.forEach(function (form) {
    form.addEventListener('submit', function (e) {
      if (form.getAttribute('action')) {
        // Cho phép gửi form thông thường lên ASP.NET Controller C# nếu form có khai báo action
        return;
      }
      e.preventDefault();
      if (typeof showToast === 'function') {
        showToast('Đã lưu dữ liệu vào hệ thống thành công!');
      }
    });
  });

  // 5. Nút Hoàn tiền đơn hàng VNPay
  var refundBtns = document.querySelectorAll('.btn-refund-order');
  refundBtns.forEach(function (btn) {
    btn.addEventListener('click', function () {
      var row = btn.closest('tr');
      var badge = null;
      if (row) {
        badge = row.querySelector('.status-cell-badge');
      }

      if (badge) {
        badge.className = 'status-badge status-badge--cancelled status-cell-badge';
        badge.textContent = 'Đã hủy & Hoàn tiền VNPay';
      }

      btn.disabled = true;
      btn.textContent = 'Đã hoàn tiền';

      if (typeof showToast === 'function') {
        showToast('Đã hủy đơn và gửi lệnh hoàn tiền VNPay thành công.');
      }
    });
  });

  // 6. Nút đóng form tạo tài khoản nhân viên / user
  var btnCloseCrudUser = document.getElementById('btn-close-staff-form');
  var formCrudUser = document.getElementById('form-create-staff');
  if (btnCloseCrudUser && formCrudUser) {
    btnCloseCrudUser.addEventListener('click', function () {
      formCrudUser.setAttribute('hidden', '');
    });
  }

  // 7. Tìm kiếm & Lọc danh sách người dùng theo Vai trò (Role) và Trạng thái (Status)
  var userSearchInput = document.getElementById('user-search-input');
  var userFilterRole = document.getElementById('user-filter-role');
  var userFilterStatus = document.getElementById('user-filter-status');
  var usersTable = document.getElementById('users-data-table');
  var totalUserCountEl = document.getElementById('total-user-count');

  function filterUsers() {
    if (!usersTable) {
      return;
    }

    var q = '';
    if (userSearchInput && userSearchInput.value) {
      q = userSearchInput.value.trim().toLowerCase();
    }

    var role = 'all';
    if (userFilterRole && userFilterRole.value) {
      role = userFilterRole.value;
    }

    var status = 'all';
    if (userFilterStatus && userFilterStatus.value) {
      status = userFilterStatus.value;
    }

    var rows = usersTable.querySelectorAll('tbody tr');
    var visibleCount = 0;

    rows.forEach(function (row) {
      var rowRole = row.getAttribute('data-user-role');
      if (!rowRole) {
        rowRole = '';
      }

      var rowStatus = row.getAttribute('data-user-status');
      if (!rowStatus) {
        rowStatus = '';
      }

      var textContent = row.textContent.toLowerCase();

      var matchQuery = false;
      if (q === '' || textContent.indexOf(q) !== -1) {
        matchQuery = true;
      }

      var matchRole = false;
      if (role === 'all' || rowRole === role) {
        matchRole = true;
      }

      var matchStatus = false;
      if (status === 'all' || rowStatus === status) {
        matchStatus = true;
      }

      // Nếu thỏa mãn cả 3 điều kiện lọc
      if (matchQuery && matchRole && matchStatus) {
        row.removeAttribute('hidden');
        visibleCount = visibleCount + 1;
      } else {
        row.setAttribute('hidden', '');
      }
    });

    if (totalUserCountEl) {
      totalUserCountEl.textContent = String(visibleCount);
    }
  }

  if (userSearchInput) {
    userSearchInput.addEventListener('input', filterUsers);
  }
  if (userFilterRole) {
    userFilterRole.addEventListener('change', filterUsers);
  }
  if (userFilterStatus) {
    userFilterStatus.addEventListener('change', filterUsers);
  }

  // 8. Khóa / Mở khóa tài khoản người dùng
  var toggleLockBtns = document.querySelectorAll('.btn-toggle-user-lock');
  toggleLockBtns.forEach(function (btn) {
    btn.addEventListener('click', function () {
      var row = btn.closest('tr');
      var badge = null;
      if (row) {
        badge = row.querySelector('.user-status-badge');
      }

      if (!badge) {
        return;
      }

      var isActive = (badge.textContent.trim() === 'Hoạt động');

      if (isActive) {
        badge.className = 'status-badge status-badge--cancelled user-status-badge';
        badge.textContent = 'Đã khóa';
        row.setAttribute('data-user-status', 'Đã khóa');
        btn.textContent = 'Mở khóa';
        btn.classList.remove('button-outline');
        if (typeof showToast === 'function') {
          showToast('Đã khóa tài khoản người dùng.');
        }
      } else {
        badge.className = 'status-badge status-badge--completed user-status-badge';
        badge.textContent = 'Hoạt động';
        row.setAttribute('data-user-status', 'Hoạt động');
        btn.textContent = 'Khóa tài khoản';
        btn.classList.add('button-outline');
        if (typeof showToast === 'function') {
          showToast('Đã mở khóa tài khoản người dùng.');
        }
      }
    });
  });

  // 9. Nút Xóa bản ghi trong các bảng quản trị
  var deleteRowBtns = document.querySelectorAll('.btn-delete-row');
  deleteRowBtns.forEach(function (btn) {
    btn.addEventListener('click', function () {
      var row = btn.closest('tr');
      if (row) {
        row.remove();
      }
      if (typeof showToast === 'function') {
        showToast('Đã xóa bản ghi thành công.');
      }
    });
  });

  // 10. Xem trước ảnh khi chọn file tải lên (Image Preview)
  var fileInputsWithPreview = document.querySelectorAll('input[type="file"][data-preview-target]');
  fileInputsWithPreview.forEach(function (input) {
    input.addEventListener('change', function () {
      var targetId = input.getAttribute('data-preview-target');
      var imgPreview = document.getElementById(targetId);

      if (imgPreview && input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
          imgPreview.src = e.target.result;
          imgPreview.removeAttribute('hidden');
        };
        reader.readAsDataURL(input.files[0]);
      }
    });
  });
}

document.addEventListener('DOMContentLoaded', function () {
  initAdminPortal();
});
