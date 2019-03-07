# Quarterstaff

## Web
- Sign-up
- Email Validation
- Link Steam Account
- Link Discord Account
- Confirm

## Bot
User Commands
- Confirm Attendance: @HGV confirm #event_id
- Remove Attendance: @HGV revoke #event_id
- Resend Invite: @HGV invite #event_id

Admin Commands (Role Based?)
- Schedule Event: @HGV schedule #event_name, #event_date
- Force Start: @HGV start #event_id
- Remove Event: @HGV cancel #event_id
- Get Lobby: @HGV lobby #event_id (should be sent in direct message)
- Kick user: @HGV kick @user#1234 #event_id (remove the user from the event)
- Blacklist user: @HGV bar @user#1234 (can not confirm attendance to events)
- Watch: @HGV watch #event_id (invite the user to the lobby as cameraman)


https://github.com/Arcana/node-dota2#readme
https://www.npmjs.com/package/dota2#dota2clientinvitetolobbysteam_id