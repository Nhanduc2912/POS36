<script setup>
import PublicNavbar from "../../components/PublicNavbar.vue";
</script>

<template>
  <div class="landing-wrapper">
    <PublicNavbar />

    <main class="main-content">
      <section class="hero-section container">
        <div class="row align-items-center min-vh-75">
          <div class="col-lg-6 text-center text-lg-start mb-5 mb-lg-0 pe-lg-5">
            <h1
              class="display-3 fw-black text-dark lh-sm mb-4 font-headline tracking-tighter"
            >
              Quản Lý Nhà Hàng <br />
              <span class="text-orange">Dễ Dàng & Tốc Độ</span>
            </h1>
            <p class="lead text-secondary mb-5 fs-6 lh-lg pe-lg-4">
              Nâng tầm trải nghiệm ẩm thực và tối ưu hóa quy trình vận hành với
              giải pháp POS thông minh nhất dành cho nhà hàng và quán cafe hiện
              đại.
            </p>
            <div
              class="d-flex flex-column flex-sm-row justify-content-center justify-content-lg-start gap-3"
            >
              <router-link
                to="/register"
                class="btn btn-orange text-white rounded-3 px-5 py-3 fw-bold shadow hover-scale"
              >
                Bắt đầu ngay
              </router-link>
            </div>
          </div>

          <div class="col-lg-6 position-relative text-center">
            <div class="blob-bg"></div>
            <div class="mockup-container mx-auto">
              <img
                src="https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?q=80&w=800&auto=format&fit=crop"
                class="img-fluid rounded-4 shadow-lg mockup-img"
                alt="POS Mockup"
              />
            </div>
          </div>
        </div>
      </section>

      <section class="social-proof py-5 text-center">
        <p class="small text-muted fw-bold text-uppercase tracking-widest mb-4">
          Tin dùng bởi các thương hiệu hàng đầu
        </p>
        <div class="d-flex flex-wrap justify-content-center gap-5 opacity-75">
          <div class="brand-item fw-black fs-5">
            <i class="bi bi-cup-hot-fill me-2"></i> Cafe Hub
          </div>
          <div class="brand-item fw-black fs-5">
            <i class="bi bi-shop me-2"></i> Foodie
          </div>
          <div class="brand-item fw-black fs-5">
            <i class="bi bi-egg-fried me-2"></i> Restauro
          </div>
        </div>
      </section>

      <section class="features-section py-5 mt-5 container">
        <div class="text-center mb-5">
          <h2 class="display-6 font-headline fw-black text-dark mb-3">
            Tính năng đột phá
          </h2>
          <div class="signature-line mx-auto rounded-pill"></div>
        </div>

        <div class="row g-4 px-lg-5">
          <div class="col-md-4">
            <div
              class="card h-100 border-0 bg-white rounded-4 p-4 card-hover shadow-sm"
            >
              <div
                class="icon-box bg-orange-soft text-orange rounded-3 d-flex align-items-center justify-content-center mb-4"
              >
                <i class="bi bi-lightning-charge-fill fs-3"></i>
              </div>
              <h5 class="font-headline fw-bold mb-3">Fast Interface</h5>
              <p class="text-muted small lh-lg mb-0">
                Giao diện tối giản, tốc độ xử lý nhanh chóng giúp nhân viên phục
                vụ khách hàng trong tích tắc.
              </p>
            </div>
          </div>
          <div class="col-md-4">
            <div
              class="card h-100 border-0 bg-white rounded-4 p-4 card-hover shadow-sm"
            >
              <div
                class="icon-box bg-orange-soft text-orange rounded-3 d-flex align-items-center justify-content-center mb-4"
              >
                <i class="bi bi-shop-window fs-3"></i>
              </div>
              <h5 class="font-headline fw-bold mb-3">Table Ordering</h5>
              <p class="text-muted small lh-lg mb-0">
                Quản lý sơ đồ bàn trực quan, gọi món tại bàn và đồng bộ dữ liệu
                ngay lập tức xuống bếp.
              </p>
            </div>
          </div>
          <div class="col-md-4">
            <div
              class="card h-100 border-0 bg-white rounded-4 p-4 card-hover shadow-sm"
            >
              <div
                class="icon-box bg-orange-soft text-orange rounded-3 d-flex align-items-center justify-content-center mb-4"
              >
                <i class="bi bi-box-seam-fill fs-3"></i>
              </div>
              <h5 class="font-headline fw-bold mb-3">Inventory Management</h5>
              <p class="text-muted small lh-lg mb-0">
                Kiểm soát nguyên vật liệu chính xác theo thời gian thực, cảnh
                báo khi tồn kho xuống thấp.
              </p>
            </div>
          </div>
        </div>
      </section>

      <section class="banner-section container py-5 my-4">
        <div
          class="bg-orange-gradient rounded-5 p-5 text-white position-relative overflow-hidden shadow-lg"
        >
          <div class="circle-decor"></div>

          <div class="row align-items-center position-relative z-2">
            <div class="col-lg-8 text-center text-lg-start mb-4 mb-lg-0">
              <h2
                class="display-5 fw-black font-headline tracking-tighter mb-3"
              >
                Tối ưu doanh thu ngay hôm nay
              </h2>
              <p class="fs-5 opacity-75 mb-0">
                Gia nhập cộng đồng hơn 5,000+ nhà hàng và quán cafe đã thành
                công cùng POS36.
              </p>
            </div>
            <div
              class="col-lg-4 d-flex justify-content-center justify-content-lg-end"
            >
              <div
                class="trial-box text-center p-4 rounded-4 border border-light border-opacity-25"
              >
                <div class="display-6 fw-black font-headline mb-1">14 Ngày</div>
                <div
                  class="small fw-bold text-uppercase tracking-widest opacity-75 mb-3"
                >
                  DÙNG THỬ MIỄN PHÍ
                </div>
                <div
                  class="border-top border-light border-opacity-25 pt-2 small fst-italic opacity-50"
                >
                  Không yêu cầu thẻ tín dụng
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section class="cta-section py-5 text-center mt-4">
        <div class="container">
          <span
            class="text-orange fw-bold text-uppercase tracking-widest small mb-3 d-block"
            >SẴN SÀNG ĐỂ BỨT PHÁ?</span
          >
          <h2
            class="display-5 font-headline fw-black text-dark mb-5 max-w-75 mx-auto tracking-tighter"
          >
            Bắt đầu hành trình chuyển đổi số cho nhà hàng của bạn
          </h2>
          <router-link
            to="/register"
            class="btn btn-orange text-white rounded-pill px-5 py-3 fw-bold fs-5 shadow-lg hover-scale mb-4"
          >
            Đăng ký miễn phí
          </router-link>

          <div
            class="d-flex justify-content-center gap-4 text-muted small fw-bold"
          >
            <div class="d-flex align-items-center">
              <i class="bi bi-check-circle-fill text-success me-2"></i> Hỗ trợ
              24/7
            </div>
            <div class="d-flex align-items-center">
              <i class="bi bi-check-circle-fill text-success me-2"></i> Triển
              khai nhanh
            </div>
            <div class="d-flex align-items-center">
              <i class="bi bi-check-circle-fill text-success me-2"></i> Cập nhật
              miễn phí
            </div>
          </div>
        </div>
      </section>
    </main>

    <footer class="bg-white border-top py-5 mt-5">
      <div class="container">
        <div class="row gy-4">
          <div class="col-lg-4">
            <h4
              class="fw-black text-orange font-headline mb-3 tracking-tighter"
            >
              POS36
            </h4>
            <p class="text-muted small pe-lg-5 lh-lg">
              Giải pháp quản lý nhà hàng hiện đại, nâng tầm hiệu quả kinh doanh
              và trải nghiệm khách hàng tại Việt Nam.
            </p>
            <div class="d-flex gap-3 text-muted">
              <i class="bi bi-globe fs-5 cursor-pointer"></i>
              <i class="bi bi-envelope-fill fs-5 cursor-pointer"></i>
              <i class="bi bi-telephone-fill fs-5 cursor-pointer"></i>
            </div>
          </div>
          <div class="col-lg-4">
            <h6 class="fw-bold mb-3">Công ty</h6>
            <ul class="list-unstyled text-muted small lh-lg">
              <li>
                <a href="#" class="text-decoration-none text-muted hover-orange"
                  >Privacy Policy</a
                >
              </li>
              <li>
                <a href="#" class="text-decoration-none text-muted hover-orange"
                  >Terms of Service</a
                >
              </li>
              <li>
                <a href="#" class="text-decoration-none text-muted hover-orange"
                  >Cookie Settings</a
                >
              </li>
            </ul>
          </div>
          <div class="col-lg-4">
            <h6 class="fw-bold mb-3">Hỗ trợ</h6>
            <ul class="list-unstyled text-muted small lh-lg">
              <li>
                <a href="#" class="text-decoration-none text-muted hover-orange"
                  >Contact Support</a
                >
              </li>
              <li>
                <a href="#" class="text-decoration-none text-muted hover-orange"
                  >Global Locations</a
                >
              </li>
            </ul>
          </div>
        </div>
        <div class="text-center text-muted small border-top pt-4 mt-4">
          © 2024 POS36 Editorial Excellence. All rights reserved.
        </div>
      </div>
    </footer>
  </div>
