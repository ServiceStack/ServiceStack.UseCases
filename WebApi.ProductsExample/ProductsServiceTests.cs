using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.Service;
using ServiceStack.ServiceClient.Web;
using ServiceStack.Text;

namespace WebApi.ProductsExample
{
    [TestFixture]
    public class ProductsServiceTests
    {
        private const string BaseUri = "http://localhost:1337";

        /// <summary>
        /// Notes:
        ///  - No Code-Gen: Same generic service client used for all operations
        ///  - When Custom Routes are specified it uses those, otherwise falls back to pre-defined routes
        ///  - IRestClient interface implemented by JSON, JSV, Xml, MessagePack, ProtoBuf Service Clients
        ///  - Service returns IEnumerable[Product] but JSON, JSV serializers happily handles List[Product] fine
        /// </summary>
        [Test]
        public void Calls_ProductsService_with_JsonServiceClient()
        {
            IRestClient client = new JsonServiceClient(BaseUri);

            "\nAll Products:".Print();
            client.Get(new FindProducts()).PrintDump();

            List<Product> toyProducts = client.Get(new FindProducts { Category = "Toys" });
            "\nToy Products:".Print();
            toyProducts.PrintDump();

            List<Product> productsOver2Bucks = client.Get(new FindProducts { PriceGreaterThan = 2 });
            "\nProducts over $2:".Print();
            productsOver2Bucks.PrintDump();

            List<Product> hardwareOver2Bucks = client.Get(new FindProducts { PriceGreaterThan = 2, Category = "Hardware" });
            "\nHardware over $2:".Print();
            hardwareOver2Bucks.PrintDump();

            Product product1 = client.Get(new GetProduct { Id = 1 });
            "\nProduct with Id = 1:".Print();
            product1.PrintDump();

            "\nIt's Hammer Time!".Print();
            Product productHammer = client.Get(new GetProduct { Name = "Hammer" });
            productHammer.PrintDump();
        }

        /* Console Output:      

            All Products:
            [
                {
                    Id: 1,
                    Name: Tomato Soup,
                    Category: Groceries,
                    Price: 1
                },
                {
                    Id: 2,
                    Name: Yo-yo,
                    Category: Toys,
                    Price: 3.75
                },
                {
                    Id: 3,
                    Name: Hammer,
                    Category: Hardware,
                    Price: 16.99
                }
            ]
         
            Toy Products:
            [
                {
                    Id: 2,
                    Name: Yo-yo,
                    Category: Toys,
                    Price: 3.75
                }
            ]

            Products over $2:
            [
                {
                    Id: 2,
                    Name: Yo-yo,
                    Category: Toys,
                    Price: 3.75
                },
                {
                    Id: 3,
                    Name: Hammer,
                    Category: Hardware,
                    Price: 16.99
                }
            ]

            Product with Id = 1:
            {
                Id: 1,
                Name: Tomato Soup,
                Category: Groceries,
                Price: 1
            }

            It's Hammer Time!
            {
                Id: 3,
                Name: Hammer,
                Category: Hardware,
                Price: 16.99
            } 
            */

    }
}