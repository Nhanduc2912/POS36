using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS36.Api.Data;
using POS36.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POS36.Api.Controllers
{
    public class TaoPhieuNhapDto
    {
        public int ChiNhanhId { get; set; }
        // ĐÃ XÓA NhaCungCap
        public string GhiChu { get; set; } = string.Empty;
        public string TrangThai { get; set; } = "Đang xử lý";
        public decimal TienThanhToan { get; set; }
        public List<ChiTietNhapDto> ChiTiets { get; set; } = new();
    }

    public class ChiTietNhapDto
    {
        public int SanPhamId { get; set; }
        public int SoLuong { get; set; }
        public decimal GiaNhap { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NhapHangController : ControllerBase
    {
        private readonly AppDbContext _context;
        public NhapHangController(AppDbContext context) { _context = context; }
        private int GetCuaHangId() => int.Parse(User.FindFirst("CuaHangId")?.Value ?? "1");

        // ==========================================
        // 1. LẤY DANH SÁCH PHIẾU NHẬP (FIX LỖI 404)
        // ==========================================
        [HttpGet("danh-sach")]
        public async Task<IActionResult> GetDanhSach([FromQuery] int chiNhanhId, [FromQuery] string? search, [FromQuery] string? startDate, [FromQuery] string? endDate, [FromQuery] string? status)
        {
            int cuaHangId = GetCuaHangId();
            var query = _context.PhieuNhaps
                .Include(p => p.ChiTiets)
                .ThenInclude(c => c.SanPham)
                .Where(p => p.CuaHangId == cuaHangId);

            if (chiNhanhId > 0) query = query.Where(p => p.ChiNhanhId == chiNhanhId);
            if (!string.IsNullOrEmpty(status)) query = query.Where(p => p.TrangThai == status);

            // Lọc theo ngày
            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
                query = query.Where(p => p.NgayNhap >= start.Date);
            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
                query = query.Where(p => p.NgayNhap <= end.Date.AddDays(1).AddTicks(-1)); // Đến hết ngày đó

            var list = await query.OrderByDescending(p => p.NgayNhap).Select(p => new
            {
                p.Id,
                p.MaChungTu,
                p.NgayNhap,
                p.TrangThai,
                p.GhiChu,
                // Tự tính tổng tiền của phiếu
                TongTien = p.ChiTiets.Sum(c => c.SoLuong * c.DonGiaNhap),
                ChiTiets = p.ChiTiets.Select(c => new
                {
                    c.SanPhamId,
                    TenSanPham = c.SanPham != null ? c.SanPham.TenSanPham : "SP đã xóa",
                    c.SoLuong,
                    c.DonGiaNhap
                })
            }).ToListAsync();

            return Ok(list);
        }

        // ==========================================
        // 2. TẠO PHIẾU NHẬP
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> TaoPhieuNhap([FromBody] TaoPhieuNhapDto request)
        {
            int cuaHangId = GetCuaHangId();
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var phieu = new PhieuNhap
                {
                    CuaHangId = cuaHangId,
                    ChiNhanhId = request.ChiNhanhId,
                    GhiChu = request.GhiChu, // Đã xóa phần gài Nhà CC vào ghi chú
                    TrangThai = request.TrangThai,
                    NgayNhap = DateTime.Now,
                    MaChungTu = $"NH{DateTime.Now:ddMM}-{new Random().Next(1000, 9999)}"
                };

                _context.PhieuNhaps.Add(phieu);
                await _context.SaveChangesAsync();

                foreach (var item in request.ChiTiets)
                {
                    _context.ChiTietPhieuNhaps.Add(new ChiTietPhieuNhap
                    {
                        PhieuNhapId = phieu.Id,
                        SanPhamId = item.SanPhamId,
                        SoLuong = item.SoLuong,
                        DonGiaNhap = item.GiaNhap
                    });

                    if (request.TrangThai == "Hoàn thành")
                    {
                        var tonKho = await _context.TonKhos
                            .FirstOrDefaultAsync(t => t.SanPhamId == item.SanPhamId && t.ChiNhanhId == request.ChiNhanhId);

                        if (tonKho == null)
                        {
                            _context.TonKhos.Add(new TonKho
                            {
                                SanPhamId = item.SanPhamId,
                                ChiNhanhId = request.ChiNhanhId,
                                SoLuong = item.SoLuong
                            });
                        }
                        else
                        {
                            tonKho.SoLuong += item.SoLuong;
                        }
                    }
                }

                // Lập phiếu chi nếu có thanh toán tiền (Đã bỏ NguoiNopNhan = NCC)
                if (request.TrangThai == "Hoàn thành" && request.TienThanhToan > 0)
                {
                    _context.PhieuThuChis.Add(new PhieuThuChi
                    {
                        CuaHangId = cuaHangId,
                        ChiNhanhId = request.ChiNhanhId,
                        MaChungTu = $"PC{DateTime.Now:ddMM}-{new Random().Next(1000, 9999)}",
                        LoaiPhieu = "Chi",
                        PhuongThuc = "Tiền mặt",
                        NguoiNopNhan = "Nhà cung cấp lẻ", // Ghi chung chung
                        HangMuc = "Chi trả tiền nhập hàng",
                        LyDo = $"Thanh toán cho phiếu nhập {phieu.MaChungTu}",
                        GiaTri = (double)request.TienThanhToan,
                        NgayGiaoDich = DateTime.Now,
                        NguoiTao = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value ?? "Admin"
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Đã lưu Phiếu Nhập Hàng thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi khi lưu phiếu: " + ex.Message);
            }
        }
    }
}