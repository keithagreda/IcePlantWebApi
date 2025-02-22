namespace POSIMSWebApi.Application.Dtos.ProductDtos
{
    public class ViewSalesParams : GenericSearchParams
    {
        public bool ThisInventory { get; set; }
        public Guid? SalesHeaderId { get; set; }
    }

}
