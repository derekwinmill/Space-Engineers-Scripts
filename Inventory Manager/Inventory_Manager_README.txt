MARS COLONY INVENTORY MANAGER - README
======================================

This README explains the intended setup, block naming, LCD interaction, button commands, and major functions for the Mars Colony Inventory Manager programmable block script.

This script is designed as a focused inventory/logistics script. It is not intended to control mining drills, autopilots, defenses, doors, or power systems. Those should remain in separate scripts or event-controller/timer-block systems.


START SAFELY
============

The script starts with:

DRY_RUN_MODE = true

While dry run is enabled, the script scans, calculates, and reports, but should not move inventory, queue production, clear LCDs, or change lights. Use this mode first.

Once the debug/status LCDs look correct, edit the script and change:

DRY_RUN_MODE = false


ENVIRONMENT MODE
================

At the top of the script is:

SPACE_MODE = false

false = Planetary/Mars priorities
true  = Space/asteroid base priorities

Most functionality is universal. This setting mainly exists so reserve priorities and future alerts can be adapted when the same script is copied to a space base.


RECOMMENDED BASE STORAGE BACKBONE
=================================

For your starter Mars main base using Economy 2 bulk cargo containers:

Ore:          2
Ingots:       2
Components:   3
Armory:       1
Fuel/Ice:     2
Food/Seeds:   1
Overflow:     1
Salvage:      1
Staging:      1

Total:        14 bulk cargo containers

Recommended starter names:

INV - Cargo - Ore 01 [Ore]
INV - Cargo - Ore 02 [Ore]
INV - Cargo - Ingots 01 [Ingot]
INV - Cargo - Ingots 02 [Ingot]
INV - Cargo - Components 01 [Component]
INV - Cargo - Components 02 [Component]
INV - Cargo - Components 03 [Component]
INV - Cargo - Armory 01 [Ammo] [Tool] [Weapons]
INV - Cargo - Fuel 01 [Fuel]
INV - Cargo - Fuel 02 [Fuel]
INV - Cargo - Food 01 [Food]
INV - Cargo - Overflow 01 [Overflow]
SAL - Cargo - Salvage Buffer 01 [Salvage]
BLD - Cargo - Staging 01 [Staged]

Build high ceilings and vertical expansion space above each storage type so another bulk container can be stacked later.


IMPORTANT TAGS
==============

[Ore]             Ore storage
[Ingot]           Ingot storage
[Component]       Component storage
[Ammo]            Ammo storage
[Tool]            Tools
[Weapons]         Player weapons
[Fuel]            Ice, bottles, fuel supplies
[Food]            Food, seeds, farming items
[Export]          Export/trade staging
[Import]          Imported resources
[Reserve]         Emergency protected storage
[Overflow]        Overflow storage
[Quarantine]      Unknown/unhandled item storage
[DoNotTrack]      Completely ignored by script
[NoExport]        Counted for base use but not export
[NoUnload]        Do not unload from this block
[NoDrain]         Do not drain gas tanks
[InputOnly]       Can receive items, not drained
[OutputOnly]      Can be drained, not filled


LCD SETUP
=========

Recommended LCDs:

INV - LCD - Status [InventoryStatus]
INV - LCD - Debug [InventoryDebug]
INV - LCD - Contracts [Contracts]
INV - LCD - Contract Status [ContractStatus]
INV - LCD - Loadout [Loadout]
INV - LCD - Loadout Status [LoadoutStatus]
INV - LCD - Light Legend [InventoryLegend]
MIN - LCD - Refinery Status [RefineryStatus]


CONTRACT LCD
============

Use the [Contracts] LCD to enter acquisition contracts.

Format:

Steel Plate = 5000
Construction Component = 1200
Motor = 300
Computer = 250
Display = 80

Lines beginning with # or // are ignored.

The script will:

1. Read the contract LCD.
2. Check reserves.
3. Queue production for missing items where possible.
4. Load available items into [TradeShip] inventories connected to [TradeDock].
5. Mark contracts ready when all requested items are loaded.
6. Broadcast a ready message if broadcasts are enabled.
7. Turn the inventory status lights green.
8. When the trade ship launches, clear/reset the contract LCD only if contracts were fulfilled and ready.


TRADE DOCK SETUP
================

Recommended connector:

LOG - Connector - Trade Dock [TradeDock]

Recommended ship cargo naming:

LOG-AIR-Trade-01 Cargo 01 [TradeShip]

The script searches for [TradeShip] inventories on the connected construct. Cargo is loaded only after the ship is connected to [TradeDock].


LOADOUT DOCK SETUP
==================

Use the loadout dock when building a new outpost so components and gases are staged into a construction rover/ship without being pulled back into the base.

Recommended connector:

BLD - Connector - Loadout Dock [LoadoutDock]

Recommended loadout ship cargo:

BLD - Cargo - Outpost Builder 01 [LoadoutShip]

Use the [Loadout] LCD:

Steel Plate = 20000
Construction Component = 5000
Interior Plate = 3000
Motor = 1500
Computer = 1000
Large Steel Tube = 600
Small Steel Tube = 1000
Metal Grid = 800
Power Cell = 800
Hydrogen Bottle = 10
Oxygen Bottle = 4

