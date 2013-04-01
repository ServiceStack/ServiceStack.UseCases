# Self-hosted ServiceStack + Node.js demo project

## Instructions

Make sure that [node.js and npm](http://nodejs.org/download/) is installed and path variables are set correctly. Both commands should return version:

```
node --version
v0.10.2

npm --version
1.2.15
```

[Site](https://github.com/ServiceStack/ServiceStack.UseCases/tree/master/NodeStackProxy/Service) has a dependency on [Redis](http://redis.io/download). Make sure that it's installed locally and listening port 6379. If you have Redis on another server then just change it in [app.config](https://github.com/ServiceStack/ServiceStack.UseCases/blob/master/NodeStackProxy/Service/app.config#L5).

### Windows

[run.cmd](https://github.com/ServiceStack/ServiceStack.UseCases/blob/master/NodeStackProxy/run.cmd) builds Service.csproj and executes `Service/bin/Debug/Service.exe` you have to see the following console:

```
Listening: http://*:8090/api/
Press any key to stop program
```

Then script download `node-modules` for node.js and runs the server for port: `8080`. 

That's it, you can see Backbone.JS with Todo SPA application geared with ServiceStack. 

### Linux & mono

