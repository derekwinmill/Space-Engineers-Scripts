# Space Engineers Management Scripts
This repository contains three key management scripts for Space Engineers, designed to work in tandem to manage power, inventory, and defense operations within a cohesive environment.

## Scripts Overview

### 1. Base Power Management Script
**Purpose:** Handles the management of power generation, storage, and distribution for the base.
  
#### **Key Features:**
- Monitors power production (solar, wind, hydrogen engines, reactors) and consumption.
- Provides battery recommendations based on day/night cycles and consumption trends.
- Manages charging priorities for docked ships (miners, drones, trade ships).
- Adjusts power focus dynamically during events (e.g., attacks).
- Multi-LCD output for status, power breakdown, recommendations, and debugging logs.
- Debug mode to simulate actions without affecting the base.

#### **Arguments:**
- `under_attack`: Focuses power on defensive systems (e.g., turrets, drones).
- `clear_attack`: Resets power priorities to normal.
- `set_day_length <hours>`: Updates the duration of the day/night cycle.
- `reset_alerts`: Resets all alerts to **green** (normal state).

#### **Implementation Instructions:**
1. Assign appropriate tags to relevant blocks for power groups:
   - `[DefenseTurret]` for turrets.
   - `[DefenseDrone]` for drones.
   - `[DefenseDock]` for connectors related to defense.
   - `[Utility]`, `[Production]` for other base systems.
2. Set up tagged LCD panels for clear output:
   - `[PowerStatusGroup]` for general power status.
   - `[PowerBreakdownGroup]` for categorized usage.
   - `[PowerRecommendationGroup]` for improvement suggestions.
   - `[DebugGroup]` for debug output.

### 2. Defense Management Script
**Purpose:** Manages base defenses, focusing on activating turrets, shields, and drones during attacks.

#### **Key Features:**
- Monitors attack triggers and communicates with other scripts (e.g., Base Power Management).
- Activates turrets, shields, and defensive drones.
- Sends "under_attack" signal to Base Power Management when an attack is detected.

#### **Arguments:**
- `enable_defenses`: Activates all defensive systems.
- `disable_defenses`: Deactivates all defensive systems.

#### **Implementation Instructions:**
1. Tag turrets and drones with `[DefenseTurret]`, `[DefenseDrone]`.
2. Ensure the Base Power Management script is running in tandem to prioritize power accordingly.

### 3. Inventory Management Script
**Purpose:** Manages inventory logistics across the base, optimizing storage and production queuing.

#### **Key Features:**
- Automates inventory sorting and storage by block tags.
- Handles docked ship inventory exchange (trade ships, miners, etc.).
- Optional prioritization for ammo supplies.
- Displays inventory data on dedicated LCDs.

#### **Arguments:**
- `force_sort`: Forces an immediate inventory sort.
- `pause_contracts`: Pauses inventory contracts.
- `resume_contracts`: Resumes inventory contracts.

#### **Implementation Instructions:**
1. Assign inventory tags to blocks (`[Ore]`, `[Component]`, etc.).
2. Use tagged LCDs (e.g., `[InventoryStatus]`, `[InventoryDebug]`) for feedback.
3. Coordinate with docked ship management in Base Power Management.

---

## Multi-Script Coordination
These scripts are designed to interact smoothly:
- **Base Power Management** handles power distribution and triggers backup systems as needed for Inventory and Defense operations.
- **Defense Management** activates during attacks and signals the Base Power Management script to focus power on combat readiness.
- **Inventory Management** works with docked ships under the Base Power Management’s charging priority system.

#### Interaction Tags:
- `[DefenseTurret]`, `[DefenseDrone]`, `[DefenseDock]`: Used by Defense and Power Management scripts.
- `[InventoryStatus]`, `[InventoryDebug]`: Used in Inventory Management for LCD outputs.

#### Key Setup Elements:
1. Ensure all necessary blocks are tagged correctly for inter-script communication.
2. Use LCD groups effectively to monitor core operations.
3. Test each script individually before integrating all three for smooth operations.

---

## Debug Mode
Each script includes a debug mode to test functionality without making changes to the base:
- Logs actions and decisions to dedicated `[DebugGroup]` LCDs.
- Provides clear visibility into script behavior for troubleshooting and refinement.

---

With these three management scripts, you can enhance your Space Engineers base, automating operations while maintaining control during critical events. Let me know if you’d like to extend functionality further!