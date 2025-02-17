# Blazor y MongoDB

---

*Una implementación optimizada y genérica para consumir un servicio de datos mongoDB en Blazor.*

MongoDB es una base de datos de documentos que ofrece una gran escalabilidad y flexibilidad. Gran desempeño y alto rendimiento tanto en la nube como localmente.

En este documento muestro la estrategia práctica y genérica para implementar un servicio mongoDB en una aplicación Blazor. Es notable como Blazor fluye y su vez tenemos por resultado un código limpio, potente y escalable. 

Mi propósito no es enseñar mongoDB o Blazor. Asumo que ya conoce lo fundamental de estas tecnologías, y que tiene un servidor mongoDB en su equipo o servicio en la nube.

> *Valga aclarar que este documento no se escribió como un tutorial. Puede estudiar el código en la publicación en GitHub (referencias).*

## El Modelo de Datos

Las clases de datos se escriben en el proyecto *Shared*. Para usar MongoDB debemos agregar la dependencia a *MongoDB.Driver* en este proyecto. La clase `Book` del ejemplo es la siguiente:

```csharp
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
```

En particular, las decoraciones `BsonId` y `JsonRepresentation(BsonType.ObjectId)` son los requisitos para cumplir con la implementación MongoDB. Con esto se genera un identificado único para cada documento.

## El Servidor Blazor

El servidor Blazor se encarga de conectarse al servicio MongoDB y administrar los datos desde una API REST como servicio al cliente Wasm de Blazor, o a cualquier consumidor. En esta implementación, los métodos retornan ya sea un objeto, o `true / false` según la operación; no formalmente `ActionResult`. Se demuestra la lógica robusta que se puede consultar con cliente *Swagger* o *Postman*.

> En esta solución se habilita el servicio Swagger en Blazor, bajo la ruta `/apis`. Así podemos validar fácilmente los controladores de la API de este ejemplo. Por supuesto también puede usar Postman, e ignorar lo referente al uso de Swagrer (lo incluí como ilustración) . 

**Referencias del Servidor**

- MongoDB.Driver
- Swashbuckle.AspNetCore

En la carpeta **Services** se centraliza todas las clases pertinentes para manejar MongoDB. En `appsettings.json` se especifican los parámetros de conexión al servicio MongoDB. La interfaz `IMongoService<T>` muestra el eje central del servicio C# contra MongoDB, y aplica genéricos, lo que le permite usar la especificación para resolver uno o varios tipos en operaciones *CRUD*

```csharp
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
```

> Note que la interfaz usa `FilterDefinition<T> filter` como parámetro en algunas funciones. Se usa la especificación MongoDB lo cual nos da un alcance bastante potente. En el controlador simplifica su uso y lo hace trasparente para el cliente.

#### El inicializador

La clase **MongoInitializer\<T\>** muestra una práctica correcta para crear servicios MongoDB en .NET Core, y generar los objetos de Inyección por dependencias. Note que el nombre del objeto, en el caso del ejemplo *Book*, se corresponde con el nombre de la Colección en MongoDB, es decir, la Colección en la base de datos se llamará *Book*.  Esto simplifica muchas cosas y permite automatizar otras, además de tener un código limpio. En **MongoInitializer\<T\>** se crea la conexión a mongoDB, se crea la base de datos si no existe, y se crea la colección si no existe.

```csharp
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
```

#### El Servicio

La clase **MongoService\<T\>** es el objeto genérico que implementa la interfaz y del cual pueden derivar otros objetos sin repetir lógica.  

```csharp
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
```

#### Parámetros de Conexión

La clase **MongoSettings** es una abstracción de `appsettings.json` en donde se especifican los parámetros de conexión al servicio MongoDB. Note que existen dos conjuntos de esto, _MongoNetWork_ y _MongoAtlas_. Para *MongoAtlas* se usan _User Secrets_. 

