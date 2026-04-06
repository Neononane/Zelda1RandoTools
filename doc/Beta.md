# Z-Tracker v2.1.0 — Release Notes

## What's New at a Glance

| Area | Summary |
|---|---|
| **Co-op Sync** | Two trackers can now stay in sync in real time during a race or co-op run |
| **Self-Hosted Server** | Host your own sync server locally instead of relying on the cloud |
| **Race Mode** | Tournament-friendly mode that locks down sensitive tracker features |
| **Dungeon Map Location Hints** | Subtle icon overlays help you guess where a dungeon room is on the map |
| **Smarter Reminders** | Each reminder type now has its own independent voice and visual toggle |
| **Overworld Quest Switching** | First Quest ↔ Second Quest switching now works live |
| **Custom Marker Tiles** | Two personal-use marker tiles with new pixel-art icons |
| **Non-Shop Item Icons** | Right-click any overworld tile without a normal popup to add an item icon overlay |
| **Options Screen** | Layout overhaul — all columns now visible and consistent |

---

## Feature Details

### Co-op Synchronization *(Headline Feature)*

Two Z-Tracker consoles can now stay synchronized during a session. When one player marks an overworld tile, collects a dungeon item, completes a room, or acquires a triforce piece, the change automatically appears on the other player's tracker.

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

A new **Dungeon Map Hint** option adds faint icon overlays to empty dungeon rooms, giving you a visual cue of the room's real-world position based on known map layouts.

- Toggle on/off from Options at any time
- Adjust opacity with the slider
- Icons only appear on unmarked rooms; they never cover your own markings

---

### Smarter Reminder System

The reminder panel now gives each reminder type its own row and independent controls:

| Reminder | Voice | Visual |
|---|---|---|
| Recorder spots remaining | ✓ | ✓ |
| Power Bracelet spots remaining | ✓ | ✓ |
| Boomstick book | ✓ | ✓ |
| Have magic key | ✓ | ✓ |
| Have ladder | ✓ | ✓ |

**Also fixed:** Switching between First Quest and Second Quest overworlds now immediately resets recorder and Power Bracelet reminder counts to the correct totals for the new quest.

Co-op Settings buttons have been moved to the Other column.

---

### Overworld Quest Switching (FQ ↔ SQ)

Switching quests via the FQ/SQ menu now works correctly in real time:

- Tiles that exist in the new quest immediately become interactive
- Tiles that don't exist in the new quest immediately gray out
- Clicking a grayed-out tile no longer opens the popup selector
- Reminder counts reset immediately on switch

---

### Custom Marker Tiles

Two personal-preference marker tiles are available in the tile selector popup.

- **PP1** — Asterisk shape on magenta background
- **PP2** — Diamond shape on cyan background

**Off by default.** Enable via **Options → Custom marker tiles**.

---

### Non-Shop Item Icons

Right-clicking any overworld tile that has no other right-click action (e.g. a bomb-dropper tile or an always-empty Second Quest tile) now opens a small item icon picker. The chosen icon appears as an overlay on the tile — useful for annotating locations with a meaningful item symbol.

- Right-click in the picker without hovering an option to **clear** a previously set icon
- Off by default; enable via **Options → Non-shop item icons**

---

### Options Screen Overhaul

- Fixed the "Other" column being pushed off-screen by a scrollbar-width bug
- "Non-shop item icons" and "Reverse scroll wheel" now appear on both the startup screen and the main Options popup
- Several labels shortened to prevent column overflow
- Dungeon Map Hint opacity slider tightened
- Random tip no longer overlaps the Settings header
- "Shops before dungeons" default changed to unchecked

---

## Upgrade Notes

Co-op settings (Console ID, Target ID, Function App URL) are saved across sessions. Debug logging and Enable Co-op default to off each launch.

Players upgrading from a prior save file will have existing overworld and dungeon progress loaded correctly.

---

# Version History

## v2.0.31.0 Change Log

### Custom Marker (PersonalPref) Tiles

- **Icons replaced with custom pixel art** — the G/H letters conflicted with Hidden Dungeon Numbers mode (which maps letters A–H to dungeons 1–8). Markers now display a hand-drawn asterisk (✳, magenta background) for PP1 and a diamond (◆, cyan background) for PP2.
- **New "Custom marker tiles" option** — added to advanced options (default **off**). When disabled, the two tiles are hidden entirely from the tile selector grid.

---

## v2.0.30.0 Change Log

### Options Screen

- **Fixed "Other" column disappearing** — the ScrollViewer wrapping the overworld/dungeon column was adding ~17px of scrollbar width, pushing the third column off the 768px canvas.
- **Advanced options hidden on startup screen** — "Non-shop item icons" and "Reverse scroll wheel" now appear on both startup and main Options popup.
- **Label shortening** — "Allow item icons on non-shop tiles" → "Non-shop item icons"; "Reverse scroll wheel (dungeon rooms)" → "Reverse scroll wheel".
- **Dungeon Map Hint opacity slider** — reduced indent and width.
- **Random tip overlap fixed** — tip is now a proper child of the bottom panel.

### Reminders

- **Split reminder rows** — five individual rows, each with its own Voice and Visual toggle.
- **Removed redundant per-item enables section.**
- **Reminder interval slider** moved to its own row.
- **Co-op buttons** moved to Other column.
- **Recorder spot count resets on quest switch.**

### Overworld Quest Switching (FQ ↔ SQ)

- **Live quest switching now works** — tiles update in real time when switching quests.
- **Click blocked on currently-empty quest tiles.**
- **Power Bracelet spot count also resets on quest switch.**

### Custom Marker (PersonalPref) Tiles

- **Icon letters changed** from '1' / '2' to **'G' / 'H'**.
- **Centered in the tile selector grid.**
- **Helper text shortened.**

### Other

- **"Shops before dungeons" default** changed to unchecked.
