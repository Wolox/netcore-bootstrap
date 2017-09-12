using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace NetCoreBootstrap.Models.Database
{
    public class User : IdentityUser 
    {
        public bool IsExternal { get; set; }
    }
}
