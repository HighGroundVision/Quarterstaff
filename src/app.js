var createError = require('http-errors');
var express = require('express');
var path = require('path');
var cookieParser = require('cookie-parser');
var bodyParser = require('body-parser');
var session = require('express-session');
var logger = require('morgan');
var passport = require('passport')
var LocalStrategy = require('passport-local').Strategy;

// Passport
passport.use(new LocalStrategy(
  function(username, password, done) {
    var user = {id: 1, username: 'rgbknights', email: 'master@rgbknights.com'};
    done(null, user);
  }
));
passport.serializeUser(function(user, done) {
  done(null, user.id);
});
passport.deserializeUser(function(id, done) {
  var user = {id: 1, username: 'rgbknights', email: 'master@rgbknights.com'};
  done(null, user);
});

// Routing
var router = express.Router();
router.get('/', function(req, res, next) {
  res.render('index', { title: 'Express', username: req.user ? req.user.username : "Unknown" });
});
router.get('/users', function(req, res, next) {
  res.send('users');
});
router.get('/users/login', function(req, res, next) {
  res.render('login');
});
router.post('/users/login', passport.authenticate('local', { 
  successRedirect: '/', 
  failureRedirect: '/users'
}));

// Express Web App
var app = express();

// view engine setup
app.set('views', path.join(__dirname, 'views'));
app.set('view engine', 'ejs');

app.use(logger('dev'));
app.use(express.json());
app.use(cookieParser());
app.use(express.static(path.join(__dirname, 'public')));
app.use(bodyParser.urlencoded({ extended: false }));
app.use(session({ secret: 'quarterstaff', resave: false, saveUninitialized: false }));
app.use(passport.initialize());
app.use(passport.session());
app.use('/', router);

// catch 404 and forward to error handler
app.use(function(req, res, next) {
  next(createError(404));
});

// error handler
app.use(function(err, req, res, next) {
  // set locals, only providing error in development
  res.locals.message = err.message;
  res.locals.error = req.app.get('env') === 'development' ? err : {};

  // render the error page
  res.status(err.status || 500);
  res.render('error');
});

module.exports = app;
