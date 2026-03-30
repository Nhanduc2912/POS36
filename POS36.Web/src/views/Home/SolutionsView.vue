<template>
  <div class="bg-surface font-body text-on-surface">
    <PublicNavbar />

    <main class="pt-5 mt-5">
      <section class="container max-w-7xl py-5 mt-4 mb-5">
        <div class="row align-items-center g-5">
          <div class="col-lg-6 z-index-1">
            <span
              class="badge bg-primary-fixed text-primary-variant px-3 py-2 rounded-pill fw-bold text-uppercase tracking-widest mb-4"
            >
              Hệ sinh thái F&B
            </span>
            <h1
              class="display-3 font-headline fw-black tracking-tighter lh-sm mb-4 text-dark"
            >
              Giải pháp <span class="text-orange fst-italic">toàn diện</span
              ><br />
              cho mọi quy mô.
            </h1>
            <p class="fs-5 text-secondary lh-lg max-w-lg mb-5">
              Từ những nhà hàng Fine Dining sang trọng đến các chuỗi Fast Casual
              tốc độ, POS36 mang đến sự tinh tế trong quản lý và chính xác trong
              vận hành.
            </p>
            <div class="d-flex flex-wrap gap-3">
              <button
                class="btn culinary-gradient text-white px-5 py-3 rounded-4 font-headline fw-bold fs-6 shadow hover-scale d-flex align-items-center gap-2 group-hover"
              >
                Khám phá ngay
                <i class="bi bi-arrow-right transition-icon fs-5"></i>
              </button>
            </div>
          </div>

          <div class="col-lg-6 position-relative d-none d-lg-block">
            <div
              class="position-absolute top-0 start-0 bg-primary-fixed opacity-50 rounded-circle blur-effect"
              style="
                width: 300px;
                height: 300px;
                margin-top: -50px;
                margin-left: -50px;
              "
            ></div>

            <div
              class="rounded-custom overflow-hidden ambient-shadow position-relative z-index-1 rotate-img transition-img"
            >
              <img
                class="img-fluid w-100 object-fit-cover"
                style="height: 500px"
                src="https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?auto=format&fit=crop&q=80&w=800"
                alt="Restaurant Interior"
              />
            </div>

            <div
              class="position-absolute bottom-0 start-0 glass-effect p-4 rounded-4 ambient-shadow ms-n4 mb-4 z-index-1 border border-white"
              style="width: 280px"
            >
              <div class="d-flex align-items-center gap-3 mb-3">
                <div class="p-2 bg-primary-fixed rounded-3">
                  <i class="bi bi-graph-up-arrow text-orange fs-5"></i>
                </div>
                <span class="font-headline fw-bold small"
                  >Tăng 35% hiệu suất phục vụ</span
                >
              </div>
              <p class="text-muted small fst-italic mb-0">
                "Kể từ khi áp dụng POS36, quy trình gọi món của chúng tôi trở
                nên mượt mà hơn bao giờ hết."
              </p>
            </div>
          </div>
        </div>
      </section>

      <section class="bg-surface-container py-5 mb-5">
        <div class="container max-w-7xl py-5">
          <div class="text-center mb-5 pb-4">
            <h2 class="display-5 font-headline fw-black mb-4">
              Danh mục giải pháp
            </h2>
            <div class="divider-orange mx-auto"></div>
          </div>

          <div class="row g-4">
            <div class="col-md-6">
              <div
                class="bg-white p-4 p-md-5 rounded-5 ambient-shadow transition-hover h-100 d-flex flex-column"
              >
                <div
                  class="d-flex justify-content-between align-items-start mb-5"
                >
                  <div>
                    <div
                      class="icon-box bg-primary-fixed rounded-4 d-flex align-items-center justify-content-center mb-4"
                      style="width: 64px; height: 64px"
                    >
                      <i class="bi bi-cup-hot-fill text-orange fs-2"></i>
                    </div>
                    <h3 class="font-headline fw-bold fs-2 mb-3 text-dark">
                      Fine Dining
                    </h3>
                    <p class="text-secondary lh-lg mb-0 pe-md-4">
                      Sự tinh tế trong từng điểm chạm, nâng tầm trải nghiệm ẩm
                      thực đẳng cấp.
                    </p>
                  </div>
                  <i
                    class="bi bi-stars text-muted opacity-25 fs-1 d-none d-sm-block"
                  ></i>
                </div>

                <div class="row g-4 mb-5 flex-grow-1">
                  <div class="col-12">
                    <h6
                      class="small fw-black text-uppercase tracking-widest text-orange mb-3"
                    >
                      Thách thức
                    </h6>
                    <ul
                      class="list-unstyled space-y-3 small text-secondary fw-medium"
                    >
                      <li class="d-flex align-items-start">
                        <i class="bi bi-x text-danger fs-5 me-2 mt-n1"></i> Quản
                        lý đặt bàn phức tạp và dễ sai sót.
                      </li>
                      <li class="d-flex align-items-start">
                        <i class="bi bi-x text-danger fs-5 me-2 mt-n1"></i> Quy
                        trình phục vụ chậm trễ, thiếu chuyên nghiệp.
                      </li>
                    </ul>
                  </div>
                  <div class="col-12 border-top pt-4">
                    <h6
                      class="small fw-black text-uppercase tracking-widest text-orange mb-3"
                    >
                      Giải pháp POS36
                    </h6>
                    <ul
                      class="list-unstyled space-y-3 small text-secondary fw-medium"
                    >
                      <li class="d-flex align-items-start">
                        <i
                          class="bi bi-check-circle-fill text-success fs-5 me-2 mt-n1"
                        ></i>
                        Sơ đồ bàn trực quan, đồng bộ thời gian thực.
                      </li>
                      <li class="d-flex align-items-start">
                        <i
                          class="bi bi-check-circle-fill text-success fs-5 me-2 mt-n1"
                        ></i>
                        Order tại bàn qua Tablet, truyền tin tức thì tới bếp.
                      </li>
                    </ul>
                  </div>
                </div>

                <div
                  class="rounded-4 overflow-hidden img-zoom-wrap"
                  style="height: 200px"
                >
                  <img
                    src="https://images.unsplash.com/photo-1414235077428-338989a2e8c0?auto=format&fit=crop&q=80&w=800"
                    class="w-100 h-100 object-fit-cover img-zoom"
                    alt="Fine Dining"
                  />
                </div>
              </div>
            </div>

            <div class="col-md-6">
              <div
                class="bg-white p-4 p-md-5 rounded-5 ambient-shadow transition-hover h-100 d-flex flex-column"
              >
                <div
                  class="d-flex justify-content-between align-items-start mb-5"
                >
                  <div>
                    <div
                      class="icon-box bg-primary-fixed rounded-4 d-flex align-items-center justify-content-center mb-4"
                      style="width: 64px; height: 64px"
                    >
                      <i class="bi bi-shop text-orange fs-2"></i>
                    </div>
                    <h3 class="font-headline fw-bold fs-2 mb-3 text-dark">
                      Chuỗi thức ăn nhanh
                    </h3>
                    <p class="text-secondary lh-lg mb-0 pe-md-4">
                      Tối ưu tốc độ vận hành và độ chính xác trong từng đơn hàng
                      lớn.
                    </p>
                  </div>
                  <i
                    class="bi bi-lightning-fill text-muted opacity-25 fs-1 d-none d-sm-block"
                  ></i>
                </div>

                <div class="row g-4 mb-5 flex-grow-1">
                  <div class="col-12">
                    <h6
                      class="small fw-black text-uppercase tracking-widest text-orange mb-3"
                    >
                      Thách thức
                    </h6>
                    <ul
                      class="list-unstyled space-y-3 small text-secondary fw-medium"
                    >
                      <li class="d-flex align-items-start">
                        <i class="bi bi-x text-danger fs-5 me-2 mt-n1"></i> Ùn
                        tắc tại quầy thanh toán giờ cao điểm.
                      </li>
                      <li class="d-flex align-items-start">
                        <i class="bi bi-x text-danger fs-5 me-2 mt-n1"></i> Khó
                        kiểm soát tồn kho giữa các chi nhánh.
                      </li>
                    </ul>
                  </div>
                  <div class="col-12 border-top pt-4">
                    <h6
                      class="small fw-black text-uppercase tracking-widest text-orange mb-3"
                    >
                      Giải pháp POS36
                    </h6>
                    <ul
                      class="list-unstyled space-y-3 small text-secondary fw-medium"
                    >
                      <li class="d-flex align-items-start">
                        <i
                          class="bi bi-check-circle-fill text-success fs-5 me-2 mt-n1"
                        ></i>
                        Màn hình KDS tại bếp, tối ưu quy trình chế biến.
                      </li>
                      <li class="d-flex align-items-start">
                        <i
                          class="bi bi-check-circle-fill text-success fs-5 me-2 mt-n1"
                        ></i>
                        Quản lý kho tập trung, cảnh báo hàng tồn tự động.
                      </li>
                    </ul>
                  </div>
                </div>

                <div
                  class="rounded-4 overflow-hidden img-zoom-wrap"
                  style="height: 200px"
                >
                  <img
                    src="https://images.unsplash.com/photo-1550547660-d9450f859349?auto=format&fit=crop&q=80&w=800"
                    class="w-100 h-100 object-fit-cover img-zoom"
                    alt="Fast Food"
                  />
                </div>
              </div>
            </div>

            <div class="col-md-6">
              <div
                class="bg-white p-4 p-md-5 rounded-5 ambient-shadow transition-hover h-100 d-flex flex-column"
              >
                <div
                  class="d-flex justify-content-between align-items-start mb-5"
                >
                  <div>
                    <div
                      class="icon-box bg-primary-fixed rounded-4 d-flex align-items-center justify-content-center mb-4"
                      style="width: 64px; height: 64px"
                    >
                      <i class="bi bi-cup-fill text-orange fs-2"></i>
                    </div>
                    <h3 class="font-headline fw-bold fs-2 mb-3 text-dark">
                      Cafe & Tiệm bánh
                    </h3>
                    <p class="text-secondary lh-lg mb-0 pe-md-4">
                      Quản lý định lượng nguyên vật liệu chặt chẽ và chăm sóc
                      khách hàng thân thiết.
                    </p>
                  </div>
                  <i
                    class="bi bi-cup-hot-fill text-muted opacity-25 fs-1 d-none d-sm-block"
                  ></i>
                </div>

                <div class="row g-4 mb-5 flex-grow-1">
                  <div class="col-12">
                    <h6
                      class="small fw-black text-uppercase tracking-widest text-orange mb-3"
                    >
                      Thách thức
                    </h6>
                    <ul
                      class="list-unstyled space-y-3 small text-secondary fw-medium"
                    >
                      <li class="d-flex align-items-start">
                        <i class="bi bi-x text-danger fs-5 me-2 mt-n1"></i> Thất
                        thoát nguyên liệu pha chế hàng ngày.
                      </li>
                      <li class="d-flex align-items-start">
                        <i class="bi bi-x text-danger fs-5 me-2 mt-n1"></i>
                        Chương trình hội viên rời rạc, không hiệu quả.
                      </li>
                    </ul>
                  </div>
                  <div class="col-12 border-top pt-4">
                    <h6
                      class="small fw-black text-uppercase tracking-widest text-orange mb-3"
                    >
                      Giải pháp POS36
                    </h6>
                    <ul
                      class="list-unstyled space-y-3 small text-secondary fw-medium"
                    >
                      <li class="d-flex align-items-start">
                        <i
                          class="bi bi-check-circle-fill text-success fs-5 me-2 mt-n1"
                        ></i>
                        Định lượng trừ kho tự động theo công thức pha chế.
                      </li>
                      <li class="d-flex align-items-start">
                        <i
                          class="bi bi-check-circle-fill text-success fs-5 me-2 mt-n1"
                        ></i>
                        Hệ thống CRM tích hợp điểm thưởng và ưu đãi.
                      </li>
                    </ul>
                  </div>
                </div>

                <div
                  class="rounded-4 overflow-hidden img-zoom-wrap"
                  style="height: 200px"
                >
                  <img
                    src="https://images.unsplash.com/photo-1554118811-1e0d58224f24?auto=format&fit=crop&q=80&w=800"
                    class="w-100 h-100 object-fit-cover img-zoom"
                    alt="Cafe"
                  />
                </div>
              </div>
            </div>

            <div class="col-md-6">
              <div
                class="bg-white p-4 p-md-5 rounded-5 ambient-shadow transition-hover h-100 d-flex flex-column"
              >
                <div
                  class="d-flex justify-content-between align-items-start mb-5"
                >
                  <div>
                    <div
                      class="icon-box bg-primary-fixed rounded-4 d-flex align-items-center justify-content-center mb-4"
                      style="width: 64px; height: 64px"
                    >
                      <i class="bi bi-music-note-list text-orange fs-2"></i>
                    </div>
                    <h3 class="font-headline fw-bold fs-2 mb-3 text-dark">
                      Bar & Pub
                    </h3>
                    <p class="text-secondary lh-lg mb-0 pe-md-4">
                      Kiểm soát đồ uống theo dung tích và quản lý hóa đơn linh
                      hoạt theo thời gian.
                    </p>
                  </div>
                  <i
                    class="bi bi-speaker-fill text-muted opacity-25 fs-1 d-none d-sm-block"
                  ></i>
                </div>

                <div class="row g-4 mb-5 flex-grow-1">
                  <div class="col-12">
                    <h6
                      class="small fw-black text-uppercase tracking-widest text-orange mb-3"
                    >
                      Thách thức
                    </h6>
                    <ul
                      class="list-unstyled space-y-3 small text-secondary fw-medium"
                    >
                      <li class="d-flex align-items-start">
                        <i class="bi bi-x text-danger fs-5 me-2 mt-n1"></i> Khó
                        theo dõi doanh thu theo từng ca làm việc.
                      </li>
                      <li class="d-flex align-items-start">
                        <i class="bi bi-x text-danger fs-5 me-2 mt-n1"></i> Tình
                        trạng nhầm lẫn đồ uống trong không gian tối.
                      </li>
                    </ul>
                  </div>
                  <div class="col-12 border-top pt-4">
                    <h6
                      class="small fw-black text-uppercase tracking-widest text-orange mb-3"
                    >
                      Giải pháp POS36
                    </h6>
                    <ul
                      class="list-unstyled space-y-3 small text-secondary fw-medium"
                    >
                      <li class="d-flex align-items-start">
                        <i
                          class="bi bi-check-circle-fill text-success fs-5 me-2 mt-n1"
                        ></i>
                        Chốt ca thông minh, báo cáo doanh thu tức thì.
                      </li>
                      <li class="d-flex align-items-start">
                        <i
                          class="bi bi-check-circle-fill text-success fs-5 me-2 mt-n1"
                        ></i>
                        Giao diện POS tối ưu hiển thị cho môi trường thiếu sáng.
                      </li>
                    </ul>
                  </div>
                </div>

                <div
                  class="rounded-4 overflow-hidden img-zoom-wrap"
                  style="height: 200px"
                >
                  <img
                    src="https://images.unsplash.com/photo-1514362545857-3bc16c4c7d1b?auto=format&fit=crop&q=80&w=800"
                    class="w-100 h-100 object-fit-cover img-zoom"
                    alt="Bar & Pub"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section class="container max-w-7xl pb-5 mb-5 mt-5">
        <div
          class="bg-dark rounded-custom p-5 text-white position-relative overflow-hidden ambient-shadow"
        >
          <div
            class="position-absolute top-0 end-0 bg-primary-fixed bg-opacity-25 rounded-circle blur-effect"
            style="
              width: 400px;
              height: 400px;
              margin-top: -200px;
              margin-right: -200px;
            "
          ></div>
          <div
            class="position-absolute bottom-0 start-0 bg-primary-fixed bg-opacity-10 rounded-circle blur-effect"
            style="
              width: 300px;
              height: 300px;
              margin-bottom: -150px;
              margin-left: -150px;
            "
          ></div>

          <div
            class="position-relative z-index-1 py-5 px-md-4 d-flex flex-column flex-md-row align-items-md-center justify-content-between gap-5"
          >
            <div class="max-w-lg">
              <h2 class="display-5 font-headline fw-black mb-4 lh-sm">
                Sẵn sàng để cách mạng hóa mô hình của bạn?
              </h2>
              <p class="fs-5 text-white-50 mb-0 lh-lg">
                Tham gia cùng hơn 10,000+ nhà hàng đã tin dùng POS36 để tối ưu
                hóa lợi nhuận ngay hôm nay.
              </p>
            </div>

            <div
              class="d-flex flex-column gap-3 w-100"
              style="max-width: 250px"
            >
              <button
                class="btn culinary-gradient text-white py-3 font-headline fw-bold fs-5 shadow hover-scale w-100"
              >
                Tìm giải pháp cho bạn
              </button>
              <button
                class="btn btn-outline-light py-3 font-headline fw-bold fs-5 bg-white bg-opacity-10 hover-scale border-secondary w-100"
              >
                Liên hệ tư vấn
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
.tracking-widest {
  letter-spacing: 2px;
}

