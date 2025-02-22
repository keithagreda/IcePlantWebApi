using POSIMSWebApi.Application.Dtos.ProductCostDetail;

namespace POSIMSWebApi.Application.Interfaces
{
    public interface IProductCostDetailService
    {
        Task CreateOrEdit(CreateOrEditProductCostDetailDto input);
        Task CreateBulk(List<CreateOrEditProductCostDetailDto> input);
    }
}