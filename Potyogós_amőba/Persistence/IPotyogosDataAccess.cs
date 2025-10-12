using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Potyogós_amőba.Persistence
{
    public interface IPotyogosDataAccess
    {       
        Task<PotyogosTable> LoadAsync(String path);
        Task SaveAsync(String path, PotyogosTable table, int _playerTimeX, int _playerTimeO);
    }
}