/* BẢNG MÀU DỰA TRÊN TAILWIND CỦA SẾP */
.bg-surface {
  background-color: #f9f9f9;
}
.bg-surface-container {
  background-color: #f3f3f3;
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
.bg-primary-fixed {
  background-color: #ffdbc8;
}
.text-primary-variant {
  color: #753400;
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
  filter: blur(80px);
}

.divider-orange {
  width: 60px;
  height: 6px;
  background-color: #ea580c;
  border-radius: 10px;
}
.rounded-custom {
  border-radius: 3rem;
}

/* ANIMATIONS & HOVERS */
.rotate-img {
  transform: rotate(2deg);
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
.img-zoom-wrap:hover .img-zoom {
  transform: scale(1.1);
}

.transition-hover {
  transition:
    transform 0.4s cubic-bezier(0.2, 0.8, 0.2, 1),
    box-shadow 0.4s;
}
.transition-hover:hover {
  transform: translateY(-8px);
  box-shadow: 0 40px 60px -20px rgba(26, 28, 28, 0.12);
}
.hover-scale {
  transition: transform 0.2s ease;
}
.hover-scale:hover {
  transform: scale(1.05);
}

.group-hover:hover .transition-icon {
  transform: translateX(5px);
  display: inline-block;
  transition: 0.2s;
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
</style>
