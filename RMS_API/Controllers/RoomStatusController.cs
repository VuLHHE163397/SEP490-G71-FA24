﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RMS_API.Models;

namespace RMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomStatusController : ControllerBase
    {
        private readonly RMS_SEP490Context _context;

        public RoomStatusController(RMS_SEP490Context context)
        {
            _context = context;
        }
        [HttpGet("GetAllStatus")]
        public IActionResult GetAllStatus()
        {
            var sta = _context.RoomStatuses.ToList();
            return Ok(sta);
        }
    }
}