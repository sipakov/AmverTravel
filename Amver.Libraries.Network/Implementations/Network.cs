using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Amver.Domain.Models;
using Amver.Libraries.Network.Interfaces;
using Newtonsoft.Json;

namespace Amver.Libraries.Network.Implementations
{
    public class Network : INetwork
    {
        public async Task<(BaseResult baseResult, string response)> LoadDataPostAsync(string url, string serializedObj,
            string bearerToken)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(serializedObj);
            var byteContent = new ByteArrayContent(buffer);

            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = new HttpClient {BaseAddress = new Uri(url)};
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + bearerToken);
            client.DefaultRequestHeaders.Add("Accept-Language", System.Globalization.CultureInfo.CurrentUICulture.Parent.Name);
            var response = await client.PostAsync(client.BaseAddress, byteContent);

            var content = await response.Content.ReadAsStringAsync();
            BaseResult baseResult;
            if (!response.IsSuccessStatusCode)
            {
                if (string.IsNullOrEmpty(content))
                {
                    baseResult = new BaseResult { Result = response.StatusCode.ToString() };
                }
                else
                {
                    baseResult = JsonConvert.DeserializeObject<BaseResult>(content);
                }
                return (baseResult, null);
            }

            baseResult = new BaseResult
            {
                Result = response.StatusCode.ToString()
            };
            return (baseResult, content);
        }

        public async Task<(BaseResult baseResult, string response)> LoadDataGetAsync(string url, string bearerToken)
        {
            var client = new HttpClient {BaseAddress = new Uri(url)};

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + bearerToken);
            client.DefaultRequestHeaders.Add("Accept-Language", System.Globalization.CultureInfo.CurrentUICulture.Parent.Name);

            var response = await client.GetAsync(client.BaseAddress);

            var content = await response.Content.ReadAsStringAsync();
            BaseResult baseResult;
            if (!response.IsSuccessStatusCode)
            {
                if (string.IsNullOrEmpty(content))
                {
                    baseResult = new BaseResult { Result = response.StatusCode.ToString() };
                }
                else
                {
                    baseResult = JsonConvert.DeserializeObject<BaseResult>(content);
                }
                return (baseResult, null);
            }

            baseResult = new BaseResult
            {
                Result = response.StatusCode.ToString()
            };
            return (baseResult, content);
        }
        
        public async Task<(BaseResult baseResult, string response)> LoadFilePostAsync(string url, byte[] stream, string bearerToken)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));
           
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + bearerToken);
            MultipartFormDataContent content1 = new MultipartFormDataContent();
            ByteArrayContent baContent = new ByteArrayContent(stream);
            content1.Add(baContent, "file", "file");
            content1.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");

            //upload MultipartFormDataContent content async and store response in response var
            var response = await client.PostAsync(url, content1);
            var content = await response.Content.ReadAsStringAsync();
            BaseResult baseResult;
            if (!response.IsSuccessStatusCode)
            {
                if (string.IsNullOrEmpty(content))
                {
                    baseResult = new BaseResult { Result = response.StatusCode.ToString() };
                }
                else
                {
                    baseResult = JsonConvert.DeserializeObject<BaseResult>(content);
                }
                return (baseResult, null);
            }

            baseResult = new BaseResult
            {
                Result = response.StatusCode.ToString()
            };
            return (baseResult, content);
        }
    }
}