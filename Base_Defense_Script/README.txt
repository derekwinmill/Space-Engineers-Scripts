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

### Planned Enhancements
- Improved repair logic for drones and base infrastructure.
- Threat prioritization based on distance, size, and weapon systems.
- Visual and audible alerts for war room scenarios.