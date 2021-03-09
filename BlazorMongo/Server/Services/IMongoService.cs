// **********************************
// Article BlazorSpread - BlazorMongo
// By: Harvey Triana
// **********************************
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorMongo.Server.Services
{
    public interface IMongoService<T>
    {
        Task<IEnumerable<T>> GetDocumentsAsync(FilterDefinition<T> filter);
        Task<IEnumerable<T>> GetDocumentsAsync();
        Task<T> GetDocumentAsync(FilterDefinition<T> filter);
        Task<bool> AddDocumentAsync(T document);
        Task<bool> UpdateDocumentAsync(FilterDefinition<T> filter, T document);
        Task<bool> DeleteDocumentAsync(FilterDefinition<T> filter);
        Task Insert(List<T> documents);
    }
}