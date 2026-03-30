<template>
  <div class="bg-surface font-body text-on-surface">
    <PublicNavbar />

    <main class="pt-5 mt-5">
      <header class="container max-w-7xl py-5 mt-4 mb-5">
        <div class="row align-items-center g-5">
          <div class="col-lg-6 z-index-1">
            <span
              class="badge bg-primary-fixed text-primary-variant px-3 py-2 rounded-pill fw-bold text-uppercase tracking-widest mb-4"
            >
              Pháp lý & Bảo mật
            </span>
            <h1
              class="display-3 font-headline fw-black tracking-tighter lh-sm mb-4 text-dark"
            >
              Chính sách <br /><span class="text-orange">bảo mật</span>
            </h1>
            <p class="fs-5 text-secondary lh-lg max-w-lg mb-0">
              Tại POS36, sự tin tưởng của bạn là ưu tiên hàng đầu. Chúng tôi cam
              kết bảo vệ dữ liệu kinh doanh của bạn bằng những công nghệ tiên
              tiến nhất.
            </p>
          </div>

          <div class="col-lg-6 position-relative d-none d-md-block">
            <div
              class="position-absolute top-0 end-0 bg-primary-fixed opacity-50 rounded-circle blur-effect"
              style="
                width: 300px;
                height: 300px;
                margin-top: -50px;
                margin-right: -50px;
              "
            ></div>

            <img
              class="img-fluid rounded-5 ambient-shadow position-relative z-index-1 object-fit-cover"
              style="aspect-ratio: 16/9"
              src="https://images.unsplash.com/photo-1563986768494-4dee2763ff3f?auto=format&fit=crop&q=80&w=800"
              alt="Data Security"
            />
          </div>
        </div>
      </header>

      <section class="container max-w-7xl py-5 mb-5">
        <div class="row g-4">
          <div class="col-lg-7">
            <div
              class="bg-white p-4 p-md-5 rounded-5 ambient-shadow transition-hover h-100 border border-transparent"
            >
              <div
                class="icon-box bg-primary-fixed rounded-4 d-flex align-items-center justify-content-center mb-4"
                style="width: 56px; height: 56px"
              >
                <i class="bi bi-database-fill text-orange fs-3"></i>
              </div>
              <h2 class="font-headline fw-bold fs-3 mb-4 text-dark">
                1. Thu thập dữ liệu
              </h2>
              <p class="text-secondary lh-lg mb-4">
                Chúng tôi thu thập thông tin để cung cấp dịch vụ tốt hơn cho tất
                cả người dùng của mình. Các loại dữ liệu bao gồm:
              </p>
              <ul class="list-unstyled space-y-4 text-secondary">
                <li class="d-flex align-items-start mb-3">
                  <i
                    class="bi bi-check-circle-fill text-orange me-3 fs-5 mt-1"
                  ></i>
                  <span
                    ><strong>Thông tin định danh:</strong> Tên doanh nghiệp, mã
                    số thuế, địa chỉ và thông tin liên hệ của người đại
                    diện.</span
                  >
                </li>
                <li class="d-flex align-items-start mb-3">
                  <i
                    class="bi bi-check-circle-fill text-orange me-3 fs-5 mt-1"
                  ></i>
                  <span
                    ><strong>Dữ liệu giao dịch:</strong> Thông tin về hóa đơn,
                    sản phẩm, doanh thu và các hoạt động kinh doanh phát sinh
                    trên nền tảng.</span
                  >
                </li>
                <li class="d-flex align-items-start">
                  <i
                    class="bi bi-check-circle-fill text-orange me-3 fs-5 mt-1"
                  ></i>
                  <span
                    ><strong>Dữ liệu kỹ thuật:</strong> Địa chỉ IP, loại thiết
                    bị, trình duyệt và lịch sử tương tác với ứng dụng
                    POS36.</span
                  >
                </li>
              </ul>
            </div>
          </div>

          <div class="col-lg-5">
            <div
              class="bg-surface-container p-4 p-md-5 rounded-5 ambient-shadow transition-hover h-100 d-flex flex-column justify-content-between"
            >
              <div>
                <div
                  class="icon-box bg-primary-fixed rounded-4 d-flex align-items-center justify-content-center mb-4"
                  style="width: 56px; height: 56px"
                >
                  <i class="bi bi-graph-up-arrow text-orange fs-3"></i>
                </div>
                <h2 class="font-headline fw-bold fs-3 mb-4 text-dark">
                  2. Sử dụng thông tin
                </h2>
                <p class="text-secondary lh-lg">
                  Thông tin được sử dụng nhằm tối ưu hóa quy trình vận hành nhà
                  hàng của bạn, gửi thông báo cập nhật tính năng mới và hỗ trợ
                  kỹ thuật kịp thời. POS36 cam kết không bao giờ bán dữ liệu của
                  bạn cho bên thứ ba vì mục đích quảng cáo.
                </p>
              </div>
              <div
                class="mt-4 pt-4 border-top border-secondary border-opacity-25"
              >
                <p class="small fw-bold text-orange fst-italic mb-0">
                  "Dữ liệu của bạn là tài sản của bạn."
                </p>
              </div>
            </div>
          </div>

          <div class="col-lg-5">
            <div
              class="bg-surface-container-highest p-4 p-md-5 rounded-5 ambient-shadow transition-hover h-100"
            >
              <div
                class="icon-box bg-primary-fixed rounded-4 d-flex align-items-center justify-content-center mb-4"
                style="width: 56px; height: 56px"
              >
                <i class="bi bi-shield-fill-check text-orange fs-3"></i>
              </div>
              <h2 class="font-headline fw-bold fs-3 mb-4 text-dark">
                3. Bảo mật dữ liệu
              </h2>
              <p class="text-secondary lh-lg mb-4">
                Chúng tôi sử dụng tiêu chuẩn mã hóa SSL/TLS cho tất cả các luồng
                dữ liệu. Hệ thống máy chủ được đặt tại các trung tâm dữ liệu đạt
                chuẩn Tier III với giám sát 24/7.
              </p>
              <div class="bg-white bg-opacity-50 p-3 rounded-4 backdrop-blur">
                <p
                  class="small text-uppercase tracking-widest fw-bold text-muted mb-2"
                  style="font-size: 0.7rem"
                >
                  Tiêu chuẩn áp dụng
                </p>
                <div class="d-flex gap-2 flex-wrap">
                  <span
                    class="badge bg-white text-dark border px-2 py-1 fw-bold shadow-sm"
                    >ISO 27001</span
                  >
                  <span
                    class="badge bg-white text-dark border px-2 py-1 fw-bold shadow-sm"
                    >GDPR READY</span
                  >
                  <span
                    class="badge bg-white text-dark border px-2 py-1 fw-bold shadow-sm"
                    >AES-256</span
                  >
                </div>
              </div>
            </div>
          </div>

          <div class="col-lg-7">
            <div
              class="bg-white p-4 p-md-5 rounded-5 ambient-shadow transition-hover h-100"
            >
              <div
                class="icon-box bg-primary-fixed rounded-4 d-flex align-items-center justify-content-center mb-4"
                style="width: 56px; height: 56px"
              >
                <i class="bi bi-person-fill-gear text-orange fs-3"></i>
              </div>
              <h2 class="font-headline fw-bold fs-3 mb-4 text-dark">
                4. Quyền của người dùng
              </h2>
              <div class="row g-4 mt-2">
                <div class="col-sm-6">
                  <h5 class="fw-bold text-dark fs-6 mb-2">Quyền truy cập</h5>
                  <p class="text-secondary small lh-lg mb-0">
                    Bạn có quyền yêu cầu trích xuất toàn bộ dữ liệu lịch sử bán
                    hàng bất kỳ lúc nào.
                  </p>
                </div>
                <div class="col-sm-6">
                  <h5 class="fw-bold text-dark fs-6 mb-2">Quyền chỉnh sửa</h5>
                  <p class="text-secondary small lh-lg mb-0">
                    Yêu cầu cập nhật hoặc sửa đổi các thông tin không chính xác
                    về doanh nghiệp.
                  </p>
                </div>
                <div class="col-sm-6">
                  <h5 class="fw-bold text-dark fs-6 mb-2">Quyền xóa bỏ</h5>
                  <p class="text-secondary small lh-lg mb-0">
                    Yêu cầu xóa vĩnh viễn tài khoản và mọi dữ liệu liên quan khi
                    ngừng sử dụng dịch vụ.
                  </p>
                </div>
                <div class="col-sm-6">
                  <h5 class="fw-bold text-dark fs-6 mb-2">Quyền phản đối</h5>
                  <p class="text-secondary small lh-lg mb-0">
                    Từ chối nhận các thông tin tiếp thị không cần thiết từ hệ
                    thống của chúng tôi.
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section class="container max-w-7xl pb-5 mb-5 mt-5">
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

          <div class="position-relative z-index-1 py-4">
            <h3 class="display-5 font-headline fw-black mb-4">
              Bạn vẫn còn thắc mắc?
            </h3>
            <p class="fs-5 opacity-75 mb-5 max-w-2xl mx-auto lh-lg">
              Đội ngũ pháp lý và hỗ trợ khách hàng của chúng tôi luôn sẵn sàng
              giải đáp mọi câu hỏi về cách thức chúng tôi xử lý dữ liệu của bạn.
            </p>
            <div class="d-flex flex-wrap justify-content-center gap-3">
              <button
                class="btn btn-light text-orange rounded-4 px-5 py-3 font-headline fw-bold fs-6 shadow-lg hover-scale"
              >
                Liên hệ ngay
              </button>
              <button
                class="btn btn-outline-light rounded-4 px-5 py-3 font-headline fw-bold fs-6 bg-white bg-opacity-10 hover-scale"
              >
                Trung tâm trợ giúp
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
.bg-surface-container-highest {
  background-color: #e2e2e2;
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
.blur-effect {
  filter: blur(80px);
}

/* BENTO GRID HACKS & HOVERS */
.rounded-custom {
  border-radius: 2rem;
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

.max-w-7xl {
  max-width: 1280px;
  margin: 0 auto;
}
.max-w-lg {
  max-width: 600px;
}
.max-w-2xl {
  max-width: 800px;
}
.z-index-1 {
  z-index: 1;
  position: relative;
}
</style>
