// ==========================================================================
// MIARA Eyewear — product.js
// Chức năng: Chi tiết sản phẩm, Slider ảnh, Chọn màu/size, Chọn tròng kính & Độ cận
// Phân công báo cáo: Thành viên 1 (Giao diện sản phẩm & Trải nghiệm mua sắm)
// ==========================================================================

// 1. Khởi tạo trang Chi tiết sản phẩm (Product Detail)
function initProductDetail() {
  var track = document.getElementById('detail-gallery-track');
  var thumbsWrap = document.getElementById('detail-thumbs');

  // Điều khiển Slider ảnh thumbnail
  if (track && thumbsWrap) {
    var thumbBtns = Array.from(thumbsWrap.querySelectorAll('button[data-thumb-idx]'));
    var currentIdx = 0;

    // Hàm chuyển sang ảnh ở vị trí idx
    function goToSlide(idx) {
      if (thumbBtns.length === 0) {
        return;
      }
      // Tính chỉ số xoay vòng: nếu vượt quá thì quay về 0
      currentIdx = (idx + thumbBtns.length) % thumbBtns.length;
      track.style.transform = 'translateX(-' + (currentIdx * 100) + '%)';

      // Cập nhật trạng thái viền sáng cho nút thumbnail tương ứng
      for (var i = 0; i < thumbBtns.length; i++) {
        var btn = thumbBtns[i];
        if (i === currentIdx) {
          btn.setAttribute('aria-pressed', 'true');
        } else {
          btn.setAttribute('aria-pressed', 'false');
        }
      }
    }

    // Gán sự kiện click vào từng ảnh thumbnail nhỏ
    for (var j = 0; j < thumbBtns.length; j++) {
      (function (index) {
        thumbBtns[index].addEventListener('click', function () {
          goToSlide(index);
        });
      })(j);
    }

    // Nút mũi tên lùi (Previous) và tiến (Next)
    var prevBtn = document.getElementById('gallery-prev');
    var nextBtn = document.getElementById('gallery-next');
    if (prevBtn) {
      prevBtn.addEventListener('click', function () {
        goToSlide(currentIdx - 1);
      });
    }
    if (nextBtn) {
      nextBtn.addEventListener('click', function () {
        goToSlide(currentIdx + 1);
      });
    }
  }

  // Chọn biến thể màu sắc (Color pills)
  var colorPills = document.getElementById('detail-color-pills');
  var colorLabel = document.getElementById('selected-color-label');
  if (colorPills) {
    var allColorButtons = colorPills.querySelectorAll('.variant-pill');
    allColorButtons.forEach(function (btn) {
      btn.addEventListener('click', function () {
        // Gỡ bỏ class active ở các nút khác
        allColorButtons.forEach(function (b) {
          b.classList.remove('active');
        });
        // Kích hoạt nút vừa bấm
        btn.classList.add('active');
        // Hiển thị tên màu được chọn
        if (colorLabel) {
          var colorVal = btn.getAttribute('data-color-val');
          if (colorVal) {
            colorLabel.textContent = colorVal;
          }
        }
      });
    });
  }

  // Chọn kích thước gọng kính (Size pills)
  var sizePills = document.getElementById('detail-size-pills');
  var sizeLabel = document.getElementById('selected-size-label');
  if (sizePills) {
    var allSizeButtons = sizePills.querySelectorAll('.variant-pill');
    allSizeButtons.forEach(function (btn) {
      btn.addEventListener('click', function () {
        allSizeButtons.forEach(function (b) {
          b.classList.remove('active');
        });
        btn.classList.add('active');
        if (sizeLabel) {
          var sizeVal = btn.getAttribute('data-size-val');
          if (sizeVal) {
            sizeLabel.textContent = sizeVal;
          }
        }
      });
    });
  }

  // Gửi form đánh giá sản phẩm (Review submission)
  var reviewForm = document.getElementById('review-form');
  var reviewList = document.getElementById('review-list');
  if (reviewForm && reviewList) {
    reviewForm.addEventListener('submit', function (e) {
      e.preventDefault();

      var ratingSelect = reviewForm.querySelector('select[name="rating"]');
      var rating = 5;
      if (ratingSelect && ratingSelect.value) {
        rating = Number(ratingSelect.value);
      }

      var commentInput = reviewForm.querySelector('textarea[name="comment"]');
      var comment = '';
      if (commentInput) {
        comment = commentInput.value.trim();
      }

      if (!comment) {
        return;
      }

      // Tạo chuỗi sao hiển thị (Ví dụ: ★★★★★)
      var stars = '★★★★★'.slice(0, rating) + '☆☆☆☆☆'.slice(0, 5 - rating);

      // Tạo thẻ bài đánh giá mới
      var article = document.createElement('article');
      article.className = 'review-card is-revealed';
      article.innerHTML =
        '<strong>Khách hàng</strong>' +
        '<span class="review-stars">' + stars + '</span>' +
        '<span class="verified-purchase">Đã mua hàng</span>' +
        '<p>' + comment.replace(/</g, '&lt;') + '</p>' +
        '<time>Vừa xong</time>';

      // Chèn lên đầu danh sách đánh giá
      reviewList.prepend(article);
      reviewForm.reset();

      if (typeof showToast === 'function') {
        showToast('Cảm ơn bạn đã gửi đánh giá sản phẩm!');
      }
    });
  }
}

