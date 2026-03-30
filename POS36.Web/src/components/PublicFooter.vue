<template>
  <footer class="w-100 border-top bg-white font-body pt-5 pb-4 mt-5">
    <div class="container max-w-7xl mx-auto px-4">
      <div class="row g-5 mb-5">
        <div class="col-lg-4">
          <div
            class="fs-3 font-headline fw-black text-dark mb-4 tracking-tighter"
          >
            POS36
          </div>
          <p class="text-secondary small lh-lg pe-lg-4">
            © 2026 POS36. Nền tảng quản lý nhà hàng hàng đầu Việt Nam. Tự hào
            đồng hành cùng sự phát triển của ngành ẩm thực Việt.
          </p>
          <div class="d-flex gap-3 mt-4">
            <a
              href="#"
              class="text-secondary fs-4 hover-text-orange transition-colors"
              ><i class="bi bi-facebook"></i
            ></a>
            <a
              href="#"
              class="text-secondary fs-4 hover-text-orange transition-colors"
              ><i class="bi bi-youtube"></i
            ></a>
            <a
              href="#"
              class="text-secondary fs-4 hover-text-orange transition-colors"
              ><i class="bi bi-linkedin"></i
            ></a>
          </div>
        </div>

        <div class="col-6 col-lg-2 offset-lg-1">
          <h6 class="font-headline fw-bold text-dark mb-4">Tính năng</h6>
          <ul class="list-unstyled space-y-3 small text-secondary">
            <li class="mb-3">
              <a
                href="#"
                class="text-decoration-none text-secondary hover-text-orange transition-colors"
                >Quản lý kho</a
              >
            </li>
            <li class="mb-3">
              <a
                href="#"
                class="text-decoration-none text-secondary hover-text-orange transition-colors"
                >Order tại bàn</a
              >
            </li>
            <li class="mb-3">
              <a
                href="#"
                class="text-decoration-none text-secondary hover-text-orange transition-colors"
                >Báo cáo tài chính</a
              >
            </li>
            <li>
              <a
                href="#"
                class="text-decoration-none text-secondary hover-text-orange transition-colors"
                >Loyalty Program</a
              >
            </li>
          </ul>
        </div>

        <div class="col-6 col-lg-2">
          <h6 class="font-headline fw-bold text-dark mb-4">Công ty</h6>
          <ul class="list-unstyled space-y-3 small text-secondary">
            <li class="mb-3">
              <router-link
                to="/about"
                class="text-decoration-none text-orange fw-bold"
                >Giới thiệu</router-link
              >
            </li>
            <li class="mb-3">
              <a
                href="#"
                class="text-decoration-none text-secondary hover-text-orange transition-colors"
                >Tin tức</a
              >
            </li>
            <li class="mb-3">
              <a
                href="#"
                class="text-decoration-none text-secondary hover-text-orange transition-colors"
                >Tuyển dụng</a
              >
            </li>
            <li>
              <a
                href="#"
                class="text-decoration-none text-secondary hover-text-orange transition-colors"
                >Liên hệ</a
              >
            </li>
          </ul>
        </div>

        <div class="col-6 col-lg-3">
          <h6 class="font-headline fw-bold text-dark mb-4">Chính sách</h6>
          <ul class="list-unstyled space-y-3 small text-secondary">
            <li class="mb-3">
              <router-link
                to="/privacy"
                class="text-decoration-none text-secondary hover-text-orange text-decoration-underline transition-colors"
                >Chính sách bảo mật</router-link
              >
            </li>
            <li class="mb-3">
              <a
                href="#"
                class="text-decoration-none text-secondary hover-text-orange text-decoration-underline transition-colors"
                >Điều khoản sử dụng</a
              >
            </li>
            <li class="mb-3">
              <a
                href="#"
                class="text-decoration-none text-secondary hover-text-orange text-decoration-underline transition-colors"
                >Quy định chung</a
              >
            </li>
            <li>
              <a
                href="#"
                class="text-decoration-none text-secondary hover-text-orange text-decoration-underline transition-colors"
                >Trung tâm trợ giúp</a
              >
            </li>
          </ul>
        </div>
      </div>

      <div class="text-center pt-4 border-top">
        <span class="small text-muted fw-bold tracking-wide text-uppercase"
          >Thiết kế bởi Đội ngũ POS36 Creative Labs</span
        >
      </div>
    </div>
  </footer>
</template>

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
.hover-text-orange:hover {
  color: #ea580c !important;
}
.transition-colors {
  transition: color 0.2s ease;
}
.max-w-7xl {
  max-width: 1280px;
  margin: 0 auto;
}
</style>
