# Power Management Script — v2.0

## Purpose
Monitors, allocates, and prioritizes power for a Space Engineers base. Reads live output from every power source, tracks battery reserves, controls non-essential loads, and coordinates with the Base Defense and Inventory Management scripts during emergencies.

---

## Features

| Feature | Description |
|---|---|
| **Live power reading** | Reads current MW from solar/wind, reactors, hydrogen engines, and batteries every tick |
| **Battery monitoring** | Tracks aggregate charge %, stored MWh, charge/discharge flow, and overnight reserve estimate |
| **Threshold alerts** | Three-tier system: Optimal → Low (25%) → Critical (10%) |
| **Emergency response** | At 10%: enables backup reactors & batteries, disables non-essentials, notifies Inventory |
| **Combat mode** | Disables non-essentials, brings backup power online when `under_attack` is received |
| **Docked ship detection** | Counts vessels connected via tagged ship-dock connectors |
| **Multi-LCD output** | Status, Recommendations, and Breakdown panels updated every 2 s |
| **Cross-script comms** | Receives from Defense PB; sends to Inventory PB |
| **Dry-run mode** | All block commands logged but not executed while `DRY_RUN_MODE = true` |

---

## Block Naming Tags

Add these tags to block CustomNames. A block can carry more than one tag.

### Power source tags
| Tag | Use on |
|---|---|
| `[HydroEngine]` | Hydrogen engine blocks (separates them from solar/wind in output totals) |
| `[BackupReactor]` | Reactors kept offline normally; auto-enabled at battery critical or combat |
| `[BackupBattery]` | Batteries kept in recharge mode normally; switched to discharge during emergencies |

### Load management
| Tag | Use on |
|---|---|
| `[NonEssential]` | Any functional block that can be safely disabled during combat or power shortage (assemblers, interior lights, refineries on secondary ore, etc.) |

### Monitoring
| Tag | Use on |
|---|---|
| `[ShipDock]` | Ship connectors you want counted as docked-ship power shares |

### LCD panels
| Tag | Panel content |
|---|---|
| `[PowerStatusGroup]` | Live status: mode, alert, generation totals, battery level & flow, grid load |
| `[PowerRecommendation]` | Actionable advice: overnight reserve warning, surplus/deficit, docked ship notice |
| `[PowerBreakdown]` | Block counts and per-source output; dry-run indicator |
| `[DebugGroup]` | Timestamped debug log (shared with other scripts using this tag) |

---

## Arguments (Programmable Block Run)

| Argument | Effect |
|---|---|
| `under_attack` | Enter combat mode: disables non-essentials, enables backup power |
| `clear_attack` | Exit combat mode: restores non-essentials and backup power standby (unless power is still low) |
| `set_day_length <hours>` | Update day/night cycle length used in overnight reserve estimates |
| `reset_alerts` | Clear alert flags and restore `currentStatus` without changing any block states |

---

## Cross-Script Integration

### Commands this script *receives*

| Sender | Command | Action |
|---|---|---|
| Base Defense PB | `under_attack` | `EnterCombatMode()` |
| Base Defense PB | `clear_attack` | `ExitCombatMode()` |

The Base Defense script must call `TryRun("under_attack")` / `TryRun("clear_attack")` on the Power Management programmable block. Tag the Power Management PB with a known name (default: `"Power Management PB"` is what other scripts look for; this script doesn't need its own name configured).

### Commands this script *sends*

| Target PB name | Command | Condition |
|---|---|---|
| `Inventory Management PB` | `power_critical` | Battery drops below 10% (first time only) |
| `Inventory Management PB` | `power_ok` | Battery recovers back above 25% after a critical event |

Configure the target name in `INVENTORY_PB_NAME` at the top of the script. The Inventory Management script handles both commands: `power_critical` pauses contracts and loadout; `power_ok` resumes them.

---

## Power State Machine

```
         ┌──────────────────────────────────────────────────┐
         │                    NORMAL                        │
         │  battery ≥ 25%  /  no combat                     │
         └───────────────┬──────────────────────────────────┘
                         │ battery < 25%
                         ▼
         ┌──────────────────────────────────────────────────┐
         │                   LOW POWER                      │
         │  warn on LCD / recommend action                  │
         └───────────────┬──────────────────────────────────┘
                         │ battery < 10%
                         ▼
         ┌──────────────────────────────────────────────────┐
         │                   CRITICAL                       │
         │  • backup reactors ON                            │
         │  • backup batteries → Discharge                  │
         │  • non-essentials disabled (if not combat)       │
         │  • sends "power_critical" to Inventory PB        │
         └───────────────┬──────────────────────────────────┘
                         │ battery ≥ 25% again
                         ▼
         ┌──────────────────────────────────────────────────┐
         │             RECOVERY (→ NORMAL)                  │
         │  • backup reactors OFF                           │
         │  • backup batteries → Recharge                   │
         │  • non-essentials re-enabled (if not combat)     │
         │  • sends "power_ok" to Inventory PB              │
         └──────────────────────────────────────────────────┘
```

Combat mode can be active at any tier. When combat clears, non-essentials and backup power return to whatever the current power state requires.

---

## Setup Instructions

1. **Set `DRY_RUN_MODE = true`** and load the script into a Programmable Block.
2. Check the debug LCD (tagged `[DebugGroup]`) — verify block counts match your base.
3. Name your PBs:
   - This script's PB: *(any name)*
   - Base Defense PB: must match `DEFENSE_PB_NAME` in this script (default `"Base Defense PB"`)
   - Inventory PB: must match `INVENTORY_PB_NAME` (default `"Inventory Management PB"`)
4. Tag your blocks as described above and confirm counts on the `[PowerBreakdown]` LCD.
5. Test combat mode manually: run the PB with argument `under_attack`, confirm non-essentials disable (dry-run only logs), then `clear_attack`.
6. Set `DRY_RUN_MODE = false` when satisfied.

---

## Interaction with Other Scripts

| Script | Integration point |
|---|---|
| **Base Defense** | Sends `under_attack` / `clear_attack` to this PB when threat state changes |
| **Inventory Management** | Receives `power_critical` / `power_ok` from this script; pauses/resumes contracts and loadout accordingly |
| **Habitat Management** | Shares `[DebugGroup]` LCDs for unified debug output; no direct PB messaging yet |
| **Base Integrity Monitoring** | Future: could send structural damage alerts to trigger combat mode here |
| **Centralized Alerts** | Future: this script could forward `lastAlert` to a central alert PB |

---

## Configuration Reference

```csharp
const bool DRY_RUN_MODE = true;           // true = safe test mode
const string DEFENSE_PB_NAME   = "Base Defense PB";
const string INVENTORY_PB_NAME = "Inventory Management PB";

const float BATTERY_CRITICAL_PCT = 0.10f; // 10% triggers emergency
const float BATTERY_LOW_PCT      = 0.25f; // 25% triggers warning
const float BATTERY_GOOD_PCT     = 0.80f; // 80% = healthy for LCD

const int SCAN_INTERVAL      = 20;  // ticks between block rescans
const int LCD_INTERVAL       = 20;  // ticks between LCD refreshes
const int THRESHOLD_INTERVAL = 5;   // ticks between power checks
```
