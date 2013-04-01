
/**
 * Module dependencies.
 */

var express = require('express')
  , routes = require('./routes')
  , http = require('http')
  , path = require('path')
  , httpProxy = require('http-proxy');

var proxy = new httpProxy.RoutingProxy();

function proxyForPattern(pattern){
    return function proxyForPattern(req, res, next) {
        if (req.url.match(pattern))
            proxy.proxyRequest(req, res, {
                host: "localhost",
                port: "8090"
            });
        else
            next();
    }
};

var app = express();

app.configure(function(){
  app.set('port', process.env.PORT || 8080);
  app.set('views', __dirname + '/views');
  app.set('view engine', 'jade');
  app.use(express.favicon());
  app.use(express.logger('dev'));
  app.use(proxyForPattern(/\/api\/.*/));
  app.use(express.bodyParser());
  app.use(express.methodOverride());
  app.use(app.router);
  app.use('/Content', express.static(path.join(__dirname, 'Content')));
  app.use('/Scripts', express.static(path.join(__dirname, 'Scripts')));
});

app.configure('development', function(){
  app.use(express.errorHandler());
});

app.get('/', routes.index);


http.createServer(app).listen(app.get('port'), function(){
  console.log("Express server listening on port " + app.get('port'));
});

