using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amver.Api.Interfaces;
using Amver.Api.Interfaces.Services;
using Amver.Domain.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Amver.Api.Controllers
{
    [Route("[controller]")]
    public class CityController : ControllerBase
    {
        private readonly ICityService _cityService;
        private readonly ICustomRequestCultureProvider _customRequestCultureProvider;
        
        public CityController(ICityService cityService, ICustomRequestCultureProvider customRequestCultureProvider)
        {
            _cityService = cityService ?? throw new ArgumentNullException(nameof(cityService));
            _customRequestCultureProvider = customRequestCultureProvider ?? throw new ArgumentNullException(nameof(customRequestCultureProvider));
        }
        
        [HttpPost("list")]
        public async Task<IEnumerable<CityDto>> GetListAsync([FromBody, BindRequired] NamePartDto namePartDto)
        {
            if (namePartDto == null) throw new ArgumentNullException(nameof(namePartDto));
            
            var currentCulture = _customRequestCultureProvider.DetermineProviderCultureResult(HttpContext);
          throw new NotImplementedException();//  return await _cityService.GetCityListByNamePartAsync(namePartDto, currentCulture);
        }
        [HttpGet("test")]
        public string GetStringTest()
        {
            return "123!Success";
        }
    }
}