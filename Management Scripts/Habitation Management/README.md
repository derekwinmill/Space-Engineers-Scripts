# Habitat Management Script

## Purpose
This script automates airlock control, pressurization, emergency protocols, and notifications for maintaining a safe and secure habitat environment in Space Engineers.

## Key Features
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
- **Power Failure Handling:**
  - Airlocks enter a safe sealed state during power loss.
- **Room Priority:**
  - Handles room-specific pressurization, prioritizing critical areas.
- **Notification System:**
  - Alerts are displayed via LCD, sound blocks, and antennas.
- **Armory Integration:**
  - Optionally checks an airlock's armory for essential supplies.

## Arguments
1. `reset_airlock <ID>`: Resets a locked airlock after re-pressurization.

## Setup Instructions
1. **Block Tag Setup:**
   - `[Airlock_<ID>_Interior]`: Interior door.
   - `[Airlock_<ID>_Exterior]`: Exterior door.
   - `[Airlock_<ID>_Vent]`: Air vent for pressurization.
   - `[Airlock_<ID>_Button]`: Button panel for controlling airlock cycling.
   - `[Airlock_<ID>_Armory]`: Armory/container for emergency supplies.
   - `[HabitatAlerts]`: LCD for displaying alerts.
   - `[EmergencyBroadcast]`: Antenna for sending emergency broadcasts.
2. **Install the Script:**
   - Place the script in a programmable block.
   - Ensure all blocks have the correct tags as outlined above.
3. **Test the Setup (recommended):**
   - Set `DEBUG_MODE = true` to simulate actions before live operations.

## Intended Uses
- Automating airlock cycling for multiple rooms in a base.
- Sealing rooms in emergencies to maintain habitat safety.
- Managing depressurization alerts and notifications.
- Coordinating multi-player or large base environments effectively.

## Suggested Implementation
- Combine with other management scripts (e.g., power, inventory) for a fully automated base.
- Test in debug mode to ensure all airlocks are properly configured.
- Use sound and broadcast features for real-time notifications during critical events.

With this script in place, your habitat will maintain security, safety, and efficiency during all scenarios.