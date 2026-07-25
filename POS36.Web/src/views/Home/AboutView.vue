<template>
  <div>
    <PublicNavbar />

    <main class="bg-light text-dark font-body pt-5 mt-5">
      <!-- ====== Hero Section ====== -->
      <section
        class="container max-w-7xl py-5 mt-4 position-relative overflow-hidden"
      >
        <div class="row align-items-center g-5">
          <div class="col-lg-6 z-index-1">
            <span
              class="badge bg-primary-fixed text-primary-variant px-3 py-2 rounded-pill fw-bold tracking-wide mb-4"
            >
              CHUYỂN ĐỔI SỐ F&B
            </span>
            <h1
              class="display-3 font-headline fw-black tracking-tighter mb-4 text-dark"
            >
              Giải Pháp POS <br /><span class="text-orange"
                >Cho Nhà Hàng Hiện Đại</span
              >
            </h1>
            <p class="fs-5 text-secondary lh-lg mb-5 max-w-lg">
              POS36 là hệ thống quản lý nhà hàng và quán cafe hiện đại, ứng dụng
              công nghệ SignalR real-time và kiến trúc microservice để tối ưu hóa
              toàn bộ quy trình vận hành F&amp;B.
            </p>
            <div class="d-flex flex-wrap gap-3">
              <router-link
                to="/register"
                class="btn culinary-gradient text-white px-4 py-3 rounded-4 fw-bold ambient-shadow hover-scale"
              >
                Dùng thử miễn phí
              </router-link>
              <router-link
                to="/features"
                class="btn btn-outline-secondary px-4 py-3 rounded-4 fw-bold hover-scale bg-white"
              >
                Khám phá tính năng
              </router-link>
            </div>
          </div>

          <div class="col-lg-6 position-relative">
            <div
              class="ratio ratio-1x1 rounded-5 overflow-hidden ambient-shadow rotate-img transition-img"
            >
              <img
                src="https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?auto=format&fit=crop&q=80&w=800"
                class="object-fit-cover"
                alt="Nhà hàng hiện đại"
              />
            </div>
            <div
              class="position-absolute bottom-0 start-0 glass-effect p-4 rounded-4 ambient-shadow ms-n4 mb-n4 d-none d-lg-block border"
              style="width: 250px"
            >
              <div class="d-flex align-items-center gap-3 mb-3">
                <div class="p-2 bg-orange-subtle rounded-3">
                  <i class="bi bi-shield-check text-orange fs-4"></i>
                </div>
                <div>
                  <div class="small fw-bold text-muted">CÔNG NGHỆ</div>
                  <div class="fs-6 font-headline fw-black text-dark">
                    Real-time SignalR
                  </div>
                </div>
              </div>
              <div class="progress" style="height: 6px">
                <div
                  class="progress-bar bg-orange"
                  role="progressbar"
                  style="width: 100%"
                ></div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- ====== Câu chuyện ====== -->
      <section class="bg-surface-container py-5">
        <div class="container max-w-7xl py-5">
          <div class="row g-5">
            <div class="col-lg-5">
              <h2 class="font-headline display-6 fw-black mb-4">
                Về dự án POS36
              </h2>
              <div class="divider-orange mb-5"></div>
              <p class="text-secondary lh-lg mb-4">
                POS36 được phát triển như một đồ án tốt nghiệp với mục tiêu xây
                dựng một hệ thống Point-of-Sale hoàn chỉnh cho ngành F&amp;B tại
                Việt Nam. Hệ thống hướng đến việc giải quyết bài toán vận hành thực
                tế của các nhà hàng và quán cafe.
              </p>
              <p class="text-secondary lh-lg">
                Ứng dụng được thiết kế theo mô hình SaaS (Software-as-a-Service),
                cho phép nhiều cửa hàng sử dụng đồng thời trên cùng một nền tảng,
                với dữ liệu được phân tách và bảo mật riêng biệt theo từng tenant.
              </p>
            </div>
            <div class="col-lg-7">
              <div class="row g-4">
                <div class="col-sm-6 d-flex flex-column gap-4">
                  <div class="ratio ratio-4x3 rounded-4 overflow-hidden">
                    <img
                      src="https://images.unsplash.com/photo-1577219491135-ce391730fb2c?auto=format&fit=crop&q=80&w=500"
                      class="object-fit-cover"
                      alt="Kitchen Display"
                    />
                  </div>
                  <div class="bg-white p-4 rounded-4 ambient-shadow border">
                    <span
                      class="display-6 font-headline fw-black text-orange d-block mb-2"
                      >Vue 3</span
                    >
                    <p class="small fw-bold mb-0 text-muted">
                      Frontend Framework
                    </p>
                  </div>
                </div>
                <div class="col-sm-6 d-flex flex-column gap-4 mt-sm-5">
                  <div class="bg-dark p-4 rounded-4 text-white ambient-shadow">
                    <span
                      class="display-6 font-headline fw-black d-block mb-2 text-warning"
                      >.NET 8</span
                    >
                    <p class="small fw-bold opacity-75 mb-0">
                      Backend API & SignalR
                    </p>
                  </div>
                  <div class="ratio ratio-4x3 rounded-4 overflow-hidden">
                    <img
                      src="https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?auto=format&fit=crop&q=80&w=500"
                      class="object-fit-cover"
                      alt="POS System"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- ====== Công nghệ sử dụng ====== -->
      <section class="container max-w-7xl py-5 my-5">
        <h2 class="font-headline display-5 fw-black text-center mb-5 pb-4">
          Công nghệ &amp; Kiến trúc
        </h2>
        <div class="row g-4">
          <div class="col-md-6">
            <div class="bg-white border p-5 rounded-4 h-100 transition-hover">
              <div
                class="icon-box bg-primary-fixed rounded-4 d-flex align-items-center justify-content-center mb-4"
                style="width: 56px; height: 56px"
              >
                <i class="bi bi-layers-fill text-orange fs-3"></i>
              </div>
              <h4 class="fw-bold">Frontend</h4>
              <p class="text-secondary">
                Vue 3 + Composition API, Vue Router, Axios, SignalR Client.
                Giao diện responsive tối ưu cho Tablet và màn hình bếp.
              </p>
            </div>
          </div>
          <div class="col-md-6">
            <div
              class="culinary-gradient text-white p-5 rounded-4 h-100 transition-hover ambient-shadow"
            >
              <div
                class="icon-box bg-white bg-opacity-25 rounded-4 d-flex align-items-center justify-content-center mb-4"
                style="width: 56px; height: 56px"
              >
                <i class="bi bi-server text-white fs-3"></i>
              </div>
              <h4 class="fw-bold">Backend</h4>
              <p class="text-white-50">
                ASP.NET Core 8 Web API, Entity Framework Core, SQL Server,
                SignalR Hubs cho real-time communication bếp – order.
              </p>
            </div>
          </div>
          <div class="col-md-6">
            <div class="bg-white border p-5 rounded-4 h-100 transition-hover">
              <div
                class="icon-box bg-primary-fixed rounded-4 d-flex align-items-center justify-content-center mb-4"
                style="width: 56px; height: 56px"
              >
                <i class="bi bi-lightning-charge-fill text-orange fs-3"></i>
              </div>
              <h4 class="fw-bold">Real-time</h4>
              <p class="text-secondary">
                SignalR WebSocket đảm bảo đồng bộ tức thời giữa màn hình gọi
                món, màn hình bếp (KDS) và quầy thanh toán.
              </p>
            </div>
          </div>
          <div class="col-md-6">
            <div
              class="bg-surface-container border p-5 rounded-4 h-100 transition-hover"
            >
              <div
                class="icon-box bg-primary-fixed rounded-4 d-flex align-items-center justify-content-center mb-4"
                style="width: 56px; height: 56px"
              >
                <i class="bi bi-shield-fill-check text-orange fs-3"></i>
              </div>
              <h4 class="fw-bold">Multi-tenant SaaS</h4>
              <p class="text-secondary">
                Mô hình SaaS cho phép nhiều cửa hàng hoạt động độc lập trên
                cùng một hệ thống, với phân quyền theo vai trò rõ ràng.
              </p>
            </div>
          </div>
        </div>
      </section>

      <!-- ====== Giá trị cốt lõi ====== -->
      <section class="bg-surface-container py-5">
        <div class="container max-w-7xl py-5">
          <div class="text-center mb-5 pb-3">
            <h2 class="font-headline display-5 fw-black mb-3">
              Mục tiêu phát triển
            </h2>
            <p class="text-secondary fs-5">
              Những giá trị mà POS36 hướng đến
            </p>
          </div>
          <div class="row g-4">
            <div class="col-md-4">
              <div
                class="bg-white p-5 rounded-4 h-100 transition-hover ambient-shadow border-0"
              >
                <div
                  class="icon-box bg-primary-fixed rounded-4 d-flex align-items-center justify-content-center mb-4"
                  style="width: 64px; height: 64px"
                >
                  <i class="bi bi-speedometer2 text-orange fs-3"></i>
                </div>
                <h3 class="font-headline fw-black mb-3">Hiệu suất</h3>
                <p class="text-secondary lh-lg mb-0">
                  Rút ngắn thời gian xử lý đơn hàng, giảm thiểu sai sót trong
                  quy trình phục vụ từ gọi món đến thanh toán.
                </p>
              </div>
            </div>
            <div class="col-md-4">
              <div
                class="bg-white p-5 rounded-4 h-100 transition-hover ambient-shadow border-0"
              >
                <div
                  class="icon-box bg-danger bg-opacity-10 rounded-4 d-flex align-items-center justify-content-center mb-4"
                  style="width: 64px; height: 64px"
                >
                  <i class="bi bi-heart-fill text-danger fs-3"></i>
                </div>
                <h3 class="font-headline fw-black mb-3">Đơn giản</h3>
                <p class="text-secondary lh-lg mb-0">
                  Giao diện trực quan, dễ dùng cho nhân viên không có nhiều kinh
                  nghiệm kỹ thuật. Đào tạo nhanh, triển khai gọn.
                </p>
              </div>
            </div>
            <div class="col-md-4">
              <div
                class="bg-white p-5 rounded-4 h-100 transition-hover ambient-shadow border-0"
              >
                <div
                  class="icon-box bg-secondary bg-opacity-10 rounded-4 d-flex align-items-center justify-content-center mb-4"
                  style="width: 64px; height: 64px"
                >
                  <i class="bi bi-shield-fill-check text-secondary fs-3"></i>
                </div>
                <h3 class="font-headline fw-black mb-3">Tin cậy</h3>
                <p class="text-secondary lh-lg mb-0">
                  Dữ liệu phân tách theo tenant, hệ thống phân quyền chặt chẽ
                  theo role. Đảm bảo an toàn thông tin kinh doanh.
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- ====== CTA ====== -->
      <section class="container max-w-7xl mb-5 pb-5 pt-5 mt-5">
        <div
          class="culinary-gradient rounded-5 p-5 text-center text-white ambient-shadow position-relative overflow-hidden"
        >
          <div class="position-relative z-index-1 py-4">
            <h2 class="display-4 font-headline fw-black mb-4">
              Sẵn sàng trải nghiệm POS36?
            </h2>
            <p class="fs-5 opacity-75 mb-5 max-w-lg mx-auto">
              Đăng ký tài khoản ngay để bắt đầu dùng thử đầy đủ tính năng.
              Không yêu cầu thẻ tín dụng.
            </p>
            <div
              class="d-flex flex-column flex-sm-row gap-3 justify-content-center"
            >
              <router-link
                to="/register"
                class="btn btn-light text-orange rounded-4 px-5 py-3 fw-bold fs-5 shadow hover-scale"
              >
                Đăng ký dùng thử ngay
              </router-link>
              <router-link
                to="/pricing"
                class="btn btn-outline-light rounded-4 px-5 py-3 fw-bold fs-5 hover-scale"
              >
                Xem bảng giá
              </router-link>
            </div>
          </div>
          <i
            class="bi bi-shop position-absolute opacity-10"
            style="
              font-size: 20rem;
              right: -50px;
              bottom: -100px;
              transform: rotate(15deg);
            "
          ></i>
        </div>
      </section>
    </main>

    <PublicFooter />
  </div>
