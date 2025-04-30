# Version 2.0.3 Beta Guide

The main feature of the Version 2.0.3 Beta release of ZTracker is the introduction of co-op synchronization between consoles. The intent is for one user to update data within their ZTracker console and see that same update propogate to another player's console. 

Co-op synchronization in this version occurs via Azure SignalR for queueing and distribution of messages as well as an Azure Function in the front end to provide synchronization front-end validation and management as well as a negotiation function that will allow flexibility in determining the Signal R source. 

# **What Is Synchronized**

This current version synchronizes Triforces, Dungeon Items, Overworld Items, Overworld Locations, Dungeon Maps, and current state of Gannon and Zelda completion. When one user has saved Zelda it will complete the Tracker for both users. 


# **Setup**

On the main launching screen there is a new button for "Co-op Settings". Clicking this button will bring up a modal with various options. 

## Enable Co-Op
 * Checking this box will turn on listening for updates and sending from the console. This box is disabled whole Function App Url is empty or has an invalid URL

## Function App Url
 * This field is a URL representing the base URL of the Azure Function App hosting the SyncUpdate and Negotiate endpoints. For beta usage populate this field with **https://ztrackersync.azurewebsites.net**

## SyncUpdate Endpoint
 * The endpoint for the hosted SyncUpdate endpoint. This should be set to **/api/SyncUpdate**. Note: this syntax is case-insensitive.

## Negotiate Endpoint
 * The endpoint for the hosted Negotiate endpoint. This should be set to **/api/Negotiate**. Note: this syntax is case-insensitive.

## Console ID
 * The ID representing the current console. This is an ID that is used to identify outbound synchronizations and for the target console to listen to. 

## Generate GUID
 * If no Console ID is desired clicking this button will generate a GUID to represent the name. 

## Target ID
 * The Console ID of the target console to listen to updates from and push updates to. 

## Save 
 * Click this to commit. No changes will persist or occur until this is clicked. 

## Enable Debug Logging
 * Normally when ZTracker runs there is a command prompt window in the background. Synchronization updates will periodically be recorded here. When debug logging is enabled, additional activities will be written to this window as well as the payload bodies shipped from this console. 

Once Co-op Settings are updated, begin using the console as expected and synchronization should occur. 

Of note: all Co-op Settings will be saved to the settings file except Debug Logging. This means coop being enabled will be the default on next launch.

# **Known Issues/Limitations**

* The timeline tracker at the bottom show incorrect timings for some item acquisitions
* The middle-click option on a Dungeon Map room will synchronize to the wrong place on the connected tracker

### Should issues be identified please file an Issue in Github, post in the ZTracker channel in Discord, or contact Neononane directly. 

