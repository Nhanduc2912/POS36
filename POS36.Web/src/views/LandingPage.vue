<script setup>
import PublicNavbar from "../components/PublicNavbar.vue";
</script>

<template>
  <div class="landing-wrapper">
    <PublicNavbar />

    <section class="hero-section d-flex align-items-center">
      <div class="container">
        <div class="row align-items-center">
          <div class="col-lg-6 text-center text-lg-start mb-5 mb-lg-0">
            <h1 class="display-4 fw-bolder text-dark mb-4">
              Quản lý nhà hàng <br />
              <span class="text-primary">Dễ dàng & Tốc độ</span>
            </h1>
            <p class="lead text-muted mb-5 pe-lg-5">
              POS36 mang đến giải pháp toàn diện: Order tại bàn, báo bếp tức
              thì, thanh toán nhanh chóng và quản lý doanh thu chặt chẽ. Dành
              riêng cho các chủ quán bận rộn.
            </p>
            <div
              class="d-flex flex-column flex-sm-row justify-content-center justify-content-lg-start gap-3"
            >
              <router-link
                to="/register"
                class="btn btn-primary btn-lg rounded-pill fw-bold px-5 shadow"
              >
                Bắt đầu ngay <i class="bi bi-arrow-right ms-2"></i>
              </router-link>
              <router-link
                to="/login"
                class="btn btn-light btn-lg rounded-pill fw-bold px-5 border"
              >
                Xem Demo
              </router-link>
            </div>
          </div>

          <div class="col-lg-6">
            <div
              class="p-5 rounded-4 shadow-lg bg-gradient text-white text-center d-flex flex-column justify-content-center align-items-center"
              style="height: 400px; background-color: #001529"
            >
              <i
                class="bi bi-display"
                style="font-size: 5rem; color: #1890ff"
              ></i>
              <h3 class="mt-3 fw-bold">Giao diện POS Siêu Tốc</h3>
              <p class="text-white-50">Tối ưu cho thao tác chạm của Thu ngân</p>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.landing-wrapper {
  /* Chừa khoảng trống cho fixed navbar */
  padding-top: 80px;
  min-height: 100vh;
  background-color: #f8f9fa;
}

.hero-section {
  min-height: calc(100vh - 80px);
}
</style>
