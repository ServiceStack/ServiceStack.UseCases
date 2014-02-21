using System.Collections.Generic;
using System.Linq;
using System.Net;
using ServiceStack;

namespace WebApi.ProductsExample
{
    public class FindProducts : IReturn<List<Product>>
    {
        public string Category { get; set; }
        public decimal? PriceGreaterThan { get; set; }
    }

    public class GetProduct : IReturn<Product>
    {
        public int? Id { get; set; }
        public string Name { get; set; }
    }

    public class ProductsService : IService
    {
        readonly Product[] products = new[] 
        { 
            new Product { Id = 1, Name = "Tomato Soup", Category = "Groceries", Price = 1 }, 
            new Product { Id = 2, Name = "Yo-yo", Category = "Toys", Price = 3.75M }, 
            new Product { Id = 3, Name = "Hammer", Category = "Hardware", Price = 16.99M } 
        };

        public object Get(FindProducts request)
        {
            var ret = products.AsQueryable();
            if (request.Category != null)
                ret = ret.Where(x => x.Category == request.Category);
            if (request.PriceGreaterThan.HasValue)
                ret = ret.Where(x => x.Price > request.PriceGreaterThan.Value);            
            return ret;
        }

        public object Get(GetProduct request)
        {
            var product = request.Id.HasValue
                ? products.FirstOrDefault(x => x.Id == request.Id.Value)
                : products.FirstOrDefault(x => x.Name == request.Name);

            if (product == null)
                throw new HttpError(HttpStatusCode.NotFound, "Product does not exist");

            return product;
        }
    }
}