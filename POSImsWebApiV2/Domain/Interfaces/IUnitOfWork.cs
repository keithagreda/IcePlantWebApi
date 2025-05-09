﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ISalesHeaderRepository SalesHeader { get; }
        ISalesDetailRepository SalesDetail {  get; }
        IProductCategoryRepository ProductCategory { get; }
        IProductRepository Product { get; }
        IStocksDetailRepository StocksDetail { get; }
        IStocksHeaderRepository StocksHeader { get; }
        IStocksReceivingRepository StocksReceiving { get; }
        IInventoryBeginningRepository InventoryBeginning { get; }
        IInventoryBeginningDetailsRepository InventoryBeginningDetails { get; }
        ICustomerRepository Customer { get; }
        IStorageLocationRepository StorageLocation { get; }
        IRemarksRepository Remarks { get; }
        IProductStocksRepository ProductStocks { get; }
        IEntityHistoryRepository EntityHistory { get; }
        IMachineRepository Machine { get; }
        IMachineProductionRepository MachineProduction { get; }
        IStockReconciliationRepository StockReconciliation { get; }
        ICreditRepository Credit { get; }
        IProductCostRepository ProductCost { get; }
        IProductCostDetailsRepository ProductCostDetail { get; }
        IVoidRequestRepository VoidRequest { get; }
        INotificationRepository Notification { get; }
        IReportRepository Report { get;  }
        IInventoryReconciliationRepository InventoryReconciliation { get;  }
        IReportDetailRepository ReportDetail { get;  }
        IExpenseRepository Expense { get;  }
        IApplicationIdentityUser ApplicationIdentityUser { get; }
        IPrinterLogsRepository PrinterLogs { get;  }

        int Complete();
        Task<int> CompleteAsync();
        IQueryable<T> ExecuteRawQuery<T>(string sql, object parameters = null);
        Task<IQueryable<T>> ExecuteRawQueryAsync<T>(string sql, object parameters = null) where T : class;
    }
}
