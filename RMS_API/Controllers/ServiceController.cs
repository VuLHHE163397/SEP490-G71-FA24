using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RMS_API.Models;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly RMS_SEP490Context _context;

        public ServiceController(RMS_SEP490Context context)
        {
            _context = context;
        }
    }
}
