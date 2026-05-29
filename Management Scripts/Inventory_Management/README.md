MARS COLONY INVENTORY MANAGER - README
======================================

This README explains the intended setup, block naming, LCD interaction, button
commands, and all script functions for the Mars Colony Inventory Manager
programmable block script.

This script is designed as a focused inventory/logistics script. It is not
intended to control mining drills, autopilots, defenses, doors, or power
systems. Those should remain in separate scripts for modularity.


SCRIPT OVERVIEW
===============

The script runs on Update10 (every 10 game ticks) and staggers its subsystems
across different tick offsets so no single update is doing too much work at once.
The main subsystems and their approximate cadences are:

  Full scan + LCD parse   every ~2 seconds   (ScanBlocks, ScanInventories,
                                               ParseContractLCDs, ParseLoadoutLCD,
                                               ParseProductionSettingsLCD)
  Inventory sorting       every ~3 seconds   (SortInventories)
  Refinery management     every ~3 seconds   (ManageRefineries)
  Salvage dock            every ~3 seconds   (ManageSalvageDock)
  Contract / trade ship   every ~3 seconds   (ManageContractsAndTradeShip)
  Loadout dock            every ~3 seconds   (ManageLoadoutDock)
  Assembler input drain   every ~3 seconds   (DrainAssemblerInputs)
  Production quotas       every ~5 seconds   (ManageProductionQuotas)
  Surplus disassembly     every ~5 seconds   (ManageSurplusDisassembly)
  Reserve monitoring      every ~2 seconds   (CheckReserves)
  LCD + lights update     every ~2 seconds   (UpdateLCDs, UpdateStatusLights)

Each subsystem can be individually enabled or disabled via the ENABLE_ flags
at the top of the script.


START SAFELY
============

The script starts with:

DRY_RUN_MODE = true

While dry run is enabled, the script scans, calculates, and reports, but will
not move inventory, queue production, clear LCDs, or change lights. Use this
mode first.

Once the debug/status LCDs look correct, edit the script and change:

DRY_RUN_MODE = false

In dry run mode, DrainAssemblerInputs will print to the programmable block echo
panel for each assembler it would act on, showing its idle state and fill
percentage. This helps verify the assembler drain logic is targeting the right
blocks before going live.


ENVIRONMENT MODE
================

At the top of the script is:

SPACE_MODE = false

false = Planetary/Mars priorities
true  = Space/asteroid base priorities

Most functionality is universal. This setting mainly exists so reserve
priorities and future alerts can be adapted when the same script is copied to
a space base.


FEATURE FLAGS
=============

Each major subsystem can be turned off independently without removing code.
Set any of the following to false to disable that feature:

ENABLE_INVENTORY_SORTING       Automatic item routing to tagged cargo
ENABLE_RESERVE_MONITORING      Reserve threshold alerts on LCDs and lights
ENABLE_PRODUCTION_QUOTAS       Auto-queue assembler production to meet minimums
ENABLE_SURPLUS_DISASSEMBLY     Auto-disassemble components above DISASSEMBLY_MAX
ENABLE_CONTRACT_SYSTEM         Read contract LCDs and track contract state
ENABLE_TRADE_SHIP_LOADING      Load cargo into connected [TradeShip] inventories
ENABLE_LOADOUT_DOCK            Stage components into connected [LoadoutShip]
ENABLE_SALVAGE_DOCK            Unload salvage rovers into appropriate storage
ENABLE_REFINERY_ASSIGNMENT     Smart ore loading and wrong-ore cleanup
ENABLE_STATUS_LIGHTS           Color-coded indicator lights via light group
ENABLE_BROADCASTS              Antenna/beacon status messages
ENABLE_DEBUG_LCD               Write detailed block counts to [InventoryDebug]

Note: DrainAssemblerInputs always runs alongside sorting and has no separate
flag. Tag individual assemblers [DoNotTrack] to exclude them.


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

Build high ceilings and vertical expansion space above each storage type so
another bulk container can be stacked later.


IMPORTANT TAGS
==============

Storage routing tags — place on cargo containers:

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
[Reserve]         Emergency protected storage — never drained by sorting
[Overflow]        Overflow storage — fallback destination for untagged items
[Quarantine]      Unknown/unhandled item storage — final fallback
[Staged]          Build staging — excluded from base availability counts

Dock and ship tags:

