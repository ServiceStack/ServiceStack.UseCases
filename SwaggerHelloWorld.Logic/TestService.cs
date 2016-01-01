using ServiceStack;

namespace SwaggerHelloWorld.Logic
{
    [Route("/test")]
    public class Test : IReturn<Test>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [Route("/testcomplex")]
    public class TestComplex
    {
        public Test Test { get; set; }
    }

    public class TestService : Service
    {
        public object Any(Test request)
        {
            return request;
        }

        public object Any(TestComplex request)
        {
            return request;
        }
    }
}
