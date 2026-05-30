MARS COLONY INVENTORY MANAGER - README
======================================

Personal-use script for Mars colony logistics. Not intended for publishing.
All configuration guidance, behaviour notes, and implementation details are
documented here rather than in inline code comments.

Scope: inventory sorting, refinery management, assembler production, trade
contracts, dock management, and farming support. Autopilot, mining, defense,
doors, and power remain in separate scripts.


SCRIPT OVERVIEW
===============

Runs on Update10. Subsystems are staggered across tick offsets to spread load.

  Full scan + LCD parse   ~2s    ScanBlocks, ScanInventories, ParseContractLCDs,
                                 ParseLoadoutLCD, ParseProductionSettingsLCD
  Irrigation fill         ~3s    FillIrrigationBlocks
  Inventory sorting       ~3s    SortInventories
  Refinery management     ~3s    ManageRefineries
  Salvage dock            ~3s    ManageSalvageDock
  Contract / trade ship   ~3s    ManageContractsAndTradeShip
  Loadout dock            ~3s    ManageLoadoutDock
  Assembler input drain   ~3s    DrainAssemblerInputs
  Production quotas       ~5s    ManageProductionQuotas
  Surplus disassembly     ~5s    ManageSurplusDisassembly
  Reserve monitoring      ~2s    CheckReserves
  LCD + lights update     ~2s    UpdateLCDs, UpdateStatusLights


START SAFELY
============

DRY_RUN_MODE = true  (default)

In dry run the script scans and reports but does not move items, queue
production, clear LCDs, or change lights. Check the [InventoryDebug] LCD
to confirm all blocks and tags are found correctly before going live.

Change to DRY_RUN_MODE = false when ready.

Dry run also prints assembler drain candidates to the PB echo panel so you
can verify the drain logic is targeting the correct blocks first.


ENVIRONMENT MODE
================

SPACE_MODE = false   Planetary / Mars priorities (default)
SPACE_MODE = true    Space / asteroid base priorities

Most logic is universal. This flag adapts reserve priorities when copying
the script to a space base.


FEATURE FLAGS
=============

Set any to false to disable that subsystem without removing code.

ENABLE_INVENTORY_SORTING       Item routing to tagged cargo
ENABLE_RESERVE_MONITORING      Reserve threshold alerts
ENABLE_PRODUCTION_QUOTAS       Auto-queue assembler production
ENABLE_SURPLUS_DISASSEMBLY     Auto-disassemble above ceiling
ENABLE_CONTRACT_SYSTEM         Contract LCD parsing and state
ENABLE_TRADE_SHIP_LOADING      Load cargo to [TradeShip]
ENABLE_LOADOUT_DOCK            Stage cargo to [LoadoutShip]
ENABLE_SALVAGE_DOCK            Unload salvage rovers
ENABLE_REFINERY_ASSIGNMENT     Smart ore loading and cleanup
ENABLE_STATUS_LIGHTS           Color-coded light group
ENABLE_BROADCASTS              Antenna / beacon messages
ENABLE_DEBUG_LCD               Block counts on [InventoryDebug]

FillIrrigationBlocks and DrainAssemblerInputs run with sorting and have
no separate flag. Tag blocks [DoNotTrack] to exclude them individually.


RECOMMENDED BASE STORAGE BACKBONE
==================================

Starter layout using Economy 2 bulk cargo containers:

INV - Cargo - Ore 01 [Ore]
INV - Cargo - Ore 02 [Ore]
INV - Cargo - Ingots 01 [Ingot]
INV - Cargo - Ingots 02 [Ingot]
INV - Cargo - Components 01 [Component]
INV - Cargo - Components 02 [Component]
INV - Cargo - Components 03 [Component]
INV - Cargo - Armory 01 [Ammo][Tool][Weapons]
INV - Cargo - Fuel 01 [Fuel]
INV - Cargo - Fuel 02 [Fuel]
INV - Cargo - Food 01 [Food]
INV - Cargo - Overflow 01 [Overflow]
SAL - Cargo - Salvage Buffer 01 [Salvage]
BLD - Cargo - Staging 01 [Staged]

Note: the script accepts both [Ingot] and [Ingots], [Component] and
[Components], [Ore] and [Ores] — plural variants are matched automatically.
Stick to one convention per world for clarity.

Build vertical expansion space above each storage type for future stacking.


IMPORTANT TAGS
==============

STORAGE ROUTING — place on cargo containers