[TradeDock]       Connector used for trade ship operations
[TradeShip]       Cargo on the connected trade ship (outbound contract goods)
[ContractReturn]  Cargo on the trade ship to be unloaded on return (payment,
                  reward, or return items). Managed automatically by the trade
                  ship script — see TRADE DOCK SETUP and CONTRACT RETURN below.
[SalvageDock]     Connector used for salvage rover unloading
[Salvage]         Salvage buffer cargo — excluded from base counts
[LoadoutDock]     Connector used for outpost loadout operations
[LoadoutShip]     Cargo on the connected loadout rover or ship

Behaviour modifier tags — can be added to any inventory block:

[DoNotTrack]      Completely ignored by the script — not scanned, not drained
[NoExport]        Counted for base use but not included in export availability
[NoUnload]        Salvage dock will not unload this block
[NoDrain]         Gas tanks with this tag will not have stockpile toggled
[InputOnly]       Can receive items but will not be drained by sorting
[OutputOnly]      Can be drained but will not receive sorted items

Refinery tags — place on refinery blocks:

[Refine:Fe]       Dedicated to Iron Ore
[Refine:Ni]       Dedicated to Nickel Ore
[Refine:Co]       Dedicated to Cobalt Ore
[Refine:Si]       Dedicated to Silicon Ore
[Refine:Mg]       Dedicated to Magnesium Ore
[Refine:Ag]       Dedicated to Silver Ore
[Refine:Au]       Dedicated to Gold Ore
[Refine:Pt]       Dedicated to Platinum Ore
[Refine:U]        Dedicated to Uranium Ore

Multiple refinery tags can be combined on one refinery:

MIN - Refinery - Iron-Nickel [Refine:Fe][Refine:Ni]

A refinery with no [Refine:] tag is treated as general-purpose and will
process whichever ore is most critically needed at any time.

Ore alert relay tags — place on timer blocks or event controllers:

[Relay:Fe]        Triggered when Iron Ore stock falls below threshold
[Relay:Ni]        Triggered when Nickel Ore stock falls below threshold
[Relay:Co]        Triggered when Cobalt Ore stock falls below threshold
[Relay:Si]        Triggered when Silicon Ore stock falls below threshold
[Relay:Mg]        Triggered when Magnesium Ore stock falls below threshold

Relay blocks fire ApplyAction("TriggerNow") with a 5-minute cooldown to avoid
spam. Wire them to alert lights, sounds, or mining rover dispatchers as needed.

Broadcast tag:

[InventoryBroadcast]   Antenna, beacon, or text panel used for status broadcasts

Multiple tags on one block are fully supported. A cargo container named:

INV - Cargo - Armory 01 [Ammo][Tool][Weapons]

correctly accepts ammo, tools, and weapons routing simultaneously. The script
routes items to the container with the most free space among all matching
containers, so fill distributes naturally across multiple same-tagged bins.


LCD SETUP
=========

Recommended LCDs:

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

LCD descriptions:

[InventoryStatus]     Key reserve levels, current status, and active alerts.
                      Reserve monitoring now reports the most critically low
                      item with CRITICAL or LOW severity and clears itself
                      automatically when reserves recover.
[InventoryDebug]      Block counts, dock detection, LCD detection. Use during
                      setup to verify the script is finding all your blocks.
[Contracts]           Input LCD — enter a single default contract here.
[Contract:Name]       Input LCD for a named contract, e.g. [Contract:Alpha].
                      Multiple named contract LCDs can be active at once.
[ContractStatus]      Shows per-contract progress bars, loaded/needed quantities,
                      production estimates, deadlines, and notes for all active
                      contracts simultaneously.
[ContractHistory]     Read-only log of the last 30 completed or departed
                      contracts with item manifests. Useful for trade records.
[Loadout]             Input LCD — enter loadout manifest for outpost builds.
[LoadoutStatus]       Shows loadout progress and dock connection state.
[InventoryLegend]     Displays the status light color key.
[ProductionSettings]  Input LCD — runtime production min/max overrides.
[RefineryStatus]      Shows each ore type against its low threshold.


CONTRACT SYSTEM
===============

The script supports multiple simultaneous named trade contracts. Each contract
lives on its own LCD, loads independently to the trade ship, and is archived
separately when fulfilled. Contracts can include components, ingots, ores, ammo,
tools, food, or any other item the base tracks.

CONTRACT LCD FORMAT
-------------------

A plain [Contracts] LCD is treated as a single contract named "Default":

  INV - LCD - Contracts [Contracts]

Named contracts use [Contract:Name] in the LCD block name:

  INV - LCD - Contract Alpha [Contract:Alpha]
  INV - LCD - Contract Beta [Contract:Beta]

