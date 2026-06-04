using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace shinnzinn2026.Services
{
    public interface IHolidayService
    {
        Task<HashSet<string>> GetHolidaysAsync();
    }
}
