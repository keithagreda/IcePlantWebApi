namespace POSIMSWebApi.Application.Interfaces
{
    public interface IRemarksService
    {
        Task<Guid> CreateRemarks(string remarks, string transNum);
    }
}