<template>
  <div class="bg-surface font-body text-on-surface">
    <PublicNavbar />

    <main class="pt-5 mt-5">
      <section class="container max-w-7xl py-5 overflow-hidden">
        <div class="row g-5 items-center align-items-center">
          <div class="col-lg-6 z-index-1">
            <span
              class="badge bg-primary-fixed text-primary-variant px-3 py-2 rounded-pill mb-4 tracking-wide small text-uppercase fw-bold"
            >
              PHÂN PHỐI ĐỘC QUYỀN
            </span>
            <h1
              class="display-4 font-headline fw-black tracking-tighter mb-4 lh-sm"
            >
              Trung tâm điều hành <br />
              <span class="text-orange">bếp số</span> mạnh mẽ
            </h1>
            <p class="fs-5 text-secondary lh-lg mb-5 max-w-lg">
              Chuyển đổi hoàn toàn trải nghiệm vận hành nhà hàng với hệ thống
              quản lý tập trung, chuẩn xác đến từng giây. Đưa sự tinh tế của ẩm
              thực vào quy trình số hóa hiện đại.
            </p>
            <div class="d-flex flex-wrap gap-3">
              <button
                class="btn culinary-gradient text-white px-4 py-3 rounded-4 fw-bold ambient-shadow hover-scale fs-6"
              >
                Trải nghiệm miễn phí
              </button>
              <button
                class="btn btn-outline-secondary px-4 py-3 rounded-4 fw-bold hover-scale bg-white fs-6 border-outline"
              >
                Xem Demo
              </button>
            </div>
          </div>

          <div class="col-lg-6 position-relative">
            <div
              class="position-absolute top-50 start-50 translate-middle bg-orange opacity-25 rounded-circle blur-effect"
              style="width: 400px; height: 400px"
            ></div>

            <div
              class="ratio ratio-4x3 rounded-5 overflow-hidden ambient-shadow rotate-img transition-img position-relative z-index-1"
            >
              <img
                src="https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?auto=format&fit=crop&q=80&w=800"
                class="object-fit-cover"
                alt="POS System"
              />
            </div>

            <div
              class="position-absolute bottom-0 start-0 glass-effect p-4 rounded-4 ambient-shadow ms-n4 mb-n4 d-none d-lg-block z-index-1 border border-white"
              style="width: 240px"
            >
              <div class="d-flex align-items-center gap-3 mb-3">
                <div class="p-2 bg-primary-fixed rounded-3">
                  <i class="bi bi-graph-up-arrow text-orange fs-5"></i>
                </div>
                <span class="font-headline fw-bold small">+24% Hiệu suất</span>
              </div>
              <div class="progress" style="height: 6px">
                <div
                  class="progress-bar bg-orange"
                  role="progressbar"
                  style="width: 75%"
                ></div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section class="bg-surface-container py-5 mt-5">
        <div class="container max-w-7xl py-5">
          <div class="text-center mb-5 pb-4">
            <h2 class="display-5 font-headline fw-black mb-3 tracking-tighter">
              Vận hành tinh hoa, quản lý thông minh
            </h2>
            <p class="text-secondary fs-5 max-w-lg mx-auto">
              Mọi tính năng được thiết kế để tối ưu hóa quy trình phục vụ và gia
              tăng lợi nhuận cho chủ nhà hàng.
            </p>
          </div>

          <div class="row g-4">
            <div class="col-lg-8">
              <div
                class="bg-white rounded-5 p-4 p-md-5 transition-hover ambient-shadow h-100 overflow-hidden group-hover-wrap"
              >
                <div class="row g-4 align-items-center h-100">
                  <div class="col-md-6">
                    <div
                      class="icon-box bg-primary-fixed rounded-4 d-flex align-items-center justify-content-center mb-4"
                      style="width: 56px; height: 56px"
                    >
                      <i class="bi bi-grid-1x2-fill text-orange fs-3"></i>
                    </div>
                    <h3 class="font-headline fw-bold fs-3 mb-3">
                      Quản lý bàn thời gian thực
                    </h3>
                    <p class="text-secondary lh-lg mb-4">
                      Theo dõi sơ đồ bàn trực quan, tình trạng món đang chế biến
                      và thời gian chờ của khách hàng chỉ với một cái chạm.
                    </p>
                    <ul class="list-unstyled space-y-3 small fw-medium">
                      <li class="mb-2">
                        <i
                          class="bi bi-check-circle-fill text-orange me-2 fs-5 align-middle"
                        ></i>
                        Đồng bộ tức thì giữa bàn - bếp
                      </li>
                      <li>
                        <i
                          class="bi bi-check-circle-fill text-orange me-2 fs-5 align-middle"
                        ></i>
                        Tự động gợi ý sắp xếp tối ưu
                      </li>
                    </ul>
                  </div>
                  <div class="col-md-6 h-100">
                    <div
                      class="rounded-4 overflow-hidden h-100 min-h-200 position-relative"
                    >
                      <img
                        src="https://images.unsplash.com/photo-1554118811-1e0d58224f24?auto=format&fit=crop&q=80&w=600"
                        class="w-100 h-100 object-fit-cover img-zoom position-absolute top-0 start-0"
                        alt="Table management"
                      />
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div class="col-lg-4">
              <div
                class="bg-white rounded-5 p-4 p-md-5 transition-hover ambient-shadow h-100 d-flex flex-column"
              >
                <div
                  class="icon-box bg-info bg-opacity-10 rounded-4 d-flex align-items-center justify-content-center mb-4"
                  style="width: 56px; height: 56px"
                >
                  <i class="bi bi-cloud-arrow-up-fill text-info fs-3"></i>
                </div>
                <h3 class="font-headline fw-bold fs-4 mb-3">Đồng bộ đám mây</h3>
                <p class="text-secondary lh-lg mb-4 flex-grow-1">
                  Truy cập dữ liệu mọi lúc, mọi nơi. Dữ liệu luôn an toàn và
                  được cập nhật liên tục ngay cả khi mất mạng.
                </p>
                <div class="rounded-4 overflow-hidden" style="height: 140px">
                  <img
                    src="https://images.unsplash.com/photo-1451187580459-43490279c0fa?auto=format&fit=crop&q=80&w=500"
                    class="w-100 h-100 object-fit-cover"
                    alt="Cloud sync"
                  />
                </div>
              </div>
            </div>

            <div class="col-lg-5">
              <div
                class="bg-white rounded-5 p-4 p-md-5 transition-hover ambient-shadow h-100 d-flex flex-column"
              >
                <div
                  class="icon-box bg-primary-fixed rounded-4 d-flex align-items-center justify-content-center mb-4"
                  style="width: 56px; height: 56px"
                >
                  <i class="bi bi-bar-chart-fill text-orange fs-3"></i>
                </div>
                <h3 class="font-headline fw-bold fs-4 mb-3">
                  Báo cáo chuyên sâu
                </h3>
                <p class="text-secondary lh-lg mb-4 flex-grow-1">
                  Phân tích doanh thu, chi phí và tồn kho với hệ thống biểu đồ
                  thông minh. Ra quyết định dựa trên dữ liệu thực.
                </p>
                <div class="row g-3">
                  <div class="col-6">
                    <div class="bg-light p-3 rounded-4 border">
                      <div
                        class="small text-muted text-uppercase fw-bold tracking-wide mb-1"
                        style="font-size: 0.7rem"
                      >
                        Doanh thu
                      </div>
                      <div class="fs-4 fw-black text-dark">125.4tr</div>
                    </div>
                  </div>
                  <div class="col-6">
                    <div class="bg-light p-3 rounded-4 border">
                      <div
                        class="small text-muted text-uppercase fw-bold tracking-wide mb-1"
                        style="font-size: 0.7rem"
                      >
                        Lượt khách
                      </div>
                      <div class="fs-4 fw-black text-dark">1,402</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div class="col-lg-7">
              <div
                class="culinary-gradient rounded-5 p-4 p-md-5 transition-hover ambient-shadow h-100 position-relative overflow-hidden group-hover-wrap"
              >
                <div
                  class="row g-4 align-items-center position-relative z-index-1 h-100"
                >
                  <div class="col-md-7 text-white">
                    <div
                      class="icon-box bg-white bg-opacity-25 rounded-4 d-flex align-items-center justify-content-center mb-4"
                      style="
                        width: 56px;
                        height: 56px;
                        backdrop-filter: blur(10px);
                      "
                    >
                      <i class="bi bi-star-fill text-white fs-3"></i>
                    </div>
                    <h3 class="font-headline fw-bold fs-3 mb-3">
                      Khách hàng thân thiết
                    </h3>
                    <p class="text-white-50 lh-lg mb-4">
                      Giữ chân khách hàng bằng hệ thống tích điểm, phân hạng
                      thành viên và ưu đãi cá nhân hóa tự động.
                    </p>
                    <button
                      class="btn btn-light text-orange fw-bold px-4 py-2 rounded-pill hover-scale"
                    >
                      Khám phá ngay
                    </button>
                  </div>
                  <div
                    class="col-md-5 d-flex justify-content-center align-items-center"
                  >
                    <div
                      class="bg-white bg-opacity-10 p-3 rounded-4 border border-white border-opacity-25 img-rotate-hover transition-img"
                    >
                      <img
                        src="https://images.unsplash.com/photo-1556740758-90de374c12ad?auto=format&fit=crop&q=80&w=400"
                        class="w-100 rounded-3 shadow"
                        alt="Loyalty App"
                      />
                    </div>
                  </div>
                </div>
                <i
                  class="bi bi-heart-fill position-absolute text-white opacity-10"
                  style="
                    font-size: 20rem;
                    top: -50px;
                    right: -50px;
                    transform: rotate(15deg);
                  "
                ></i>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section class="py-5 mb-5 mt-4">
        <div class="container max-w-5xl">
          <div
            class="culinary-gradient rounded-custom p-5 text-center text-white position-relative overflow-hidden ambient-shadow"
          >
            <div
              class="position-absolute top-0 end-0 bg-white bg-opacity-10 rounded-circle blur-effect"
              style="
                width: 400px;
                height: 400px;
                margin-top: -150px;
                margin-right: -150px;
              "
            ></div>
            <div
              class="position-absolute bottom-0 start-0 bg-dark bg-opacity-10 rounded-circle blur-effect"
              style="
                width: 300px;
                height: 300px;
                margin-bottom: -100px;
                margin-left: -100px;
              "
            ></div>

            <div class="position-relative z-index-1 py-4">
              <h2 class="display-4 font-headline fw-black mb-4 lh-sm">
                Sẵn sàng nâng tầm <br />nhà hàng của bạn?
              </h2>
              <p class="fs-5 text-white-50 mb-5 max-w-lg mx-auto">
                Bắt đầu ngay hôm nay với 14 ngày dùng thử đầy đủ tính năng.
                Không yêu cầu thẻ tín dụng.
              </p>
              <div
                class="d-flex flex-column flex-sm-row justify-content-center gap-3"
              >
                <button
                  class="btn btn-light text-orange px-5 py-3 rounded-4 font-headline fw-bold fs-5 shadow-lg hover-scale"
                >
                  Dùng thử miễn phí
                </button>
                <button
                  class="btn btn-outline-light px-5 py-3 rounded-4 font-headline fw-bold fs-5 border-2 hover-scale bg-white bg-opacity-10"
                >
                  Liên hệ tư vấn
                </button>
              </div>
            </div>
          </div>
        </div>
      </section>
    </main>

    <PublicFooter />
  </div>