// 2. Khởi tạo trang Chọn tròng kính & Tải đơn kính khúc xạ (Lenses & Prescription)
function initLensesPage() {
  var form = document.getElementById('lens-prescription-form');
  if (!form) {
    return;
  }

  var framePrice = 249000;
  var summaryLensPrice = document.getElementById('summary-lens-price');
  var summaryCombinedPrice = document.getElementById('summary-combined-price');
  var cards = form.querySelectorAll('.lens-radio-card');
  var radios = form.querySelectorAll('input[name="lensOptionId"]');

  // Hàm tính lại tổng tiền khi khách chọn đổi gói tròng kính khác
  function updatePrice() {
    var lensPrice = 0;
    cards.forEach(function (card) {
      var r = card.querySelector('input[name="lensOptionId"]');
      var isChecked = false;
      if (r && r.checked) {
        isChecked = true;
      }
      card.classList.toggle('is-selected', isChecked);

      if (isChecked && r) {
        var priceAttr = r.getAttribute('data-lens-price');
        if (priceAttr) {
          lensPrice = Number(priceAttr);
        }
      }
    });

    // Định dạng tiền bằng hàm formatVND
    var displayLensText = '0 ₫';
    if (lensPrice > 0) {
      if (typeof formatVND === 'function') {
        displayLensText = '+' + formatVND(lensPrice);
      } else {
        displayLensText = '+' + lensPrice.toLocaleString('vi-VN') + ' ₫';
      }
    }

    if (summaryLensPrice) {
      summaryLensPrice.textContent = displayLensText;
    }

    var totalCombined = framePrice + lensPrice;
    if (summaryCombinedPrice) {
      if (typeof formatVND === 'function') {
        summaryCombinedPrice.textContent = formatVND(totalCombined);
      } else {
        summaryCombinedPrice.textContent = totalCombined.toLocaleString('vi-VN') + ' ₫';
      }
    }
  }

  // Lắng nghe sự kiện thay đổi radio tròng kính
  radios.forEach(function (r) {
    r.addEventListener('change', updatePrice);
  });

  // Chuyển tab: Tự nhập thông số độ cận vs Tải ảnh đơn kính
  var modeTabs = form.querySelectorAll('.rx-tab[data-rx-mode]');
  var panelManual = document.getElementById('rx-panel-manual');
  var panelUpload = document.getElementById('rx-panel-upload');

  modeTabs.forEach(function (tab) {
    tab.addEventListener('click', function () {
      modeTabs.forEach(function (t) {
        t.classList.remove('active');
      });
      tab.classList.add('active');

      var mode = tab.getAttribute('data-rx-mode');
      if (mode === 'upload') {
        if (panelManual) {
          panelManual.setAttribute('hidden', '');
        }
        if (panelUpload) {
          panelUpload.removeAttribute('hidden');
        }
      } else {
        if (panelUpload) {
          panelUpload.setAttribute('hidden', '');
        }
        if (panelManual) {
          panelManual.removeAttribute('hidden');
        }
      }
    });
  });

  // Hiển thị tên file xem trước khi chọn ảnh đơn kính từ máy tính
  var fileInput = document.getElementById('rx-file-input');
  var previewWrap = document.getElementById('rx-image-preview-wrap');
  var fileNameEl = document.getElementById('rx-image-filename');

  if (fileInput && previewWrap) {
    fileInput.addEventListener('change', function () {
      if (fileInput.files && fileInput.files[0]) {
        if (fileNameEl) {
          fileNameEl.textContent = fileInput.files[0].name;
        }
        previewWrap.removeAttribute('hidden');
        if (typeof showToast === 'function') {
          showToast('Đã tải lên ảnh đơn kính: ' + fileInput.files[0].name);
        }
      }
    });
  }

  // Khởi tạo tính toán ban đầu
  updatePrice();
}

document.addEventListener('DOMContentLoaded', function () {
  initProductDetail();
  initLensesPage();
});
