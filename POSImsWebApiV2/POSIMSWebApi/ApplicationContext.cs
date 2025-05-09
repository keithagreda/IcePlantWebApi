﻿using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using POSIMSWebApi.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi
{
    public class ApplicationContext : DbContext
    {
        private readonly AuditInterceptor _auditInterceptor;
        private IHttpContextAccessor _httpContextAccessor;

        static ApplicationContext()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
        public ApplicationContext(DbContextOptions<ApplicationContext> options, AuditInterceptor auditInterceptor, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _auditInterceptor = auditInterceptor;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(_auditInterceptor);
            //optionsBuilder.AddInterceptors(_softDeleteInterceptor);
        }
        //public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        //{
        //    var entries = ChangeTracker
        //        .Entries()
        //        .Where(e =>
        //            e.State == EntityState.Modified
        //            || e.State == EntityState.Added
        //            || e.State == EntityState.Deleted
        //        )
        //        .ToList();

        //    foreach (var entry in entries)
        //    {
        //        var history = new EntityHistory
        //        {
        //            EntityName = entry.Entity.GetType().Name,
        //            EntityId = entry
        //                .Properties.First(p => p.Metadata.IsPrimaryKey())
        //                .CurrentValue.ToString(),
        //            Changes = GetChanges(entry),
        //            ChangeTime = DateTime.Now,
        //            Action = entry.State.ToString(),
        //            ChangedBy = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name)
        //        };
        //        EntityHistories.Add(history);
        //    }

        //    return base.SaveChangesAsync();
        //}

        //public override int SaveChanges()
        //{
        //    var entries = ChangeTracker
        //        .Entries()
        //        .Where(e =>
        //            e.State == EntityState.Modified
        //            || e.State == EntityState.Added
        //            || e.State == EntityState.Deleted
        //        )
        //        .ToList();

        //    foreach (var entry in entries)
        //    {
        //        var history = new EntityHistory
        //        {
        //            EntityName = entry.Entity.GetType().Name,
        //            EntityId = entry
        //                .Properties.First(p => p.Metadata.IsPrimaryKey())
        //                .CurrentValue.ToString(),
        //            Changes = GetChanges(entry),
        //            ChangeTime = DateTime.Now,
        //            Action = entry.State.ToString(),
        //            ChangedBy = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name)
        //        };
        //        EntityHistories.Add(history);
        //    }

        //    return base.SaveChanges();
        //}

        //private string GetChanges(EntityEntry modifiedEntity)
        //{
        //    var changes = new StringBuilder();
        //    foreach (var property in modifiedEntity.OriginalValues.Properties)
        //    {
        //        var originalValue = modifiedEntity.OriginalValues[property];
        //        var currentValue = modifiedEntity.CurrentValues[property];
        //        if (Equals(originalValue, currentValue))
        //        {
        //            changes.AppendLine(
        //                string.Format(
        //                    "{0}: \"{1}\"",
        //                    property.Name,
        //                    originalValue
        //                )
        //            );
        //        }
        //        if (!Equals(originalValue, currentValue))
        //        {
        //            changes.AppendLine(
        //                string.Format(
        //                    "{0}: From \"{1}\" to \"{2}\"\\",
        //                    property.Name,
        //                    originalValue,
        //                    currentValue
        //                )
        //            );
        //        }
        //    }
        //    return changes.ToString();
        //}

        public DbSet<SalesHeader> SalesHeaders { get; set; }
        public DbSet<SalesDetail> SalesDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<StocksReceiving> StocksReceivings { get; set; }
        public DbSet<StockDamageHeader> StockDamageHeaders { get; set; }
        public DbSet<StockDamageDetail> StockDamageDetails { get; set; }
        public DbSet<SalesReturn> SalesReturns { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<StocksHeader> StocksHeaders { get; set; }
        public DbSet<StocksDetail> StocksDetails { get; set; }
        public DbSet<InventoryBeginning> InventoryBeginnings { get; set; }
        public DbSet<InventoryBeginningDetails> InventoryBeginningDetails { get; set; }
        public DbSet<ProductStocks> ProductStocks { get; set; }
        public DbSet <EntityHistory> EntityHistories { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<MachineProduction> MachineProductions { get; set; }
        public DbSet<StockReconciliation> StockReconciliations { get; set; }
        public DbSet<Credit> Credits { get; set; }
        public DbSet<ProductCost> ProductCosts { get; set; }
        public DbSet<ProductCostDetails> ProductCostDetails { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportDetail> ReportDetails { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<VoidRequest> VoidRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<InventoryReconciliation> InventoryReconciliations { get; set; }
        public DbSet<PrinterLogs> PrinterLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Check if the entity has an IsDeleted property
                var isDeletedProperty = entityType.FindProperty("IsDeleted");
                if (isDeletedProperty != null && isDeletedProperty.ClrType == typeof(bool))
                {
                    // Get the entity type
                    var parameter = Expression.Parameter(entityType.ClrType, "e");

                    // Create expression: e => e.IsDeleted == false
                    var filter = Expression.Lambda(
                        Expression.Equal(
                            Expression.Property(parameter, "IsDeleted"),
                            Expression.Constant(false)
                        ),
                        parameter
                    );

                    // Apply filter to entity
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }

        }

    }
}
