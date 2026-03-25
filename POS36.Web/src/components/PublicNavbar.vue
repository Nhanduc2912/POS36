<template>
  <nav
    class="navbar navbar-expand-lg bg-white fixed-top shadow-sm py-3 px-lg-5"
  >
    <div class="container-fluid max-w-7xl mx-auto">
      <router-link
        to="/"
        class="navbar-brand fw-black text-orange fs-3 tracking-tighter"
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
        <ul class="navbar-nav fw-bold text-secondary gap-lg-4">
          <li class="nav-item">
            <a
              class="nav-link active text-orange border-bottom border-2 border-orange pb-1"
              href="#"
              >Features</a
            >
          </li>
          <li class="nav-item">
            <a class="nav-link hover-orange" href="#">Solutions</a>
          </li>
          <li class="nav-item">
            <a class="nav-link hover-orange" href="#">Pricing</a>
          </li>
          <li class="nav-item">
            <a class="nav-link hover-orange" href="#">Resources</a>
          </li>
        </ul>
      </div>

      <div class="d-none d-lg-flex align-items-center gap-4">
        <router-link
          to="/login"
          class="text-dark fw-bold text-decoration-none hover-opacity"
        >
          Log in
        </router-link>
        <router-link
          to="/register"
          class="btn btn-orange rounded-pill px-4 py-2 fw-bold text-white shadow-sm hover-scale"
        >
          Sign Up
        </router-link>
      </div>
    </div>
  </nav>
</template>

<style scoped>
@import url("https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@600;800&display=swap");

.navbar-brand {
  font-family: "Plus Jakarta Sans", sans-serif;
  letter-spacing: -1px;
}
.fw-black {
  font-weight: 800;
}
.text-orange {
  color: #e65c00 !important;
}
.border-orange {
  border-color: #e65c00 !important;
}
.btn-orange {
  background-color: #e65c00;
  border: none;
  transition: all 0.3s ease;
}
.btn-orange:hover {
  background-color: #cc5200;
}
.hover-orange:hover {
  color: #e65c00 !important;
}
.hover-opacity:hover {
  opacity: 0.7;
}
.hover-scale {
  transition: transform 0.2s ease;
}
.hover-scale:hover {
  transform: scale(1.05);
}
.max-w-7xl {
  max-width: 1200px;
}
</style>
