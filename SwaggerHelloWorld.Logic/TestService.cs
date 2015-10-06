using ServiceStack;

namespace SwaggerHelloWorld.Logic
{
    [Route("/test")]
    public class Test : IReturn<Test>
    {
        public int Id { get; set; }
    }

    public class TestService : Service
    {
        public object Any(Test request)
        {
            return request;
        }
    }
}