```csharp
namespace BlazorMongo.Server.Services
{
    public class MongoSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
```

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "MongoNetWork": { // local
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "BooksStore"
  },
  "MongoAtlas": { 
    "ConnectionString": "<Use User Secret>",
    "DatabaseName": "BooksStore"
  }
}
```

> **Habilitar Swagger en el servidor Blazor**
> 
> Por supuesto, es equivalente a lo que se emplea en una API de ASP.NET Core NET 5.0, puesto que el servidor de Blazor es en sí una aplicación del mismo tipo. Agregamos una referencia Swashbuckle.AspNetCore y escribimos lo correspondiente en Startup.cs. 
>
> 
> ```csharp
> ...
> namespace BlazorMongo.Server
> {
>     public class Startup
>     {
>         ...
>         public void ConfigureServices(IServiceCollection services)
>         {
>             ...
>             // Swagger (by ilustration)
>             services.AddSwaggerGen(c => {
>                 c.SwaggerDoc("v1", new OpenApiInfo { 
>                     Title = "Blazor Mongo API", Version = "v1" 
>                 });
>             });
>         }
> 
>         public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
>         {
>             if (env.IsDevelopment()) {
>                 ...
>                 // Swagger http://localhost:port/apis
>                 app.UseSwagger();
>                 app.UseSwaggerUI(c => {
>                     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Axis.Api v1");
>                     c.RoutePrefix = "apis";
>                 });
>             }
>             ...
>         }
>     }
> }
> ```

#### Controladores API

Finalmente escribimos los controladores API, lo cual es básicamente simple pues inyecta `IMongoService<T>` siendo T el tipo que deseamos hacer el *CRUD*. Para el ejemplo apuntamos al objeto `Book`, creando el controlador `BooksController` en la carpeta Controllers: 

```csharp
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
```

En el controlador se resuelve el asunto de las funciones de la API mongoDB correspondientes. Como algo adicional y opcional, se agrega el método `GetSeedData` que permitirá agregar un consunto de datos de ejemplo en caso de estar vacía la colección.

---

## El Cliente Blazor

El cliente Wasm de Blazor no requiere de ninguna dependencia de MongoDB, solo es un consumidor de la API que le entrega el servidor Blazor. Los _CRUD_ se hacen en el código de componentes. En el ejemplo, consiste en dos Componentes sencillos de Blazor: `Books.razor` y `BookEditor.razor`. 

**Componente Books**

```html
@page "/books"
@inject HttpClient _http
@inject NavigationManager _navigation

<h3>Books Store</h3>
<hr />
<p>A collection of Book objects fetched from a Mongo database</p>
<h5>
    <a href="/BookEditor/"><span class="oi oi-book"></span> Add Book</a>
</h5>
<br />
@if (items == null) {
    <h6>@echo</h6>
}
else {
    <table class="table">
        <thead>
            <tr>
                <td>Title</td>
                <td>Author</td>
                <td>Price</td>
                <td>InStock</td>
                <td></td>
            </tr>
        </thead>
        <tbody>
            @foreach (var i in items) {
                <tr>
                    <td>@i.Name</td>
                    <td>@i.Author</td>
                    <td>@i.Price</td>
                    <td>@i.InStock</td>
                    <td nowrap>
                        <span style="cursor:pointer;color:goldenrod"
                              @onclick="@(e => OnEdit(i.Id))"
                              class="oi oi-pencil"></span>
                        <span style="cursor:pointer;color:salmon"
                              @onclick="@(e => OnDelete(i.Id))"
                              class="oi oi-trash"></span>
                    </td>
                </tr>
            }
        </tbody>
    </table>
    if (items.Count == 0) {
        <h5>
            <a href="#"
               @onclick:preventDefault="true"
               @onclick="@SeedData">
                <i class="oi oi-data-transfer-upload"></i> Add Data Sample
            </a>
        </h5>
    }
}
```

```csharp
@code {
    List<Book> items;
    string echo;

    protected override async Task OnInitializedAsync()
    {
        echo = "Queryinhg database...";
        try {
            items = await _http.GetFromJsonAsync<List<Book>>("api/books");
        }
        catch (Exception exception) {
            echo = "Exception: " + exception.Message;
        }
    }

    void OnCreate()
    {
        _navigation.NavigateTo($"BookEditor/");
    }

    void OnEdit(string id)
    {
        _navigation.NavigateTo($"BookEditor/{id}");
    }

    async Task OnDelete(string id)
    {
        items.Remove(items.Find(x => x.Id == id));
        // update database
        await _http.DeleteAsync($"api/books/{id}");
    }

    async Task SeedData()
    {
        echo = "Seeding database.collection...";
        try {
            items = await _http.GetFromJsonAsync<List<Book>>("api/books/SeedData");
        }
        catch (Exception exception) {
            echo = "Exception: " + exception.Message;
        }
    }
}
```

**Componente BooksEditor**

```html
@page "/BookEditor/{id?}"
@inject NavigationManager _navigation
@inject HttpClient _http

