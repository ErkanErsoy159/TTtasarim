using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TTtasarim.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminTestController : ControllerBase
    {
        //Bu route sadece admin rolündeki kullanıcılar içindir
        [HttpGet("admin-only")]
        [Authorize(Roles = "admin")]
        public IActionResult AdminOnlyData()
        {
            return Ok("Bu endpoint sadece admin kullanıcılar içindir.");
        }

        //Bu route herkese açık (sadece test amaçlı)
        [HttpGet("public")]
        public IActionResult PublicData()
        {
            return Ok("Bu endpoint herkes tarafından görülebilir.");
        }
    }
}
