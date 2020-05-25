using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amver.Domain.Dto;
using Amver.WebApi.Interfaces;
using Amver.WebApi.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Amver.WebApi.Controllers
{
    [Route("[controller]")]
    public class CountryController : ControllerBase
    {
        private readonly ICountryService _countryService;
        private readonly ICustomRequestCultureProvider _customRequestCultureProvider;
        public CountryController(ICountryService countryService, ICustomRequestCultureProvider customRequestCultureProvider)
        {
            _countryService = countryService ?? throw new ArgumentNullException(nameof(countryService));
            _customRequestCultureProvider = customRequestCultureProvider ?? throw new ArgumentNullException(nameof(customRequestCultureProvider));
        }

        [HttpPost("list")]
        public async Task<IEnumerable<CountryDto>> GetListAsync([FromBody, BindRequired] NamePartDto namePartDto)
        {
            if (namePartDto == null) throw new ArgumentNullException(nameof(namePartDto));
            var currentCulture = _customRequestCultureProvider.DetermineProviderCultureResult(HttpContext);
            return await _countryService.GetCountryListByNamePartAsync(namePartDto, currentCulture);
        }
    }
}