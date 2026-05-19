# Power Management Script

## Purpose
The Power Management Script is designed to monitor, allocate, and prioritize power resources for a Space Engineers base. It dynamically adjusts power for systems including solar panels, batteries, hydrogen engines, and reactors, ensuring efficient energy management.

## Key Features
- **Power Monitoring:** Tracks real-time power output and consumption.
- **Battery Recommendations:** Suggests additional batteries based on day/night cycles.
- **Docked Ship Management:** Prioritizes charging for docked vessels (drones, miners, trade ships).
- **Attack Response:** Focuses power on defense systems during enemy attacks.
- **Multi-LCD Output:** Outputs power status, usage breakdown, and recommendations to grouped LCDs.
- **Debug Mode:** Simulates actions and logs information without affecting the base.

## Arguments
- `under_attack`: Redirects power to turrets, shields, and defensive drones.
- `clear_attack`: Resets power allocation to normal.
- `set_day_length <hours>`: Updates the day/night cycle duration.
- `reset_alerts`: Resets alerts to normal (green state).

## Setup Instructions
1. **Assign Tags to Blocks:**
   - `[PowerStatusGroup]`: LCDs for general power status.
   - `[PowerRecommendationGroup]`: LCDs for power recommendations.
   - `[DebugGroup]`: Debugging LCDs.
   - `[DefenseTurret]`, `[DefenseDrone]`, `[DefenseDock]`: Defensive systems and docks.
2. **Load the Script:**
   - Paste the script into a programmable block.
3. **Configure LCD Panels:**
   - Name LCDs according to their roles.
4. **Connect Defensive and Utility Groups:**
   - Ensure tags for relevant systems (production, utilities, defenses) are set.

## Intended Uses
- Bases with complex power systems and docked ship management.
- Automatically handling energy allocation during day/night cycles.
- Prioritizing energy usage during combat or power shortages.

## Suggested Implementation
1. Test the script in **DEBUG_MODE = true** to ensure proper tag configuration.
2. Use consistent block tags for cross-script communication.
3. Combine with inventory and defense management scripts for a cohesive automation setup.

With this script, your base will dynamically adjust to changing power demands, enhance operational efficiency, and remain combat-ready during critical moments.