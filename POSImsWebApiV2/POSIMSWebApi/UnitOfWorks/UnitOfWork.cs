﻿using Dapper;
using DataAccess.EFCore.Repositories;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationContext _context;
        public UnitOfWork(ApplicationContext context)
        {
            _context = context;
            SalesHeader = new SalesHeaderRepository(_context);
            SalesDetail = new SalesDetailRepository(_context);
            ProductCategory = new ProductCategoryRepository(_context);
            Product = new ProductRepository(_context);
            StocksDetail = new StocksDetailRepository(_context);
            StocksReceiving = new StocksReceivingRepository(_context);
            StocksHeader= new StocksHeaderRepository(_context);
            InventoryBeginning = new InventoryBeginningRepository(_context);
            InventoryBeginningDetails = new InventoryBeginningDetailsRepository(_context);
            Customer = new CustomerRepository(_context);
            StorageLocation = new StorageLocationRepository(_context);
            Remarks = new RemarksRepository(_context);
            ProductStocks = new ProductStocksRepository(_context);
            EntityHistory = new EntityHistoryRepository(_context);
            Machine = new MachineRepository(_context);
            MachineProduction = new MachineProductionRepository(_context);
            StockReconciliation = new StockReconciliationRepository(_context);
            Credit = new CreditRepository(_context);
            ProductCost = new ProductCostRepository(_context);
            ProductCostDetail = new ProductCostDetailsRepository(_context);
            Report = new ReportRepository(_context);
            ReportDetail = new ReportDetailRepository(_context);
            Expense = new ExpenseRepository(_context);
            VoidRequest = new VoidRequestRepository(_context);
            Notification = new NotificationRepository(_context);
            InventoryReconciliation = new InventoryReconciliationRepository(_context);
            PrinterLogs = new PrinterLogsRepository(_context);
        }
        public ISalesHeaderRepository SalesHeader { get; private set; }
        public ISalesDetailRepository SalesDetail { get; private set; }
        public IProductCategoryRepository ProductCategory { get; private set; }
        public IProductRepository Product { get; private set; }
        public IStocksReceivingRepository StocksReceiving { get; private set; }
        public IStocksDetailRepository StocksDetail { get; private set; }
        public IStocksHeaderRepository StocksHeader { get; private set; }
        public IInventoryBeginningRepository InventoryBeginning { get; private set; }
        public IInventoryBeginningDetailsRepository InventoryBeginningDetails { get; private set; }
        public ICustomerRepository Customer { get; private set; }
        public IStorageLocationRepository StorageLocation { get; private set; }
        public IRemarksRepository Remarks { get; private set; }
        public IProductStocksRepository ProductStocks { get; private set; }
        public IEntityHistoryRepository EntityHistory { get; private set; }
        public IMachineRepository Machine { get; }
        public IMachineProductionRepository MachineProduction { get; private set; }
        public IStockReconciliationRepository StockReconciliation { get; private set; }
        public ICreditRepository Credit { get; private set; }
        public IProductCostRepository ProductCost { get; private set; }
        public IProductCostDetailsRepository ProductCostDetail { get; private set; }
        public IReportRepository Report { get; private set; }
        public IReportDetailRepository ReportDetail { get; private set; }
        public IExpenseRepository Expense { get; private set; }
        public  IVoidRequestRepository VoidRequest { get; private set; }
        public INotificationRepository Notification { get; private set; }
        public IApplicationIdentityUser ApplicationIdentityUser { get; private set; }
        public IInventoryReconciliationRepository InventoryReconciliation { get; private set; }
        public IPrinterLogsRepository PrinterLogs { get; private set; }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public IQueryable<T> ExecuteRawQuery<T>(string sql, object parameters = null)
        {
            using var connection = _context.Database.GetDbConnection();
            return connection.Query<T>(sql, parameters).AsQueryable();
        }

        public async Task<IQueryable<T>> ExecuteRawQueryAsync<T>(string sql, object parameters = null) where T : class
        {
            return await Task.FromResult(_context.Set<T>().FromSqlRaw(sql, parameters).AsQueryable());
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
