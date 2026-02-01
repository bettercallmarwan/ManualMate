using ManualMate.Domain.Interfaces;
using ManualMate.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManualMate.Application.Interfaces.Repositories
{
    public interface IItemRepository : IGenericRepository<Item, int>
    {
        Task<bool> ItemExists(int id);
    }
}
