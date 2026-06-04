using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace shinnzinn2026.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "HolidaysJson";

        public HolidayService(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        public async Task<HashSet<string>> GetHolidaysAsync()
        {
            if (_cache.TryGetValue(CacheKey, out HashSet<string> cached)) return cached;

            try
            {
                var client = _httpClientFactory.CreateClient();
                var json = await client.GetStringAsync("https://holidays-jp.github.io/api/v1/date.json");
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                var set = dict != null ? new HashSet<string>(dict.Keys) : new HashSet<string>();
                _cache.Set(CacheKey, set, TimeSpan.FromHours(6));
                return set;
            }
            catch
            {
                var fallback = new HashSet<string> { "2026-01-01" };
                _cache.Set(CacheKey, fallback, TimeSpan.FromMinutes(30));
                return fallback;
            }
        }
    }
}
