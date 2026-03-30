<template>
  <div>
    <PublicNavbar />

    <main class="bg-light text-dark font-body pt-5 mt-5">
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
              Định Nghĩa Lại <br /><span class="text-orange"
                >Culinary Kinetic</span
              >
            </h1>
            <p class="fs-5 text-secondary lh-lg mb-5 max-w-lg">
              POS36 không chỉ là phần mềm quản lý. Chúng tôi là động lực thúc
              đẩy sự luân chuyển không ngừng của ngành ẩm thực, kết hợp tinh hoa
              dịch vụ truyền thống với độ chính xác tuyệt đối của công nghệ số.
            </p>
            <div class="d-flex flex-wrap gap-3">
              <button
                class="btn culinary-gradient text-white px-4 py-3 rounded-4 fw-bold ambient-shadow hover-scale"
              >
                Tìm hiểu câu chuyện
              </button>
              <button
                class="btn btn-outline-secondary px-4 py-3 rounded-4 fw-bold hover-scale bg-white"
              >
                Xem Hệ Sinh Thái
              </button>
            </div>
          </div>

          <div class="col-lg-6 position-relative">
            <div
              class="ratio ratio-1x1 rounded-5 overflow-hidden ambient-shadow rotate-img transition-img"
            >
              <img
                src="https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?auto=format&fit=crop&q=80&w=800"
                class="object-fit-cover"
                alt="Luxury Restaurant"
              />
            </div>
            <div
              class="position-absolute bottom-0 start-0 glass-effect p-4 rounded-4 ambient-shadow ms-n4 mb-n4 d-none d-lg-block border"
              style="width: 250px"
            >
              <div class="d-flex align-items-center gap-3 mb-3">
                <div class="p-2 bg-orange-subtle rounded-3">
                  <i class="bi bi-shop text-orange fs-4"></i>
                </div>
                <div>
                  <div class="small fw-bold text-muted">KHÁCH HÀNG</div>
                  <div class="fs-4 font-headline fw-black text-dark">
                    15,000+
                  </div>
                </div>
              </div>
              <div class="progress" style="height: 6px">
                <div
                  class="progress-bar bg-orange"
                  role="progressbar"
                  style="width: 85%"
                ></div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section class="bg-surface-container py-5">
        <div class="container max-w-7xl py-5">
          <div class="row g-5">
            <div class="col-lg-5">
              <h2 class="font-headline display-6 fw-black mb-4">
                Câu chuyện của chúng tôi
              </h2>
              <div class="divider-orange mb-5"></div>
              <p class="text-secondary lh-lg mb-4">
                Khởi nguồn từ niềm đam mê với ẩm thực và sự thấu hiểu sâu sắc
                những nỗi đau trong quản lý vận hành, POS36 ra đời với một tầm
                nhìn duy nhất: Giải phóng các nhà kinh doanh khỏi những con số
                rối rắm để họ tập trung vào điều quan trọng nhất -
                <strong>Trải nghiệm của thực khách</strong>.
              </p>
              <p class="text-secondary lh-lg">
                Chúng tôi gọi đó là "Culinary Kinetic" - sự chuyển động không
                ngừng nghỉ của hương vị, dịch vụ và công nghệ. Mỗi dòng code
                chúng tôi viết ra đều hướng tới việc làm cho dòng chảy này trở
                nên mượt mà, hiệu quả và bền vững hơn.
              </p>
            </div>
            <div class="col-lg-7">
              <div class="row g-4">
                <div class="col-sm-6 d-flex flex-column gap-4">
                  <div class="ratio ratio-4x3 rounded-4 overflow-hidden">
                    <img
                      src="https://images.unsplash.com/photo-1577219491135-ce391730fb2c?auto=format&fit=crop&q=80&w=500"
                      class="object-fit-cover"
                    />
                  </div>
                  <div class="bg-white p-4 rounded-4 ambient-shadow border">
                    <span
                      class="display-5 font-headline fw-black text-orange d-block mb-2"
                      >10+</span
                    >
                    <p class="small fw-bold mb-0 text-muted">
                      Năm kinh nghiệm F&B
                    </p>
                  </div>
                </div>
                <div class="col-sm-6 d-flex flex-column gap-4 mt-sm-5">
                  <div class="bg-dark p-4 rounded-4 text-white ambient-shadow">
                    <span
                      class="display-5 font-headline fw-black d-block mb-2 text-warning"
                      >24/7</span
                    >
                    <p class="small fw-bold opacity-75 mb-0">
                      Hỗ trợ kỹ thuật toàn quốc
                    </p>
                  </div>
                  <div class="ratio ratio-4x3 rounded-4 overflow-hidden">
                    <img
                      src="https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?auto=format&fit=crop&q=80&w=500"
                      class="object-fit-cover"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section class="container max-w-7xl py-5 my-5">
        <h2 class="font-headline display-5 fw-black text-center mb-5 pb-4">
          Hành trình vươn tầm
        </h2>
        <div class="row g-4">
          <div class="col-md-6">
            <div class="bg-white border p-5 rounded-4 h-100 transition-hover">
              <span
                class="display-2 font-headline fw-black text-muted opacity-25 d-block mb-3"
                >2014</span
              >
              <h4 class="fw-bold">Khởi nghiệp & Đam mê</h4>
              <p class="text-secondary">
                Những viên gạch đầu tiên được đặt xuống tại Hà Nội với đội ngũ
                chỉ 5 kỹ sư tâm huyết.
              </p>
            </div>
          </div>
          <div class="col-md-6">
            <div
              class="culinary-gradient text-white p-5 rounded-4 h-100 transition-hover ambient-shadow"
            >
              <span
                class="display-2 font-headline fw-black opacity-50 d-block mb-3"
                >2018</span
              >
              <h4 class="fw-bold">Đột phá Cloud</h4>
              <p class="text-white-50">
                Ra mắt nền tảng điện toán đám mây thế hệ mới cho phép quản lý từ
                xa.
              </p>
            </div>
          </div>
          <div class="col-md-6">
            <div class="bg-white border p-5 rounded-4 h-100 transition-hover">
              <span
                class="display-2 font-headline fw-black text-muted opacity-25 d-block mb-3"
                >2020</span
              >
              <h4 class="fw-bold">Vững vàng qua đại dịch</h4>
              <p class="text-secondary">
                Hỗ trợ 5,000+ nhà hàng chuyển đổi sang mô hình giao hàng trực
                tuyến.
              </p>
            </div>
          </div>
          <div class="col-md-6">
            <div
              class="bg-surface-container border p-5 rounded-4 h-100 transition-hover"
            >
              <span
                class="display-2 font-headline fw-black text-muted opacity-25 d-block mb-3"
                >2022</span
              >
              <h4 class="fw-bold">Mở rộng khu vực</h4>
              <p class="text-secondary">
                Hiện diện tại 63 tỉnh thành và bắt đầu tiến quân ra thị trường
                Đông Nam Á.
              </p>
            </div>
          </div>
          <div class="col-md-12">
            <div
              class="bg-dark text-white p-5 rounded-4 d-md-flex align-items-center justify-content-between transition-hover ambient-shadow mt-2"
            >
              <div>
                <span
                  class="display-4 font-headline fw-black text-warning opacity-75 d-block mb-2"
                  >2024 - HIỆN TẠI</span
                >
                <h3 class="fw-bold mb-3">Hệ sinh thái Culinary Kinetic</h3>
                <p class="text-white-50 mb-0 max-w-lg">
                  Chính thức tích hợp Trí tuệ Nhân tạo (AI) vào phân tích hành
                  vi người dùng và tối ưu chuỗi cung ứng nguyên liệu.
                </p>
              </div>
              <i
                class="bi bi-stars text-warning opacity-25 d-none d-md-block"
                style="font-size: 8rem"
              ></i>
            </div>
          </div>
        </div>
      </section>

      <section class="bg-surface-container py-5">
        <div class="container max-w-7xl py-5">
          <div class="text-center mb-5 pb-3">
            <h2 class="font-headline display-5 fw-black mb-3">
              Giá trị cốt lõi
            </h2>
            <p class="text-secondary fs-5">
              Kim chỉ nam cho mọi quyết định và hành động tại POS36
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
                  <i class="bi bi-lightbulb-fill text-orange fs-3"></i>
                </div>
                <h3 class="font-headline fw-black mb-3">Sáng tạo</h3>
                <p class="text-secondary lh-lg mb-0">
                  Không chấp nhận sự lỗi thời. Chúng tôi luôn tìm kiếm những
                  phương thức mới để giải quyết bài toán vận hành phức tạp nhất.
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
                <h3 class="font-headline fw-black mb-3">Tận tâm</h3>
                <p class="text-secondary lh-lg mb-0">
                  Mỗi khách hàng là một đối tác chiến lược. Sự thành công của
                  bạn chính là thước đo duy nhất cho giá trị của chúng tôi.
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
                <h3 class="font-headline fw-black mb-3">Chính trực</h3>
                <p class="text-secondary lh-lg mb-0">
                  Minh bạch trong công nghệ, trung thực trong dịch vụ. Chúng tôi
                  xây dựng niềm tin từ những điều nhỏ nhất.
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section class="container max-w-7xl py-5 my-5">
        <div
          class="d-flex flex-column flex-md-row justify-content-between align-items-md-end mb-5 pb-3"
        >
          <div class="max-w-lg">
            <h2 class="font-headline display-5 fw-black mb-3">
              Đội ngũ lãnh đạo
            </h2>
            <p class="text-secondary fs-5 mb-0">
              Những bộ óc chiến lược đứng sau hành trình chuyển đổi số ngành F&B
              Việt Nam.
            </p>
          </div>
          <button
            class="btn btn-link text-orange fw-bold text-decoration-none mt-3 mt-md-0 d-flex align-items-center gap-2 group-hover"
          >
            Gia nhập đội ngũ <i class="bi bi-arrow-right transition-icon"></i>
          </button>
        </div>

        <div class="row g-4">
          <div class="col-sm-6 col-lg-3">
            <div class="team-card group">
              <div class="ratio ratio-3x4 rounded-4 overflow-hidden mb-3">
                <img
                  src="https://images.unsplash.com/photo-1560250097-0b93528c311a?auto=format&fit=crop&q=80&w=400"
                  class="object-fit-cover team-img"
                  alt="CEO"
                />
              </div>
              <h4 class="font-headline fw-bold mb-1">Nguyễn Văn A</h4>
              <p class="text-orange fw-bold small">Founder & CEO</p>
            </div>
          </div>
          <div class="col-sm-6 col-lg-3">
            <div class="team-card group">
              <div class="ratio ratio-3x4 rounded-4 overflow-hidden mb-3">
                <img
                  src="https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&q=80&w=400"
                  class="object-fit-cover team-img"
                  alt="CTO"
                />
              </div>
              <h4 class="font-headline fw-bold mb-1">Trần Thị B</h4>
              <p class="text-orange fw-bold small">Chief Technology Officer</p>
            </div>
          </div>
          <div class="col-sm-6 col-lg-3">
            <div class="team-card group">
              <div class="ratio ratio-3x4 rounded-4 overflow-hidden mb-3">
                <img
                  src="https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=crop&q=80&w=400"
                  class="object-fit-cover team-img"
                  alt="CFO"
                />
              </div>
              <h4 class="font-headline fw-bold mb-1">Lê Văn C</h4>
              <p class="text-orange fw-bold small">Chief Financial Officer</p>
            </div>
          </div>
          <div class="col-sm-6 col-lg-3">
            <div class="team-card group">
              <div class="ratio ratio-3x4 rounded-4 overflow-hidden mb-3">
                <img
                  src="https://images.unsplash.com/photo-1580489944761-15a19d654956?auto=format&fit=crop&q=80&w=400"
                  class="object-fit-cover team-img"
                  alt="UX"
                />
              </div>
              <h4 class="font-headline fw-bold mb-1">Phạm Thị D</h4>
              <p class="text-orange fw-bold small">Head of Product Design</p>
            </div>
          </div>
        </div>
      </section>

      <section class="container max-w-7xl mb-5 pb-5">
        <div
          class="culinary-gradient rounded-5 p-5 text-center text-white ambient-shadow position-relative overflow-hidden"
        >
          <div class="position-relative z-index-1 py-4">
            <h2 class="display-4 font-headline fw-black mb-4">
              Sẵn sàng khởi tạo dòng chảy <br class="d-none d-md-block" />
              kinh doanh của bạn?
            </h2>
            <p class="fs-5 opacity-75 mb-5 max-w-lg mx-auto">
              Tham gia cùng 15,000+ doanh nghiệp F&B đã tin dùng POS36 để nâng
              tầm trải nghiệm thực khách.
            </p>
            <div
              class="d-flex flex-column flex-sm-row gap-3 justify-content-center"
            >
              <button
                class="btn btn-light text-orange rounded-4 px-5 py-3 fw-bold fs-5 shadow hover-scale"
              >
                Bắt đầu dùng thử miễn phí
              </button>
              <button
                class="btn btn-outline-light rounded-4 px-5 py-3 fw-bold fs-5 hover-scale"
              >
                Liên hệ tư vấn 1-1
              </button>
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

      <footer class="bg-white border-top py-5 mt-5">
        <div class="container max-w-7xl">
          <div class="row g-5 mb-5">
            <div class="col-lg-4">
              <h4 class="font-headline fw-black text-dark mb-4">POS36</h4>
              <p class="text-secondary small lh-lg pe-lg-4">
                © 2026 POS36. Nền tảng quản lý nhà hàng hàng đầu Việt Nam. Tự
                hào đồng hành cùng sự phát triển của ngành ẩm thực Việt.
              </p>
              <div class="d-flex gap-3 mt-4">
                <a href="#" class="text-secondary fs-4 hover-text-orange"
                  ><i class="bi bi-facebook"></i
                ></a>
                <a href="#" class="text-secondary fs-4 hover-text-orange"
                  ><i class="bi bi-youtube"></i
                ></a>
                <a href="#" class="text-secondary fs-4 hover-text-orange"
                  ><i class="bi bi-linkedin"></i
                ></a>
              </div>
            </div>
            <div class="col-sm-4 col-lg-2 offset-lg-1">
              <h6 class="font-headline fw-bold mb-4">Tính năng</h6>
              <ul class="list-unstyled space-y-3 small text-secondary">
                <li class="mb-3">
                  <a
                    href="#"
                    class="text-decoration-none text-secondary hover-text-orange"
                    >Quản lý kho</a
                  >
                </li>
                <li class="mb-3">
                  <a
                    href="#"
                    class="text-decoration-none text-secondary hover-text-orange"
                    >Order tại bàn</a
                  >
                </li>
                <li class="mb-3">
                  <a
                    href="#"
                    class="text-decoration-none text-secondary hover-text-orange"
                    >Báo cáo tài chính</a
                  >
                </li>
                <li>
                  <a
                    href="#"
                    class="text-decoration-none text-secondary hover-text-orange"
                    >Loyalty Program</a
                  >
                </li>
              </ul>
            </div>
            <div class="col-sm-4 col-lg-2">
              <h6 class="font-headline fw-bold mb-4">Công ty</h6>
              <ul class="list-unstyled space-y-3 small text-secondary">
                <li class="mb-3">
                  <a href="#" class="text-decoration-none text-orange fw-bold"
                    >Giới thiệu</a
                  >
                </li>
                <li class="mb-3">
                  <a
                    href="#"
                    class="text-decoration-none text-secondary hover-text-orange"
                    >Tin tức</a
                  >
                </li>
                <li class="mb-3">
                  <a
                    href="#"
                    class="text-decoration-none text-secondary hover-text-orange"
                    >Tuyển dụng</a
                  >
                </li>
                <li>
                  <a
                    href="#"
                    class="text-decoration-none text-secondary hover-text-orange"
                    >Liên hệ</a
                  >
                </li>
              </ul>
            </div>
            <div class="col-sm-4 col-lg-3">
              <h6 class="font-headline fw-bold mb-4">Chính sách</h6>
              <ul class="list-unstyled space-y-3 small text-secondary">
                <li class="mb-3">
                  <a
                    href="#"
                    class="text-decoration-none text-secondary hover-text-orange text-decoration-underline"
                    >Chính sách bảo mật</a
                  >
                </li>
                <li class="mb-3">
                  <a
                    href="#"
                    class="text-decoration-none text-secondary hover-text-orange text-decoration-underline"
                    >Điều khoản sử dụng</a
                  >
                </li>
                <li class="mb-3">
                  <a
                    href="#"
                    class="text-decoration-none text-secondary hover-text-orange text-decoration-underline"
                    >Quy định chung</a
                  >
                </li>
                <li>
                  <a
                    href="#"
                    class="text-decoration-none text-secondary hover-text-orange text-decoration-underline"
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
    </main>
  </div>
</template>

<script setup>
import PublicNavbar from "../../components/PublicNavbar.vue";
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

/* Hiệu ứng Ảnh Team Trắng Đen -> Màu */
.team-img {
  filter: grayscale(100%);
  transition: filter 0.5s ease;
}
.team-card:hover .team-img {
  filter: grayscale(0%);
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
