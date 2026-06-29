<template>
  <div class="container-fluid px-4 py-4">
    <!-- Header -->
    <div class="d-flex justify-content-between align-items-center mb-4 pb-3 border-bottom">
      <div>
        <h4 class="fw-bold text-dark mb-1">
          <i class="bi bi-gear-fill text-primary me-2"></i> THIẾT LẬP CỬA HÀNG
        </h4>
        <p class="text-muted mb-0 small">Cấu hình tham số vận hành, phân quyền nhân viên và thông tin thương hiệu.</p>
      </div>
      <button class="btn btn-primary fw-bold px-4 rounded-pill shadow-sm d-flex align-items-center gap-2" @click="saveAll" :disabled="saving">
        <span v-if="saving" class="spinner-border spinner-border-sm"></span>
        <i v-else class="bi bi-cloud-arrow-up-fill fs-5"></i>
        {{ saving ? 'Đang lưu...' : 'LƯU TẤT CẢ' }}
      </button>
    </div>

    <!-- Layout chia hai cột -->
    <div class="row g-4">
      <!-- Cột trái: Tab Navigation -->
      <div class="col-lg-3 col-md-4">
        <div class="card border-0 shadow-sm rounded-3 p-2">
          <div class="nav flex-column nav-pills custom-settings-pills">
            <button v-for="t in tabs" :key="t.key" 
                    class="nav-link text-start py-3 px-3 mb-1 rounded-3 d-flex align-items-center gap-3" 
                    :class="{active: activeTab === t.key}" 
                    @click="activeTab = t.key">
              <i :class="'bi bi-' + t.icon + ' fs-5'"></i>
              <span class="fw-semibold">{{ t.label }}</span>
            </button>
          </div>
        </div>
      </div>

      <!-- Cột phải: Tab Content -->
      <div class="col-lg-9 col-md-8">
        <!-- TAB 1: Thông tin cơ bản -->
        <div v-show="activeTab === 'info'" class="tab-pane-content">
          <div class="card border-0 shadow-sm rounded-3 p-4 mb-4">
            <h5 class="fw-bold text-primary mb-4 pb-2 border-bottom d-flex align-items-center gap-2">
              <i class="bi bi-shop fs-4"></i> Thông Tin Thương Hiệu
            </h5>
            
            <div class="row g-3">
              <div class="col-md-6">
                <label class="form-label fw-semibold text-secondary small">Tên cửa hàng <span class="text-danger">*</span></label>
                <input v-model="info.tenCuaHang" class="form-control form-control-lg bg-light border-0 text-dark fs-6" placeholder="Quán Ăn ABC" />
              </div>
              <div class="col-md-6">
                <label class="form-label fw-semibold text-secondary small">Số điện thoại liên hệ</label>
                <input v-model="info.soDienThoai" class="form-control form-control-lg bg-light border-0 text-muted fs-6" placeholder="0901234567" disabled />
                <div class="form-text small text-warning"><i class="bi bi-info-circle me-1"></i>Liên hệ Super Admin để thay đổi số điện thoại dùng thử/SaaS</div>
              </div>
              <div class="col-md-6">
                <label class="form-label fw-semibold text-secondary small">Email cửa hàng</label>
                <input v-model="info.email" type="email" class="form-control form-control-lg bg-light border-0 text-dark fs-6" placeholder="quan@gmail.com" />
              </div>
              <div class="col-md-6">
                <label class="form-label fw-semibold text-secondary small">Địa chỉ hoạt động</label>
                <input v-model="info.diaChi" class="form-control form-control-lg bg-light border-0 text-dark fs-6" placeholder="123 Nguyễn Văn A, Q1..." />
              </div>
              <div class="col-12">
                <label class="form-label fw-semibold text-secondary small">Đường dẫn Logo (URL)</label>
                <input v-model="info.logoUrl" class="form-control form-control-lg bg-light border-0 text-dark fs-6 mb-3" placeholder="https://abc.com/logo.png" />
                <div v-if="info.logoUrl" class="logo-preview-card p-3 border rounded-3 bg-light d-inline-block">
                  <span class="d-block text-secondary small fw-semibold mb-2">Xem trước Logo:</span>
                  <img :src="info.logoUrl" class="img-thumbnail border-0 shadow-sm" style="max-height:80px; object-fit:contain;" />
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- TAB 2: Tùy chọn POS -->
        <div v-show="activeTab === 'pos'" class="tab-pane-content">
          <div class="card border-0 shadow-sm rounded-3 p-4 mb-4">
            <h5 class="fw-bold text-primary mb-4 pb-2 border-bottom d-flex align-items-center gap-2">
              <i class="bi bi-sliders fs-4"></i> Tùy Chọn Vận Hành POS
            </h5>

            <div class="toggle-list-group">
              <!-- Switch 1 -->
              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Cho phép giảm giá thủ công ở POS</h6>
                  <p class="text-muted mb-0 small">Thu ngân có thể điều chỉnh giảm giá theo % hoặc tiền mặt trực tiếp trên giỏ hàng.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.POS_ChophepGiamGia" />
                </div>
              </div>

              <!-- Switch 2 -->
              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Tự động in hóa đơn ngay sau khi thanh toán</h6>
                  <p class="text-muted mb-0 small">Máy in POS sẽ tự kích hoạt xuất hóa đơn giấy ngay khi nhận tín hiệu thanh toán thành công.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.POS_TuDongIn" />
                </div>
              </div>

              <!-- Switch Cho phép bán âm -->
              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Cho phép bán hàng vượt quá số lượng tồn kho (Bán âm)</h6>
                  <p class="text-muted mb-0 small">Bật chức năng này để Thu ngân và Order tiếp tục tính tiền/thêm món ngay cả khi hệ thống hết tồn kho thực tế.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.Kho_ChoPhepBanAm" />
                </div>
              </div>

              <!-- Switch 3 -->
              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Yêu cầu xác nhận khi gửi đơn xuống Bếp</h6>
                  <p class="text-muted mb-0 small">Hiện hộp thoại hỏi lại nhân viên order để tránh thao tác nhầm hoặc trùng lặp.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.POS_XacNhanGuiBep" />
                </div>
              </div>

              <!-- Switch 4 -->
              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Hiển thị mã QR thanh toán động VietQR</h6>
                  <p class="text-muted mb-0 small">Tự tạo và hiển thị mã chuyển khoản nhanh có số tiền kèm theo khi Thu ngân bấm thanh toán.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.POS_HienQR" />
                </div>
              </div>

              <!-- NEW Switch 5 -->
              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center" :class="{'opacity-50': !cfgBool.POS_HienQR}">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Chỉ hiển thị mã QR trên màn hình Thu ngân (Thu ngân Only)</h6>
                  <p class="text-muted mb-0 small">Không hiện QR trên màn hình Order của nhân viên bàn ăn khác để tránh gây gián đoạn công việc.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.POS_HienQrThuNganOnly" :disabled="!cfgBool.POS_HienQR" />
                </div>
              </div>

              <!-- NEW Switch 6a -->
              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Cho phép nhân viên Order thanh toán Tiền mặt ngay tại bàn</h6>
                  <p class="text-muted mb-0 small">Nhân viên phục vụ có thể nhận tiền mặt và xác nhận hoàn tất thanh toán hóa đơn trực tiếp cho khách.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.POS_OrderThanhToanTienMat" />
                </div>
              </div>

              <!-- NEW Switch 6b -->
              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center" :class="{'opacity-50': !cfgBool.POS_HienQR}">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Cho phép nhân viên Order thanh toán chuyển khoản QR tại bàn</h6>
                  <p class="text-muted mb-0 small">Nhân viên phục vụ có thể mở mã VietQR động trực tiếp tại bàn để khách hàng quét chuyển khoản nhanh.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.POS_OrderThanhToanQR" :disabled="!cfgBool.POS_HienQR" />
                </div>
              </div>

              <!-- NEW Switch 7 -->
              <div class="toggle-item-row py-3 border-bottom d-flex flex-column align-items-stretch">
                <div class="d-flex justify-content-between align-items-center">
                  <div>
                    <h6 class="fw-bold text-dark mb-1">Cho phép nhân viên Order tạm in hóa đơn/in thử</h6>
                    <p class="text-muted mb-0 small">Thêm chức năng in thử hóa đơn tạm tính ngay từ giao diện điện thoại của nhân viên Order.</p>
                  </div>
                  <div class="form-check form-switch form-switch-lg">
                    <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.POS_OrderInBillNgay" />
                  </div>
                </div>
                <div v-if="cfgBool.POS_OrderInBillNgay" class="mt-3 p-3 rounded bg-white border border-light-subtle row g-2">
                  <div class="col-md-6">
                    <label class="form-label fw-semibold text-secondary small">Chế độ in hóa đơn của Order:</label>
                    <select v-model="cfg.POS_OrderInBillCheDo" class="form-select bg-light border-0 text-dark">
                      <option value="Direct">In trực tiếp từ điện thoại nhân viên</option>
                      <option value="Remote">Gửi lệnh in về máy Thu Ngân ở quầy</option>
                      <option value="Ask">Hỏi ý kiến nhân viên mỗi khi in</option>
                    </select>
                  </div>
                </div>
              </div>

              <!-- NEW Switch 8 -->
              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Cho phép in nhiều bản hóa đơn tại Thu ngân</h6>
                  <p class="text-muted mb-0 small">Bật nút in bill linh động trên giao diện tính tiền của Thu ngân để in lại bất cứ lúc nào.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.POS_ThuNganInNhieuBill" />
                </div>
              </div>

              <!-- NEW Switch 9 -->
              <div class="toggle-item-row py-3 d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Cho phép Thu ngân xem lịch sử & in lại hóa đơn cũ</h6>
                  <p class="text-muted mb-0 small">Hiển thị danh sách hóa đơn đã thanh toán gần nhất ở POS để Thu ngân truy xuất đối chiếu hoặc in lại.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.POS_ThuNganXemLichSu" />
                </div>
              </div>
            </div>

            <!-- Các ô nhập số cấu hình POS -->
            <div class="row g-3 mt-4 pt-3 border-top">
              <div class="col-md-6">
                <label class="form-label fw-semibold text-secondary small">Tỉ lệ Giảm Giá Tối Đa (%)</label>
                <input type="number" v-model.number="cfg.POS_GiamGiaMax" class="form-control bg-light border-0 text-dark fs-6" min="0" max="100" />
              </div>
              <div class="col-md-6">
                <label class="form-label fw-semibold text-secondary small">Cảnh báo hạn mức kho (đơn vị)</label>
                <input type="number" v-model.number="cfg.POS_CanhBaoKho" class="form-control bg-light border-0 text-dark fs-6" min="0" />
              </div>
            </div>
          </div>
        </div>

        <!-- TAB 3: Phân quyền nhân viên (NEW) -->
        <div v-show="activeTab === 'permission'" class="tab-pane-content">
          <div class="card border-0 shadow-sm rounded-3 p-4 mb-4">
            <h5 class="fw-bold text-primary mb-4 pb-2 border-bottom d-flex align-items-center gap-2">
              <i class="bi bi-shield-check fs-4"></i> Phân Quyền Nhân Viên Chi Tiết
            </h5>
            <p class="text-muted small mb-3">Tùy biến quyền thao tác vận hành riêng lẻ cho nhân viên Order và nhân viên Thu ngân nhằm tối ưu hóa quy trình bảo mật.</p>

            <div class="toggle-list-group">
              <!-- Quyền 1 -->
              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Cho phép nhân viên Order được quyền HỦY MÓN</h6>
                  <p class="text-muted mb-0 small">Cho phép nhân viên phục vụ hủy món ăn đã gửi xuống bếp. (Khuyên tắt để quản lý kiểm soát tránh gian lận).</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.Perm_Order_HuyMon" />
                </div>
              </div>

              <!-- Quyền 2 -->
              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Cho phép nhân viên Order thực hiện CHUYỂN BÀN / TÁCH BÀN / GHÉP BÀN</h6>
                  <p class="text-muted mb-0 small">Cho phép nhân viên tự thao tác chuyển sơ đồ phục vụ của khách ngay trên điện thoại di động.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.Perm_Order_ChuyenTach" />
                </div>
              </div>

              <!-- Quyền 3 -->
              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Cho phép Thu ngân HỦY / XÓA hóa đơn chưa thanh toán</h6>
                  <p class="text-muted mb-0 small">Cho phép Thu ngân tự hủy hóa đơn khách gọi sai. Nếu tắt, chỉ có Chủ cửa hàng hoặc Quản lý được thao tác.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.Perm_ThuNgan_XoaHoaDon" />
                </div>
              </div>

              <!-- Quyền 4 -->
              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Cho phép Thu ngân tự hủy món đã gửi bếp</h6>
                  <p class="text-muted mb-0 small">Thu ngân có thể hủy món ăn đã gửi xuống Bếp/Bar mà không cần Quản lý duyệt.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.Perm_ThuNgan_HuyMonDaGui" />
                </div>
              </div>

              <!-- Quyền 5 -->
              <div class="toggle-item-row py-3 d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Yêu cầu xác thực PIN Quản lý khi hủy món cuối / xóa bàn</h6>
                  <p class="text-muted mb-0 small">Yêu cầu nhập mã bảo mật khi Thu ngân muốn xóa bàn hoặc hủy toàn bộ món ăn trong đơn hàng.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.POS_YeuCauMatKhauHuyBill" />
                </div>
              </div>
            </div>
          </div>

          <!-- PHÂN QUYỀN ADMIN CHO TỪNG THU NGÂN -->
          <div class="card border-0 shadow-sm rounded-3 p-4">
            <h5 class="fw-bold text-primary mb-1 d-flex align-items-center gap-2">
              <i class="bi bi-person-badge-fill fs-4"></i> Phân Quyền Truy Cập Admin cho Thu Ngân
            </h5>
            <p class="text-muted small mb-4">
              Cấp phép từng Thu ngân được vào Admin để xem một số chức năng nhất định
              mà không cần cấp toàn quyền Quản lý.
            </p>

            <div v-if="loadingThuNgan" class="text-center py-4">
              <div class="spinner-border spinner-border-sm text-primary me-2"></div>
              <span class="text-muted small">Đang tải danh sách Thu ngân...</span>
            </div>

            <div v-else-if="thuNganList.length === 0" class="text-center py-5 text-muted">
              <i class="bi bi-people fs-1 d-block mb-2 opacity-25"></i>
              <p class="small mb-2">Chưa có nhân viên Thu ngân nào trong chi nhánh.</p>
              <router-link to="/admin/employees" class="btn btn-sm btn-outline-primary rounded-pill px-3">
                <i class="bi bi-person-plus me-1"></i> Thêm nhân viên Thu ngân
              </router-link>
            </div>

            <div v-else class="d-flex flex-column gap-3">
              <div
                v-for="nv in thuNganList"
                :key="nv.id"
                class="border rounded-3 overflow-hidden"
              >
                <!-- Header nhân viên -->
                <div class="d-flex align-items-center justify-content-between px-4 py-3 bg-light border-bottom">
                  <div class="d-flex align-items-center gap-3">
                    <div class="rounded-circle d-flex align-items-center justify-content-center text-white fw-bold flex-shrink-0" style="width:40px;height:40px;background:#0284c7;font-size:1rem;">
                      {{ nv.tenNhanVien.charAt(0).toUpperCase() }}
                    </div>
                    <div>
                      <div class="fw-bold text-dark">{{ nv.tenNhanVien }}</div>
                      <div class="text-muted small font-monospace">{{ nv.tenDangNhap }} &nbsp;·&nbsp; {{ nv.maNhanVien }}</div>
                    </div>
                  </div>
                  <div class="d-flex align-items-center gap-2">
                    <span v-if="nv.quyenArr.length > 0" class="badge bg-success bg-opacity-10 text-success rounded-pill px-3">
                      <i class="bi bi-shield-check me-1"></i>{{ nv.quyenArr.length }} quyền
                    </span>
                    <span v-else class="badge bg-secondary bg-opacity-10 text-secondary rounded-pill px-3">
                      <i class="bi bi-shield-slash me-1"></i>Chưa cấp quyền
                    </span>
                    <button
                      v-if="nv.isActive !== false"
                      @click="nv.expanded = !nv.expanded"
                      class="btn btn-sm rounded-pill px-3"
                      :class="nv.expanded ? 'btn-primary' : 'btn-outline-primary'"
                    >
                      <i :class="nv.expanded ? 'bi bi-chevron-up me-1' : 'bi bi-chevron-down me-1'"></i>
                      {{ nv.expanded ? 'Thu gọn' : 'Thiết lập quyền' }}
                    </button>
                    <span v-else class="text-muted small fst-italic">Tài khoản bị khóa</span>
                  </div>
                </div>

                <!-- Checklist quyền (mở rộng) -->
                <div v-show="nv.expanded" class="px-4 py-3 bg-white">
                  <div class="row g-2 mb-3">
                    <div v-for="q in ALL_QUYEN_THUNGAN" :key="q.ma" class="col-md-6">
                      <label
                        class="d-flex align-items-start gap-2 p-2 rounded-2 w-100"
                        :style="nv.quyenArr.includes(q.ma)
                          ? 'background:#e8f5e9;border:1px solid #c8e6c9;cursor:pointer;'
                          : 'background:#f8f9fa;border:1px solid #e9ecef;cursor:pointer;'"
                      >
                        <input
                          type="checkbox"
                          class="form-check-input flex-shrink-0 mt-1"
                          :checked="nv.quyenArr.includes(q.ma)"
                          @change="toggleQuyen(nv, q.ma)"
                        />
                        <div>
                          <div class="fw-semibold text-dark" style="font-size:0.85rem;">{{ q.ten }}</div>
                          <div class="text-muted" style="font-size:0.73rem;">{{ q.moTa }}</div>
                        </div>
                      </label>
                    </div>
                  </div>
                  <div class="d-flex justify-content-between align-items-center pt-2 border-top">
                    <button @click="clearQuyen(nv)" class="btn btn-sm btn-outline-danger rounded-pill px-3">
                      <i class="bi bi-shield-slash me-1"></i>Thu hồi tất cả
                    </button>
                    <button @click="saveQuyen(nv)" class="btn btn-sm btn-success rounded-pill px-4 fw-bold">
                      <i class="bi bi-cloud-check me-1"></i>Lưu quyền
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- TAB 4: Tích điểm khách hàng -->
        <div v-show="activeTab === 'loyalty'" class="tab-pane-content">
          <div class="card border-0 shadow-sm rounded-3 p-4 mb-4">
            <h5 class="fw-bold text-primary mb-4 pb-2 border-bottom d-flex align-items-center gap-2">
              <i class="bi bi-star-fill fs-4"></i> Chương Trình Khách Hàng Thân Thiết
            </h5>

            <div class="toggle-item-row py-3 mb-4 d-flex justify-content-between align-items-center bg-light p-3 rounded-3">
              <div>
                <h6 class="fw-bold text-dark mb-1">Bật tích lũy điểm khi mua hàng</h6>
                <p class="text-muted mb-0 small">Khách hàng được cộng điểm tự động khi hóa đơn được thanh toán thành công.</p>
              </div>
              <div class="form-check form-switch form-switch-lg">
                <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.Loyalty_BatTat" />
              </div>
            </div>

            <div class="row g-4" :class="{'opacity-50 pointer-events-none': !cfgBool.Loyalty_BatTat}">
              <div class="col-md-6">
                <label class="form-label fw-semibold text-secondary small">Tỉ lệ quy đổi chi tiêu (VNĐ → 1 Điểm)</label>
                <div class="input-group input-group-lg">
                  <input type="number" v-model.number="cfg.Loyalty_TiLeKiem" class="form-control bg-light border-0 text-dark" min="1000" step="1000" :disabled="!cfgBool.Loyalty_BatTat" />
                  <span class="input-group-text bg-white border-0 text-muted small fw-bold">VNĐ = 1 Điểm</span>
                </div>
              </div>
              <div class="col-md-6">
                <label class="form-label fw-semibold text-secondary small">Giá trị sử dụng điểm quy đổi (1 Điểm → VNĐ)</label>
                <div class="input-group input-group-lg">
                  <input type="number" v-model.number="cfg.Loyalty_TiLeDoiDiem" class="form-control bg-light border-0 text-dark" min="1" :disabled="!cfgBool.Loyalty_BatTat" />
                  <span class="input-group-text bg-white border-0 text-muted small fw-bold">VNĐ / 1 điểm</span>
                </div>
              </div>

              <!-- Hạng thành viên -->
              <div class="col-12 mt-4">
                <label class="form-label fw-bold text-muted small text-uppercase tracking-wider d-block mb-3">Ngưỡng Tích Lũy Nâng Hạng Thành Viên</label>
                <div class="row g-3">
                  <!-- Rank Đồng -->
                  <div class="col-md-4">
                    <div class="card border border-amber bg-amber-light text-amber-dark p-3 text-center shadow-xs">
                      <div class="fw-bold mb-2"><i class="bi bi-award me-1"></i>Hạng Đồng</div>
                      <input type="number" v-model.number="cfg.Loyalty_NguongDong" class="form-control form-control-sm text-center border-0 bg-white" min="0" placeholder="0" :disabled="!cfgBool.Loyalty_BatTat" />
                    </div>
                  </div>
                  <!-- Rank Bạc -->
                  <div class="col-md-4">
                    <div class="card border border-slate bg-slate-light text-slate-dark p-3 text-center shadow-xs">
                      <div class="fw-bold mb-2"><i class="bi bi-award-fill me-1"></i>Hạng Bạc</div>
                      <input type="number" v-model.number="cfg.Loyalty_NguongBac" class="form-control form-control-sm text-center border-0 bg-white" min="0" placeholder="500" :disabled="!cfgBool.Loyalty_BatTat" />
                    </div>
                  </div>
                  <!-- Rank Vàng -->
                  <div class="col-md-4">
                    <div class="card border border-gold bg-gold-light text-gold-dark p-3 text-center shadow-xs">
                      <div class="fw-bold mb-2"><i class="bi bi-trophy-fill me-1"></i>Hạng Vàng</div>
                      <input type="number" v-model.number="cfg.Loyalty_NguongVang" class="form-control form-control-sm text-center border-0 bg-white" min="0" placeholder="2000" :disabled="!cfgBool.Loyalty_BatTat" />
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- TAB 5: Bảo mật & Gói dịch vụ -->
        <div v-show="activeTab === 'security'" class="tab-pane-content">
          <div class="card border-0 shadow-sm rounded-3 p-4 mb-4">
            <h5 class="fw-bold text-primary mb-4 pb-2 border-bottom d-flex align-items-center gap-2">
              <i class="bi bi-shield-lock-fill fs-4"></i> Bảo Mật Vận Hành
            </h5>

            <div class="toggle-list-group mb-4">
              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Yêu cầu nhập mã PIN nhân viên</h6>
                  <p class="text-muted mb-0 small">Nhân viên cần xác nhận mật mã PIN cá nhân khi thực hiện các giao dịch nhạy cảm.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.Security_YeuCauPIN" />
                </div>
              </div>

              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Tự động đăng xuất tài khoản khi treo màn hình</h6>
                  <p class="text-muted mb-0 small">Khóa màn hình làm việc của thu ngân khi không phát hiện tương tác sau một thời gian nhất định.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.Security_AutoLogout" />
                </div>
              </div>

              <!-- Thêm thiết lập vận hành thông minh & bảo mật khác -->
              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Tự động khóa sổ doanh thu lúc 23:59 hàng ngày</h6>
                  <p class="text-muted mb-0 small">Khóa tất cả hóa đơn đã bán của ngày cũ, nhân viên sẽ không thể sửa đổi hay xóa hóa đơn cũ.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.POS_TuDongKhoaSo" />
                </div>
              </div>

              <div class="toggle-item-row py-3 border-bottom d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Bắt buộc phải chọn món trước khi thanh toán</h6>
                  <p class="text-muted mb-0 small">Ngăn chặn tạo hóa đơn rỗng (0 món) khi tính tiền tại quầy thu ngân.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.POS_ThanhToanBatBuocChonMon" />
                </div>
              </div>

              <div class="toggle-item-row py-3 d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="fw-bold text-dark mb-1">Cho phép khách hàng hoàn trả món sau khi thanh toán</h6>
                  <p class="text-muted mb-0 small">Cho phép thu ngân nhập phiếu trả đồ và hoàn tiền đối với các hóa đơn đã hoàn tất thanh toán.</p>
                </div>
                <div class="form-check form-switch form-switch-lg">
                  <input class="form-check-input" type="checkbox" role="switch" v-model="cfgBool.POS_ChoPhepHoanTraMon" />
                </div>
              </div>
            </div>

            <div class="row g-3 mt-2">
              <div class="col-md-6" v-if="cfgBool.Security_AutoLogout">
                <label class="form-label fw-semibold text-secondary small">Thời gian tự động khóa (phút)</label>
                <input type="number" v-model.number="cfg.Security_TimeoutPhut" class="form-control bg-light border-0 text-dark fs-6" min="5" max="120" />
              </div>
              <div class="col-md-6">
                <label class="form-label fw-semibold text-secondary small">Mã PIN xác thực nhanh của Chủ quán/Quản lý</label>
                <input type="text" maxlength="4" v-model="cfg.Security_AdminPIN" class="form-control bg-light border-0 text-dark fs-6 font-monospace text-center fw-bold" style="width: 150px;" placeholder="1234" />
              </div>
            </div>
          </div>

          <!-- Gói dịch vụ SaaS -->
          <div class="card border-0 shadow-sm rounded-3 p-4 bg-gradient-brand text-white">
            <h5 class="fw-bold mb-3 d-flex align-items-center gap-2">
              <i class="bi bi-info-circle-fill fs-4"></i> Trạng Thái Thuê Bao Phần Mềm
            </h5>
            <div class="d-flex justify-content-between align-items-center flex-wrap gap-3">
              <div>
                <span class="badge bg-white text-primary rounded-pill fw-bold px-3 py-2 fs-6 mb-2 d-inline-block">
                  GÓI: {{ info.goiDichVu ? info.goiDichVu.toUpperCase() : 'DÙNG THỬ' }}
                </span>
                <p class="mb-0 fs-6">Hạn sử dụng dịch vụ: <strong class="text-warning">{{ formatDate(info.ngayHetHan) }}</strong></p>
              </div>
              <router-link to="/admin/subscription" class="btn btn-warning fw-bold px-4 rounded-pill shadow d-flex align-items-center gap-2">
                <i class="bi bi-arrow-up-circle-fill fs-5"></i> NÂNG CẤP GÓI DỊCH VỤ
              </router-link>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, inject, watch } from 'vue';
