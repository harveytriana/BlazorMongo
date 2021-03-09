// **********************************
// Article BlazorSpread - BlazorMongo
// By: Harvey Triana
// **********************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics;
using System.Linq;

namespace BlazorMongo.Server.Services
{
    public static class SeedData
    {
        public static async Task<List<T>> GetDataSample<T>(IMongoService<T> ms)
        {
            var file = $"{Startup.PATH}/Data/{typeof(T).Name}_SEED.json";
            if (File.Exists(file)) {
                try {
                    var documents = JsonSerializer.Deserialize<List<T>>(File.ReadAllText(file));
                    // write
                    var data = await ms.GetDocumentsAsync();
                    if (data.ToList().Any() == false) {
                        foreach (T document in documents) {// prevent others
                            await ms.AddDocumentAsync(document);
                        }
                    }
                    return documents;
                }
                catch (Exception exception) {
                    Trace.WriteLine($"Exception: {exception.Message}");
                }
            }
            return new List<T>();
        }
    }
}
