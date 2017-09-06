using Microsoft.AspNetCore.Mvc;
using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Repositories;
using Microsoft.EntityFrameworkCore;
using System;

namespace NetCoreBootstrap.Controllers
{
    public class TestingEndpoints : Controller
    {

        
        [HttpGet("{number}")]
        public int Get(int number)
        {
            return number;
        }

        [HttpPost]
        public IActionResult Error()
        {
            Response.StatusCode = 400;
            return Json(new {Error = "Error"});
        }

        [HttpPost]
        public IActionResult DateConvert(int year, int month, int day)
        {
            var Date = new DateTime(year,month,day);
            return Json(new {Date = Date.ToString()});
        }
    }
}