</template>

<script setup>
// Nhớ import Navbar và Footer vào để nó hiển thị nhé sếp!
import PublicNavbar from "../../components/PublicNavbar.vue";
import PublicFooter from "../../components/PublicFooter.vue";
</script>

<style scoped>
@import url("https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;800&family=Manrope:wght@400;500;600&display=swap");

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
  letter-spacing: 1px;
}

/* BẢNG MÀU DỰA TRÊN TAILWIND CONFIG CỦA SẾP */
.bg-surface {
  background-color: #f9f9f9;
}
.bg-surface-container {
  background-color: #eeeeee;
}
.text-on-surface {
  color: #1a1c1c;
}
.text-secondary {
  color: #5f5e5e;
}
.text-orange {
  color: #ea580c !important;
}
.bg-orange {
  background-color: #ea580c !important;
}
.bg-primary-fixed {
  background-color: #ffdbc8;
}
.text-primary-variant {
  color: #753400;
}
.border-outline {
  border-color: #e0c0af !important;
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
.blur-effect {
  filter: blur(60px);
}

/* BENTO GRID HACKS */
.min-h-200 {
  min-height: 200px;
}
.rounded-custom {
  border-radius: 3rem;
}

/* ANIMATIONS & HOVERS */
.rotate-img {
  transform: rotate(3deg);
}
.transition-img {
  transition: transform 0.7s cubic-bezier(0.2, 0.8, 0.2, 1);
}
.rotate-img:hover {
  transform: rotate(0deg);
}

.img-zoom {
  transition: transform 0.7s cubic-bezier(0.2, 0.8, 0.2, 1);
}
.group-hover-wrap:hover .img-zoom {
  transform: scale(1.1);
}

.img-rotate-hover {
  transform: rotate(6deg);
}
.group-hover-wrap:hover .img-rotate-hover {
  transform: rotate(0deg);
}

.transition-hover {
  transition:
    transform 0.4s cubic-bezier(0.2, 0.8, 0.2, 1),
    box-shadow 0.4s;
}
.transition-hover:hover {
  transform: translateY(-10px);
  box-shadow: 0 40px 60px -20px rgba(26, 28, 28, 0.15);
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
.max-w-5xl {
  max-width: 1024px;
  margin: 0 auto;
}
.max-w-lg {
  max-width: 600px;
}
.z-index-1 {
  z-index: 1;
  position: relative;
}
</style>
