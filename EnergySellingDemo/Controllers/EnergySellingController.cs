using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnergySellingDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnergySellingController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;

        private readonly ILogger<EnergySellingController> _logger;
        public EnergySellingController(ILogger<EnergySellingController> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public IEnumerable<EnergySellingDTO> Get()
        {
            string cacheKey = "energySellinglist";
            List<EnergySellingDTO> result = new();
            ////Get Value from Cache
            if (_memoryCache.TryGetValue(cacheKey, out List<EnergySellingDTO> energySellinglist))
            {
                result = energySellinglist.ToList();
            }
            return result.ToArray();
        }

        [HttpPost]
        public string Post([FromBody] EnergySellingDTO energySelling)
        {
            string cacheKey = "energySellinglist";
            List<EnergySellingDTO> energySellinglist;

            /*If Date is weekoff day then allow 10 % discount on actual price. */
            if (IsItWeekOffDay(energySelling.Date))
            {
                var tenPercentDiscount = (energySelling.Price * 10) / 100;
                energySelling.Price = (energySelling.Price - tenPercentDiscount);
                energySelling.isDiscount = true;
            }
            else
                energySelling.isDiscount = false;

            var energySellinglist1 = new List<EnergySellingDTO>();
            if (_memoryCache.TryGetValue(cacheKey, out energySellinglist))
                energySellinglist1 = energySellinglist.ToList();

            if (energySellinglist1 != null && energySellinglist1.Count > 0)                
                energySellinglist1.Add(energySelling); //update list
            else                
                energySellinglist1.Add(energySelling); //Add

            energySellinglist = energySellinglist1;

            ////Set Value to Cache
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(30));
            _memoryCache.Set(cacheKey, energySellinglist, cacheEntryOptions);
            return "Inserted Successfully";
        }

        /// <summary>
        /// Check weather the day is weekoff
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private bool IsItWeekOffDay(DateTime date)
        {
            return (date.DayOfWeek == DayOfWeek.Saturday) || (date.DayOfWeek == DayOfWeek.Sunday);
        }
    }
}
