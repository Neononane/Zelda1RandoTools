# ZTracker v2.1.0 — Release Notes


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

---

### Self-Hosted Sync Server

**Setup:**
- Open **Co-op Host Settings** from the Options menu
- Set a port number, enable the session, and start the host
- Point your Co-op Client Settings at `http://localhost:{port}` 

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

**These tiles are off by default.** Enable them via **Options → Custom marker tiles** (advanced section, not shown on the startup screen).

---


---

## Upgrade Notes

Co-op settings (Console ID, Target ID, Function App URL, and Enable Co-op) are saved to the settings file and will persist across sessions. Debug logging is intentionally **not** saved — it defaults off each launch.

Players upgrading from a save file on `master` will have their existing overworld and dungeon progress loaded correctly. The new reminder categories default to the same behavior as the previous combined rows.
