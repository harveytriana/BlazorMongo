// **********************************
// Article BlazorSpread - BlazorMongo
// By: Harvey Triana
// **********************************
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorMongo.Client
{
    public static class Utils
    {
        public static async Task<bool> ResponseResult(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.OK) {
                var js = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<bool>(js);
            }
            return false;
        }
    }
}