</template>

<style scoped>
@import url("https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@700;800&family=Manrope:wght@400;500;600&display=swap");

.landing-wrapper {
  font-family: "Manrope", sans-serif;
  background-color: #fafafa;
  min-height: 100vh;
  padding-top: 76px;
}

.font-headline {
  font-family: "Plus Jakarta Sans", sans-serif;
}
.fw-black {
  font-weight: 800;
}
.tracking-tighter {
  letter-spacing: -1.5px;
}
.tracking-widest {
  letter-spacing: 2px;
}
.text-orange {
  color: #e65c00;
}

.btn-orange {
  background-color: #e65c00;
  border: none;
  transition: all 0.3s ease;
}
.btn-orange:hover {
  background-color: #cc5200;
  transform: translateY(-2px);
}

.hover-scale {
  transition: transform 0.2s ease;
}
.hover-scale:hover {
  transform: scale(1.05);
}

.hover-bg-gray:hover {
  background-color: #f1f1f1;
}
.hover-orange:hover {
  color: #e65c00 !important;
}

.cursor-pointer {
  cursor: pointer;
}
.cursor-pointer:hover {
  color: #e65c00;
}

/* Hero section mockup */
.blob-bg {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: 80%;
  height: 80%;
  background: rgba(230, 92, 0, 0.15);
  filter: blur(60px);
  border-radius: 50%;
  z-index: 0;
}