</template>

<script setup>
import PublicNavbar from "../../components/PublicNavbar.vue";
import PublicFooter from "../../components/PublicFooter.vue";
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
.hover-text-orange:hover {
  color: #ea580c !important;
  transition: 0.2s;
}

.bg-primary-fixed {
  background-color: #ffdbc8;
}
.text-primary-variant {
  color: #753400;
}
.bg-surface-container {
  background-color: #f8fafc;
}

.culinary-gradient {
  background: linear-gradient(135deg, #994700 0%, #ff7a00 100%);
  border: none;
}
.ambient-shadow {
  box-shadow: 0 40px 60px -20px rgba(26, 28, 28, 0.06);
}
.glass-effect {
  backdrop-filter: blur(24px);
  background: rgba(255, 255, 255, 0.85);
}

.divider-orange {
  width: 60px;
  height: 6px;
  background-color: #ea580c;
  border-radius: 10px;
}

/* Hiệu ứng ảnh xoay nhẹ */
.rotate-img {
  transform: rotate(3deg);
}
.transition-img {
  transition: transform 0.7s ease;
}
.transition-img:hover {
  transform: rotate(0deg);
}

/* Hiệu ứng Hover Card */
.transition-hover {
  transition:
    transform 0.3s ease,
    box-shadow 0.3s ease;
}
.transition-hover:hover {
  transform: translateY(-5px);
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
}
.hover-scale {
  transition: transform 0.2s ease;
}
.hover-scale:hover {
  transform: scale(1.05);
}

.max-w-7xl {
  max-width: 1280px;
  margin: 0 auto;
}
.max-w-lg {
  max-width: 600px;
}
.z-index-1 {
  z-index: 1;
  position: relative;
}

.icon-box {
  flex-shrink: 0;
}
</style>
