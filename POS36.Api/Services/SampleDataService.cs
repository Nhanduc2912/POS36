using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.DTOs;
using POS36.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POS36.Api.Services
{
    public interface ISampleDataService
    {
        Task<bool> HasExistingDataAsync(int cuaHangId);
        Task<bool> InitializeSampleDataAsync(int cuaHangId, SampleDataRequest request);
    }

    public class SampleDataService : ISampleDataService
    {
        private readonly AppDbContext _context;

        public SampleDataService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasExistingDataAsync(int cuaHangId)
        {
            // Bỏ qua global query filter để kiểm tra tuyệt đối
            bool hasProducts = await _context.SanPhams.IgnoreQueryFilters()
                .AnyAsync(s => s.CuaHangId == cuaHangId);
            bool hasTables = await _context.Bans.IgnoreQueryFilters()
                .AnyAsync(b => b.CuaHangId == cuaHangId);

            return hasProducts || hasTables;
        }

        public async Task<bool> InitializeSampleDataAsync(int cuaHangId, SampleDataRequest request)
        {
            // Kiểm tra bảo vệ trước khi sinh
            if (await HasExistingDataAsync(cuaHangId))
            {
                return false;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy chi nhánh mặc định đầu tiên hoặc tạo mới nếu chưa có
                var chiNhanh = await _context.ChiNhanhs.IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.CuaHangId == cuaHangId && !c.IsDeleted);

                if (chiNhanh == null)
                {
                    chiNhanh = new ChiNhanh
                    {
                        CuaHangId = cuaHangId,
                        TenChiNhanh = "Chi nhánh Trung tâm",
                        DiaChi = "Mặc định",
                        IsDeleted = false
                    };
                    _context.ChiNhanhs.Add(chiNhanh);
                    await _context.SaveChangesAsync();
                }

                // 1. CẤU HÌNH THAM SỐ VẬN HÀNH (THIẾT LẬP CỬA HÀNG)
                var settingsToSave = new System.Collections.Generic.Dictionary<string, string>();

                // Mặc định dựa trên mô hình nếu người dùng không truyền CustomSettings
                if (request.ModelType == "fastfood")
                {
                    settingsToSave["POS_HienQrThuNganOnly"] = "true";
                    settingsToSave["POS_HienQR"] = "true";
                    settingsToSave["POS_TuDongIn"] = "false"; // Mặc định tắt tự in cho fastfood để tiện lợi
                    settingsToSave["Perm_ThuNgan_XoaHoaDon"] = "true"; // Quầy fastfood cho phép xóa để xử lý nhanh
                    settingsToSave["POS_YeuCauMatKhauHuyBill"] = "true";
                }
                else
                {
                    settingsToSave["POS_HienQrThuNganOnly"] = "false";
                    settingsToSave["POS_HienQR"] = "true";
                    settingsToSave["POS_TuDongIn"] = "true";
                    settingsToSave["Perm_ThuNgan_XoaHoaDon"] = "false"; // Tránh gian lận
                    settingsToSave["POS_YeuCauMatKhauHuyBill"] = "true";
                }

                // Thiết lập bảo mật & phân quyền chung mặc định
                settingsToSave["POS_TuDongKhoaSo"] = "true";
                settingsToSave["Perm_ThuNgan_HuyMonDaGui"] = "false";
                settingsToSave["POS_ThanhToanBatBuocChonMon"] = "true";
                settingsToSave["POS_ChoPhepHoanTraMon"] = "true";
                settingsToSave["Security_YeuCauPIN"] = "true";

                // Nếu frontend gửi CustomSettings lên thì ghi đè các thiết lập mặc định
                if (request.CustomSettings != null)
                {
                    foreach (var kvp in request.CustomSettings)
                    {
                        settingsToSave[kvp.Key] = kvp.Value;
                    }
                }

                // Lưu các thiết lập vào database
                foreach (var kvp in settingsToSave)
                {
                    var tl = await _context.ThietLaps.IgnoreQueryFilters()
                        .FirstOrDefaultAsync(t => t.CuaHangId == cuaHangId && t.MaThietLap == kvp.Key);
                    if (tl == null)
                    {
                        _context.ThietLaps.Add(new ThietLap { CuaHangId = cuaHangId, MaThietLap = kvp.Key, DuLieu = kvp.Value });
                    }
                    else
                    {
                        tl.DuLieu = kvp.Value;
                    }
                }

                // 2. KHỞI TẠO BÀN GHẾ & KHU VỰC
                if (request.ImportTables)
                {
                    var khuVuc = new KhuVuc
                    {
                        CuaHangId = cuaHangId,
                        ChiNhanhId = chiNhanh.Id,
                        TenKhuVuc = request.ModelType == "fastfood" ? "Quầy & Mang đi" : "Khu vực sảnh chính"
                    };
                    _context.KhuVucs.Add(khuVuc);
                    await _context.SaveChangesAsync();

                    if (request.ModelType == "fastfood")
                    {
                        // Thức ăn nhanh: Sinh 1-3 bàn phụ trợ để thanh toán nhanh hoặc ngồi ăn tại chỗ
                        int ffTableCount = Math.Clamp(request.TableCount, 1, 3);
                        _context.Bans.Add(new Ban { CuaHangId = cuaHangId, KhuVucId = khuVuc.Id, TenBan = "Quầy Thu Ngân", MaBan = "QUAY_01", TrangThai = "Trống" });
                        for (int i = 1; i < ffTableCount; i++)
                        {
                            _context.Bans.Add(new Ban { CuaHangId = cuaHangId, KhuVucId = khuVuc.Id, TenBan = $"Bàn Ăn Nhanh {i}", MaBan = $"BAN_FF{i}", TrangThai = "Trống" });
                        }
                    }
                    else
                    {
                        // Các mô hình khác: Sinh số bàn tùy chọn
                        for (int i = 1; i <= request.TableCount; i++)
                        {
                            _context.Bans.Add(new Ban { CuaHangId = cuaHangId, KhuVucId = khuVuc.Id, TenBan = $"Bàn 0{i}", MaBan = $"B0{i}", TrangThai = "Trống" });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                // 3. KHỞI TẠO DANH MỤC & SẢN PHẨM MẪU
                var listSpMau = new List<(string Ten, decimal GiaBan, decimal GiaNhap, string Nhom)>();

                if (request.ModelType == "restaurant")
                {
                    listSpMau.Add(("Lẩu Thái hải sản chua cay", 289000, 120000, "Món Lẩu & Nướng"));
                    listSpMau.Add(("Bò nướng tảng sốt tiêu xanh", 199000, 80000, "Món ăn chính"));
                    listSpMau.Add(("Gỏi ngó sen tôm thịt", 85000, 30000, "Món khai vị"));
                    listSpMau.Add(("Cơm chiên hải sản hoàng kim", 65000, 25000, "Món ăn chính"));
                    listSpMau.Add(("Khoai tây chiên bơ tỏi", 35000, 15000, "Món khai vị"));
                    listSpMau.Add(("Nước ngọt Coca-Cola (Lon)", 15000, 6000, "Đồ uống"));
                    listSpMau.Add(("Rau câu dừa tráng miệng", 25000, 8000, "Đồ uống"));
                }
                else if (request.ModelType == "pub")
                {
                    listSpMau.Add(("Bia Tiger tháp 3L", 290000, 180000, "Bia & Nước giải khát"));
                    listSpMau.Add(("Mực lá nướng muối ớt", 180000, 90000, "Hải sản tươi sống"));
                    listSpMau.Add(("Lẩu riêu cua sườn sụn", 350000, 150000, "Món ăn chính"));
                    listSpMau.Add(("Đậu hũ chiên sả ớt giòn", 30000, 12000, "Món ăn kèm"));
                    listSpMau.Add(("Gà rang muối hột", 140000, 60000, "Món ăn chính"));
                    listSpMau.Add(("Đậu phộng rang tỏi ớt", 20000, 6000, "Món ăn kèm"));
                }
                else if (request.ModelType == "fastfood")
                {
                    listSpMau.Add(("Hamburger Bò phô mai đặc biệt", 49000, 20000, "Hamburger & Gà rán"));
                    listSpMau.Add(("Gà rán giòn cay KFC", 35000, 15000, "Hamburger & Gà rán"));
                    listSpMau.Add(("Pizza hải sản viền phô mai", 129000, 60000, "Pizza & Mỳ Ý"));
                    listSpMau.Add(("Mỳ Ý sốt bò bằm cay", 59000, 22000, "Pizza & Mỳ Ý"));
                    listSpMau.Add(("Khoai tây lắc phô mai giòn", 29000, 10000, "Đồ ăn kèm"));
                    listSpMau.Add(("Ly Pepsi tươi lớn", 15000, 4000, "Nước giải khát"));
                }
                else // default is "cafe"
                {
                    listSpMau.Add(("Cà phê phin sữa đá Việt Nam", 26000, 10000, "Cà phê"));
                    listSpMau.Add(("Cà phê đen đá đậm vị", 22000, 8000, "Cà phê"));
                    listSpMau.Add(("Trà đào cam sả thanh mát", 39000, 12000, "Trà hoa quả"));
                    listSpMau.Add(("Trà sữa Ô Long trân châu đen", 42000, 15000, "Trà sữa"));
                    listSpMau.Add(("Trân châu đường đen (Topping)", 8000, 2000, "Topping"));
                    listSpMau.Add(("Bánh flan caramen tráng miệng", 20000, 7000, "Topping"));
                }

                // Cắt bớt danh sách sản phẩm theo ProductCount yêu cầu của người dùng
                var selectedSpList = listSpMau.Take(Math.Min(request.ProductCount, listSpMau.Count)).ToList();

                var mapDanhMuc = new Dictionary<string, DanhMuc>();
                var mapSanPhams = new List<SanPham>();

                if (request.ImportProducts)
                {
                    foreach (var sp in selectedSpList)
                    {
                        if (!mapDanhMuc.ContainsKey(sp.Nhom))
                        {
                            var dm = new DanhMuc { CuaHangId = cuaHangId, TenDanhMuc = sp.Nhom, IsDeleted = false };
                            _context.DanhMucs.Add(dm);
                            await _context.SaveChangesAsync();
                            mapDanhMuc[sp.Nhom] = dm;
                        }

                        var newSp = new SanPham
                        {
                            CuaHangId = cuaHangId,
                            DanhMucId = mapDanhMuc[sp.Nhom].Id,
                            TenSanPham = sp.Ten,
                            GiaBan = sp.GiaBan,
                            TrangThai = true
                        };
                        _context.SanPhams.Add(newSp);
                        mapSanPhams.Add(newSp);
                    }
                    await _context.SaveChangesAsync();
                }

                // 4. KHỞI TẠO GIAO DỊCH MẪU
                if (request.ImportTransactions && mapSanPhams.Any())
                {
                    // A. Tạo phiếu nhập kho mẫu (Định giá vốn và nạp tồn kho)
                    var phieuNhap = new PhieuNhap
                    {
                        CuaHangId = cuaHangId,
                        ChiNhanhId = chiNhanh.Id,
                        NgayNhap = DateTime.Now.AddDays(-3),
                        TrangThai = "Hoàn thành",
                        MaChungTu = $"NH-DEMO-{new Random().Next(100, 999)}",
                        GhiChu = "Nhà CC: Nhà phân phối hàng mẫu | Ghi chú: Nhập kho tự động khi tạo dữ liệu chạy thử"
                    };
                    _context.PhieuNhaps.Add(phieuNhap);
                    await _context.SaveChangesAsync();

                    foreach (var spInfo in selectedSpList)
                    {
                        var dbSp = mapSanPhams.FirstOrDefault(s => s.TenSanPham == spInfo.Ten);
                        if (dbSp != null)
                        {
                            // Chi tiết phiếu nhập
                            _context.ChiTietPhieuNhaps.Add(new ChiTietPhieuNhap
                            {
                                PhieuNhapId = phieuNhap.Id,
                                SanPhamId = dbSp.Id,
                                SoLuong = 100,
                                DonGiaNhap = spInfo.GiaNhap
                            });

                            // Khởi tạo bảng Tồn kho
                            _context.TonKhos.Add(new TonKho
                            {
                                SanPhamId = dbSp.Id,
                                ChiNhanhId = chiNhanh.Id,
                                SoLuong = 100
                            });
                        }
                    }
                    await _context.SaveChangesAsync();

                    // B. Tạo danh sách hóa đơn mẫu rải rác 3 ngày gần đây để vẽ biểu đồ
                    int billCount = Math.Clamp(request.TransactionCount, 1, 50);
                    var rand = new Random();

                    // Lấy bàn mẫu đầu tiên đã tạo để gán vào hóa đơn
                    var tempBan = _context.Bans.Local.FirstOrDefault() 
                                  ?? await _context.Bans.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.CuaHangId == cuaHangId);
                    int sampleBanId = tempBan?.Id ?? 0;

                    for (int i = 0; i < billCount; i++)
                    {
                        // Chọn 1-2 sản phẩm ngẫu nhiên cho hóa đơn này
                        var selectedSp1 = mapSanPhams[rand.Next(mapSanPhams.Count)];
                        var sp1Info = selectedSpList.First(s => s.Ten == selectedSp1.TenSanPham);
                        int qty1 = rand.Next(1, 3);
                        decimal totalVal = selectedSp1.GiaBan * qty1;
                        decimal totalCost = sp1Info.GiaNhap * qty1;
                        var method = rand.Next(0, 2) == 0 ? "Tiền mặt" : "Chuyển khoản";
                        var orderTime = DateTime.Now.AddDays(-i).AddHours(-rand.Next(1, 10));

                        var order = new HoaDon
                        {
                            CuaHangId = cuaHangId,
                            ChiNhanhId = chiNhanh.Id,
                            BanId = sampleBanId,
                            NgayTao = orderTime,
                            NgayThanhToan = orderTime,
                            TrangThai = "Đã thanh toán",
                            TongTien = totalVal,
                            PhuongThucThanhToan = method
                        };
                        _context.HoaDons.Add(order);
                        await _context.SaveChangesAsync();

                        // Chi tiết hóa đơn
                        _context.ChiTietHoaDons.Add(new ChiTietHoaDon
                        {
                            HoaDonId = order.Id,
                            SanPhamId = selectedSp1.Id,
                            SoLuong = qty1,
                            DonGia = selectedSp1.GiaBan,
                            GiaVon = sp1Info.GiaNhap,
                            TrangThaiMon = "Đã giao"
                        });

                        // Cập nhật tồn kho (trừ bớt)
                        var tk = await _context.TonKhos.FirstOrDefaultAsync(t => t.SanPhamId == selectedSp1.Id && t.ChiNhanhId == chiNhanh.Id);
                        if (tk != null)
                        {
                            tk.SoLuong = Math.Max(0, tk.SoLuong - qty1);
                        }

                        // Lập phiếu chi trả tiền bán ra trong Sổ quỹ
                        _context.PhieuThuChis.Add(new PhieuThuChi
                        {
                            CuaHangId = cuaHangId,
                            ChiNhanhId = chiNhanh.Id,
                            MaChungTu = $"PT{DateTime.Now.AddDays(-i):ddMM}-{new Random().Next(100, 999)}",
                            LoaiPhieu = "Thu",
                            PhuongThuc = method,
                            NguoiNopNhan = "Khách hàng mẫu",
                            HangMuc = "Doanh thu bán hàng",
                            LyDo = $"Nhận tiền thanh toán hóa đơn HD{order.Id}",
                            GiaTri = (double)totalVal,
                            NgayGiaoDich = orderTime,
                            NguoiTao = "Hệ thống"
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
