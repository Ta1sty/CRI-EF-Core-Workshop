﻿// <auto-generated />
using System;
using EFCoreWorkshop.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EFCoreWorkshop.Migrations.Migrations
{
    [DbContext(typeof(WorkshopContext))]
    [Migration("20230926105616_FixMigration")]
    partial class FixMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("EFCoreWorkshop.Model.Entities.TaskEntity", b =>
                {
                    b.Property<Guid>("TaskId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<TimeSpan>("Length")
                        .HasColumnType("time");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("WorkerId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TaskId");

                    b.HasIndex("WorkerId");

                    b.ToTable("Tasks");
                });

            modelBuilder.Entity("EFCoreWorkshop.Model.Entities.WorkerEntity", b =>
                {
                    b.Property<Guid>("WorkerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Hired")
                        .HasColumnType("datetime2");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("WorkerId");

                    b.ToTable("Worker");
                });

            modelBuilder.Entity("EFCoreWorkshop.Model.Entities.TaskEntity", b =>
                {
                    b.HasOne("EFCoreWorkshop.Model.Entities.WorkerEntity", "WorkerEntity")
                        .WithMany("TaskEntities")
                        .HasForeignKey("WorkerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("WorkerEntity");
                });

            modelBuilder.Entity("EFCoreWorkshop.Model.Entities.WorkerEntity", b =>
                {
                    b.Navigation("TaskEntities");
                });
#pragma warning restore 612, 618
        }
    }
}