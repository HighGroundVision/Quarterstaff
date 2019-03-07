var express = require('express');
var router = express.Router();

/* GET home page. */
router.get('/', function(req, res, next) {
  res.render('index', { title: 'Express', username: req.user ? req.user.username : "Unknown" });
});

module.exports = router;
