<template>
  <nav
    class="navbar navbar-expand-lg fixed-top glass-navbar shadow-sm transition-all"
  >
    <div class="container-fluid max-w-7xl mx-auto px-4 py-2">
      <router-link
        to="/"
        class="navbar-brand fw-black text-primary fs-3 tracking-tighter"
      >
        POS36
      </router-link>

      <button
        class="navbar-toggler border-0 shadow-none"
        type="button"
        data-bs-toggle="collapse"
        data-bs-target="#navbarNav"
      >
        <span class="navbar-toggler-icon"></span>
      </button>

      <div
        class="collapse navbar-collapse justify-content-center"
        id="navbarNav"
      >
        <ul class="navbar-nav gap-lg-4 fw-bold">
          <li class="nav-item">
            <router-link
              to="/features"
              class="nav-link custom-link"
              exact-active-class="active-orange"
              >Tính năng</router-link
            >
          </li>
          <li class="nav-item">
            <router-link
              to="/solutions"
              class="nav-link custom-link"
              exact-active-class="active-orange"
              >Giải pháp</router-link
            >
          </li>
          <li class="nav-item">
            <router-link
              to="/pricing"
              class="nav-link custom-link"
              exact-active-class="active-orange"
              >Bảng giá</router-link
            >
          </li>
          <li class="nav-item">
            <router-link
              to="/about"
              class="nav-link custom-link"
              exact-active-class="active-orange"
              >Giới thiệu</router-link
            >
          </li>
          <li class="nav-item">
            <router-link
              to="/privacy"
              class="nav-link custom-link"
              exact-active-class="active-orange"
              >Bảo mật</router-link
            >
          </li>
        </ul>
      </div>

      <div class="d-none d-lg-flex align-items-center gap-4">
        <router-link
          to="/login"
          class="text-secondary fw-bold text-decoration-none hover-text-primary transition-all"
        >
          Đăng nhập
        </router-link>
        <router-link
          to="/register"
          class="btn culinary-gradient text-white rounded-4 px-4 py-2 fw-bold shadow-sm hover-scale"
        >
          Dùng thử miễn phí
        </router-link>
      </div>
    </div>
  </nav>
</template>

<style scoped>
@import url("https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@600;800&display=swap");

.glass-navbar {
  background-color: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  font-family: "Plus Jakarta Sans", sans-serif;
}

.text-primary {
  color: #ea580c !important;
} /* Cam đậm */
.hover-text-primary:hover {
  color: #ea580c !important;
}

.custom-link {
  color: #475569 !important;
  transition: all 0.2s ease;
  border-bottom: 2px solid transparent;
}

.custom-link:hover {
  color: #ea580c !important;
}

/* Tự động gạch chân màu cam khi đang ở đúng trang */
.active-orange {
  color: #ea580c !important;
  border-bottom: 2px solid #ea580c;
}

.culinary-gradient {
  background: linear-gradient(135deg, #994700 0%, #ff7a00 100%);
  border: none;
}

.hover-scale {
  transition: transform 0.2s ease;
}
.hover-scale:hover {
  transform: scale(1.05);
}
.tracking-tighter {
  letter-spacing: -1px;
}
.fw-black {
  font-weight: 800;
}
.max-w-7xl {
  max-width: 1200px;
}
</style>