[Ore]             Ore storage
[Ingot]           Ingot storage
[Component]       Component storage
[Ammo]            Ammo storage
[Tool]            Tools
[Weapons]         Player weapons
[Fuel]            Ice, bottles, fuel supplies
[Food]            Food and seeds — items flow IN only, never drained out
[Irrigation]      Farming / irrigation blocks — filled with food/seeds as
                  first priority before general sorting runs. Never drained.
                  Excluded from base availability counts.
[Export]          Export staging — not counted as available
[Reserve]         Protected storage — never drained by sorting
[Overflow]        Fallback for unrouted items
[Quarantine]      Final fallback if overflow is also full or missing
[Staged]          Build staging — excluded from base availability counts

DOCK AND SHIP

[TradeDock]       Connector — trade ship dock
[TradeShip]       Cargo on trade ship (outbound)
[ContractReturn]  Cargo on trade ship to unload on return (payment / reward).
                  Managed by the trade ship script automatically — see
                  CONTRACT RETURN and TradeShipControlScriptAddFunction.txt.
[SalvageDock]     Connector — salvage rover dock
[Salvage]         Salvage buffer — excluded from base counts
[LoadoutDock]     Connector — outpost loadout dock
[LoadoutShip]     Cargo on loadout rover or ship

BEHAVIOUR MODIFIERS — add to any inventory block

[DoNotTrack]      Ignored entirely — not scanned, not drained, not counted
[NoExport]        Counted for base use, excluded from export availability
[NoUnload]        Salvage dock will not unload this block
[NoDrain]         Gas tanks — stockpile state will not be toggled
[InputOnly]       Receives items but is never drained
[OutputOnly]      Drained but never receives sorted items

REFINERY — place on refinery blocks

[Refine:Fe]       Iron Ore only
[Refine:Ni]       Nickel Ore only
[Refine:Co]       Cobalt Ore only
[Refine:Si]       Silicon Ore only
[Refine:Mg]       Magnesium Ore only
[Refine:Ag]       Silver Ore only
[Refine:Au]       Gold Ore only
[Refine:Pt]       Platinum Ore only
[Refine:U]        Uranium Ore only

Multiple tags on one refinery are supported:
MIN - Refinery - Iron-Nickel [Refine:Fe][Refine:Ni]

No [Refine:] tag = general-purpose, processes whatever ingot is most needed.

ORE RELAY — place on timer blocks or event controllers

[Relay:Fe]  [Relay:Ni]  [Relay:Co]  [Relay:Si]  [Relay:Mg]

Triggered via ApplyAction("TriggerNow") when that ore falls below its low
threshold. 5-minute cooldown between triggers. Wire to alert lights, sounds,
or mining rover dispatch timers.

PRIORITY AMMO

[PriorityAmmo]    Inventory blocks (near turrets / hangars) that receive
                  priority ammo top-up from general stock. Target levels set
                  in the PRIORITIZED_AMMO dictionary in the script.

BROADCAST

[InventoryBroadcast]   Antenna or beacon used for status messages.

MULTI-TAG SUPPORT

All tags are independent Contains checks — any number can coexist on one
block. The script routes items to the container with the most free space
among all containers matching that tag, so fill distributes naturally.

Weapon blocks (turrets, gatling guns, missile launchers, interior turrets)
are never drained regardless of tags — the script excludes all
IMyUserControllableGun blocks from sorting at the type level.


LCD SETUP
=========

Recommended block names:

INV - LCD - Status [InventoryStatus]
INV - LCD - Debug [InventoryDebug]
INV - LCD - Contracts [Contracts]
INV - LCD - Contract Status [ContractStatus]
INV - LCD - Contract History [ContractHistory]
INV - LCD - Loadout [Loadout]
INV - LCD - Loadout Status [LoadoutStatus]
INV - LCD - Light Legend [InventoryLegend]
INV - LCD - Production Settings [ProductionSettings]
MIN - LCD - Refinery Status [RefineryStatus]

[InventoryStatus]     Reserve levels, current status, active alerts.
                      Shows most critically low reserve with LOW or CRITICAL
                      severity. Clears automatically when all reserves recover.
[InventoryDebug]      Block counts, dock detection, storage tag validation,
                      routing warnings. Use during setup and troubleshooting.
                      STORAGE TAG CHECK section shows OK or MISSING for each
                      required tag. ROUTING WARNINGS shows items sent to
                      overflow because their target container was not found.
