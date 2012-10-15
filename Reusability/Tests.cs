using NUnit.Framework;
using ServiceStack.ServiceClient.Web;
using ServiceStack.Text;

namespace Reusability
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void Can_send_to_GoogleAppEngine_JSON_Python_Service()
        {
            var client = new JsonServiceClient("http://servicestackdemo.appspot.com");
            var receipt = client.Post(new EmailMessage { 
                    To = "demis.bellot@gmail.com",
                    Subject = "Hello from JsonServiceClient",
                    Body = "ServiceStack SMessage",
                });

            receipt.PrintDump();
        }
    }
}