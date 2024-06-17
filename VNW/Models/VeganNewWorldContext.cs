using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore; //for DbContext
using VNW.Models;
using VNW.ViewModels;

namespace VNW.Models
{
    public class VeganNewWorldContext : DbContext
    {
        public VeganNewWorldContext(DbContextOptions<VeganNewWorldContext> options) : base(options)
        {

        }

        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }

        //::auto created by vs
        public DbSet<VNW.Models.Category> Category { get; set; }

        //::add for end user
        public DbSet<VNW.Models.Customer> Customer { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //::time stamp sample from AI
            modelBuilder.Entity<Order>()
                .Property(o => o.TimeStamp)
                .IsRowVersion() //::key, otherwise DbUpdateConcurrencyException
                .IsConcurrencyToken()
                ;

            //::OD table has compsite PK
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => new {
                    e.OrderId,
                    e.ProductId
                });

                //entity.ToTable("Order Details");

                //entity.HasIndex(e => e.OrderId)
                //    .HasName("OrdersOrder_Details");

                //entity.HasIndex(e => e.ProductId)
                //                .HasName("ProductsOrder_Details");

                //entity.Property(e => e.OrderId).HasColumnName("OrderID");

                //entity.Property(e => e.ProductId).HasColumnName("ProductID");

                //entity.Property(e => e.Quantity).HasDefaultValueSql("((1))");

                //entity.Property(e => e.UnitPrice).HasColumnType("money");

                //entity.HasOne(d => d.Order)
                //                .WithMany(p => p.OrderDetails)
                //                .HasForeignKey(d => d.OrderId)
                //                .OnDelete(DeleteBehavior.ClientSetNull)
                //                .HasConstraintName("FK_Order_Details_Orders");

                //entity.HasOne(d => d.Product)
                //                .WithMany(p => p.OrderDetails)
                //                .HasForeignKey(d => d.ProductId)
                //                .OnDelete(DeleteBehavior.ClientSetNull)
                //                .HasConstraintName("FK_Order_Details_Products");
            });
        }

        public DbSet<VNW.ViewModels.ProductsViewModel> ProductsViewModel { get; set; }

        public DbSet<VNW.ViewModels.OrderViewModel> OrderViewModel { get; set; }
    }


}
