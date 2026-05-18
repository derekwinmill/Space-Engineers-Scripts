MARS COLONY INVENTORY MANAGER - README
======================================

This README explains the intended setup, block naming, LCD interaction, button commands, and major functions for the Mars Colony Inventory Manager programmable block script.

This script is designed as a focused inventory/logistics script. It is not intended to control mining drills, autopilots, defenses, doors, or power systems. Those should remain in separate scripts for modularity.


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


### NEW FEATURE: PRIORITIZE AMMO 
===============================
This script now supports prioritization of specific ammo types for defensive readiness. 

**Setup:**
Use the `[PriorityAmmo]` tag for any inventory blocks that should receive priority ammo supplies. The script will maintain target levels of specified ammo by transferring items to tagged blocks.

**Target Ammo Levels:**
You can define desired amounts in the script under the `PRIORITIZED_AMMO` dictionary. For example:
```csharp
Dictionary<string, double> PRIORITIZED_AMMO = new Dictionary<string, double>()
{
    { "5.56x45mm NATO magazine", 500 },
    { "Missile 200mm", 50 },
    { "Autocannon Magazine", 200 },
};
```

**Priority Workflow:**
1. All ammo inventories will continuously scan for shortfalls relative to PRIORITIZED_AMMO levels.
2. Items will be moved from general inventory to `[PriorityAmmo]` blocks as needed.
3. Dry run mode will report priority ammo actions without physically moving items.

**Usage Tip:**
Place `[PriorityAmmo]` tags on block inventories near drone hangars or weapon areas for efficient resupply.

---

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
