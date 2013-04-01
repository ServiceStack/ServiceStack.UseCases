using System;
using Funq;
using ServiceStack.Configuration;
using ServiceStack.Redis;
using ServiceStack.ServiceInterface;
using ServiceStack.Common.Web;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;

namespace Backbone.Todos
{
    /// <summary>
    /// Define your ServiceStack web service request (i.e. Request DTO).
    /// </summary>
    public class Todo
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public int Order { get; set; }
        public bool Done { get; set; }
    }

    /// <summary>
    /// Create your ServiceStack rest-ful web service implementation. 
    /// </summary>
    public class TodoService : ServiceStack.ServiceInterface.Service
    {
        /// <summary>
        /// Gets or sets the Redis Manager. The built-in IoC used with ServiceStack autowires this property.
        /// </summary>
        public IRedisClientsManager RedisManager { get; set; }

        public object Get(Todo todo)
        {
            //Return a single Todo if the id is provided.
            if (todo.Id != default(long))
            {
                return RedisManager.ExecAs<Todo>(r => r.GetById(todo.Id));
            }

            //Return all Todos items.
            return RedisManager.ExecAs<Todo>(r => r.GetAll());
        }

        /// <summary>
        /// Handles creating and updating the Todo items.
        /// </summary>
        /// <param name="todo">The todo.</param>
        /// <returns></returns>
        public Todo Post(Todo todo)
        {
            RedisManager.ExecAs<Todo>(r =>
            {
                //Get next id for new todo
                if (todo.Id == default(long)) todo.Id = r.GetNextSequence();
                r.Store(todo);
            });
            return todo;
        }

        /// <summary>
        /// Handles creating and updating the Todo items.
        /// </summary>
        /// <param name="todo">The todo.</param>
        /// <returns></returns>
        public Todo Put(Todo todo)
        {
            return Post(todo);
        }

        public object Delete(Todo todo)
        {
            RedisManager.ExecAs<Todo>(r => r.DeleteById(todo.Id));
            return null;
        }
    }

    /// <summary>
    /// Create your ServiceStack web service application with a singleton AppHost.
    /// </summary>  
    public class ToDoAppHost : AppHostHttpListenerBase
    {
        /// <summary>
        /// Initializes a new instance of your ServiceStack application, with the specified name and assembly containing the services.
        /// </summary>
        public ToDoAppHost() : base("Backbone.js TODO", typeof(TodoService).Assembly) { }

        /// <summary>
        /// Configure the container with the necessary routes for your ServiceStack application.
        /// </summary>
        /// <param name="container">The built-in IoC used with ServiceStack.</param>
        public override void Configure(Container container)
        {
            //Configure ServiceStack Json web services to return idiomatic Json camelCase properties.
            JsConfig.EmitCamelCaseNames = true;

            JsConfig.TreatEnumAsInteger = true;

            //Register Redis factory in Funq IoC. 
            var settings = new AppSettings();
            container.Register<IRedisClientsManager>(new BasicRedisClientManager(settings.GetString("Redis.HostPort")));

            //Register user-defined REST Paths
            Routes
              .Add<Todo>("/todos")
              .Add<Todo>("/todos/{Id}");
        }
    }
}