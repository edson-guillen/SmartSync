﻿using SmartSync.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Application.Interfaces
{
    public interface IResidenciaService : IBaseService<Residencia>
    {
        Task LigarTodosDispositivos(Guid residenciaId);
        Task DesligarTodosDispositivos(Guid residenciaId);
    }
}