.mockup-container {
  position: relative;
  z-index: 1;
  max-width: 80%;
  transform: perspective(1000px) rotateY(-15deg) rotateX(5deg) scale(0.9);
  transition: transform 0.5s ease;
}

.mockup-container:hover {
  transform: perspective(1000px) rotateY(0deg) rotateX(0deg) scale(1);
}

.mockup-img {
  border: 10px solid #222; /* Giả làm viền tablet đen */
}

/* Features */
.signature-line {
  width: 60px;
  height: 4px;
  background-color: #e65c00;
}

.bg-orange-soft {
  background-color: #ffe6d5;
}

.card-hover {
  transition:
    transform 0.3s ease,
    box-shadow 0.3s ease;
}
.card-hover:hover {
  transform: translateY(-8px);
  box-shadow: 0 1rem 3rem rgba(0, 0, 0, 0.08) !important;
}

/* Orange Banner */
.bg-orange-gradient {
  background: linear-gradient(135deg, #d95300 0%, #ff8800 100%);
}

.circle-decor {
  position: absolute;
  top: -50%;
  right: -10%;
  width: 500px;
  height: 500px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.1);
  z-index: 1;
}

.trial-box {
  background: rgba(255, 255, 255, 0.15);
  backdrop-filter: blur(10px);
}

@media (max-width: 992px) {
  .mockup-container {
    transform: none;
    margin-top: 2rem;
  }
}
</style>
