/*
  Habitat Management Script
  Automates airlock control, pressurization, emergency protocols, and notifications for Space Engineers bases.
*/

// Configuration Constants
const bool DEBUG_MODE = true; // Simulate actions in debug mode
const string ALERT_LCD_TAG = "[HabitatAlerts]";
const string BROADCAST_TAG = "[EmergencyBroadcast]";
const string DEBUG_LCD_TAG = "[Airlock_Debug]";

// Data Structures
class Airlock
{
    public string Identifier;
    public IMyDoor InteriorDoor;
    public IMyDoor ExteriorDoor;
    public IMyAirVent Vent;
    public IMyButtonPanel InteriorButton;
    public IMyButtonPanel ExteriorButton;
    public IMyButtonPanel CycleButton;
    public IMyCargoContainer Armory;
    public bool IsLocked = false;
    public bool IsInEmergency = false;
}

Dictionary<string, Airlock> airlocks = new Dictionary<string, Airlock>();

Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    InitializeAirlocks();
    LogDebug("Habitat Management Script Initialized.");
}

void Main(string argument, UpdateType updateSource)
{
    // Handle script arguments
    if (!string.IsNullOrWhiteSpace(argument))
    {
        var args = argument.ToLower().Split(' ');
        if (args[0] == "reset_airlock" && args.Length > 1)
        {
            ResetAirlock(args[1]);
        }
        else
        {
            LogDebug($"Unknown argument: {argument}");
        }
        return;
    }

    // Regular update loop
    foreach (var airlock in airlocks.Values)
    {
        MonitorRoomPressure(airlock);
    }
}

void InitializeAirlocks()
{
    var buttons = new List<IMyButtonPanel>();
    var doors = new List<IMyDoor>();
    var vents = new List<IMyAirVent>();
    var containers = new List<IMyCargoContainer>();

    GridTerminalSystem.GetBlocksOfType(buttons);
    GridTerminalSystem.GetBlocksOfType(doors);
    GridTerminalSystem.GetBlocksOfType(vents);
    GridTerminalSystem.GetBlocksOfType(containers);

    foreach (var button in buttons)
    {
        string id = GetIdentifierFromName(button.CustomName);
        if (!string.IsNullOrEmpty(id))
        {
            if (!airlocks.ContainsKey(id)) airlocks[id] = new Airlock { Identifier = id };

            if (button.CustomName.Contains("Interior")) airlocks[id].InteriorButton = button;
            if (button.CustomName.Contains("Exterior")) airlocks[id].ExteriorButton = button;
            if (button.CustomName.Contains("Cycle")) airlocks[id].CycleButton = button;
        }
    }

    foreach (var door in doors)
    {
        string id = GetIdentifierFromName(door.CustomName);
        if (!string.IsNullOrEmpty(id))
        {
            if (!airlocks.ContainsKey(id)) airlocks[id] = new Airlock { Identifier = id };

            if (door.CustomName.Contains("Interior")) airlocks[id].InteriorDoor = door;
            if (door.CustomName.Contains("Exterior")) airlocks[id].ExteriorDoor = door;
        }
    }

    foreach (var vent in vents)
    {
        string id = GetIdentifierFromName(vent.CustomName);
        if (!string.IsNullOrEmpty(id))
        {
            if (!airlocks.ContainsKey(id)) airlocks[id] = new Airlock { Identifier = id };

            airlocks[id].Vent = vent;
        }
    }

    foreach (var container in containers)
    {
        string id = GetIdentifierFromName(container.CustomName);
        if (!string.IsNullOrEmpty(id))
        {
            if (!airlocks.ContainsKey(id)) airlocks[id] = new Airlock { Identifier = id };

            airlocks[id].Armory = container;
        }
    }
}

void MonitorRoomPressure(Airlock airlock)
{
    if (airlock.Vent == null) return; // Skip if no vent assigned

    float oxygenLevel = airlock.Vent.GetOxygenLevel();

    if (oxygenLevel < 0.05f) // Emergency threshold
    {
        LockAirlock(airlock, true);
        AlertRoomDepressurization(airlock.Identifier);
    }
    else if (oxygenLevel >= 0.05f && airlock.IsLocked)
    {
        UnlockAirlock(airlock);
    }
}

void LockAirlock(Airlock airlock, bool isEmergency = false)
{
    if (airlock.InteriorDoor != null) airlock.InteriorDoor.Enabled = false;
    if (airlock.ExteriorDoor != null) airlock.ExteriorDoor.Enabled = false;

    airlock.IsLocked = true;
    if (isEmergency)
    {
        airlock.IsInEmergency = true;
    }
}

void UnlockAirlock(Airlock airlock)
{
    if (airlock.InteriorDoor != null) airlock.InteriorDoor.Enabled = true;
    if (airlock.ExteriorDoor != null) airlock.ExteriorDoor.Enabled = true;

    airlock.IsLocked = false;
    airlock.IsInEmergency = false;

    AlertRoomStabilized(airlock.Identifier);
}

void ResetAirlock(string airlockId)
{
    if (!airlocks.ContainsKey(airlockId)) return;

    var airlock = airlocks[airlockId];
    if (airlock.IsInEmergency)
    {
        LockAirlock(airlock, false); // Unlock without emergency status
        WriteToLCD(ALERT_LCD_TAG, $"Airlock {airlockId} reset successfully and operational.");
    }
}

void AlertRoomDepressurization(string airlockId)
{
    WriteToLCD(ALERT_LCD_TAG, $"Emergency Alert: Room {airlockId} Depressurized - LOCKED");

    var alertSound = GridTerminalSystem.GetBlockWithName("[EmergencySoundBlock]") as IMySoundBlock;
    alertSound?.Play();

    SendBroadcast($"Emergency: Room {airlockId} Depressurized");
}

void AlertRoomStabilized(string airlockId)
{
    WriteToLCD(ALERT_LCD_TAG, $"Room {airlockId} Stabilized - LOCKDOWN CLEARED");
}

void SendBroadcast(string message)
{
    var antennas = new List<IMyRadioAntenna>();
    GridTerminalSystem.GetBlocksOfType(antennas, a => a.CustomName.Contains(BROADCAST_TAG));

    foreach (var antenna in antennas)
    {
        if (DEBUG_MODE)
        {
            LogDebug($"Broadcast message: {message}");
        }
        else
        {
            antenna.Enabled = true;
            antenna.TransmitMessage(message, MyTransmitTarget.Everyone);
        }
    }
}

void WriteToLCD(string tag, string content)
{
    var lcds = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType(lcds, lcd => lcd.CustomName.Contains(tag));

    foreach (var lcd in lcds)
    {
        lcd.WriteText(content);
    }
}

string GetIdentifierFromName(string name)
{
    var match = System.Text.RegularExpressions.Regex.Match(name, @"Airlock_([A-Za-z0-9_-]+)");
    return match.Success ? match.Groups[1].Value : null;
}

void LogDebug(string message)
{
    var debugLCD = GridTerminalSystem.GetBlockWithName(DEBUG_LCD_TAG) as IMyTextPanel;
    if (debugLCD != null)
        debugLCD.WriteText(message + "\n", true);

    Echo($"DEBUG: {message}");
}