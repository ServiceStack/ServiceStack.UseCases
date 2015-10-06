using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ServiceStack;
using ServiceStack.Api.Swagger;
using ServiceStack.Text;
using SwaggerHelloWorld.Logic;
using Container = Funq.Container;

namespace HelloWorld
{
    //Configure AppHost
    public class AppHost : AppHostBase
    {
        public AppHost() : base("Hello ServiceStack", 
            typeof(TestService).Assembly, typeof(HelloService).Assembly) {}

        public override void Configure(Container container)
        {
            JsConfig.EmitCamelCaseNames = true;
            Plugins.Add(new SwaggerFeature());
            //
            // Configure CORS. You can access Swagger services with 
            // 
            Plugins.Add(new CorsFeature("http://petstore.swagger.wordnik.com"));
        }
    }

    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            new AppHost().Init();
        }
    }
    
    //Define Request and Response DTOs
    [Route("/hellotext/{Name}", Summary = "Hello Text Service", Notes = "More description about Hello Text Service.")]
    public class HelloText
    {
        [ApiMember(Name = "Name", Description = "Name Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string Name { get; set; }
    }

    [Route("/helloimage/{Name}", Summary = "Hello Image Service", Notes = "More description about Hello Image Service.")]
    public class HelloImage
    {
        [ApiMember(Name = "Name", Description = "Name Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string Name { get; set; }

        [ApiMember(Name = "Width", ParameterType = "path", DataType = "int", IsRequired = false)]
        public int? Width { get; set; }
        [ApiMember(Name = "Height", ParameterType = "path", DataType = "int", IsRequired = false)]
        public int? Height { get; set; }
        public int? FontSize { get; set; }
        public string Foreground { get; set; }
        public string Background { get; set; }
    }

    [Route("/hello/{Name}", Summary = "Hello Service", Notes = "More description about Hello Service")]
    public class Hello : IReturn<HelloResponse>
    {
        [ApiMember(Name = "Name", Description = "Name Description", ParameterType = "path", DataType = "string", IsRequired = true)]
        public string Name { get; set; }
    }

    public class HelloResponse
    {
        public string Result { get; set; }
    }

    //Implementation
    public class HelloService : Service
    {
        [AddHeader(ContentType = "text/plain")]
        public object Get(HelloText request)
        {
            return "<h1>Hello, {0}!</h1>".Fmt(request.Name);
        }

        [AddHeader(ContentType = "image/png")]
        public object Get(HelloImage request)
        {
            var width = request.Width.GetValueOrDefault(640);
            var height = request.Height.GetValueOrDefault(360);
            var bgColor = request.Background != null ? Color.FromName(request.Background) : Color.ForestGreen;
            var fgColor = request.Foreground != null ? Color.FromName(request.Foreground) : Color.White;

            var image = new Bitmap(width, height);
            using (var g = Graphics.FromImage(image))
            {
                g.Clear(bgColor);

                var drawString = "Hello, {0}!".Fmt(request.Name);
                var drawFont = new Font("Times", request.FontSize.GetValueOrDefault(40));
                var drawBrush = new SolidBrush(fgColor);
                var drawRect = new RectangleF(0, 0, width, height);

                var drawFormat = new StringFormat {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center };

                g.DrawString(drawString, drawFont, drawBrush, drawRect, drawFormat);

                var ms = new MemoryStream();
                image.Save(ms, ImageFormat.Png);
                return ms;
            }
        }

        public object Get(Hello request)
        {
            return new HelloResponse { Result = "Hello, {0}!".Fmt(request.Name) };

            //C# client can call with:
            //var response = client.Get(new Hello { Name = "ServiceStack" });
        }
    }
}