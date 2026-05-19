/*
Power Management Script
Version: 1.0
Designed for Space Engineers
*/

const bool DEBUG_MODE = true; // Toggle debug mode for simulations
const string STATUS_LCD_TAG = "[PowerStatusGroup]";
const string RECOMMENDATION_LCD_TAG = "[PowerRecommendationGroup]";
const string DEBUG_LCD_TAG = "[DebugGroup]";

List<IMyTextPanel> statusLCDs = new List<IMyTextPanel>();
List<IMyTextPanel> recommendationLCDs = new List<IMyTextPanel>();
List<IMyTextPanel> debugLCDs = new List<IMyTextPanel>();

Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    InitializeLCDGroups();
    Echo("Power Management Script initialized.");
}

void Main(string argument, UpdateType updateSource)
{
    if (!string.IsNullOrWhiteSpace(argument))
    {
        switch (argument.ToLower())
        {
            case "under_attack":
                FocusPowerOnDefense();
                break;
            case "clear_attack":
                ResetPowerAfterAttack();
                break;
            case string a when a.StartsWith("set_day_length"):
                SetDayLength(a);
                break;
            case "reset_alerts":
                ResetAlerts();
                break;
            default:
                LogDebug($"Invalid argument: {argument}");
                break;
        }
    }

    PowerMonitoringUpdate();
}

void InitializeLCDGroups()
{
    var allLCDs = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType(allLCDs);

    statusLCDs = allLCDs.Where(lcd => lcd.CustomName.Contains(STATUS_LCD_TAG)).ToList();
    recommendationLCDs = allLCDs.Where(lcd => lcd.CustomName.Contains(RECOMMENDATION_LCD_TAG)).ToList();
    debugLCDs = allLCDs.Where(lcd => lcd.CustomName.Contains(DEBUG_LCD_TAG)).ToList();
}

void PowerMonitoringUpdate()
{
    WriteToLCDGroup(statusLCDs, GetCorePowerStatus());
    WriteToLCDGroup(recommendationLCDs, GetPowerRecommendations());
}

string GetCorePowerStatus()
{
    // Placeholder for power status information
    return "Core Power Overview\n- Solar + Wind: 8.1 MW\n- Hydrogen Engines: 0.0 MW\n- Reactors: 3.2 MW\n- Base Load: 6.4 MW\n- Reserves: 54% (Batteries)\nCurrent State: GREEN - All Systems Normal\n";
}

string GetPowerRecommendations()
{
    // Placeholder for power recommendations
    return "Power Recommendations\n- Baseline Power: Sufficient\n- Recommendation: Add 2 Batteries for Overnight Reserves\n";
}

void FocusPowerOnDefense()
{
    LogDebug("Under attack! Redirecting power to defense systems.");
}

void ResetPowerAfterAttack()
{
    LogDebug("Clearing attack status. Restoring normal power allocation.");
}

void SetDayLength(string argument)
{
    var parts = argument.Split(' ');
    
    if (parts.Length > 1 && double.TryParse(parts[1], out double dayLength))
    {
        LogDebug($"Setting day length to {dayLength} hours.");
    }
    else
    {
        LogDebug("Invalid day length argument.");
    }
}

void ResetAlerts()
{
    LogDebug("Alerts have been reset to green.");
}

void WriteToLCDGroup(List<IMyTextPanel> lcdGroup, string content)
{
    foreach (var lcd in lcdGroup)
    {
        lcd.WriteText(content);
    }
}

void LogDebug(string message)
{
    if (DEBUG_MODE)
    {
        foreach (var lcd in debugLCDs)
        {
            lcd.WriteText(message + "\n", true);
        }
        Echo(message);
    }
}