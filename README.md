# SteamTools
Steam tools for running the Steam client on a shared computer where multiple users have a Steam account. This is a problem since there can only one Steam client process running on a computer. When a user starts Steam, then locks the computer, the Steam client stays active on that session. When another users logs in to the computer, that user can't start the Steam client because it's still active on another session.

The SteamTools consists of two parts, the SteamStopperService and the SteamLauncher. The SteamStopperService is a webservice that will brutally kill the Steam client process if another user is still logged on and has the Steam client running. The SteamLauncher will only work if the SteamStopperService is running, it sends a request to the SteamStopperService to stop another Steam client process and then launches Steam on the current user credentials.

Installing: 
Build the SteamStopperService. Start a Developer command prompt as administrator, browse to the folder where the SteamStopperService has been build and run "installutil SteamStopperService.exe". 

Build the SteamLauncher, copy the executable to a location that's reachable to all users having a Steam account. Run the SteamLauncher executable, fill in the credentials and start a Steam client.
