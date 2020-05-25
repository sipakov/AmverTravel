using System.Threading.Tasks;
using Amver.Domain.Models;

namespace LocalizationCityAndCountryTable
{
    public interface ILocalizationService
    {
        Task<string> TranslateTargetText(string input);
        
        Task<BaseResult> FillTargetColumn();
    }
}