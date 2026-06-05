using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS36.Api.DTOs;
using POS36.Api.Services;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace POS36.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SampleDataController : ControllerBase
    {
        private readonly ISampleDataService _sampleDataService;

        public SampleDataController(ISampleDataService sampleDataService)
        {
            _sampleDataService = sampleDataService;
        }

        private int GetCuaHangId()
        {
            var claim = User.FindFirst("CuaHangId");
            if (claim == null) throw new UnauthorizedAccessException("Token không hợp lệ");
            return int.Parse(claim.Value);
        }

        [HttpGet("check-status")]
        public async Task<IActionResult> CheckStatus()
        {
            try
            {
                int cuaHangId = GetCuaHangId();
                bool hasData = await _sampleDataService.HasExistingDataAsync(cuaHangId);
                return Ok(new { hasData });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi kiểm tra trạng thái cửa hàng: {ex.Message}");
            }
        }

        [HttpPost("generate")]
        [Authorize(Roles = "ChuCuaHang")]
        public async Task<IActionResult> GenerateSampleData([FromBody] SampleDataRequest request)
        {
            try
            {
                int cuaHangId = GetCuaHangId();
                
                // Kiểm tra xem đã có dữ liệu chưa
                bool hasData = await _sampleDataService.HasExistingDataAsync(cuaHangId);
                if (hasData)
                {
                    return BadRequest(new { message = "Cửa hàng của bạn đã có dữ liệu thực tế. Không thể khởi tạo dữ liệu mẫu đè lên!" });
                }

                bool result = await _sampleDataService.InitializeSampleDataAsync(cuaHangId, request);
                if (result)
                {
                    return Ok(new { message = "Khởi tạo dữ liệu mẫu thành công!" });
                }

                return BadRequest(new { message = "Khởi tạo dữ liệu mẫu thất bại." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi trong quá trình khởi tạo dữ liệu mẫu: {ex.Message}");
            }
        }
    }
}