import axios from 'axios';
import { globalState } from '../store';
const swal = inject('$swal');

const activeTab = ref('info');
const saving = ref(false);

const tabs = [
  { key: 'info', label: 'Thông tin CH', icon: 'shop' },
  { key: 'pos', label: 'Tùy chọn POS', icon: 'sliders' },
  { key: 'permission', label: 'Phân quyền', icon: 'shield-lock' },
  { key: 'loyalty', label: 'Tích điểm', icon: 'star' },
  { key: 'security', label: 'Bảo mật', icon: 'shield' },
];

const info = reactive({ tenCuaHang: '', soDienThoai: '', email: '', diaChi: '', logoUrl: '', trangThai: '', ngayHetHan: null, goiDichVu: '' });

const cfg = reactive({
  POS_GiamGiaMax: '20', POS_CanhBaoKho: '5',
  Loyalty_TiLeKiem: '10000', Loyalty_TiLeDoiDiem: '100',
  Loyalty_NguongDong: '0', Loyalty_NguongBac: '500', Loyalty_NguongVang: '2000',
  Security_TimeoutPhut: '30',
  POS_OrderInBillCheDo: 'Ask',
  Security_AdminPIN: '1234',
});

const cfgBool = reactive({
  POS_ChophepGiamGia: true, POS_TuDongIn: false, POS_XacNhanGuiBep: false, POS_HienQR: true,
  POS_HienQrThuNganOnly: false,
  POS_OrderThanhToanTienMat: false,
  POS_OrderThanhToanQR: false,
  POS_OrderInBillNgay: false,
  POS_ThuNganInNhieuBill: false,
  POS_ThuNganXemLichSu: true,
  Perm_Order_HuyMon: true,
  Perm_Order_ChuyenTach: true,
  Perm_ThuNgan_XoaHoaDon: true,
  Loyalty_BatTat: false, Security_YeuCauPIN: false, Security_AutoLogout: true,
  POS_TuDongKhoaSo: true,
  Perm_ThuNgan_HuyMonDaGui: false,
  POS_ThanhToanBatBuocChonMon: true,
  POS_ChoPhepHoanTraMon: true,
  POS_YeuCauMatKhauHuyBill: true,
  Kho_ChoPhepBanAm: true,
});

