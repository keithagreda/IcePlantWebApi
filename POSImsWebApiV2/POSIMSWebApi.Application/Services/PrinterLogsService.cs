using Domain.ApiResponse;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Services
{
    public class PrinterLogsService : IPrinterLogsService
    {
        private readonly IUnitOfWork _unitOfWork;
        public PrinterLogsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> CreatePrinterLogs(string transNum)
        {
            PrinterLogs printerLogs = new PrinterLogs
            {
                Description = transNum
            };

            await _unitOfWork.PrinterLogs.AddAsync(printerLogs);
            await _unitOfWork.CompleteAsync();
            return ApiResponse<bool>.Success(true);
        }

        public async Task<ApiResponse<int>> GetPrinterLogs(string transNum)
        {
            return ApiResponse<int>.Success(await _unitOfWork.PrinterLogs.GetQueryable().Where(e => e.Description == transNum).CountAsync());
        }
    }
}
