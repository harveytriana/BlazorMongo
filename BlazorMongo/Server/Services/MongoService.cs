// **********************************
// Article BlazorSpread - BlazorMongo
// By: Harvey Triana
// **********************************
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorMongo.Server.Services
{
    public class MongoService<T> : IMongoService<T>
    {
        static IMongoCollection<T> _documents;

        public MongoService(IMongoCollection<T> documents)
        {
            _documents = documents;
        }

        public async Task<bool> AddDocumentAsync(T document)
        {
            try {
                await _documents.InsertOneAsync(document);
                return true;
            }
            catch {
                return false;
            }
        }

        public async Task<bool> DeleteDocumentAsync(FilterDefinition<T> filter)
        {
            try {
                await _documents.DeleteOneAsync(filter);
                return true;
            }
            catch {
                return false;
            }
        }

        public async Task<T> GetDocumentAsync(FilterDefinition<T> filter)
        {
            try {
                var result = await _documents.FindAsync(filter);
                return result.FirstOrDefault();
            }
            catch {
                return default;
            }
        }

        public async Task<IEnumerable<T>> GetDocumentsAsync(FilterDefinition<T> filter)
        {
            try {
                return (await _documents.FindAsync(filter)).ToList();
            }
            catch {
            }
            return new List<T>();
        }

        public async Task<IEnumerable<T>> GetDocumentsAsync()
        {
            try {
                return (await _documents.FindAsync(b => true)).ToList();
            }
            catch {
            }
            return new List<T>();
        }

        public async Task Insert(List<T> documents)
        {
            try {
                await _documents.InsertManyAsync(documents);
                //
                Console.WriteLine("Success Seed");
            }
            catch (Exception exception) {
                Console.WriteLine($"Exception: {exception.Message}");
            }

        }

        public async Task<bool> UpdateDocumentAsync(FilterDefinition<T> filter, T document)
        {
            try {
                await _documents.ReplaceOneAsync(filter, document);
                return true;
            }
            catch (Exception exception) {
                Console.WriteLine($"Exception: {exception.Message}");
            }
            return false;
        }
    }
}
