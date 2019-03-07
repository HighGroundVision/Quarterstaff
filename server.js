require('dotenv').config()

var utilities = require('./utilities');
var app = require('./src/web/app');
var bot = require('./src/bot/app');
var http = require('http');

var port = utilities.normalizePort(process.env.PORT || '3000');
app.set('port', port);

var server = http.createServer(app);
server.on('error', utilities.onError);
server.on('listening', utilities.onListening);
server.listen(port);

bot.login(`${process.env.DISCORD_TOKEN}`);