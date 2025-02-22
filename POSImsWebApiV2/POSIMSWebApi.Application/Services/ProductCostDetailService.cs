using Domain.Entities;
using Domain.Interfaces;
using POSIMSWebApi.Application.Dtos.ProductCostDetail;
using POSIMSWebApi.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Services
{
    public class ProductCostDetailService : IProductCostDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductCostDetailService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        /// <summary>
        /// a function that creates or edit a productcostdetail entity
        /// if there is no id
        /// meant to be used for productcost log when a sales is made
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task CreateOrEdit(CreateOrEditProductCostDetailDto input)
        {
            if (input.Id is null)
            {
                await Create(input);
            }
        }

        private async Task Create(CreateOrEditProductCostDetailDto input)
        {
            var toCreate = new ProductCostDetails
            {
                ProductCostId = input.ProductCostId,
                SalesHeaderId = input.SalesHeaderId,
            };

            await _unitOfWork.ProductCostDetail.AddAsync(toCreate);
            await _unitOfWork.CompleteAsync();
            return;
        }

        public async Task CreateBulk(List<CreateOrEditProductCostDetailDto> input)
        {
            var toCreate = new List<ProductCostDetails>();
            foreach(var item in input)
            {
                var res = new ProductCostDetails
                {
                    ProductCostId = item.ProductCostId,
                    SalesHeaderId = item.SalesHeaderId,
                };
                toCreate.Add(res);
            }
            await _unitOfWork.ProductCostDetail.AddRangeAsync(toCreate);
            await _unitOfWork.CompleteAsync();
            return;
        }
    }
}
