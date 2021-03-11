// **********************************
// Article BlazorSpread - BlazorMongo
// By: Harvey Triana
// **********************************
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BlazorMongo.Shared.Models
{
    public class Book
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public bool InStock { get; set; }
        public decimal Price { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        public override string ToString() => $"{Name}, {Author}";
    }
}