All named contract LCDs are scanned automatically every cycle. There is no
limit on how many can be active at once.

LCD text format:

  Steel Plate = 5000
  Iron Ore = 20000
  Missile 200mm = 100
  # Deadline: Before server reset
  # Notes: Priority delivery to Outpost 3
  # Regular comments starting with # or // are ignored

Lines starting with # Deadline: and # Notes: are parsed as metadata and
displayed on the [ContractStatus] LCD alongside the item progress. All other
comment lines are ignored.

HOW LOADING WORKS
-----------------

When the trade ship docks at [TradeDock]:

1. The script reads all active contract LCDs.
2. For each contract item, it checks base stock minus the reserve floor minus
   quantities already committed to other active contracts.
3. Available items are moved to [TradeShip] cargo on the connected construct.
4. Items not yet in stock are queued for assembler production automatically.
   The production system checks the existing assembler queue before adding
   jobs, so no item is queued twice.
5. The [ContractStatus] LCD updates every cycle with per-item progress bars,
   loaded vs needed quantities, production state, and estimated completion time.
6. When all items across all contracts are loaded the status lights turn green,
   the dock status shows READY, and a broadcast is sent if enabled.
7. When the ship departs, completed contracts are archived to [ContractHistory]
   and their LCDs are cleared. Incomplete contracts are left so loading can
   resume on the next trip.

CONTRACT AVAILABILITY GATE
---------------------------

The script never loads items below the RESERVES floor. If a contract requests
5,000 Steel Plate but only 3,000 are above reserve, only 3,000 are loaded and
the remaining 2,000 are queued for production. Multiple contracts competing for
the same item do not double-draw — each contract's committed quantity is
subtracted from what is available to other contracts.

CONTRACT STATUS LCD DISPLAY
----------------------------