[Contracts]           Input — default contract (named "Default").
[Contract:Name]       Input — named contract e.g. [Contract:Alpha].
                      Multiple named contract LCDs supported simultaneously.
[ContractStatus]      Per-contract progress bars, quantities, production
                      estimates, deadlines, and notes.
[ContractHistory]     Last 30 completed/departed contracts. Read-only.
[Loadout]             Input — outpost build manifest.
[LoadoutStatus]       Loadout progress and dock state.
[InventoryLegend]     Status light color key.
[ProductionSettings]  Input — runtime quota and disassembly overrides.
[RefineryStatus]      Ore stocks vs low thresholds.


INVENTORY SORTING
=================

SortInventories runs every ~3 seconds and moves items to their correct
tagged containers based on item type. FillIrrigationBlocks runs immediately
before each sort pass to top up farming blocks first.

ITEM TYPE ROUTING

The script uses item TypeId and SubtypeId from the SE API to identify items.
Routing categories and the vanilla items they cover:

[Ore]        All ore subtypes, Stone
[Ingot]      All ingot subtypes, Silicon Wafer, Magnesium Powder
[Fuel]       Hydrogen Bottle, Oxygen Bottle, Ice
[Ammo]       5.56x45mm NATO magazine, 25x184mm NATO magazine, Missile 200mm,
             Autocannon Magazine, Small Autocannon Magazine, Assault Cannon
             Shell, Artillery Shell, Large Railgun Sabot, Small Railgun Sabot,
             S-10 Pistol Magazine, MR-30E Pistol Magazine
[Tool]       All welder, grinder, and hand drill tiers
[Weapons]    All rifle, pistol, and launcher types
[Food]       Organic, Space Wheat, Wheat, Mushroom, Red Beet, Carrot, Tomato,
             Cooked Meat, Raw Meat, Egg, Bread, Flour, all seed variants
[Component]  Steel Plate, Interior Plate, Construction Component, Metal Grid,
             Large Steel Tube, Small Steel Tube, Motor, Computer, Display,
             Medical Component, Radio-communication Component, Power Cell,
             Solar Cell, Bulletproof Glass, Girder, Reactor Component,
             Thrust Component, Explosives, Detector Components, Canvas,
             Gravity Generator Components, Zone Chip

Items not matching any category are routed to [Overflow]. If [Overflow] is
also missing they go to [Quarantine]. The [InventoryDebug] LCD will show a
ROUTING WARNINGS entry for any item that could not find its correct container,
and a STORAGE TAG CHECK entry for any required tag with no container on grid.

WHAT IS NEVER DRAINED

  [Food] containers         food flows in, never out
  [Irrigation] blocks       filled by FillIrrigationBlocks, never drained
  [Reserve] containers      protected — untouched by sorting
  [InputOnly] blocks        receive only
  [DoNotTrack] blocks       ignored entirely
  [TradeShip] cargo         managed by contract system only
  [LoadoutShip] cargo       managed by loadout dock only
  [Staged] containers       excluded from all routing
  [Export] containers       excluded from drain
  Weapon blocks             all IMyUserControllableGun types — turrets,
                            gatling guns, missile launchers, interior turrets


IRRIGATION SYSTEM
=================

Tag any farming or irrigation block with [Irrigation] in its block name:

FAR - Irrigation - Bed 01 [Irrigation]
FAR - Irrigation - Bed 02 [Irrigation]

Each sort cycle, FillIrrigationBlocks runs before general sorting and fills
every [Irrigation] block to capacity with food-category items (seeds, organic
matter). It pulls from [Food] containers first, then any other drainable
source as a fallback. This ensures farming blocks are never starved by the
sort cycle. Once filled, irrigation blocks are never drained back out.

Irrigation block contents are excluded from base availability counts so
seeds inside them do not count toward contracts or production calculations.


ASSEMBLER INPUT DRAINING
========================

DrainAssemblerInputs runs every ~3 seconds alongside sorting. It drains the
input slot (slot 0) of an assembler back to appropriate cargo in two cases:

1. Queue is empty — job is finished or was cancelled. Returns leftover
   ingredients so the next job can pull fresh stock cleanly.

2. Input fill reaches ASSEMBLER_INPUT_DRAIN_THRESHOLD (default 0.90) — an
   overfull input blocks conveyor delivery for the next queued item.