const boolKeys = Object.keys(cfgBool);
const allKeys = [...Object.keys(cfg), ...boolKeys].join(',');

const formatDate = (d) => d ? new Date(d).toLocaleDateString('vi-VN') : '—';

const load = async () => {
  try {
    const [si, tl] = await Promise.all([
      axios.get('/api/ThietLap/store-info'),
      axios.get('/api/ThietLap/batch', { params: { keys: allKeys } }),
    ]);
    Object.assign(info, si.data);
    const data = tl.data;
    for (const k of Object.keys(cfg)) if (data[k] !== undefined && data[k] !== '') cfg[k] = data[k];
    for (const k of boolKeys) if (data[k] !== undefined) cfgBool[k] = data[k] === 'true';
  } catch (e) { console.error(e); }
};

const saveAll = async () => {
  saving.value = true;
  try {
    const batch = { ...cfg };
    for (const k of boolKeys) batch[k] = String(cfgBool[k]);

    await Promise.all([
      axios.put('/api/ThietLap/store-info', { tenCuaHang: info.tenCuaHang, email: info.email, diaChi: info.diaChi, logoUrl: info.logoUrl }),
      axios.post('/api/ThietLap/batch', batch),
    ]);
    swal.fire({ toast: true, position: 'top-end', icon: 'success', title: 'Đã lưu thiết lập thành công!', timer: 2000, showConfirmButton: false });
  } catch (e) {
    swal.fire('Lỗi', 'Lưu thiết lập thất bại!', 'error');
  } finally { saving.value = false; }
};

