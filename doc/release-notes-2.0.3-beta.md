# ZTracker v2.0.3 Beta — Release Notes

> **Branch:** `2.0.X-Initial---CoopSync`
> **Compared against:** `master`
> **Audience:** Players, race organizers, and tournament staff

---

## What's New at a Glance

| Area | Summary |
|---|---|
| **Co-op Sync** | Two trackers can now stay in sync in real time during a race or co-op run |
| **Self-Hosted Server** | Host your own sync server locally instead of relying on the cloud |
| **Race Mode** | Tournament-friendly mode that locks down sensitive tracker features |
| **Dungeon Map Location Hints** | Subtle icon overlays help you guess where a dungeon room is on the map |
| **Smarter Reminders** | Each reminder type now has its own independent voice and visual toggle |
| **Overworld Quest Switching** | First Quest ↔ Second Quest switching now actually works live |
| **Custom Marker Tiles** | Two personal-use marker tiles with new pixel-art icons |
| **Options Screen** | Layout overhaul — all columns now visible and consistent |

---

## Feature Details

### Co-op Synchronization *(Headline Feature)*

Two ZTracker consoles can now stay synchronized during a session. When one player marks an overworld tile, collects a dungeon item, completes a room, or acquires a triforce piece, the change automatically appears on the other player's tracker.

**What syncs:**
- Triforce pieces
- Dungeon items and box states
- Overworld tile marks
- Dungeon map room states (room type, monster detail, floor drop, completion)
- Dungeon door states
- Starting items and hearts
- Ganon and Zelda completion flags

**Setup:**
1. Open **Co-op Client Settings** from the Options menu
2. Enter the Function App URL (`https://ztrackersync.azurewebsites.net` for the shared cloud server)
3. Assign a **Console ID** for yourself and a **Target ID** matching your co-op partner
4. Check **Enable Co-op Sync** and save

Both consoles must use matching Console/Target IDs pointing at each other. Changes sync automatically during play — no manual refresh needed.

---

### Self-Hosted Sync Server

Don't want to rely on the shared Azure cloud instance? You can now run a local SignalR host directly from ZTracker.

**Setup:**
- Open **Co-op Host Settings** from the Options menu
- Set a port number, enable the session, and start the host
- Point your Co-op Client Settings at `http://localhost:{port}` instead of the Azure URL

This is ideal for LAN events, private leagues, or anyone who wants full control over their infrastructure.

---

### Race Mode

New option that disables features that could give an unfair advantage or expose information during a race. When enabled, the affected features are hidden or restricted.

Enable it from the startup screen or Options menu before beginning a race session.

---

### Dungeon Map Location Hints

Ever look at your dungeon map and wonder where on the overworld it actually sits? A new **Dungeon Map Hint** option adds faint icon overlays to empty dungeon rooms, giving you a visual guess of the room's real-world position based on known map layouts.

- Toggle on/off from Options at any time
- Adjust opacity with the slider — dial it back if you find it distracting
- Works alongside your existing room markings; icons only appear on unmarked rooms

---

### Smarter Reminder System

The reminder panel has been reorganized to give each reminder type its own row and independent controls:

| Reminder | Voice | Visual |
|---|---|---|
| Recorder spots remaining | ✓ | ✓ |
| Power Bracelet spots remaining | ✓ | ✓ |
| Boomstick book | ✓ | ✓ |
| Have magic key | ✓ | ✓ |
| Have ladder | ✓ | ✓ |

Previously, these were grouped into two combined rows with shared toggles, making it impossible to silence one without silencing others.

**Also fixed:** When switching between First Quest and Second Quest overworlds, recorder and Power Bracelet reminder counts now immediately reset to the correct totals for the new quest (FQ has 1 whistle spot; SQ has 10).

Co-op Settings buttons have been moved out of the Reminders column and into the Other column where they make more sense.

---

### Overworld Quest Switching (FQ ↔ SQ)

Switching between First Quest and Second Quest overworlds via the FQ/SQ menu now works correctly. Previously, quest-specific tiles were initialized once at startup and never updated — switching the quest had no visual effect.

**Now:**
- Tiles that exist in the new quest immediately become interactive
- Tiles that don't exist in the new quest immediately gray out
- Clicking on a grayed-out (quest-empty) tile no longer opens the popup selector
- Recorder and Power Bracelet reminder counts reset to the correct totals for the active quest

---

### Custom Marker Tiles

Two personal-preference marker tiles — useful for annotating spots with custom meaning — are now available in the tile selector popup.

- **PP1** — Asterisk shape on magenta background
- **PP2** — Diamond shape on cyan background

The letters that were previously used (G/H) conflicted with Hidden Dungeon Numbers mode (which uses A–H for dungeons 1–8). Both markers now use hand-drawn pixel-art shapes that have no ambiguity.

**These tiles are off by default.** Enable them via **Options → Custom marker tiles** (advanced section, not shown on the startup screen).

---

### Options Screen Overhaul

The startup and in-session Options screens received a layout fix that was causing the "Other" column to get pushed off-screen:

- Removed a hidden scrollbar that was consuming ~17px and overflowing the 768px canvas
- Shortened several long option labels that were forcing the first column too wide
- Tightened the Dungeon Map Hint opacity slider row
- Fixed the random tip text overlapping the "Settings…" header on long tips
- Advanced-only options (like "Alphabetize hint zone list") are now hidden on the startup screen to keep it compact

---

## Known Issues

The following limitations are acknowledged in this release:

- The timeline tracker at the bottom shows incorrect timings for some item acquisitions
- When marking two item shops, the second item only syncs if the first item is the Wood Arrow
- Middle-clicking a dungeon map room syncs to the wrong position on the connected tracker

---

## Upgrade Notes

Co-op settings (Console ID, Target ID, Function App URL, and Enable Co-op) are saved to the settings file and will persist across sessions. Debug logging is intentionally **not** saved — it defaults off each launch.

Players upgrading from a save file on `master` will have their existing overworld and dungeon progress loaded correctly. The new reminder categories default to the same behavior as the previous combined rows.
