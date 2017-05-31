using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MyMVCProject.Models.Database;

namespace MyMVCProject.Migrations
{
    [DbContext(typeof(DataBaseContext))]
    partial class DataBaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("MyMVCProject.Models.Database.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id");

                    b.Property<DateTime>("CreateAt")
                        .HasColumnName("createAt");

                    b.Property<string>("Email")
                        .HasColumnName("email");

                    b.Property<string>("FirstName")
                        .HasColumnName("firstname");

                    b.Property<string>("LastName")
                        .HasColumnName("lastname");

                    b.Property<DateTime>("UpdateAt")
                        .HasColumnName("updateAt");

                    b.HasKey("Id");

                    b.ToTable("user");
                });
        }
    }
}
