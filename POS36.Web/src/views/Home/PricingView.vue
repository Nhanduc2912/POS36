<template>
  <div>
    <PublicNavbar />

    <main class="bg-light text-dark font-body pt-5 mt-5">
      <section class="container max-w-7xl py-5 mt-4 text-center">
        <span
          class="text-orange font-headline fw-bold tracking-wide small text-uppercase mb-3 d-block"
          >Đầu tư thông minh</span
        >
        <h1
          class="display-4 font-headline fw-black tracking-tighter mb-4 text-dark lh-sm"
        >
          Một mức giá duy nhất. <br class="d-none d-md-block" />
          <span class="text-orange fst-italic">Mở khóa toàn bộ sức mạnh.</span>
        </h1>
        <p class="fs-5 text-secondary max-w-lg mx-auto mb-5">
          Không giới hạn tính năng, không chi phí ẩn, không chia gói phức tạp.
          Chỉ cần chọn chu kỳ thanh toán phù hợp với dòng tiền của bạn.
        </p>

        <div
          class="d-inline-flex bg-white rounded-pill p-1 ambient-shadow mb-5 position-relative border"
        >
          <button
            class="btn rounded-pill px-4 py-2 fw-bold font-headline transition-all"
            :class="
              !isYearly
                ? 'bg-orange text-white shadow-sm'
                : 'text-secondary border-0 hover-text-orange'
            "
            @click="isYearly = false"
          >
            Trả theo tháng
          </button>
          <button
            class="btn rounded-pill px-4 py-2 fw-bold font-headline transition-all position-relative"
            :class="
              isYearly
                ? 'bg-orange text-white shadow-sm'
                : 'text-secondary border-0 hover-text-orange'
            "
            @click="isYearly = true"
          >
            Trả theo năm
            <span
              class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger shadow-sm border border-white"
              style="font-size: 0.7rem; margin-top: 2px"
            >
              Tiết kiệm 20%
            </span>
          </button>
        </div>
      </section>

      <section class="container max-w-lg mx-auto mb-5">
        <div
          class="culinary-gradient rounded-5 p-5 text-white ambient-shadow hover-lift text-center position-relative overflow-hidden z-index-1"
        >
          <i
            class="bi bi-star-fill position-absolute text-white opacity-10"
            style="
              font-size: 15rem;
              top: -50px;
              right: -50px;
              transform: rotate(20deg);
            "
          ></i>

          <div class="position-relative z-index-1">
            <h2 class="font-headline fw-black mb-1 fs-3">
              Gói Toàn Diện Cấp Doanh Nghiệp
            </h2>
            <p class="text-white-50 mb-4">
              Mọi tính năng, hỗ trợ 24/7, cập nhật trọn đời.
            </p>

            <div class="mb-4">
              <span class="display-2 fw-black text-white">{{
                isYearly ? "399k" : "499k"
              }}</span>
              <span class="text-white-50 fs-5">/tháng</span>
            </div>

            <div
              class="bg-white bg-opacity-10 rounded-3 p-3 mb-5 border border-white border-opacity-25"
            >
              <p class="text-white small mb-0 fw-medium">
                <i class="bi bi-info-circle-fill me-1 text-warning"></i>
                {{
                  isYearly
                    ? "Thanh toán mỗi năm một lần (4.788.000đ/năm)."
                    : "Thanh toán linh hoạt từng tháng một. Hủy bất cứ lúc nào."
                }}
              </p>
            </div>

            <button
              class="btn btn-light text-orange w-100 py-3 fw-black rounded-4 shadow-lg hover-scale fs-5 text-uppercase tracking-wide"
            >
              Bắt đầu 14 ngày dùng thử
            </button>
            <p class="small text-white-50 mt-3 mb-0">
              Không yêu cầu thẻ tín dụng
            </p>
          </div>
        </div>
      </section>

      <section class="container max-w-7xl py-5 mb-5">
        <div class="text-center mb-5">
          <h2 class="font-headline fw-black display-6">
            Bao gồm tất cả. Không thỏa hiệp.
          </h2>
          <p class="text-secondary">
            Tại sao phải mua thêm tính năng khi bạn có thể sở hữu tất cả ngay từ
            ngày đầu tiên?
          </p>
        </div>

        <div class="row g-4">
          <div class="col-md-4">
            <div
              class="bg-white p-4 rounded-4 ambient-shadow h-100 border-top border-4 border-orange"
            >
              <div
                class="icon-box bg-orange-subtle rounded-circle d-flex align-items-center justify-content-center mb-4"
                style="width: 50px; height: 50px"
              >
                <i class="bi bi-shop text-orange fs-4"></i>
              </div>
              <h4 class="font-headline fw-bold mb-3">Vận hành cốt lõi</h4>
              <ul
                class="list-unstyled space-y-3 small text-secondary fw-medium"
              >
                <li class="mb-2">
                  <i class="bi bi-check-lg text-success me-2 fs-5"></i> Bán hàng
                  tại quầy & Order tại bàn
                </li>
                <li class="mb-2">
                  <i class="bi bi-check-lg text-success me-2 fs-5"></i> Quản lý
                  sơ đồ bàn trực quan
                </li>
                <li class="mb-2">
                  <i class="bi bi-check-lg text-success me-2 fs-5"></i> Chế độ
                  Offline không gián đoạn
                </li>
                <li>
                  <i class="bi bi-check-lg text-success me-2 fs-5"></i> Kết nối
                  máy in bếp & tem nhãn
                </li>
              </ul>
            </div>
          </div>
          <div class="col-md-4">
            <div
              class="bg-white p-4 rounded-4 ambient-shadow h-100 border-top border-4 border-orange"
            >
              <div
                class="icon-box bg-orange-subtle rounded-circle d-flex align-items-center justify-content-center mb-4"
                style="width: 50px; height: 50px"
              >
                <i class="bi bi-box-seam text-orange fs-4"></i>
              </div>
              <h4 class="font-headline fw-bold mb-3">Quản lý chuyên sâu</h4>
              <ul
                class="list-unstyled space-y-3 small text-secondary fw-medium"
              >
                <li class="mb-2">
                  <i class="bi bi-check-lg text-success me-2 fs-5"></i> Quản lý
                  kho nguyên vật liệu
                </li>
                <li class="mb-2">
                  <i class="bi bi-check-lg text-success me-2 fs-5"></i> Định mức
                  cấu thành món ăn
                </li>
                <li class="mb-2">
                  <i class="bi bi-check-lg text-success me-2 fs-5"></i> CRM &
                  Tích điểm hội viên
                </li>
                <li>
                  <i class="bi bi-check-lg text-success me-2 fs-5"></i> Quản lý
                  thu chi & Công nợ
                </li>
              </ul>
            </div>
          </div>
          <div class="col-md-4">
            <div
              class="bg-white p-4 rounded-4 ambient-shadow h-100 border-top border-4 border-orange"
            >
              <div
                class="icon-box bg-orange-subtle rounded-circle d-flex align-items-center justify-content-center mb-4"
                style="width: 50px; height: 50px"
              >
                <i class="bi bi-robot text-orange fs-4"></i>
              </div>
              <h4 class="font-headline fw-bold mb-3">Quyền năng AI & Hỗ trợ</h4>
              <ul
                class="list-unstyled space-y-3 small text-secondary fw-medium"
              >
                <li class="mb-2">
                  <i class="bi bi-check-lg text-success me-2 fs-5"></i> Báo cáo
                  doanh thu thông minh AI
                </li>
                <li class="mb-2">
                  <i class="bi bi-check-lg text-success me-2 fs-5"></i> Tự động
                  đề xuất chiến lược bán hàng
                </li>
                <li class="mb-2">
                  <i class="bi bi-check-lg text-success me-2 fs-5"></i>
                  Dashboard quản trị từ xa (Web/App)
                </li>
                <li>
                  <i class="bi bi-check-lg text-success me-2 fs-5"></i> Hỗ trợ
                  kỹ thuật ưu tiên 24/7
                </li>
              </ul>
            </div>
          </div>
        </div>
      </section>

      <section class="container max-w-lg mx-auto py-5 mb-5">
        <h2 class="font-headline fw-bold text-center mb-5 display-6">
          Câu hỏi thường gặp
        </h2>
        <div class="accordion accordion-flush bg-transparent" id="pricingFAQ">
          <div
            class="accordion-item border-0 mb-3 bg-white rounded-4 ambient-shadow overflow-hidden"
          >
            <h2 class="accordion-header">
              <button
                class="accordion-button collapsed fw-bold p-4 text-dark bg-white"
                type="button"
                data-bs-toggle="collapse"
                data-bs-target="#faq1"
              >
                Tôi mở thêm chi nhánh thì tính phí thế nào?
              </button>
            </h2>
            <div
              id="faq1"
              class="accordion-collapse collapse"
              data-bs-parent="#pricingFAQ"
            >
              <div class="accordion-body px-4 pb-4 pt-0 text-secondary">
                Mức giá trên được tính cho 1 điểm bán (Cửa hàng). Khi bạn mở
                rộng thêm chi nhánh mới, bạn chỉ cần mua thêm 1 gói tương tự cho
                chi nhánh đó. Tất cả dữ liệu chuỗi sẽ được đồng bộ lên cùng một
                tài khoản quản trị trung tâm hoàn toàn miễn phí.
              </div>
            </div>
          </div>

          <div
            class="accordion-item border-0 mb-3 bg-white rounded-4 ambient-shadow overflow-hidden"
          >
            <h2 class="accordion-header">
              <button
                class="accordion-button collapsed fw-bold p-4 text-dark bg-white"
                type="button"
                data-bs-toggle="collapse"
                data-bs-target="#faq2"
              >
                Hết 14 ngày dùng thử, dữ liệu của tôi có bị mất không?
              </button>
            </h2>
            <div
              id="faq2"
              class="accordion-collapse collapse"
              data-bs-parent="#pricingFAQ"
            >
              <div class="accordion-body px-4 pb-4 pt-0 text-secondary">
                Tuyệt đối không! Dữ liệu của bạn được lưu trữ an toàn. Khi bạn
                quyết định nâng cấp lên tài khoản trả phí, mọi dữ liệu món ăn,
                hóa đơn, tồn kho từ quá trình dùng thử sẽ được giữ nguyên để bạn
                tiếp tục kinh doanh ngay lập tức.
              </div>
            </div>
          </div>

          <div
            class="accordion-item border-0 bg-white rounded-4 ambient-shadow overflow-hidden"
          >
            <h2 class="accordion-header">
              <button
                class="accordion-button collapsed fw-bold p-4 text-dark bg-white"
                type="button"
                data-bs-toggle="collapse"
                data-bs-target="#faq3"
              >
                Tôi muốn chuyển từ phần mềm cũ sang POS36 có được hỗ trợ không?
              </button>
            </h2>
            <div
              id="faq3"
              class="accordion-collapse collapse"
              data-bs-parent="#pricingFAQ"
            >
              <div class="accordion-body px-4 pb-4 pt-0 text-secondary">
                Có. Đội ngũ kỹ thuật của POS36 sẽ hỗ trợ bạn xuất dữ liệu (Menu,
                Khách hàng, Tồn kho) từ hệ thống cũ và import trực tiếp vào
                POS36 hoàn toàn miễn phí. Quá trình chuyển đổi diễn ra nhanh
                chóng mà không làm gián đoạn việc bán hàng.
              </div>
            </div>
          </div>
        </div>
      </section>

      <section class="container max-w-7xl pb-5 mb-5 mt-4">
        <div
          class="bg-dark rounded-5 p-5 text-center text-white ambient-shadow position-relative overflow-hidden border-bottom border-5 border-orange"
        >
          <div class="position-relative z-index-1 py-4">
            <h2 class="display-5 font-headline fw-black mb-4">
              Cài đặt chỉ trong 5 phút
            </h2>
            <p class="fs-5 opacity-75 mb-4 max-w-lg mx-auto">
              Không cần chuyên gia IT. Giao diện trực quan giúp nhân viên của
              bạn thao tác thuần thục ngay trong ca làm việc đầu tiên.
            </p>
            <div class="d-flex justify-content-center">
              <button
                class="btn culinary-gradient text-white rounded-pill px-5 py-3 fw-black fs-5 shadow-lg hover-scale"
              >
                Tạo tài khoản ngay
              </button>
            </div>
          </div>
        </div>
      </section>
    </main>

    <PublicFooter />
  </div>