// ========== PHÂN QUYỀN ADMIN CHO THU NGÂN ==========
const ALL_QUYEN_THUNGAN = [
  { ma: 'view_orders',       ten: 'Danh sách đơn hàng',   moTa: 'Xem tất cả đơn hàng đã tạo' },
  { ma: 'view_cashbook',     ten: 'Thu & Chi',            moTa: 'Xem sổ quỹ, phiếu thu chi' },
  { ma: 'view_daily_summary',ten: 'Tổng kết cuối ngày',  moTa: 'Xem báo cáo tổng kết theo ngày' },
  { ma: 'view_sales_report', ten: 'Báo cáo bán hàng',     moTa: 'Xem doanh thu, thống kê' },
  { ma: 'view_lai_gop',      ten: 'Báo cáo Lãi gộp',      moTa: 'Xem báo cáo lợi nhuận' },
  { ma: 'view_customers',    ten: 'Quản lý khách hàng',  moTa: 'Xem, thêm, tích điểm khách' },
  { ma: 'view_import_stock', ten: 'Nhập hàng',           moTa: 'Xem phiếu nhập, xác nhận nhập kho' },
  { ma: 'view_inventory',    ten: 'Kiểm kê kho',         moTa: 'Tạo phiếu kiểm kê, điều chỉnh tồn' },
  { ma: 'view_products',     ten: 'Xem thực đơn',        moTa: 'Xem danh sách sản phẩm (chỉ xem)' },
  { ma: 'view_ai_report',    ten: 'Trợ lý AI',           moTa: 'Dùng AI phân tích báo cáo' },
];

