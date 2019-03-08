var fs = require("fs");
var crypto = require("crypto");
var steam = require("steam");
var dota2 = require("dota2");

require('dotenv').config();

var steamClient = new steam.SteamClient();
var steamUser = new steam.SteamUser(steamClient);
var steamFriends = new steam.SteamFriends(steamClient);
var Dota2 = new dota2.Dota2Client(steamClient, true);

// Load in server list if we've saved one before
if (fs.existsSync('servers.json')) {
  steam.servers = JSON.parse(fs.readFileSync('servers.json'));
}

/* Steam logic */
var onSteamLogOn = function onSteamLogOn(logonResp) {
  if (logonResp.eresult == steam.EResult.OK) {
      steamFriends.setPersonaState(steam.EPersonaState.Busy); // to display your steamClient's status as "Online"
      steamFriends.setPersonaName(`${process.env.STEAM_NAME}`); // to change its nickname
      console.log("Logged on.");
      Dota2.launch();
      Dota2.on("ready", function() {
          console.log("Node-dota2 ready.");
      });
      Dota2.on("unready", function onUnready() {
          console.log("Node-dota2 unready.");
      });
      Dota2.on("chatMessage", function(channel, personaName, message) {
          // console.log([channel, personaName, message].join(", "));
      });
      Dota2.on("guildInvite", function(guildId, guildName, inviter) {
          // Dota2.setGuildAccountRole(guildId, 75028261, 3);
      });
      Dota2.on("unhandled", function(kMsg) {
          console.log("UNHANDLED MESSAGE " + dota2._getMessageName(kMsg));
      });
      
      setTimeout(function(){ 
        Dota2.exit();
        steamClient.disconnect();
      }, 5000);
  }
},
onSteamServers = function onSteamServers(servers) {
  console.log("Received servers.");
  fs.writeFile('servers.json', JSON.stringify(servers), (err)=>{
      if (err) {
        if (this.debug) {
          console.log("Error writing ");
        }
      }
      else {
        if (this.debug) {
          console.log("");
        }
      }
  });
},
onSteamLogOff = function onSteamLogOff(eresult) {
  console.log("Logged off from Steam.");
},
onSteamError = function onSteamError(error) {
  console.log("Connection closed by server: "+error);
};

steamUser.on('updateMachineAuth', function(sentry, callback) {
  var hashedSentry = crypto.createHash('sha1').update(sentry.bytes).digest();
  fs.writeFileSync('.sentry', hashedSentry);
  console.log("sentry file saved");
  callback({ sha_file: hashedSentry });
});


// Login, only passing authCode if it exists
var logOnDetails = {
  "account_name": `${process.env.STEAM_USER}`,
  "password": `${process.env.STEAM_PWD}`
};

var steam_guard_code = `${process.env.STEAM_GUARD}`;
if (steam_guard_code) {
  logOnDetails.auth_code = steam_guard_code;
}
var two_factor_code = `${process.env.STEAM_CODE}`;
if (two_factor_code) {
  logOnDetails.two_factor_code = two_factor_code;
}

try {
  var sentry = fs.readFileSync('.sentry');
  if (sentry.length) {
    logOnDetails.sha_sentryfile = sentry;
  }
} catch (beef) {
  console.log("Cannot load the sentry. " + beef);
}

steamClient.on('connected', function() {
  console.log("Steam Connected");
  steamUser.logOn(logOnDetails);
});
steamClient.on('logOnResponse', onSteamLogOn);
steamClient.on('loggedOff', onSteamLogOff);
steamClient.on('error', onSteamError);
steamClient.on('servers', onSteamServers);

// steamClient.connect();
// steamClient.disconnect();

module.exports = steamClient;