The script treats loadout cargo as assigned inventory. It should not count those items as available for contracts or normal production calculations.


SALVAGE DOCK SETUP
==================

Recommended connector:

SAL - Connector - Salvage Dock [SalvageDock]

When a salvage rover/ship connects, the script unloads allowed items into base storage and can drain connected salvage hydrogen/oxygen tanks by using stockpile behavior.

Use [NoUnload] or [DoNotTrack] on salvage cargo that should not be unloaded.
Use [NoDrain] on gas tanks that should not be drained.


REFINERY ASSIGNMENT
===================

Dedicated refineries can be assigned to ore types with tags:

MIN - Refinery - Iron [Refine:Iron]
MIN - Refinery - Nickel [Refine:Nickel]
MIN - Refinery - Cobalt [Refine:Cobalt]
MIN - Refinery - Silicon [Refine:Silicon]
MIN - Refinery - Magnesium [Refine:Magnesium]
MIN - Refinery - Silver [Refine:Silver]
MIN - Refinery - Gold [Refine:Gold]
MIN - Refinery - Platinum [Refine:Platinum]
MIN - Refinery - Uranium [Refine:Uranium]

The script attempts to load matching ore into the matching refinery.

If ore drops below its configured threshold, the script can:

- Show a low-ore alert
- Update the refinery status LCD
- Turn on ore-specific light groups
- Trigger relay blocks if available

Recommended ore status light groups:

IronOreStatusLights
NickelOreStatusLights
CobaltOreStatusLights
SiliconOreStatusLights
MagnesiumOreStatusLights

Recommended relay tags:

[Relay:Iron]
[Relay:Nickel]
[Relay:Cobalt]
[Relay:Silicon]
[Relay:Magnesium]


STATUS LIGHTS
=============

Create a block group named:

InventoryStatusLights

Put both the inventory room light and main hall inventory indicator light into this group.

The script writes a legend to [InventoryLegend].

Default colors:

Green  = Ready / healthy
Yellow = Producing / sorting
Blue   = Docked / loading
Red    = Blocked / missing materials
Purple = Contracts or loadout paused
White  = Idle / waiting
Off    = Script stopped or no light group


BROADCAST BLOCK
===============

Optional broadcast blocks should include:

[InventoryBroadcast]

This can be used on an antenna, beacon, or LCD/text panel. The script attempts to write the alert/ready text to the block.


BUTTON COMMANDS
===============

Buttons should run the programmable block with one of these arguments:

force sort
pause contracts
resume contracts
pause loadout
resume loadout
reset contracts
clear alerts
status

Recommended buttons:

INV - Button - Force Sort
INV - Button - Pause Contracts
INV - Button - Resume Contracts
INV - Button - Reset Contracts
INV - Button - Clear Alerts
INV - Button - Status Refresh


RESERVES
========

Reserves are configured at the top of the script in the RESERVES dictionary.

The script calculates availability as:

Available = Base inventory - reserve - assigned/staged inventory

The intent is to protect your base first, then fulfill contracts and loadouts with safe surplus.

[Reserve] containers are also protected from normal draining.


PRODUCTION QUOTAS
=================

Production quotas are configured at the top of the script in PRODUCTION_QUOTAS.

The script tries to keep listed components, ammo, tools, weapons, and bottles at or above quota by queueing assembler jobs.

Blueprint names may vary by update or mods. If a specific item does not queue correctly, check the debug LCD and adjust the BlueprintForItem section in the script.


SURPLUS DISASSEMBLY
===================

The DISASSEMBLY_MAX dictionary controls what can be automatically disassembled when too much is in storage.

Only listed items are eligible.

Example:

Radio-communication Component = 200

If you salvage POIs and collect 850 radio components, the script can queue the surplus above 200 for disassembly.

Tools, weapons, ammo, bottles, food, and contract-assigned items should not be automatically disassembled unless deliberately added to the list.


DEBUG LCD
=========

The [InventoryDebug] LCD reports:

- Dry run status
- Space/planetary mode
- Last command
- Last alert
- Number of blocks found
- Trade connector found/connected
- Loadout/transport connector found/connected
- Salvage connector found/connected
- Important LCDs found/missing

This is meant to help diagnose missing tags, bad connector setup, or missing cargo containers.


STARTER WORLD ADVICE
====================

For your new Mars world, it is safe to start with:

- Plumb drill controlled by event controllers and timer blocks
- Basic refineries and assemblers
- Inventory room
- Cargo storage backbone
- Status/debug LCDs
- DRY_RUN_MODE enabled

You do not need trade ships, salvage docks, outpost loadouts, or refinery assignments on day one. You can build those later and enable/use them as the base grows.


KNOWN LIMITATIONS OF THIS STARTER SCRIPT
========================================

This is a first functional framework. Some item display names and blueprint IDs may need adjustment depending on current vanilla definitions, mods, DLC blocks, or future game updates.

Always test in dry-run mode first.

If an item does not sort or assemble correctly, the likely fix is to adjust either:

- ItemDisplayName()
- BlueprintForItem()
- The item name used on an LCD

The script is intentionally modular so it can be upgraded later without rewriting the whole system.
