# Version 2.0.15.5 Beta Guide

The main feature of the Version 2.0.15.5 Beta release of Z-Tracker is the introduction of co-op synchronization between consoles. The intent is for one user to update data within their Z-Tracker console and see that same update propogate to another player's console. Other, smaller features exist as well in this release

You can find the latest copy of the Beta release [here](https://github.com/Neononane/Zelda1RandoTools/blob/2.0.X-Initial---CoopSync/UserCustomAssets/ZTracker_v2.0.15.5Beta.zip)

## New Features in this Beta
* Co-op is enabled between two Z-Tracker consoles (detailed below).
* The addition of a "Race Mode" flag that will disable Overworld routing recommendations, Dungeon routing recommendations, and audio clues during gameplay
* The addition of a "Dungeon Map Location Hint" flag that will place a transparent icon in the Dungeon Maps over any unmarked room. The icon aligns to the image of a key, a bomb, or a rupee which is displayed in the HUD during gameplay. This helps line up where rooms are on the Dungeon Map
* The version button will display the complete version when clicked enabling better tracking of beta releases
* Some minor text updates
* A kitty has been swapped for a catbird

# **What Is the Coop Feature Set?**

This current version synchronizes Triforces, Dungeon Items, Overworld Items, Overworld Locations, Dungeon Maps, and current state of Gannon and Zelda completion. When one user has saved Zelda it will complete the Tracker for both users. 

Co-op synchronization in this version occurs via SignalR for queueing and distribution of messages as well as front end code to provide synchronization, validation, and management as well as a negotiation function that will allow flexibility in determining the SignalR source. 

## What is SignalR?

SignalR is a lightweight tool designed for near real-time communication between servers and clients. In essence we are using this as a way to route messages to the right people at the right time in a way that can begin to scale aggressively for larger-scale Z-Tracker projects.

SignalR can be hosted either in the cloud (there is a free version available in Azure) or it can be hosted on a user's machine. Z-Tracker enables the self-hosting as described below. The document will not detail how to setup a cloud-base Azure SignalR instance but can reevaluae this based on demand.

## **Setup as a Client**

On the main launching screen there is a new button for "Co-op Client Settings". These settings exist for the purpose of connecting to an existing SignalR host, either hosted by the other player or in the cloud. Clicking this button will bring up a modal with various options. 

### Enable Co-Op Sync
 * Checking this box will turn on listening for updates and sending from the console. This box is disabled while Function App Url is empty or has an invalid URL

### Enable Debug Logging
 * Normally when ZTracker runs there is a command prompt window in the background. Synchronization updates will periodically be recorded here. When debug logging is enabled, additional activities will be written to this window as well as the payload bodies shipped from this console. 

### Function App Url
 * This field is a URL representing the base URL of the endpoint hosting the SyncUpdate and Negotiate endpoints. If using a cloud instance it will look similar to **https://mysignalrhost.azurewebsites.net**. If being hosted by another Z-Tracker user it will likely be their IP address and hosted port number similar to **http://8.8.8.8:5000**.

### Negotiate Endpoint
 * The endpoint for the hosted Negotiate endpoint. This should be set to **/api/Negotiate** unless you have a unique hosting of SignalR. The default hosting behavior from Z-Tracker is to use this endpoint. Note: this syntax is case-insensitive.

### SyncUpdate Endpoint
 * The endpoint for the hosted SyncUpdate endpoint. This should be set to **/api/SyncUpdate** unless you have a unique hosting of SignalR. The default hosting behavior from Z-Tracker is to use this endpoint. Note: this syntax is case-insensitive.

### Console ID
 * The ID representing the current console. This is an ID that is used to identify outbound synchronizations and for the target console to listen to. It can be any alphanumeric string under 255 characters and is likely to be the name of the user. Ex: Neononane

### Generate GUID
 * If no Console ID is desired clicking this button will generate a random GUID to represent the name. 

### Target Console ID
 * The Console ID of the target console to listen to updates from and push updates to. This is the value of "Console ID" on your teammate's Z-Tracker setup.

### Save 
 * Click this to commit. No changes will persist or occur until this is clicked. 

## **Setup as a Host**

On the main launching screen there is a new button for "Co-op Host Settings". These settings exist for the purpose of launching a local SignalR host. Clicking this button will bring up a modal with various options. 

### Enable Hosting
 * Checking this box will enable the Launch button and it serves as a two-step requirement to ensure the hosting has been done correctly. This cannot be checked while the Port Number text box contains an invalid port number.

### Port Number
 * This represents the port number to host the local SignalR instance on. This should be a port number that is not blocked by a firewall for incoming and outgoing traffic and should have incoming traffic routed there when accessed.

### Launch
* This launches the SignalR instance locally using the configured port number and automatically connects to the designated SignalR instance in the Co-op Client Settings list. Note: the URL in the Co-op Client Settings should be updated before launching SignalR and the Function URL will likely be "http://localhost:5000" or similar based on chosen port number.

### Stop
* Terminate any running instance of the SignalR host.
   
Once Co-op Settings are updated, begin using the console as expected and synchronization should occur. 

Of note: all Co-op Settings will be saved to the settings file except Debug Logging and Enable Co-op Sync. This means coop will be disabled by default on next launch.

# **Known Issues/Limitations**

* The timeline tracker at the bottom shows incorrect timings for some item acquisitions
* When marking two item shops the second item is only synchronized if the first item is the Wood Arrow
* The middle-click option on a Dungeon Map room may synchronize to the wrong location on the tracker.
* Shutting down Z-Tracker should kill any running local SignalR instance. However if a remote user is connected it may not shut down too and require manually ending the process.

### Should issues be identified please file an Issue in Github, post in the ZTracker channel in Discord, or contact Neononane directly. 