const thuNganList = ref([]);
const loadingThuNgan = ref(false);

const loadThuNganList = async () => {
  const branchId = globalState.value.activeBranchId;
  if (!branchId) return;
  loadingThuNgan.value = true;
  try {
    const res = await axios.get(`/api/NhanVien/danh-sach?chiNhanhId=${branchId}`);
    thuNganList.value = (res.data || [])
      .filter(nv => nv.vaiTro === 'ThuNgan')
      .map(nv => ({
        ...nv,
        expanded: false,
        quyenArr: (nv.quyenThuNgan || '').split(',').map(q => q.trim()).filter(Boolean),
        get quyenDuocCap() { return this.quyenArr.length; }
      }));
  } catch (e) { console.error(e); }
  finally { loadingThuNgan.value = false; }
};

// Toggle 1 quyền trong array cục bộ
const toggleQuyen = (nv, ma) => {
  const idx = nv.quyenArr.indexOf(ma);
  if (idx >= 0) nv.quyenArr.splice(idx, 1);
  else nv.quyenArr.push(ma);
};

// Xóa toàn bộ quyền
const clearQuyen = (nv) => { nv.quyenArr = []; };

// Lưu quyền lên server
const saveQuyen = async (nv) => {
  try {
    await axios.put(`/api/NhanVien/${nv.id}/quyen-admin`, { quyenThuNgan: nv.quyenArr.join(',') });
    swal.fire({
      toast: true, position: 'top-end', icon: 'success',
      title: `Đã cập nhật quyền cho ${nv.tenNhanVien}!`,
      timer: 1800, showConfirmButton: false
    });
    nv.expanded = false;
  } catch (e) {
    swal.fire('Lỗi', e.response?.data?.message || 'Không thể lưu quyền', 'error');
  }
};

