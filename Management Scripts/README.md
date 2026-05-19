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
- `reset_alerts`: Resets all alerts to *green* (normal state).

### 2. Defense Management Script
**Purpose:** Manages base defenses, focusing on activating turrets, shields, and drones during attacks.

#### **Key Features:**
- Monitors attack triggers and communicates with other scripts (e.g., Base Power Management).
- Activates turrets, shields, and defensive drones.
- Sends `under_attack` signal to Base Power Management when an attack is detected.

#### **Arguments:**
- `enable_defenses`: Activates all defensive systems.
- `disable_defenses`: Deactivates all defensive systems.

---

### 3. Inventory Management Script
**Purpose:** Manages inventory logistics across the base, optimizing storage and production queuing.

#### **Key Features:**
- Sorts and stores inventory based on block tags.
- Handles docked ship inventory exchange (trade ships, miners, etc.).
- Optimizes inventory sorting for ammo supplies.
- Displays inventory data on dedicated LCDs.

#### **Arguments:**
- `force_sort`: Forces an immediate inventory sort.
- `pause_contracts`: Pauses inventory contracts.
- `resume_contracts`: Resumes inventory contracts.

---

### 4. Habitat Management Script
**Purpose:** Automates airlock control, pressurization, emergency protocols, and notifications for maintaining a safe and secure habitat environment.

#### **Key Features:**
- **Airlock Automation:**
  - Supports three-button airlock controls: Interior, Exterior, and Inside the Airlock.
  - Automates door cycling and pressurization adjustments.
- **Emergency Protocols:**
  - Locks and powers down airlocks for depressurized rooms to prevent entry.
  - Displays LCD alerts, plays alarm sounds, and broadcasts the emergency.
- **Debug Mode:**
  - Simulates actions without affecting the base, useful for testing.
- **Multi-Crew Safety:**
  - Ensures only one airlock cycle is processed at a time.
- **Notification System:**
  - Alerts are displayed via LCD, sound blocks, and antennas.
- **Armory Integration:**
  - Optionally checks an airlock's armory for essential supplies.

---

## Multi-Script Coordination
These scripts are designed to interact smoothly:
- **Base Power Management** handles power distribution and triggers backup systems as needed for Inventory and Defense operations.
- **Defense Management** activates during attacks and signals the Base Power Management script to focus power on combat readiness.
- **Inventory Management** works with docked ship management in Base Power Management to ensure resources are queued properly.
- **Habitat Management** provides safety for players by automating airlocks and responding immediately to emergencies.

## Interaction Tags:
- `[DefenseTurret]`, `[DefenseDrone]`, `[DefenseDock]`: Used by Defense and Power Management scripts.
- `[InventoryStatus]`, `[InventoryDebug]`: Used in Inventory Management for LCD outputs.

## Key Setup Elements:
1. Ensure all necessary blocks are tagged correctly for inter-script communication.
2. Use LCD groups effectively to monitor core operations.
3. Test each script individually before integrating all three for smooth operations.

---

With these four management scripts, you can enhance your Space Engineers base, automating operations while maintaining control during critical events. Let me know if you'd like to extend functionality further!