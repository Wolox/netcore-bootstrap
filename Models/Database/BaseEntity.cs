using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NetCoreBootstrap.Models.Database
{
    public class BaseEntity
    {
        [Column("id")]
        public int Id { get; set; }


        [Column("createAt")]
        public DateTime CreateAt { get; set; }


        [Column("updateAt")]
        public DateTime UpdateAt { get; set; }
    }
}
