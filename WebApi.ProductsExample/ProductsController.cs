using WebApi.ProductsExample;

/* Tutorial example from: 
 * Your First ASP.NET Web API (C#): 
 * http://www.asp.net/web-api/overview/getting-started-with-aspnet-web-api/tutorial-your-first-web-api
 */
namespace HelloWebAPI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    public class ProductsController : ApiController
    {
        Product[] products = new Product[] 
        { 
            new Product { Id = 1, Name = "Tomato Soup", Category = "Groceries", Price = 1 }, 
            new Product { Id = 2, Name = "Yo-yo", Category = "Toys", Price = 3.75M }, 
            new Product { Id = 3, Name = "Hammer", Category = "Hardware", Price = 16.99M } 
        };

        public IEnumerable<Product> GetAllProducts()
        {
            return products;
        }

        public Product GetProductById(int id)
        {
            var product = products.FirstOrDefault((p) => p.Id == id);
            if (product == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return product;
        }

        public IEnumerable<Product> GetProductsByCategory(string category)
        {
            return products.Where(
                (p) => string.Equals(p.Category, category,
                    StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Enhancing your services:
    ///   - Added Get by categoryName
    ///   - Added Find by Price
    /// </summary>
    public class ProductsControllerV2 : ApiController
    {
        Product[] products = new Product[] 
        { 
            new Product { Id = 1, Name = "Tomato Soup", Category = "Groceries", Price = 1 }, 
            new Product { Id = 2, Name = "Yo-yo", Category = "Toys", Price = 3.75M }, 
            new Product { Id = 3, Name = "Hammer", Category = "Hardware", Price = 16.99M } 
        };

        public IEnumerable<Product> GetAllProducts()
        {
            return products;
        }

        public Product GetProductById(int id)
        {
            var product = products.FirstOrDefault((p) => p.Id == id);
            if (product == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return product;
        }

        public Product GetProductByName(string categoryName)
        {
            var product = products.FirstOrDefault((p) => p.Name == categoryName);
            if (product == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return product;
        }

        public IEnumerable<Product> GetProductsByCategory(string category)
        {
            return products.Where(
                (p) => string.Equals(p.Category, category,
                    StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Product> GetProductsByPriceGreaterThan(decimal price)
        {
            return products.Where((p) => p.Price > price);
        }
    }
}




//Stubs to keep the compiler happy since it's not a WebApi project
namespace System.Net.Http {
    public class HttpResponseException : Exception {
        private HttpStatusCode status;
        public HttpResponseException(HttpStatusCode status) {
            this.status = status;
        }
    }
}
namespace System.Web.Http {
    public class ApiController { }
}