// Load khi chuyển sang tab phân quyền
watch(activeTab, (val) => { if (val === 'permission') loadThuNganList(); });

onMounted(load);
</script>

<style scoped>
.custom-settings-pills .nav-link {
  background: transparent;
  color: #64748b;
  border-radius: 8px;
  transition: all 0.2s ease;
}
.custom-settings-pills .nav-link:hover {
  background: #f1f5f9;
  color: #1e293b;
}
.custom-settings-pills .nav-link.active {
  background: #e0f2fe;
  color: #0284c7;
}

.toggle-item-row {
  transition: background 0.15s ease;
}
.toggle-item-row:hover {
  background: #faf8f5;
  padding-left: 8px;
  padding-right: 8px;
  border-radius: 6px;
}

.bg-gradient-brand {
  background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
}

.pointer-events-none {
  pointer-events: none;
}

/* Custom switch sizing */
.form-switch-lg .form-check-input {
  width: 3.2rem;
  height: 1.6rem;
  cursor: pointer;
}

/* Rank cards styles */
.border-amber { border-color: #fde68a !important; }
.bg-amber-light { background-color: #fffbeb; }
.text-amber-dark { color: #b45309; }

.border-slate { border-color: #cbd5e1 !important; }
.bg-slate-light { background-color: #f1f5f9; }
.text-slate-dark { color: #475569; }

.border-gold { border-color: #fcd34d !important; }
.bg-gold-light { background-color: #fffdf5; }
.text-gold-dark { color: #92400e; }
</style>