Idle detection uses GetQueue() count directly rather than IsProducing or
IsQueueEmpty, which can flicker false for one tick between jobs and cause
spurious drains on an active assembler.

Assemblers tagged [DoNotTrack] are skipped.
Dry run prints drain candidates to the PB echo panel.

Recommended naming:

INV - Assembler - Main 01
INV - Assembler - Main 02
INV - Assembler - Disassembly 01


REFINERY MANAGEMENT
===================

GENERAL REFINERIES (no [Refine:] tag)

Scores each ore by: ingot in stock / ingot reserve target. Lowest ratio wins.
Ejects any non-winning ore from the input back to [Ore] storage before loading
the winner so the refinery switches immediately rather than burning the queue.

DEDICATED REFINERIES ([Refine:X] tags)

Same reserve-ratio scoring but restricted to the refinery's allowed ore set.
Also ejects any ore not in the allowed set — covers manual mis-dumps and
conveyor contamination. Wrong ore returned to [Ore] storage before correct
ore is loaded. A two-tag refinery switches between its two ores automatically
as reserves shift.

ORE LOW THRESHOLDS

When ore stock falls below these values, loading stops, the ore light group
turns red, the status LCD updates, and the relay block fires (5-min cooldown):

  Iron Ore      10000    Silver Ore    2500
  Nickel Ore     5000    Gold Ore      2500
  Cobalt Ore     5000    Platinum Ore  1000
  Silicon Ore    5000    Uranium Ore   1000
  Magnesium Ore  2500

Adjustable in the ORE_LOW_THRESHOLDS dictionary in the script.

Naming examples:

MIN - Refinery - Iron [Refine:Fe]
MIN - Refinery - Nickel [Refine:Ni]
MIN - Refinery - General 01
MIN - Refinery - Iron-Nickel [Refine:Fe][Refine:Ni]


PRODUCTION QUOTAS
=================

ManageProductionQuotas runs every ~5 seconds. It queues assembler jobs when:

  - Any item is below its PRODUCTION_QUOTAS minimum
  - Any active contract item is below available stock
  - Any loadout item is not yet staged

Before queuing, it reads GetQueue() across all assemblers and subtracts what
is already queued, preventing duplicate job stacking on every cycle.

Jobs are distributed to the assembler with the fewest queued items so work
spreads across all available assemblers.

Blueprints are resolved via MyDefinitionId.Parse inside a try/catch. A parse
failure writes to lastAlert on the debug LCD rather than silently stopping.

Runtime overrides via [ProductionSettings] LCD — see PRODUCTION SETTINGS LCD.

Combat mode doubles ammo quotas via COMBAT_AMMO_MULTIPLIER (default 2.0).


SURPLUS DISASSEMBLY
===================

ManageSurplusDisassembly runs every ~5 seconds. Queues disassembly when any
item tracked in DISASSEMBLY_MAX exceeds its ceiling. Suspended during combat.

Override per item via [ProductionSettings] LCD. Set max to 0 to disable
disassembly for that item without removing it from the dictionary.


RESERVE MONITORING
==================

RESERVES defines protected minimum stock levels. Two effects:

1. Alert — the most critically low item (lowest ratio to target) shows as
   LOW (<100% of target) or CRITICAL (<25% of target) on [InventoryStatus].
   Alert clears automatically when all reserves recover.

2. Availability gate — AvailableForUse = stock minus reserve minus all active
   contract commitments. Contracts and loadouts only draw from surplus above
   both floors. Production quotas do not queue items already at reserve.

Set in the RESERVES dictionary in the script. Not currently LCD-configurable.


CONTRACT SYSTEM
===============

Supports multiple simultaneous named contracts. Each lives on its own LCD,
loads independently to the trade ship, and archives separately on completion.

CONTRACT LCD FORMAT

Plain [Contracts] LCD = single contract named "Default".
Named contract = [Contract:Name] in the LCD block name.

  INV - LCD - Contracts [Contracts]
  INV - LCD - Contract Alpha [Contract:Alpha]
  INV - LCD - Contract Beta [Contract:Beta]

LCD text format:

  Steel Plate = 5000
  Iron Ore = 20000
  Missile 200mm = 100
  # Deadline: Before server reset
  # Notes: Priority delivery to Outpost 3
  # All other # and // lines are ignored

# Deadline: and # Notes: lines are parsed and shown on [ContractStatus].

HOW LOADING WORKS

