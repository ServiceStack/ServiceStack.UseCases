# Self-hosted ServiceStack + Node.js demo project

The project demonstrates how you can easily build single page applications (SPA) using [node.js](http://nodejs.org/download/) with Express, Jade templates and geared with ServiceStack as a backend API engine. The application works for Windows and Linuxes. To simplify development node.js used a proxy to route all "/api" requests to SericeStack but for heavy loaded sites you can use [nginx](http://wiki.nginx.org/Main). 

## Instructions

Make sure that [node.js and npm](http://nodejs.org/download/) is installed and path variables are set correctly (`Windows`: install it with `.msi`, `Linux/Mac`: use [install](https://github.com/ServiceStack/ServiceStack.UseCases/blob/master/NodeStackProxy/install) script or read [how-to-install-nodejs](http://howtonode.org/how-to-install-nodejs) )

Both commands should return version:

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

NOTE: Service is actually a Windows Service. So you can also install it with: 

```
installutil <Path of the Service.exe>
```


### Linux & mono

[run](https://github.com/ServiceStack/ServiceStack.UseCases/blob/master/NodeStackProxy/run) script automatically compiles Service.sln file using `xbuild`. Then it tries to shutdown the previous Service process and run it again. When service is up and running, it downloads all required `node-modules` and run node.js app server using port: `8080`.

```
chmod +x ./run
./run
```
