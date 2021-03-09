// **********************************
// Article BlazorSpread - BlazorMongo
// By: Harvey Triana
// **********************************
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorMongo.Server.Services;
using BlazorMongo.Shared.Models;
using MongoDB.Driver;

namespace BlazorMongo.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        readonly IMongoService<Book> _ms;

        public BooksController(IMongoService<Book> ms)
        {
            _ms = ms;
        }

        // GET: api/<BooksController>
        [HttpGet]
        public async Task<IEnumerable<Book>> Get()
        {
            return await _ms.GetDocumentsAsync();
        }

        // GET api/<BooksController>/XYZ
        [HttpGet("{id}")]
        public async Task<Book> Get(string id)
        {
            // create filter
            var filter = Builders<Book>.Filter.Eq(x => x.Id, id);
            // service action
            return await _ms.GetDocumentAsync(filter);
        }

        // POST api/<BooksController>
        [HttpPost]
        public async Task<bool> Post([FromBody] Book value)
        {
            return await _ms.AddDocumentAsync(value);
        }

        // PUT api/<BooksController>
        [HttpPut]
        public async Task<bool> Put([FromBody] Book value)
        {
            // create filter
            var filter = Builders<Book>.Filter.Eq(x => x.Id, value.Id);
            // service action 
            return await _ms.UpdateDocumentAsync(filter, value);
        }

        // DELETE api/<BooksController>/XYZ
        [HttpDelete("{id}")]
        public async Task<bool> Delete(string id)
        {
            // create filter
            var filter = Builders<Book>.Filter.Eq(x => x.Id, id);
            // service action
            return await _ms.DeleteDocumentAsync(filter);
        }

        // Custom method
        [HttpGet("SeedData")]
        public async Task<IEnumerable<Book>> GetSeedData()
        {
            return await SeedData.GetDataSample(_ms);
        }
    }
}