1. Script reads all contract LCDs each scan cycle.
2. Per item: checks stock minus reserve minus quantities committed to other
   active contracts. Available surplus is moved to [TradeShip] cargo.
3. Shortfalls are queued for assembler production (no duplicate jobs).
4. [ContractStatus] shows per-item progress bars, state, and ETA.
5. All items loaded → lights green, READY status, broadcast sent.
6. Ship departs → completed contracts archived to [ContractHistory], their
   LCDs cleared. Incomplete contracts remain for next trip.

CONTRACT STATUS LCD

  --- Alpha [READY] ---
  Deadline: Before server reset
  Steel Plate
    [####################] 5000/5000 DONE
  Missile 200mm
    [########............] 400/1000 PRODUCING
    Need to produce: 600
  Progress: 68%
  Est. production: 5m 20s

Three item states: DONE, Loading, PRODUCING.
ETA based on ASSEMBLER_THROUGHPUT_PER_SECOND — tune to your assembler setup.

CONTRACT RETURN

[ContractReturn]-tagged cargo on the docked trade ship is automatically
unloaded to base storage on dock. Items route to correct containers
(components → [Component], ingots → [Ingot], etc.).

This tag is managed by the trade ship's own script, not set manually:
  On departure  — ship script removes [ContractReturn] from all cargo.
  On return     — ship script adds [ContractReturn] to all cargo.

Full implementation in TradeShipControlScriptAddFunction.txt.

CONTRACT HISTORY

[ContractHistory] holds the last 30 archived entries in reverse order.
Each entry records contract name, deadline (if set), and item manifest.

CONTRACT COMMANDS

  pause contracts           Pause all loading
  resume contracts          Resume all loading
  reset contracts           Clear all contracts and loaded state
  pause contract:Name       Pause one named contract
  resume contract:Name      Resume one named contract
  complete contract:Name    Manually archive and clear one contract
  complete contracts        Archive all ready contracts at once


PRODUCTION SETTINGS LCD
=======================

Override PRODUCTION_QUOTAS and DISASSEMBLY_MAX at runtime without recompiling.

Format:   Item Name = min/max

  Steel Plate = 10000/0       (produce to 10000, no disassembly ceiling)
  Welder = 8/20               (produce to 8, disassemble above 20)
  Autocannon Magazine = 0/500 (do not produce, disassemble above 500)

Left value = production minimum. Right value = disassembly maximum.
0 on either side disables that behaviour for the item.
Lines starting with # or // are ignored.
LCD values take precedence over script dictionaries.


TRADE DOCK SETUP
================

LOG - Connector - Trade Dock [TradeDock]
LOG-AIR-Trade-01 Cargo 01 [TradeShip]

Loads contract items to [TradeShip] cargo once the ship is physically docked.
On departure, fulfilled contract LCDs are cleared if
REQUIRE_READY_BEFORE_CLEARING_CONTRACTS = true.
Incomplete contracts remain for the next trip.

For return cargo: see CONTRACT RETURN above and
TradeShipControlScriptAddFunction.txt.


LOADOUT DOCK SETUP
==================

BLD - Connector - Loadout Dock [LoadoutDock]
BLD - Cargo - Outpost Builder 01 [LoadoutShip]

Use to stage components and gas into a construction rover without those
items counting toward base availability during the build.

[Loadout] LCD format — same as contracts:

  Steel Plate = 20000
  Construction Component = 5000
  Motor = 1500
  Computer = 1000
  Hydrogen Bottle = 10
  Oxygen Bottle = 4

Loadout cargo is excluded from base stock counts. Gas tanks on the docked
rover are set to Stockpile and fill to DEFAULT_LOADOUT_HYDROGEN_FILL (0.80)
and DEFAULT_LOADOUT_OXYGEN_FILL (0.50) by default.


SALVAGE DOCK SETUP
==================

SAL - Connector - Salvage Dock [SalvageDock]
SAL - Cargo - Salvage Buffer 01 [Salvage]

Unloads all inventory from a docked rover into appropriate base containers.
Gas tanks on the rover fill from base on dock (Stockpile on). Tag any tank
[NoDrain] to exclude it from stockpile toggling.

[Salvage] items are excluded from base availability counts until sorted.

Multi-dock guard: if a [TradeShip] or [LoadoutShip] is detected on the
salvage connector, unloading is skipped and a status message is written.


STATUS LIGHTS
=============

Block group name: InventoryStatusLights

Green    All contracts loaded, ready for departure
Yellow   Producing or sorting in progress
Blue     Docked and loading
Red      Alert active (reserve low, ship full, ore low)
Purple   Contracts or loadout paused
White    Idle
Off      Script stopped or group not found

[InventoryLegend] LCD displays this key in-game.

Per-ore light groups (turn red when ore falls below threshold):

  IronOreStatusLights       CobaltOreStatusLights
  NickelOreStatusLights     SiliconOreStatusLights
                            MagnesiumOreStatusLights


BUTTON / TIMER COMMANDS
=======================

General:
  force sort           Rescan and sort all inventories immediately
  status               Rescan and refresh all LCDs
  clear alerts         Clear the last alert field on status LCD

Contract:
  pause contracts           Pause all contract loading
  resume contracts          Resume all contract loading
  reset contracts           Clear all contracts and state
  pause contract:Name       Pause one named contract
  resume contract:Name      Resume one named contract
  complete contract:Name    Archive and clear one contract
  complete contracts        Archive all ready contracts

Loadout:
  pause loadout        Halt loadout staging
  resume loadout       Resume loadout staging

Cross-script (sent via TryRun from other PBs):
  combat_mode          Activate combat — doubles ammo quotas, pauses contracts
  clear_combat         Deactivate combat — restores normal quotas
  power_critical       Power emergency — pauses non-essential operations
  power_ok             Power restored — resumes paused operations


CROSS-SCRIPT INTEGRATION
========================

Set PB names at the top of the script:

  DEFENSE_PB_NAME = "Base Defense PB"
  POWER_PB_NAME   = "Power Management PB"

combat_mode     Doubles ammo production quotas. Pauses contracts and loadout.
clear_combat    Restores quotas. Resumes contracts and loadout.
power_critical  Pauses contracts and loadout. Tracks power-triggered pauses
                separately so user-initiated pauses are not overridden.
power_ok        Resumes only what power_critical paused.


PRIORITIZE AMMO
===============

Tag inventory blocks near turrets or hangars with [PriorityAmmo]. The script
maintains target stock levels in those blocks by moving ammo from general
[Ammo] storage as needed.

Target levels in the PRIORITIZED_AMMO dictionary in the script:

  "5.56x45mm NATO magazine"  — 500
  "Missile 200mm"            — 50
  "Autocannon Magazine"      — 200

Dry run reports transfers without moving. Combat mode also doubles the base
production quota for all ammo types via COMBAT_AMMO_MULTIPLIER.


CONFIGURATION REFERENCE
=======================

Key tuneable values at the top of the script:

DRY_RUN_MODE                      true / false
SPACE_MODE                        true / false
COMBAT_AMMO_MULTIPLIER            2.0
ASSEMBLER_INPUT_DRAIN_THRESHOLD   0.90  (90% input fill triggers drain)
ASSEMBLER_THROUGHPUT_PER_SECOND   2.0   (tune to your assembler setup for ETA)
TRADE_SHIP_FULL_THRESHOLD         0.95
LOADOUT_SHIP_FULL_THRESHOLD       0.95
AUTO_CLEAR_CONTRACTS_ON_DEPARTURE true / false
REQUIRE_READY_BEFORE_CLEARING     true / false
DEFAULT_LOADOUT_HYDROGEN_FILL     0.80
DEFAULT_LOADOUT_OXYGEN_FILL       0.50
RELAY_COOLDOWN_TICKS              3000  (~5 min at Update10)

Tick intervals (lower = more frequent, higher sim cost):

FULL_SCAN_INTERVAL_TICKS    20
SORT_INTERVAL_TICKS         30
PRODUCTION_INTERVAL_TICKS   50
LCD_INTERVAL_TICKS          20


DEBUG LCD REFERENCE
===================

[InventoryDebug] shows the following sections each cycle:

STORAGE TAG CHECK
  OK or MISSING for every required storage tag. MISSING means no container
  on the grid carries that tag — items of that type will land in [Overflow].

ROUTING WARNINGS
  Any item sent to overflow this cycle because its correct container was not
  found. Clears each LCD update. If an item appears here repeatedly, check
  the container tag spelling or add a container for that type.

BLOCKS FOUND
  Counts of cargo containers, assemblers, refineries, connectors, etc.
  Use to confirm the script is finding all your blocks after a new build.

DOCK DETECTION
  Found / missing and connected / disconnected state for each dock connector.

LCDS
  Found / missing for key input LCDs.
