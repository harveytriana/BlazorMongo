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

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Author")]
        public string Author { get; set; }

        [BsonElement("InStock")]
        public bool InStock { get; set; }

        [BsonElement("Price")]
        public decimal Price { get; set; }

        [BsonElement("CreationDate")]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        public override string ToString() => $"{Name}, {Author}";
    }
}