<h3>Books Store</h3>
<hr />
<div spellcheck="false">
    <table class="table table-borderless">
        <tr>
            <td>Title</td>
            <td>
                <input class="form-control" type="text" @bind="item.Name" />
            </td>
        </tr>
        <tr>
            <td>Author</td>
            <td>
                <input class="form-control" type="text" @bind="item.Author" />
            </td>
        </tr>
        <tr>
            <td>Price</td>
            <td>
                <input class="form-control" type="text" @bind="item.Price" />
            </td>
        </tr>
        <tr>
            <td>In Stock</td>
            <td>
                <input type="checkbox" @bind="item.InStock" />
            </td>
        </tr>
        <tr>
            <td></td>
            <td>
                <input class="btn btn-primary"
                       type="submit"
                       style="width:170px;margin:4px;"
                       value="Update" @onclick="Save" />
                <input class="btn btn-danger"
                       type="button"
                       style="width:170px;margin:4px;"
                       value="Cancel" @onclick="Cancel" />
            </td>
        </tr>
    </table>
</div>
<hr />
<p>@echo</p>
```

```csharp
@code {
    [Parameter] public string id { get; set; }

    Book item = new Book();
    string echo;

    protected override async Task OnInitializedAsync()
    {
        if (id != null) {// put
            item = await _http.GetFromJsonAsync<Book>($"api/books/{id}");
        }
    }

    protected async Task Save()
    {
        HttpResponseMessage response;
        if (id == null) {
            item.CreationDate = DateTime.Today;
            response = await _http.PostAsJsonAsync($"api/books", item);
        }
        else {
            response = await _http.PutAsJsonAsync($"api/books", item);
        }
        if (await Utils.ResponseResult(response)) {
            _navigation.NavigateTo("/Books");
        }
        else {
            echo = "The operation was not completed";
        }
    }

    protected void Cancel() => _navigation.NavigateTo("/Books");
}
```

Finalmente, agregamos una opción al menú lateral con `href="books"` y texto `Fetch data MongoDB`

___

### Ejecución

Teniendo en cuanta que tenemos MongoDB instalado, podemos ejecutar el proyecto. La primera vez que entramos al enlace `Fetch data MongoDB`, veremos que hay un enlace para agregar una muestra de datos; o si lo prefiere empezar a crear. 

<img src="file:///C:/_study/Blog/Documents/Screens/mongo_1.png" title="" alt="" data-align="center">

Con la muestra de datos cargada a la base de datos, veremos:

<img src="file:///C:/_study/Blog/Documents/Screens/mongo_2.png" title="" alt="" data-align="center">

Finalmente, al editar o crear, veremos la página:

<img src="file:///C:/_study/Blog/Documents/Screens/mongo_3.png" title="" alt="" data-align="center">

Si se agrego y configuro el herramienta Swagger, podemos invocarla con la siguiente dirección: https://localhost:44325/apis/ (44325 corresponde al puerto de desarrollo) con lo que se obtiene:

<img src="file:///C:/_study/Blog/Documents/Screens/mongo_4.png" title="" alt="" data-align="center">

## Conclusiones

He dispuesto lineamientos de cómo crear una aplicación Blazor con MongoDB. Sin bien no es la estrategia más simple, enmarca una táctica pragmática para edificar un código más reutilizable y sólido en soluciones grandes.

---

#### Referencias

- [Documentación de MongoDB](https://docs.mongodb.com/)

- [Controlador MongoDB para C#](https://docs.mongodb.com/drivers/csharp/)

- [GitHub - harveytriana/BlazorMongo](https://github.com/harveytriana/BlazorMongo)

---

`Licencia MIT. Autor: Harvey Triana. Contacto: admin@blazorspread.net`
