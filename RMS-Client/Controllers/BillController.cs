﻿using Microsoft.AspNetCore.Mvc;

namespace RMS_Client.Controllers
{
    public class BillController : Controller
    {
        public IActionResult ListBill()
        {
            return View("~/Views/Bill/ListBill.cshtml");
        }     

    }
}