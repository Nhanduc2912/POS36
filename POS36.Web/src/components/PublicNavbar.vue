<script setup></script>

<template>
  <nav
    class="navbar navbar-expand-lg navbar-light bg-white fixed-top shadow-sm"
  >
    <div class="container">
      <router-link class="navbar-brand fw-bold text-primary fs-3" to="/">
        <i class="bi bi-shop"></i> POS36
      </router-link>

      <button
        class="navbar-toggler"
        type="button"
        data-bs-toggle="collapse"
        data-bs-target="#navbarNav"
      >
        <span class="navbar-toggler-icon"></span>
      </button>

      <div class="collapse navbar-collapse" id="navbarNav">
        <ul class="navbar-nav ms-auto align-items-center">
          <li class="nav-item me-3">
            <a class="nav-link text-dark fw-medium" href="#features"
              >Tính năng</a
            >
          </li>
          <li class="nav-item me-3">
            <a class="nav-link text-dark fw-medium" href="#pricing">Bảng giá</a>
          </li>
          <li class="nav-item me-2">
            <router-link
              to="/login"
              class="btn btn-outline-primary px-4 rounded-pill fw-bold"
            >
              Đăng nhập
            </router-link>
          </li>
          <li class="nav-item">
            <router-link
              to="/register"
              class="btn btn-primary px-4 rounded-pill fw-bold shadow-sm"
            >
              Dùng thử miễn phí
            </router-link>
          </li>
        </ul>
      </div>
    </div>
  </nav>
</template>
