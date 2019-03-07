var passport = require('passport')
var LocalStrategy = require('passport-local').Strategy;
var SteamStrategy = require('passport-steam').Strategy;
var DiscordStrategy = require('passport-discord').Strategy;

var Database = [
  { id: 1, username: 'rgbknights', email: 'master@rgbknights.com', steam: '76561197973295540', discord: '319169880219975680' }
];

passport.use(new LocalStrategy(
  function(username, password, done) {
    var user = Database.find(_ => _.username == username);
    if(user) {
      done(null, user);
    } else {
      done(new Error("Invalid User"));
    }
  }
));

passport.use(new SteamStrategy({
    returnURL: `${process.env.WEB_HOST}/users/steam/callback`,
    realm: `${process.env.WEB_HOST}`,
    apiKey: `${process.env.STEAM_KEY}`
  },
  function(identifier, profile, done) {
    var user = Database.find(_ => _.steam == profile.id);
    if(user) {
      done(null, user);
    } else {
      done(new Error("Invalid User"));
    }
  }
));

passport.use(new DiscordStrategy({
  clientID: `${process.env.DISCORD_ID}`,
  clientSecret: `${process.env.DISCORD_SECRET}`,
  callbackURL: `${process.env.WEB_HOST}/users/discord/callback`,
  scope: ['identify'] // 'email'
},
function(accessToken, refreshToken, profile, done) {
  var user = Database.find(_ => _.discord == profile.id);
  if(user) {
    done(null, user);
  } else {
    done(new Error("Invalid User"));
  }
}));

passport.serializeUser(function(user, done) {
  done(null, user.id);
});

passport.deserializeUser(function(id, done) {
  var user = Database.find(_ => _.id == id);
  if(user) {
    done(null, user);
  } else {
    done(new Error("Invalid User"));
  }
});

module.exports = passport;