/*
 * Base Defense Script - Initial Draft
 * Author: derekwinmill
 * 
 * Description:
 * This script manages the defensive capabilities of the base, including autonomous drone operation,
 * base health monitoring, and manual override controls via war room buttons and control seat toolbars.
 *
 * Features:
 * - Autonomous drone patrol, engagement, and recall operations.
 * - Base block health monitoring and alert system.
 * - Manual overrides for targeted and group drone commands.
 * - LCD feedback for drone status, base health, and event logs.
 *
 * IMPORTANT: Ensure tagged blocks follow the naming conventions provided in the README.
 */

// Global Constants and Variables
readonly string DRONE_TAG = "[Drone]"; // Tag to identify drone programmable blocks
readonly string MONITORED_TAG = "[Monitored]"; // Tag for monitored base blocks
readonly string DEBUG_LCD_NAME = "Debug LCD"; // Name of LCD for debug logging

bool debugMode = true; // Toggle debug mode for testing (does not issue real commands)
List<IMyProgrammableBlock> allDrones = new List<IMyProgrammableBlock>(); // List of all drone PBs
List<IMyTerminalBlock> monitoredBlocks = new List<IMyTerminalBlock>(); // List of blocks to monitor
IMyTextPanel debugLCD;

// Program Initialization
public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update every 10 ticks

    // Initialize Debug LCD
    debugLCD = GridTerminalSystem.GetBlockWithName(DEBUG_LCD_NAME) as IMyTextPanel;
    if (debugLCD != null)
    {
        debugLCD.ContentType = ContentType.TEXT_AND_IMAGE;
        debugLCD.WriteText("Initializing Base Defense System...\n");
    }
    else
    {
        Echo("Debug LCD not found. Debug logs will only output to console.");
    }

    // Gather all drones and monitored blocks
    UpdateDroneList();
    UpdateMonitoredBlocksList();
}

// Main Execution Loop
public void Main(string argument, UpdateType updateSource)
{
    if (!string.IsNullOrWhiteSpace(argument))
    {
        HandleCommands(argument);
    }

    // Update base systems and statuses
    MonitorBaseBlocks();
    UpdateDroneStatuses();
    LogDebug("Main loop executed");
}

// Update the list of drones
void UpdateDroneList()
{
    allDrones.Clear();
    GridTerminalSystem.GetBlocksOfType(allDrones, drone => drone.CustomName.Contains(DRONE_TAG));
    LogDebug($"Drones updated: {allDrones.Count} found.");
}

// Update the list of monitored blocks
void UpdateMonitoredBlocksList()
{
    monitoredBlocks.Clear();
    GridTerminalSystem.GetBlocks(monitoredBlocks, block => block.CustomName.Contains(MONITORED_TAG));
    LogDebug($"Monitored blocks updated: {monitoredBlocks.Count} found.");
}

// Handle incoming commands
void HandleCommands(string command)
{
    if (command == "deploy_all")
    {
        SendCommandToAllDrones("deploy");
    }
    else if (command == "recall_all")
    {
        SendCommandToAllDrones("recall");
    }
    else if (command.StartsWith("patrol_drone_"))
    {
        string droneId = command.Substring("patrol_drone_".Length);
        SendCommandToSpecificDrone(droneId, "patrol");
    }
    else if (command.StartsWith("recall_drone_"))
    {
        string droneId = command.Substring("recall_drone_".Length);
        SendCommandToSpecificDrone(droneId, "recall");
    }
    else
    {
        LogDebug($"Unknown command: {command}");
    }
}

// Send a specific command to all drones
void SendCommandToAllDrones(string command)
{
    foreach (var drone in allDrones)
    {
        if (!drone.IsRunning)
        {
            if (debugMode)
            {
                LogDebug($"Simulating command to {drone.CustomName}: {command}");
            }
            else
            {
                drone.TryRun(command);
                LogDebug($"Command sent to {drone.CustomName}: {command}");
            }
        }
    }
}

// Send a specific command to a specific drone
void SendCommandToSpecificDrone(string droneId, string command)
{
    var drone = allDrones.FirstOrDefault(d => d.CustomName.Contains(droneId));
    if (drone != null && !drone.IsRunning)
    {
        if (debugMode)
        {
            LogDebug($"Simulating command to {drone.CustomName}: {command}");
        }
        else
        {
            drone.TryRun(command);
            LogDebug($"Command sent to {drone.CustomName}: {command}");
        }
    }
    else
    {
        LogDebug($"Drone {droneId} not found or busy.");
    }
}

// Monitor the health of base blocks
void MonitorBaseBlocks()
{
    foreach (var block in monitoredBlocks)
    {
        if (!block.IsFunctional)
        {
            LogDebug($"Block {block.CustomName} is damaged.");
        }
    }
}

// Update drone statuses (e.g., for LCD feedback)
void UpdateDroneStatuses()
{
    string statusOutput = "Drone Status:\n";
    foreach (var drone in allDrones)
    {
        statusOutput += $"- {drone.CustomName}: {drone.GetDetailedInfo()}\n";
    }

    DisplayOnLCD(debugLCD, statusOutput);
}

// Display text on a specified LCD
void DisplayOnLCD(IMyTextPanel lcd, string message)
{
    if (lcd != null)
    {
        lcd.WriteText(message);
    }
}

// Log debug messages
void LogDebug(string message)
{
    if (debugMode && debugLCD != null)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        debugLCD.WriteText($"[{timestamp}] {message}\n", true);
    }
    Echo(message);
}
