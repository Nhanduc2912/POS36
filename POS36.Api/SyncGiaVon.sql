-- Script đồng bộ Giá vốn bình quân gia quyền (MAC) cho dữ liệu cũ
-- Chạy 1 lần sau khi nâng cấp hệ thống
UPDATE nvl
SET nvl.GiaVonHienTai = ISNULL(mac.GiaVonMac, 0)
FROM NguyenVatLieus nvl
LEFT JOIN (
    SELECT 
        ct.NguyenVatLieuId,
        CASE WHEN SUM(ct.SoLuong) > 0 
             THEN ROUND(SUM(ct.SoLuong * ct.DonGiaNhap) / SUM(ct.SoLuong), 2)
             ELSE 0 
        END AS GiaVonMac
    FROM ChiTietPhieuNhaps ct
    GROUP BY ct.NguyenVatLieuId
) mac ON nvl.Id = mac.NguyenVatLieuId;

-- Kiểm tra kết quả
SELECT Id, TenNguyenVatLieu, GiaVonHienTai FROM NguyenVatLieus;
