using SmartSync.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Application.Interfaces
{
    public interface IComodoService : IBaseService<Comodo>
    {
        Task LigarTodosDispositivos(Guid comodoId);
        Task DesligarTodosDispositivos(Guid comodoId);
    }
}
