using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Amver.Domain.Entities;
using Amver.Domain.Models;
using Amver.EfCli;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LocalizationCityAndCountryTable
{
    public class LocalizationService : ILocalizationService
    {
        private readonly IContextFactory<ApplicationContext> _context;

        public LocalizationService(IContextFactory<ApplicationContext> context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<BaseResult> FillTargetColumn()
        {
            await using var context = _context.CreateContext();
            
            var targetArray = context.Cities.OrderBy(x=>x.Id);
            try
            {
                foreach (var item in targetArray)
                {
                    if (!string.IsNullOrEmpty(item.ruRu)) continue;

                    var targetText = item.Name;

                    var translatedText = await TranslateTargetText(targetText);

                    item.ruRu = translatedText;
                    //for anti block ip
                    await Task.Delay(100);
                }
            }

            catch (Exception e)
            {
                await context.SaveChangesAsync();
                Console.WriteLine(e);
                throw;
            }

            await context.SaveChangesAsync();
            return new BaseResult();
        }

        public async Task<string> TranslateTargetText(string input)
        {
            // Set the language from/to in the url (or pass it into this function)
            const string fromLang = "en";
            const string toLang = "ru";
            var url =
                $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={fromLang}&tl={toLang}&dt=t&q={Uri.EscapeUriString(input)}";
            var httpClient = new HttpClient();
            var result = await httpClient.GetStringAsync(url);

            // Get all json data
            var jsonData = JsonConvert.DeserializeObject<List<dynamic>>(result);

            // Extract just the first array element (This is the only data we are interested in)
            var translationItems = jsonData[0];

            // Translation Data
            var translation = "";

            // Loop through the collection extracting the translated objects
            foreach (object item in translationItems)
            {
                // Convert the item array to IEnumerable
                var translationLineObject = item as IEnumerable;

                // Convert the IEnumerable translationLineObject to a IEnumerator
                if (translationLineObject == null) continue;
                var translationLineString = translationLineObject.GetEnumerator();

                // Get first object in IEnumerator
                translationLineString.MoveNext();

                // Save its value (translated text)
                translation += $" {Convert.ToString(translationLineString.Current)}";
            }

            // Remove first blank character
            if (translation.Length > 1)
            {
                translation = translation.Substring(1);
            }

            ;

            // Return translation
            return translation;
        }
    }
}