var express = require('express');
var passport = require('passport');
var router = express.Router();

router.get('/', function(req, res, next) {
  res.send('users');
});

router.get('/login', function(req, res, next) {
  res.render('login');
});
router.post('/login', 
  passport.authenticate('local', {  successRedirect: '/',  failureRedirect: '/users' })
);

router.get('/steam', 
  passport.authenticate('steam')
);
router.get('/steam/callback',
  passport.authenticate('steam', { failureRedirect: '/login' }),
  function(req, res) {
    // Successful authentication, redirect home.
    res.redirect('/');
  }
);

router.get('/discord', passport.authenticate('discord'));
router.get('/discord/callback', passport.authenticate('discord', {
    failureRedirect: '/'
}), function(req, res) {
    // Successful authentication, redirect home.
    res.redirect('/');
});

module.exports = router;
