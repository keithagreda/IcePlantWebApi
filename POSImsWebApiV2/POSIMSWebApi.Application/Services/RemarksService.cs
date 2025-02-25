using Domain.Entities;
using Domain.Interfaces;
using POSIMSWebApi.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Services
{
    public class RemarksService : IRemarksService
    {
        private readonly IUnitOfWork _unitOfWork;
        public RemarksService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        /// <summary>
        /// A function that creates remarks
        /// doesn't save must save after getting id
        /// </summary>
        /// <param name="remarks"></param>
        /// <param name="transNum"></param>
        /// <returns>Guid</returns>
        public async Task<Guid> CreateRemarks(string remarks, string transNum)
        {
            var entity = new Remarks
            {
                Id = Guid.NewGuid(),
                Description = remarks,
                TransNum = transNum
            };

            await _unitOfWork.Remarks.AddAsync(entity);
            return entity.Id;
        }
    }
}
