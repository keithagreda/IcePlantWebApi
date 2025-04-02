using POSIMSWebApi.Application.Dtos.Pagination;

namespace POSIMSWebApi.Application.Dtos.ProductDtos
{
    public class GenericSearchParams : PaginationParams
    {
        public string? FilterText { get; set; }
    }

    public class GenericSearchParamsWithDate : GenericSearchParams
    {
        public DateTime? Date { get; set; }
    }

    public class GenericSearchParamsWithDateRange : GenericSearchParams
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
