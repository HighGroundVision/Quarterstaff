var express = require('express');
var passport = require('passport');
var router = express.Router();

router.get('/', function(req, res, next) {
  res.send('users');
});

router.get('/login', function(req, res, next) {
  res.render('login');
});

router.post('/login', passport.authenticate('local', { 
  successRedirect: '/', 
  failureRedirect: '/users'
}));

module.exports = router;
