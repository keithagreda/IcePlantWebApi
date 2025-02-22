using Domain.ApiResponse;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Application.Dtos.Pagination;
using POSIMSWebApi.Application.Dtos.ProductCost;
using POSIMSWebApi.Application.Dtos.ProductDtos;
using POSIMSWebApi.Authentication;
using POSIMSWebApi.QueryExtensions;

namespace POSIMSWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCostController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductCostController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet("GetAll")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<ActionResult<ApiResponse<PaginatedResult<ProductCostDto>>>> GetAll([FromQuery] GetProductCostInput input)
        {
            var query = _unitOfWork.ProductCost.GetQueryable()
                .Include(e => e.ProductFk)
                .WhereIf(input.IsActive is not null, e => e.IsActive == input.IsActive)
                .WhereIf(!string.IsNullOrWhiteSpace(input.FilterText), e => false || e.ProductFk.Name.Contains(input.FilterText) || e.Name.Contains(input.FilterText))
                .OrderBy(e => e.ProductFk.Name)
                .Select(e => new ProductCostDto
                {
                    Id = e.Id,
                    IsActive = e.IsActive,
                    Name = e.Name,
                    ProductId = e.ProductId,
                    Amount = e.Amount
                });

            var paginated = await query.ToPaginatedResult(input.PageNumber, input.PageSize).ToListAsync();
            var count = await query.CountAsync();

            return ApiResponse<PaginatedResult<ProductCostDto>>.Success(
                new PaginatedResult<ProductCostDto>(paginated, count, (int)input.PageNumber, (int)input.PageSize)
                );
        }

        //[HttpGet("GetProductCosting")]
        //[Authorize(Roles = UserRole.Admin)]
        //public async Task<ActionResult<ApiResponse<PaginatedResult<GetProductCostingDto>>>> GetProductCosting([FromQuery] GenericSearchParams input)
        //{
        //    var query = _unitOfWork.ProductCost.GetQueryable()
        //        .Include(e => e.ProductFk)
        //        .Include(e => e.ProductCostDetails)
        //        .WhereIf(!string.IsNullOrWhiteSpace(input.FilterText), e => false || e.ProductFk.Name.Contains(input.FilterText) || e.Name.Contains(input.FilterText))
        //        .GroupBy(e => e.ProductFk.Name)
        //        .Select(e => new GetProductCostingDto
        //        {
        //            ProductName = e.Key,
        //            ProductCosting = e.GroupBy(e => e.Id).Select(e => e.Select(e => e.ProductCostDetails.))
        //        });

        //    var paginated = await query.ToPaginatedResult(input.PageNumber, input.PageSize).ToListAsync();
        //    var count = await query.CountAsync();

        //    return ApiResponse<PaginatedResult<ProductCostDto>>.Success(
        //        new PaginatedResult<ProductCostDto>(paginated, count, (int)input.PageNumber, (int)input.PageSize)
        //        );
        //}


        /// <summary>
        /// A function that allows a user to create and edit product cost
        /// if edit it sets the previous product cost to inactive and create new product cost
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost("CreateOrEdit")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<ActionResult<ApiResponse<string>>> CreateOrEdit(CreateOrEditProductCostDto input)
        {
            var existing = _unitOfWork.ProductCost.GetQueryable()
                .Where(e => e.Name.Contains(input.Name) && e.ProductId == input.ProductId)
                .WhereIf(input.Id is not null, e => e.Id != input.Id);


            if(await existing.AnyAsync())
            {
                return ApiResponse<string>.Fail("Invalid Action! Product Cost Already Exists");
            }

            if(input.Id is null)
            {
                return Ok(await Create(input));
            }

            return Ok(await Edit(input, await existing.FirstOrDefaultAsync()));
        }

        private async Task<ApiResponse<string>> Create(CreateOrEditProductCostDto input)
        {
            var newProductCost = new ProductCost
            {
                Amount = input.Amount,
                IsActive = true,
                ProductId = input.ProductId,
                Name = input.Name
            };
            await _unitOfWork.ProductCost.AddAsync(newProductCost);
            await _unitOfWork.CompleteAsync();
            return ApiResponse<string>.Success("Successfully created product cost!");
        }

        private async Task<ApiResponse<string>> Edit(CreateOrEditProductCostDto input, ProductCost? pc)
        {
            if(pc is null)
            {
                return ApiResponse<string>.Fail("Error! Product Cost Not Found");
            }
            pc.IsActive = false;
            return await Create(input); 
        }
    }
}
