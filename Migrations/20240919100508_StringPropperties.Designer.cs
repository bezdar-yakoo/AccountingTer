﻿// <auto-generated />
using System;
using AccountingTer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AccountingTer.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20240919100508_StringPropperties")]
    partial class StringPropperties
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.33");

            modelBuilder.Entity("AccountingTer.Models.BalanceEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsAdded")
                        .HasColumnType("INTEGER");

                    b.Property<int>("OwnerBalanceId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("OwnerId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Value")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("BalanceEvents");
                });

            modelBuilder.Entity("AccountingTer.Models.Owner", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Balance")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Owners");
                });

            modelBuilder.Entity("AccountingTer.Models.StringProperties", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("StringProperties");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Key = "IdsForBackupDataBase",
                            Value = "475031431"
                        },
                        new
                        {
                            Id = 2,
                            Key = "DailyStatisticHour",
                            Value = "13"
                        },
                        new
                        {
                            Id = 3,
                            Key = "ChatsForStatistic",
                            Value = "475031431"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
