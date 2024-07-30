﻿// <auto-generated />
using System;
using MarketMonitor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MarketMonitor.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("MarketMonitor.Entities.DatacenterEntity", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("Name");

                    b.ToTable("Datacenters");
                });

            modelBuilder.Entity("MarketMonitor.Entities.GameItemEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Icon")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<bool>("Marketable")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("GameItems");
                });

            modelBuilder.Entity("MarketMonitor.Entities.RetainerEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<ulong>("OwnerId")
                        .HasColumnType("bigint unsigned");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Retainers");
                });

            modelBuilder.Entity("MarketMonitor.Entities.TrackedItemEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("DatacenterId")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<int>("ItemId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("LastNotify")
                        .HasColumnType("datetime(6)");

                    b.Property<ulong>("SellerId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int?>("WorldId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DatacenterId");

                    b.HasIndex("ItemId");

                    b.HasIndex("SellerId");

                    b.HasIndex("WorldId");

                    b.ToTable("TrackedItems");
                });

            modelBuilder.Entity("MarketMonitor.Entities.UserEntity", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<ulong>("Id"));

                    b.Property<string>("Datacenter")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<TimeSpan>("NotifyFreq")
                        .HasColumnType("time(6)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("MarketMonitor.Entities.WorldEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("Id");

                    b.ToTable("Worlds");
                });

            modelBuilder.Entity("MarketMonitor.Entities.RetainerEntity", b =>
                {
                    b.HasOne("MarketMonitor.Entities.UserEntity", "Owner")
                        .WithMany("Retainers")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("MarketMonitor.Entities.TrackedItemEntity", b =>
                {
                    b.HasOne("MarketMonitor.Entities.DatacenterEntity", "Datacenter")
                        .WithMany()
                        .HasForeignKey("DatacenterId");

                    b.HasOne("MarketMonitor.Entities.GameItemEntity", "Item")
                        .WithMany()
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MarketMonitor.Entities.UserEntity", "Seller")
                        .WithMany()
                        .HasForeignKey("SellerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MarketMonitor.Entities.WorldEntity", "World")
                        .WithMany()
                        .HasForeignKey("WorldId");

                    b.Navigation("Datacenter");

                    b.Navigation("Item");

                    b.Navigation("Seller");

                    b.Navigation("World");
                });

            modelBuilder.Entity("MarketMonitor.Entities.UserEntity", b =>
                {
                    b.Navigation("Retainers");
                });
#pragma warning restore 612, 618
        }
    }
}
