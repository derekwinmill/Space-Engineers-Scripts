# Base Defense Script

### Overview
The Base Defense Script is designed to streamline and manage the defensive capabilities of your Space Engineers base. It combines features like autonomous drone operations, base monitoring, and manual control to ensure your base is always protected against threats.

### Key Features
1. **Drone Management**:
   - Autonomous control for patrol, engagement, and recall operations.
   - Manual overrides via button panels or control seat toolbars.

2. **Threat Monitoring**:
   - Detects incoming threats and activates defensive responses.
   - Provides real-time updates and alters system priorities during attacks.

3. **Base System Health**:
   - Monitors `[Monitored]` blocks for damage or destruction.
   - Automatically adjusts priorities in emergencies.

4. **War Room Integration**:
   - LCDs provide clear feedback on drone status, base health, and power usage.
   - Button panels and a control seat allow manual overrides of defenses.

### How to Use
1. **Setup**:
   - Place this script in your programmable block.
   - Tag blocks with appropriate identifiers (e.g., `[Monitored]`, `[Drone]`, etc.).

2. **Automated Actions**:
   - The script operates autonomously to monitor threats, manage drones, and maintain system health.

3. **Manual Actions**:
   - Use button panels or control seat toolbars to issue specific commands or override automatic behavior.

### Naming Conventions
Consistency in naming is critical for the Base Defense Script to work effectively. Use the following conventions for block tagging:
- **[Monitored]**: Blocks that should be monitored for health or damage.
- **[Drone]**: Indicates programmable blocks or subgrids reserved for drone operation.
- **[Ammo]**: Blocks related to ammunition production, storage, or supply.
- **[Power]**: Critical power blocks such as reactors, batteries, or hydrogen engines.

#### Example:
- `Cargo Container [Ammo] [Monitored]`
- `Battery [Power] [Monitored]`

### LCD Usages
The script uses multiple LCDs to provide a clear overview of defensive operations:
1. **Drone Status LCDs**:
   - Tracks each drone's current status (e.g., Patrolling, Engaging, Returning, or Damaged).
   - Example Output:
     ```
     Drone Status:
     - Drone 01: Patrolling
     - Drone 02: Engaging (Manual)
     - Drone 03: Returning to Base
     ```

2. **Base Status LCDs**:
   - Monitors key systems such as power reserves, production blocks, and `[Monitored]` block health.
   - Example Output:
     ```
     Base System Status:
     - Batteries: 87% Charged
     - Reactors: Optimal
     - Turrets Online: 8/8
     ```

3. **Event Log LCDs**:
   - Displays recent events for debugging and situational awareness.
   - Example Output:
     ```
     Event Log:
     14:23 - Drone 02 Engaged Enemy at 2.4km
     14:24 - Turret 05 Detected Threat: Small Grid
     14:25 - Battery Critical: Reserve Systems Activated
     ```

### Room Designs
The Base Defense Script utilizes specific rooms for optimized defense management:

1. **War Room**:
   - **Purpose**: The main control center for managing base defenses.
   - **Design**:
     - Equip LCDs for drone status, base health, and event logs.
     - Integrate button panels for manual group commands (e.g., Deploy All Drones, Recall All).
     - Include a control seat for drone-specific commands.

2. **Drone Hangar**:
   - **Purpose**: Hosting and maintaining drones.
   - **Design**:
     - Ensure each drone has a dedicated dock with welders for repairs.
     - Display a status LCD showing docked drones and their health.

3. **Power Core Room**:
   - **Purpose**: Manages all critical power systems.
   - **Design**:
     - Display power system statuses (e.g., battery charge, reactor output).
     - Provide emergency override buttons for power rerouting.

4. **Monitoring Room**:
   - **Purpose**: Dedicated room for tracking `[Monitored]` blocks.
   - **Design**:
     - LCDs show damage or destruction logs.
     - Lights or audible alarms for immediate feedback on damaged components.

### Planned Enhancements
- Improved repair logic for drones and base infrastructure.
- Threat prioritization based on distance, size, and weapon systems.
- Visual and audible alerts for war room scenarios.