﻿using SmartSync.Domain.Entities;
using SmartSync.Infraestructure.Persistence.Context;
using SmartSync.Infraestructure.Persistence.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSync.Infraestructure.Persistence.Repositories
{
    public class ComodoRepository(SmartSyncDbContext context) : BaseRepository<Comodo>(context), IComodoRepository
    {
    }
}
