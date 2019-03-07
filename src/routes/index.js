var express = require('express');
var router = express.Router();

router.get('/', function(req, res, next) {
  var data = { 
    title: 'Express', 
    username: req.user ? req.user.username : "Unknown" 
  };
  res.render('index', data);
});

module.exports = router;