</template>

<script setup>
import { ref } from "vue";
import PublicNavbar from "../../components/PublicNavbar.vue";
import PublicFooter from "../../components/PublicFooter.vue";

// Biến trạng thái để điều khiển thanh Toggle (Mặc định chọn theo năm để chốt sale)
const isYearly = ref(true);
</script>

<style scoped>
@import url("https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;800&family=Manrope:wght@400;600&display=swap");

.font-headline {
  font-family: "Plus Jakarta Sans", sans-serif;
}
.font-body {
  font-family: "Manrope", sans-serif;
}
.fw-black {
  font-weight: 800;
}
.tracking-tighter {
  letter-spacing: -1.5px;
}
.tracking-wide {
  letter-spacing: 1.5px;
}

.text-orange {
  color: #ea580c !important;
}
.bg-orange {
  background-color: #ea580c !important;
}
.bg-orange-subtle {
  background-color: #ffedd5 !important;
}
.border-orange {
  border-color: #ea580c !important;
}
.hover-text-orange:hover {
  color: #ea580c !important;
  transition: 0.2s;
}

.culinary-gradient {
  background: linear-gradient(135deg, #994700 0%, #ff7a00 100%);
  border: none;
}
.ambient-shadow {
  box-shadow: 0 40px 60px -20px rgba(26, 28, 28, 0.06);
}

.hover-lift {
  transition:
    transform 0.4s cubic-bezier(0.2, 0.8, 0.2, 1),
    box-shadow 0.4s;
}
.hover-lift:hover {
  transform: translateY(-10px) !important;
  box-shadow: 0 40px 60px -20px rgba(26, 28, 28, 0.15);
}
.hover-scale {
  transition: transform 0.2s ease;
}
.hover-scale:hover {
  transform: scale(1.05);
}

/* Custom Accordion */
.accordion-button:focus {
  box-shadow: none;
  border-color: rgba(0, 0, 0, 0.125);
}
.accordion-button:not(.collapsed) {
  background-color: #fff;
  color: #ea580c;
  box-shadow: none;
}
.accordion-button::after {
  background-size: 1rem;
}

.max-w-7xl {
  max-width: 1280px;
  margin: 0 auto;
}
.max-w-lg {
  max-width: 800px;
  margin: 0 auto;
}
.z-index-1 {
  z-index: 1;
  position: relative;
}
</style>