The [ContractStatus] LCD shows a live view of every active contract:

  --- Alpha [READY] ---
  Deadline: Before server reset
  Notes: Priority delivery to Outpost 3
  Steel Plate
    [####################] 5000/5000 DONE
  Missile 200mm
    [########............] 400/1000 PRODUCING
    Need to produce: 600
  Progress: 68%
  Est. production: 5m 20s

Progress bars are 20 characters wide. Items show one of three states:
DONE (fully loaded), Loading (in stock, being moved), or PRODUCING (assemblers
are manufacturing the shortfall). The production estimate is based on
ASSEMBLER_THROUGHPUT_PER_SECOND in the script — tune this to match your actual
assembler count and upgrade level.

CONTRACT RETURN (INCOMING PAYMENT / REWARD ITEMS)
--------------------------------------------------

When the trade ship returns from a mission, any cargo blocks tagged
[ContractReturn] on the ship are automatically unloaded into base storage
when the ship docks at [TradeDock]. Items route to the correct containers
exactly as the salvage dock does — components to [Component], ores to [Ore],
ingots to [Ingot], and so on.

The [ContractReturn] tag is not intended to be set manually. It is designed
to be managed automatically by the trade ship's own control script:

  On departure — the ship script strips [ContractReturn] from all cargo
                 so outbound containers are clean on the next dock.

  On return    — the ship script adds [ContractReturn] to all cargo so
                 the base script unloads payment and reward items on dock.

This makes the full trade loop automatic with no player interaction required.
See TradeShipControlScriptAddFunction.txt in the trade ship script subfolder
for the full implementation guide and code.

CONTRACT HISTORY LCD
--------------------

Every contract that departs or is manually completed is archived to
[ContractHistory]. The log holds the last 30 entries and displays them in
reverse chronological order (most recent first) so recent deliveries are
always at the top. Each entry records the contract name, deadline if set,
and the full item manifest.

CONTRACT COMMANDS
-----------------

See BUTTON / TIMER COMMANDS for the full command list. Contract-specific
commands include:

  pause contract:Alpha      Pause a single named contract without affecting others
  resume contract:Alpha     Resume a paused named contract
  complete contract:Alpha   Manually archive a contract and clear its LCD
  complete contracts        Archive all currently ready contracts at once
  pause contracts           Pause all contract loading
  resume contracts          Resume all contract loading
  reset contracts           Clear all active contracts and loaded state


PRODUCTION SETTINGS LCD
=======================

Use the [ProductionSettings] LCD to override production quotas and disassembly
maximums at runtime without recompiling the script.

Format:

  Item Name = min/max

Examples:

  Welder = 8/20
  Elite Automatic Rifle = 2/5
  Autocannon Magazine = 0/500

The left value sets the production minimum (0 = do not produce).
The right value sets the disassembly maximum (0 = do not disassemble).
Lines beginning with # or // are ignored.

LCD values take precedence over the hardcoded PRODUCTION_QUOTAS and
DISASSEMBLY_MAX dictionaries in the script. This lets you adjust quotas
for a specific project or season without touching the script.


TRADE DOCK SETUP
================

Recommended connector:

LOG - Connector - Trade Dock [TradeDock]

Recommended ship cargo naming:

LOG-AIR-Trade-01 Cargo 01 [TradeShip]

The script searches for [TradeShip] cargo on the connected construct and loads
contract items into it. Cargo is only loaded after the ship is physically
connected to [TradeDock].

On departure, completed contract LCDs are cleared automatically if
REQUIRE_READY_BEFORE_CLEARING_CONTRACTS is true and all items were loaded.
Incomplete contracts are left for the next trip.

For return cargo handling, see CONTRACT RETURN above and the trade ship
implementation note in TradeShipControlScriptAddFunction.txt.


LOADOUT DOCK SETUP
==================

Use the loadout dock when building a new outpost so components and gases are
staged into a construction rover or ship without being pulled back into the
base inventory counts.

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

The script treats loadout cargo as assigned inventory. Those items are not
counted as available for contracts or normal production calculations.

Gas tanks on the loadout ship are automatically set to Stockpile when docked
so they fill from the base. Hydrogen fills to 80% and Oxygen to 50% by default.
Adjust via DEFAULT_LOADOUT_HYDROGEN_FILL and DEFAULT_LOADOUT_OXYGEN_FILL.


SALVAGE DOCK SETUP
==================

Use the salvage dock to automatically unload a salvage or mining rover when it
docks. The script pulls all inventory from connected blocks into the appropriate
tagged cargo containers on the base.

Recommended connector:

SAL - Connector - Salvage Dock [SalvageDock]

The salvage dock also manages gas tank drain direction. When a salvage vehicle
docks, its tanks are set to Stockpile (filling from base) and base tanks are
set to non-stockpile so gas transfers correctly. Tag any tank [NoDrain] to
exclude it from this behaviour.

Tag salvage buffer cargo:

SAL - Cargo - Salvage Buffer 01 [Salvage]

Items in [Salvage] containers are excluded from base availability counts so
they do not affect contract or production calculations until sorted.

Multi-dock guard: if a trade ship or loadout rover is detected on the salvage
connector, the salvage unload is skipped automatically to prevent accidentally
emptying the wrong ship.


REFINERY MANAGEMENT
===================

The script manages refinery inputs automatically based on current ingot
reserve levels. There are two refinery modes:

GENERAL REFINERIES (no [Refine:] tag)

A general refinery processes whichever ore will produce the ingot furthest
below its reserve target. Each ore is scored by:

  ingot currently in stock / ingot reserve target

The ore with the lowest ratio wins. Before loading the winning ore the script
ejects any other ore already in the refinery input back to [Ore] storage, so
the refinery switches immediately rather than burning through a stale queue.

DEDICATED REFINERIES ([Refine:X] tags)

A dedicated refinery only processes the ore types listed in its name tags.
The same reserve-ratio logic picks which of its allowed ores to load next.
The script ejects any ore that does not belong in this refinery — including
ore that was manually dumped or arrived via conveyor from a general ore
container — before loading the correct ore. Wrong ore is returned to [Ore]
storage automatically.

A dedicated refinery with two tags picks whichever of its two ores is most
needed at that moment and switches between them automatically as reserves shift.

ORE LOW THRESHOLDS

If ore storage drops below the ORE_LOW_THRESHOLDS minimum for any ore type,
the script will not attempt to load that ore into a refinery. Instead it
triggers an ore alert: the relevant ore light group turns red, the last alert
field updates on the status LCD, and the associated relay block fires if one
is tagged and its cooldown has expired.

Default thresholds (adjustable in script):

Iron Ore       10000
Nickel Ore      5000
Cobalt Ore      5000
Silicon Ore     5000
Magnesium Ore   2500
Silver Ore      2500
Gold Ore        2500
Platinum Ore    1000
Uranium Ore     1000

Recommended refinery naming examples:

MIN - Refinery - Iron [Refine:Fe]
MIN - Refinery - Nickel [Refine:Ni]
MIN - Refinery - General 01
MIN - Refinery - Iron-Nickel [Refine:Fe][Refine:Ni]


ASSEMBLER INPUT DRAINING
========================

The script monitors assembler input inventories (slot 0) and drains them back
to appropriate cargo containers under two conditions:

1. The assembler is idle (no active job and queue is empty). Leftover
   ingredients from a completed or cancelled job are returned to cargo so they
   do not occupy space and the assembler can pull fresh components cleanly for
   the next queued item.

2. The assembler input fill level reaches 90% or more. An overfull input
   prevents the conveyor system from delivering components for the next queued
   job. The script evacuates the input to keep it below the threshold even
   while the assembler is still running.

The 90% threshold is adjustable at the top of the script:

ASSEMBLER_INPUT_DRAIN_THRESHOLD = 0.90

Items drained from assembler inputs are routed through the same container
lookup used by inventory sorting, so components go to [Component] storage,
ingots go to [Ingot] storage, and so on.

Assemblers tagged [DoNotTrack] are skipped entirely.

In dry run mode the script prints to the programmable block echo panel for
each assembler it would act on, showing the assembler name, whether it is
idle, and the current input fill percentage.

Recommended assembler naming:

INV - Assembler - Main 01
INV - Assembler - Main 02
INV - Assembler - Disassembly 01


PRODUCTION QUOTAS
=================

The script queues assembler production automatically whenever a tracked item
falls below its minimum stock level. Quotas are defined in the PRODUCTION_QUOTAS
dictionary in the script and can be overridden at runtime via the
[ProductionSettings] LCD without recompiling.

Production is queued for:

  - Items below their PRODUCTION_QUOTAS minimum
  - Items needed for active contracts that exceed available stock
  - Items on the active loadout manifest that are not yet staged

Before queuing any job the script reads the existing assembler queue across all
assemblers and subtracts what is already queued. This prevents duplicate jobs
from stacking on every 5-second cycle when a large deficit exists.

Production jobs are distributed across all available assemblers by queue depth —
the assembler with the fewest queued items receives each new job. This spreads
work across your full assembler bank instead of piling everything into one.

During combat mode, ammo production quotas are multiplied by
COMBAT_AMMO_MULTIPLIER (default 2.0).

Items with a quota of 0 are not produced unless overridden by the LCD.


SURPLUS DISASSEMBLY
===================

When ENABLE_SURPLUS_DISASSEMBLY is true, the script queues disassembly jobs
for any item that exceeds its DISASSEMBLY_MAX ceiling. Only items listed in
the DISASSEMBLY_MAX dictionary are eligible for automatic disassembly.
Disassembly is suspended during combat mode.

DISASSEMBLY_MAX can be overridden per item via the [ProductionSettings] LCD.
Set the max value to 0 to disable automatic disassembly for that item.


RESERVE MONITORING
==================

RESERVES defines the protected minimum stock levels for ingots, components,
gases, food, and seeds. These values serve two purposes:

1. Alert threshold — if any reserve falls below its target the [InventoryStatus]
   LCD updates with the most critically low item shown as LOW or CRITICAL. The
   alert clears itself automatically once all reserves recover. The status LCD
   shows the full picture for every tracked item every cycle regardless of
   alert state.

2. Availability gate — the AvailableForUse calculation subtracts the reserve
   and all active contract commitments from current stock. Contracts and loadouts
   only draw from the surplus above both floors, and production quotas will not
   queue items already at reserve level.

Reserve levels are set in the RESERVES dictionary in the script and are not
currently configurable via LCD.


STATUS LIGHTS
=============

Add lighting blocks to a block group named:

InventoryStatusLights

The script sets the color of all lights in this group based on current state:

Green    All contracts loaded and ready for departure
Yellow   Producing or sorting in progress
Blue     Docked and loading (trade, loadout, or salvage)
Red      Alert active (reserve low, ship full, ore low)
Purple   Contracts or loadout paused
White    Idle, waiting for input or connection
Off      Script stopped or light group not found

The [InventoryLegend] LCD displays this key in-game.

Per-ore status light groups can optionally be set up:

IronOreStatusLights
NickelOreStatusLights
CobaltOreStatusLights
SiliconOreStatusLights
MagnesiumOreStatusLights

These groups turn red when the corresponding ore falls below its low threshold.


BUTTON / TIMER COMMANDS
=======================

Run these commands from a button panel, timer block, or the programmable block
run field. Commands are case-insensitive.

General:

  force sort                  Immediately rescans and sorts all inventories
  status                      Rescans and refreshes all LCDs
  clear alerts                Clears the last alert field on the status LCD

Contract commands:

  pause contracts             Pause all contract loading
  resume contracts            Resume all contract loading
  reset contracts             Clear all active contracts and loaded state
  pause contract:Name         Pause one named contract (e.g. pause contract:Alpha)
  resume contract:Name        Resume one named contract
  complete contract:Name      Manually archive a contract and clear its LCD
  complete contracts          Archive all currently ready contracts at once

Loadout commands:

  pause loadout               Halts loadout staging
  resume loadout              Resumes loadout staging

Cross-script commands (sent by other PBs via TryRun):

  combat_mode                 Activate combat mode — see CROSS-SCRIPT INTEGRATION
  clear_combat                Deactivate combat mode
  power_critical              Pause non-essential operations during power emergency
  power_ok                    Restore operations after power recovery


CROSS-SCRIPT INTEGRATION
========================

This script can receive commands from other programmable blocks via TryRun.
Set the names of the other script PBs at the top of the script:

DEFENSE_PB_NAME   = "Base Defense PB"
POWER_PB_NAME     = "Power Management PB"

Commands sent TO this script by other scripts:

combat_mode       Sent by Base Defense PB when the base is under attack.
                  Doubles ammo production quotas, pauses contracts and
                  loadout, and sets status to combat mode.

clear_combat      Sent by Base Defense PB when the threat is cleared.
                  Restores normal quotas and resumes contracts and loadout.

power_critical    Sent by Power Management PB during a power emergency.
                  Pauses contracts and loadout to reduce assembler load.
                  Tracks which pauses were triggered by power so they can
                  be restored cleanly without overriding user-initiated pauses.

power_ok          Sent by Power Management PB when power is restored.
                  Resumes only the operations paused by the power_critical
                  signal, leaving any user-initiated pauses in place.

This script does not currently send outbound commands to other scripts but is
structured to support SendToPB calls if future integration is needed.


PRIORITIZE AMMO
===============

The script supports prioritization of specific ammo types for defensive
readiness.

Setup:

Use the [PriorityAmmo] tag for any inventory blocks that should receive
priority ammo supplies. The script will maintain target levels of specified
ammo by transferring items to tagged blocks.

Target ammo levels are defined in the PRIORITIZED_AMMO dictionary:

  { "5.56x45mm NATO magazine", 500 }
  { "Missile 200mm", 50 }
  { "Autocannon Magazine", 200 }

Priority workflow:

1. All ammo inventories continuously scan for shortfalls relative to
   PRIORITIZED_AMMO levels.
2. Items are moved from general inventory to [PriorityAmmo] blocks as needed.
3. Dry run mode reports priority ammo actions without moving items.

Usage tip:

Place [PriorityAmmo] tags on inventory blocks near drone hangars or weapon
areas for efficient resupply. During combat mode the base ammo quota is
also doubled via COMBAT_AMMO_MULTIPLIER.


CONFIGURATION REFERENCE
=======================

All tuneable values are at the top of the script. Commonly adjusted settings:

DRY_RUN_MODE                       true/false — safe testing mode
SPACE_MODE                         true/false — switches priority profile
COMBAT_AMMO_MULTIPLIER             2.0 — ammo quota multiplier during combat
ASSEMBLER_INPUT_DRAIN_THRESHOLD    0.90 — input fill % that triggers drain
ASSEMBLER_THROUGHPUT_PER_SECOND    2.0 — items/sec used for contract ETA estimate;
                                         tune to your actual assembler count and
                                         upgrade level
TRADE_SHIP_FULL_THRESHOLD          0.95 — stops loading when ship is this full
LOADOUT_SHIP_FULL_THRESHOLD        0.95 — stops staging when rover is this full
AUTO_CLEAR_CONTRACTS_ON_DEPARTURE  true/false — clears fulfilled contract LCDs
                                               when ship departs
REQUIRE_READY_BEFORE_CLEARING      true/false — only clears LCDs if fully loaded
DEFAULT_LOADOUT_HYDROGEN_FILL      0.80 — hydrogen fill target for loadout ship
DEFAULT_LOADOUT_OXYGEN_FILL        0.50 — oxygen fill target for loadout ship
RELAY_COOLDOWN_TICKS               3000 — ticks between ore relay triggers
                                         (~5 minutes at Update10)

Tick interval constants (lower = more frequent, higher sim speed cost):

FULL_SCAN_INTERVAL_TICKS           20
SORT_INTERVAL_TICKS                30
PRODUCTION_INTERVAL_TICKS         50
LCD_INTERVAL_TICKS                 20
