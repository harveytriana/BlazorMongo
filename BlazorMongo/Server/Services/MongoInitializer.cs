// **********************************
// Article BlazorSpread - BlazorMongo
// By: Harvey Triana
// **********************************
using MongoDB.Driver;
using System.Diagnostics;

namespace BlazorMongo.Server.Services
{
    public static class MongoInitializer
    {
        /// <summary>
        /// Creates a Mongo database and Collection
        /// </summary>
        /// <returns></returns>
        public static MongoService<T> Initialize<T>(MongoSettings settings)
        {
            Trace.WriteLine($"MongoInitializer for {typeof(T).Name}");

            var collectionName = typeof(T).Name;
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            var collection = database.GetCollection<T>(collectionName);

            return new MongoService<T>(collection);
        }
    }
}
