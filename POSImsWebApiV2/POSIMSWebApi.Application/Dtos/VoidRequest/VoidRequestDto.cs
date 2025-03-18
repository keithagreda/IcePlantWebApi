using Domain.Entities;
using Domain.Enums;
using POSIMSWebApi.Application.Dtos.ProductDtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Dtos.VoidRequest
{
    public class VoidRequestDto
    {
        public Guid Id { get; set; }
        public VoidRequestStatus Status { get; set; }
        public Guid? SalesHeaderId { get; set; }
        public string? ApproverId { get; set; }
    }

    public class GetVoidRequest : VoidRequestDto
    {
        public string TransNum { get; set; }
        public string ApproverName { get; set; }
        public string RequesterName { get; set; }
        public string DateRequested { get; set; }
        public string StrStatus { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class GetVoidRequestInput : GenericSearchParams
    {

    }
}
