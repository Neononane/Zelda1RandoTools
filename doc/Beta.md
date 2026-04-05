# Version 2.0.31.0 Change Log

## Custom Marker (PersonalPref) Tiles

- **Icons replaced with custom pixel art** — the G/H letters that were used for PP1/PP2 markers conflicted with Hidden Dungeon Numbers mode, which maps letters A–H to dungeons 1–8. The markers now display a hand-drawn asterisk (✳, magenta background) for PP1 and a diamond (◆, cyan background) for PP2.
- **New "Custom marker tiles" option** — added to the advanced options section (hidden on startup screen, default **off**). When enabled, the two custom marker tiles appear in the tile selector popup. When disabled (default), they are hidden entirely.

---

# Version 2.0.30.0 Change Log

## Options Screen

- **Fixed "Other" column disappearing** — the ScrollViewer wrapping the overworld/dungeon column was adding ~17px of scrollbar width at all times, pushing the third column off the 768px canvas. Removed the ScrollViewer; advanced options are now filtered per-context instead.
- **Advanced options hidden on startup screen** — "Non-shop item icons" and "Reverse scroll wheel" now appear on both the startup screen and the main Options popup. "Alphabetize hint zone list" is hidden on startup to keep height in check.
- **Label shortening** — several option labels were shortened to prevent column 1 from overflowing into column 3:
  - "Allow item icons on non-shop tiles" → "Non-shop item icons"
  - "Reverse scroll wheel (dungeon rooms)" → "Reverse scroll wheel" (context moved to tooltip)
- **Dungeon Map Hint opacity slider** — reduced left indent and slider width so the row no longer sets the column-width ceiling.
- **Random tip overlap fixed** — the tip was placed in a zero-height floating Canvas that caused it to visually overlap the "Settings…" header whenever the tip was long. The tip is now a proper child of the bottom panel so overlap is structurally impossible regardless of tip length.

## Reminders

- **Split reminder rows** — "Recorder/PB/Boomstick" and "Have magic key/ladder" were two combined rows. They are now five individual rows, each with its own Voice and Visual toggle:
  - Recorder spots
  - Power Bracelet spots
  - Boomstick book
  - Have magic key
  - Have ladder
- **Removed redundant "Enable reminders for individual items" section** — individual Voice/Visual toggles in each row replace that feature cleanly.
- **Reminder interval slider** moved to its own row below the label.
- **Co-op Client Settings and Co-op Host Settings** buttons moved from the Reminders column to the Other column.
- **Recorder spot count resets on quest switch** — after switching between First Quest and Second Quest overworlds, the reminder now immediately announces the correct spot count for the new quest (FQ: 1 whistle spot; SQ: 10 whistle spots) rather than waiting an extra interval cycle.

## Overworld Quest Switching (FQ ↔ SQ)

- **Live quest switching now works** — switching from First Quest to Second Quest (or back) via the FQ/SQ menu now correctly grays out / un-grays tiles in real time. Previously, tiles that change between quests were initialized with a permanent X icon and no interactive handler, so switching had no visible effect.
- **Right-click / left-click blocked on currently-empty quest tiles** — tiles that are AlwaysEmpty in the active quest no longer open the tile selector popup. The block is dynamic (checked at click time) so it automatically lifts when the quest switches.
- **Power Bracelet spot count also resets on quest switch** alongside the recorder spot count.

## Custom Marker (PersonalPref) Tiles

- **Icon letters changed** from '1' / '2' to **'G' / 'H'** to avoid confusion with dungeon numbers.
- **Centered in the tile selector grid** — PP1 and PP2 are now in the middle of the bottom row rather than left-aligned.
- **Helper text shortened** — "(your own custom marker)" → "(custom marker)".

## Other

- **"Shops before dungeons" default** changed from checked to unchecked.

---

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

# **What Is the Co-op Feature Set?**

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

