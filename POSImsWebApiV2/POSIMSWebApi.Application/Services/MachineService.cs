using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using POSIMSWebApi.Application.Dtos.Machine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSIMSWebApi.Application.Services
{
    public class MachineService
    {
        private readonly IUnitOfWork _unitOfWork;
        public MachineService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}
