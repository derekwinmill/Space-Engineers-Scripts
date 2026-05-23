/*
 * Power Management Script
 * Version: 2.0
 * Designed for Space Engineers
 *
 * Cross-script integration:
 *   Receives from Base Defense PB:  "under_attack", "clear_attack"
 *   Sends to Inventory Mgmt PB:     "power_critical", "power_ok"
 *
 * Block naming tags (append to a block's CustomName):
 *   [PowerStatusGroup]    - LCD panels showing live power status
 *   [PowerRecommendation] - LCD panels showing recommendations
 *   [PowerBreakdown]      - LCD panels showing per-source breakdown
 *   [DebugGroup]          - LCD panels for debug log output
 *   [NonEssential]        - Functional blocks disabled during combat or power emergency
 *   [BackupReactor]       - Reactors that are offline normally, enabled during emergencies
 *   [BackupBattery]       - Batteries held in reserve, discharged during emergencies
 *   [HydroEngine]         - Hydrogen engine power producers
 *   [ShipDock]            - Ship connectors to monitor for docked vessels
 */

// ============================================================
// USER CONFIGURATION
// ============================================================

const bool DRY_RUN_MODE = true; // Set false when ready to issue real block commands

// Programmable block CustomNames for cross-script communication
const string DEFENSE_PB_NAME   = "Base Defense PB";
const string INVENTORY_PB_NAME = "Inventory Management PB";

// Battery thresholds (fraction 0.0–1.0)
const float BATTERY_CRITICAL_PCT = 0.10f; // 10% — emergency mode
const float BATTERY_LOW_PCT      = 0.25f; // 25% — warning
const float BATTERY_GOOD_PCT     = 0.80f; // 80% — considered healthy

// Tick cadence (at Update10: 1 tick ≈ 1/6 s real-time)
const int SCAN_INTERVAL      = 20; // ~2 s: rescan block lists
const int LCD_INTERVAL       = 20; // ~2 s: refresh LCD panels
const int THRESHOLD_INTERVAL = 5;  // ~0.5 s: check power levels

// Block name tags
const string TAG_NON_ESSENTIAL  = "[NonEssential]";
const string TAG_BACKUP_REACTOR = "[BackupReactor]";
const string TAG_BACKUP_BATTERY = "[BackupBattery]";
const string TAG_HYDRO_ENGINE   = "[HydroEngine]";
const string TAG_SHIP_DOCK      = "[ShipDock]";

// LCD name tags
const string STATUS_LCD_TAG    = "[PowerStatusGroup]";
const string RECOMMEND_LCD_TAG = "[PowerRecommendation]";
const string BREAKDOWN_LCD_TAG = "[PowerBreakdown]";
const string DEBUG_LCD_TAG     = "[DebugGroup]";

// ============================================================
// INTERNAL STATE
// ============================================================

int    tick           = 0;
bool   combatMode     = false;
bool   powerCritical  = false;
bool   powerLow       = false;
string currentStatus  = "Initializing";
string lastAlert      = "None";
double dayLengthHours = 2.0;

// Live power readings (MW / MWh)
float renewableMW      = 0f; // solar + wind turbines
float reactorMW        = 0f; // all online reactors
float hydroMW          = 0f; // hydrogen engines
float battDischarge    = 0f; // battery output to grid
float battCharge       = 0f; // battery draw from grid
float gridLoadMW       = 0f; // estimated total consumer draw
float batteryPct       = 0f; // 0–1 aggregate charge level
float batteryStoredMWh = 0f;
float batteryMaxMWh    = 0f;
int   dockedShips      = 0;

// Block lists
List<IMyPowerProducer>  renewables       = new List<IMyPowerProducer>();
List<IMyPowerProducer>  hydroEngines     = new List<IMyPowerProducer>();
List<IMyBatteryBlock>   mainBatteries    = new List<IMyBatteryBlock>();
List<IMyBatteryBlock>   backupBatteries  = new List<IMyBatteryBlock>();
List<IMyReactor>        mainReactors     = new List<IMyReactor>();
List<IMyReactor>        backupReactors   = new List<IMyReactor>();
List<IMyFunctionalBlock> nonEssentials   = new List<IMyFunctionalBlock>();
List<IMyShipConnector>  shipDocks        = new List<IMyShipConnector>();
List<IMyTextPanel>      statusLCDs       = new List<IMyTextPanel>();
List<IMyTextPanel>      recommendLCDs    = new List<IMyTextPanel>();
List<IMyTextPanel>      breakdownLCDs    = new List<IMyTextPanel>();
List<IMyTextPanel>      debugLCDs        = new List<IMyTextPanel>();

// ============================================================
// INITIALIZATION
// ============================================================

Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    ScanBlocks();
    Echo("Power Management v2.0 initialized.");
}

// ============================================================
// MAIN LOOP
// ============================================================

void Main(string argument, UpdateType updateSource)
{
    tick++;

    if (!string.IsNullOrWhiteSpace(argument))
        HandleCommand(argument.Trim().ToLower());

    if (tick % SCAN_INTERVAL == 1)
        ScanBlocks();

    ReadPowerData();

    if (tick % THRESHOLD_INTERVAL == 0)
        CheckThresholds();

    if (tick % LCD_INTERVAL == 3)
        UpdateLCDs();

    Echo("Power Management v2.0");
    Echo("Mode:    " + (combatMode ? "COMBAT" : "Normal"));
    Echo("Battery: " + FormatPct(batteryPct));
    Echo("Status:  " + currentStatus);
    Echo("Alert:   " + lastAlert);
}

// ============================================================
// COMMANDS
// ============================================================

void HandleCommand(string cmd)
{
    if (cmd == "under_attack")
    {
        EnterCombatMode();
    }
    else if (cmd == "clear_attack")
    {
        ExitCombatMode();
    }
    else if (cmd == "reset_alerts")
    {
        lastAlert     = "None";
        powerCritical = false;
        powerLow      = false;
        currentStatus = "Alerts reset";
        LogDebug("Alerts manually reset.");
    }
    else if (cmd.StartsWith("set_day_length "))
    {
        double v;
        string valStr = cmd.Substring("set_day_length ".Length).Trim();
        if (double.TryParse(valStr, out v) && v > 0)
        {
            dayLengthHours = v;
            LogDebug("Day length set to " + v + " hrs.");
        }
        else
        {
            LogDebug("Invalid day length value: " + valStr);
        }
    }
    else
    {
        LogDebug("Unknown command: " + cmd);
    }
}

// ============================================================
// COMBAT MODE
// ============================================================

void EnterCombatMode()
{
    if (combatMode) return;
    combatMode = true;

    SetFunctionalBlocks(nonEssentials, false);
    SetReactors(backupReactors, true);
    SetBatteryMode(backupBatteries, ChargeMode.Discharge);

    currentStatus = "COMBAT MODE";
    lastAlert     = "Under attack - non-essential systems offline";
    LogDebug(lastAlert);
}

void ExitCombatMode()
{
    if (!combatMode) return;
    combatMode = false;

    if (!powerCritical && !powerLow)
    {
        SetFunctionalBlocks(nonEssentials, true);
        SetReactors(backupReactors, false);
        SetBatteryMode(backupBatteries, ChargeMode.Recharge);
    }

    lastAlert     = "None";
    currentStatus = "Combat cleared - normal operations";
    LogDebug(currentStatus);
}

// ============================================================
// BLOCK SCANNING
// ============================================================

void ScanBlocks()
{
    renewables.Clear();
    hydroEngines.Clear();
    mainBatteries.Clear();
    backupBatteries.Clear();
    mainReactors.Clear();
    backupReactors.Clear();
    nonEssentials.Clear();
    shipDocks.Clear();
    statusLCDs.Clear();
    recommendLCDs.Clear();
    breakdownLCDs.Clear();
    debugLCDs.Clear();

    // Power producers: sort by type / tag
    var allProducers = new List<IMyPowerProducer>();
    GridTerminalSystem.GetBlocksOfType(allProducers);
    for (int i = 0; i < allProducers.Count; i++)
    {
        IMyPowerProducer p = allProducers[i];
        if (p is IMyBatteryBlock || p is IMyReactor) continue;
        if (p.CustomName.Contains(TAG_HYDRO_ENGINE))
            hydroEngines.Add(p);
        else
            renewables.Add(p); // solar panels, wind turbines, etc.
    }

    // Batteries: main vs backup
    var allBats = new List<IMyBatteryBlock>();
    GridTerminalSystem.GetBlocksOfType(allBats);
    for (int i = 0; i < allBats.Count; i++)
    {
        if (allBats[i].CustomName.Contains(TAG_BACKUP_BATTERY))
            backupBatteries.Add(allBats[i]);
        else
            mainBatteries.Add(allBats[i]);
    }

    // Reactors: main vs backup
    var allReactors = new List<IMyReactor>();
    GridTerminalSystem.GetBlocksOfType(allReactors);
    for (int i = 0; i < allReactors.Count; i++)
    {
        if (allReactors[i].CustomName.Contains(TAG_BACKUP_REACTOR))
            backupReactors.Add(allReactors[i]);
        else
            mainReactors.Add(allReactors[i]);
    }

    // Non-essential blocks and ship docks
    GridTerminalSystem.GetBlocksOfType<IMyFunctionalBlock>(nonEssentials,
        b => b.CustomName.Contains(TAG_NON_ESSENTIAL));
    GridTerminalSystem.GetBlocksOfType(shipDocks,
        c => c.CustomName.Contains(TAG_SHIP_DOCK));

    // LCD panels
    var allLCDs = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType(allLCDs);
    for (int i = 0; i < allLCDs.Count; i++)
    {
        IMyTextPanel lcd = allLCDs[i];
        if (lcd.CustomName.Contains(STATUS_LCD_TAG))    statusLCDs.Add(lcd);
        if (lcd.CustomName.Contains(RECOMMEND_LCD_TAG)) recommendLCDs.Add(lcd);
        if (lcd.CustomName.Contains(BREAKDOWN_LCD_TAG)) breakdownLCDs.Add(lcd);
        if (lcd.CustomName.Contains(DEBUG_LCD_TAG))     debugLCDs.Add(lcd);
    }

    LogDebug(string.Format(
        "Scan: {0} renewable, {1} reactor, {2} hydro, {3} bat, {4} non-essential, {5} docks",
        renewables.Count,
        mainReactors.Count + backupReactors.Count,
        hydroEngines.Count,
        mainBatteries.Count + backupBatteries.Count,
        nonEssentials.Count,
        shipDocks.Count));
}

// ============================================================
// POWER DATA READING
// ============================================================

void ReadPowerData()
{
    renewableMW = 0f;
    hydroMW     = 0f;
    reactorMW   = 0f;
    battDischarge    = 0f;
    battCharge       = 0f;
    batteryStoredMWh = 0f;
    batteryMaxMWh    = 0f;

    for (int i = 0; i < renewables.Count; i++)
        renewableMW += renewables[i].CurrentOutput;

    for (int i = 0; i < hydroEngines.Count; i++)
        hydroMW += hydroEngines[i].CurrentOutput;

    for (int i = 0; i < mainReactors.Count; i++)
        reactorMW += mainReactors[i].CurrentOutput;
    for (int i = 0; i < backupReactors.Count; i++)
        reactorMW += backupReactors[i].CurrentOutput;

    for (int i = 0; i < mainBatteries.Count; i++)
    {
        battDischarge    += mainBatteries[i].CurrentOutput;
        battCharge       += mainBatteries[i].CurrentInput;
        batteryStoredMWh += mainBatteries[i].CurrentStoredPower;
        batteryMaxMWh    += mainBatteries[i].MaxStoredPower;
    }
    for (int i = 0; i < backupBatteries.Count; i++)
    {
        battDischarge    += backupBatteries[i].CurrentOutput;
        battCharge       += backupBatteries[i].CurrentInput;
        batteryStoredMWh += backupBatteries[i].CurrentStoredPower;
        batteryMaxMWh    += backupBatteries[i].MaxStoredPower;
    }

    batteryPct = batteryMaxMWh > 0f ? batteryStoredMWh / batteryMaxMWh : 0f;

    // Grid load = all power going to non-battery consumers
    // = (generators + battery discharge) - battery charging
    gridLoadMW = renewableMW + reactorMW + hydroMW + battDischarge - battCharge;
    if (gridLoadMW < 0f) gridLoadMW = 0f;

    dockedShips = 0;
    for (int i = 0; i < shipDocks.Count; i++)
        if (shipDocks[i].Status == MyShipConnectorStatus.Connected) dockedShips++;
}

// ============================================================
// POWER THRESHOLD CHECKS
// ============================================================

void CheckThresholds()
{
    if (batteryMaxMWh <= 0f)
    {
        lastAlert     = "WARNING: No batteries detected";
        currentStatus = "No battery data";
        return;
    }

    if (batteryPct < BATTERY_CRITICAL_PCT)
    {
        if (!powerCritical)
        {
            powerCritical = true;
            powerLow      = true;
            lastAlert     = "CRITICAL: Battery at " + FormatPct(batteryPct) + " - Emergency power enabled";
            LogDebug(lastAlert);

            SetReactors(backupReactors, true);
            SetBatteryMode(backupBatteries, ChargeMode.Discharge);
            if (!combatMode) SetFunctionalBlocks(nonEssentials, false);

            SendToPB(INVENTORY_PB_NAME, "power_critical");
        }
        else
        {
            lastAlert = "CRITICAL: Battery at " + FormatPct(batteryPct);
        }
        currentStatus = "EMERGENCY - Battery Critical";
    }
    else if (batteryPct < BATTERY_LOW_PCT)
    {
        bool justRecoveredFromCritical = powerCritical;
        powerCritical = false;
        powerLow      = true;

        if (justRecoveredFromCritical)
            lastAlert = "WARNING: Recovering from critical - " + FormatPct(batteryPct);
        else
            lastAlert = "WARNING: Battery low at " + FormatPct(batteryPct);

        currentStatus = "LOW POWER";
    }
    else
    {
        bool wasAbnormal = powerCritical || powerLow;
        powerCritical = false;
        powerLow      = false;

        if (wasAbnormal && !combatMode)
        {
            SetReactors(backupReactors, false);
            SetBatteryMode(backupBatteries, ChargeMode.Recharge);
            SetFunctionalBlocks(nonEssentials, true);
            SendToPB(INVENTORY_PB_NAME, "power_ok");
            LogDebug("Power normalized - backup offline, non-essentials restored.");
        }

        lastAlert = "None";
        if (combatMode)
            currentStatus = "COMBAT MODE";
        else if (batteryPct >= BATTERY_GOOD_PCT)
            currentStatus = "Optimal";
        else
            currentStatus = "Normal Operations";
    }
}

// ============================================================
// BLOCK CONTROL
// ============================================================

void SetFunctionalBlocks(List<IMyFunctionalBlock> blocks, bool enabled)
{
    if (DRY_RUN_MODE)
    {
        LogDebug("[DryRun] " + (enabled ? "Enable" : "Disable") + " " + blocks.Count + " non-essential blocks");
        return;
    }
    for (int i = 0; i < blocks.Count; i++)
        blocks[i].Enabled = enabled;
}

void SetReactors(List<IMyReactor> reactors, bool enabled)
{
    if (DRY_RUN_MODE)
    {
        LogDebug("[DryRun] " + (enabled ? "Enable" : "Disable") + " " + reactors.Count + " backup reactors");
        return;
    }
    for (int i = 0; i < reactors.Count; i++)
        reactors[i].Enabled = enabled;
}

void SetBatteryMode(List<IMyBatteryBlock> bats, ChargeMode mode)
{
    if (DRY_RUN_MODE)
    {
        LogDebug("[DryRun] Set " + bats.Count + " backup batteries to " + mode.ToString());
        return;
    }
    for (int i = 0; i < bats.Count; i++)
        bats[i].ChargeMode = mode;
}

// ============================================================
// CROSS-SCRIPT COMMUNICATION
// ============================================================

void SendToPB(string pbName, string command)
{
    IMyProgrammableBlock pb = GridTerminalSystem.GetBlockWithName(pbName) as IMyProgrammableBlock;
    if (pb == null)
    {
        Echo("WARNING: PB not found: " + pbName);
        return;
    }
    if (!DRY_RUN_MODE)
        pb.TryRun(command);
    else
        LogDebug("[DryRun] -> '" + command + "' to " + pbName);
}

// ============================================================
// LCD OUTPUT
// ============================================================

void UpdateLCDs()
{
    WriteLCDs(statusLCDs,    BuildStatusText());
    WriteLCDs(recommendLCDs, BuildRecommendationText());
    WriteLCDs(breakdownLCDs, BuildBreakdownText());
}

string BuildStatusText()
{
    var sb = new StringBuilder();
    sb.AppendLine("=== POWER STATUS ===");
    sb.AppendLine("Dry Run: " + (DRY_RUN_MODE ? "ON" : "OFF"));
    sb.AppendLine("Mode:    " + (combatMode ? "*** COMBAT ***" : "Normal"));
    sb.AppendLine("Status:  " + currentStatus);
    sb.AppendLine("Alert:   " + lastAlert);
    sb.AppendLine();
    sb.AppendLine("--- Generation ---");
    sb.AppendLine("Solar/Wind: " + FormatMW(renewableMW));
    sb.AppendLine("Reactors:   " + FormatMW(reactorMW));
    sb.AppendLine("Hydrogen:   " + FormatMW(hydroMW));
    sb.AppendLine();
    sb.AppendLine("--- Batteries ---");
    string flowStr;
    if (battCharge > battDischarge)
        flowStr = "charging  " + FormatMW(battCharge - battDischarge);
    else if (battDischarge > 0f)
        flowStr = "discharging " + FormatMW(battDischarge - battCharge);
    else
        flowStr = "idle";
    sb.AppendLine("Charge:  " + FormatPct(batteryPct) +
                  " (" + FormatMWh(batteryStoredMWh) + " / " + FormatMWh(batteryMaxMWh) + ")");
    sb.AppendLine("Flow:    " + flowStr);
    sb.AppendLine();
    sb.AppendLine("Grid Load:    " + FormatMW(gridLoadMW));
    sb.AppendLine("Docked Ships: " + dockedShips);
    return sb.ToString();
}

string BuildRecommendationText()
{
    var sb = new StringBuilder();
    sb.AppendLine("=== RECOMMENDATIONS ===");

    if (batteryPct < BATTERY_CRITICAL_PCT)
    {
        sb.AppendLine("!! EMERGENCY - backup power active");
        sb.AppendLine("!! Non-essentials disabled");
    }
    else if (batteryPct < BATTERY_LOW_PCT)
    {
        sb.AppendLine("! Battery low - reduce consumption");
        if (backupReactors.Count > 0)
            sb.AppendLine("! Consider enabling backup reactors");
    }
    else if (batteryPct >= BATTERY_GOOD_PCT && renewableMW > gridLoadMW * 0.9f)
    {
        sb.AppendLine("Surplus power - batteries charging.");
        sb.AppendLine("Production can be expanded safely.");
    }
    else
    {
        sb.AppendLine("Power levels nominal.");
    }

    sb.AppendLine();

    if (renewableMW < 0.5f)
        sb.AppendLine("! Renewable output low (night or obstruction)");
    if (dockedShips > 0)
        sb.AppendLine("Docked ships (" + dockedShips + ") sharing grid - watch draw");

    if (gridLoadMW > 0f)
    {
        float hoursReserve = batteryStoredMWh / gridLoadMW;
        sb.AppendLine("Reserve: " + hoursReserve.ToString("0.0") + " hrs at current draw");

        float nightDraw = gridLoadMW * (float)(dayLengthHours * 0.5);
        sb.AppendLine("Est. overnight draw: " + FormatMWh(nightDraw));
        if (batteryStoredMWh < nightDraw)
            sb.AppendLine("! Insufficient reserves for overnight");
    }

    sb.AppendLine();
    sb.AppendLine("Day cycle: " + dayLengthHours + " hrs");
    return sb.ToString();
}

string BuildBreakdownText()
{
    var sb = new StringBuilder();
    sb.AppendLine("=== POWER BREAKDOWN ===");
    sb.AppendLine("Solar/Wind:        " + renewables.Count.ToString().PadLeft(3) + "  " + FormatMW(renewableMW));
    sb.AppendLine("Main Reactors:     " + mainReactors.Count.ToString().PadLeft(3) + "  " + FormatMW(reactorMW));
    sb.AppendLine("Backup Reactors:   " + backupReactors.Count.ToString().PadLeft(3));
    sb.AppendLine("Hydro Engines:     " + hydroEngines.Count.ToString().PadLeft(3) + "  " + FormatMW(hydroMW));
    sb.AppendLine("Main Batteries:    " + mainBatteries.Count.ToString().PadLeft(3) + "  " + FormatPct(batteryPct));
    sb.AppendLine("Backup Batteries:  " + backupBatteries.Count.ToString().PadLeft(3));
    sb.AppendLine("Non-Essentials:    " + nonEssentials.Count.ToString().PadLeft(3) + " blocks tagged");
    sb.AppendLine("Ship Docks:        " + shipDocks.Count.ToString().PadLeft(3) + "  " + dockedShips + " connected");
    sb.AppendLine();
    sb.AppendLine("Dry Run: " + (DRY_RUN_MODE ? "ON" : "OFF"));
    return sb.ToString();
}

void WriteLCDs(List<IMyTextPanel> panels, string content)
{
    for (int i = 0; i < panels.Count; i++)
    {
        panels[i].ContentType = ContentType.TEXT_AND_IMAGE;
        panels[i].WriteText(content);
    }
}

// ============================================================
// DEBUG
// ============================================================

void LogDebug(string message)
{
    Echo(message);
    for (int i = 0; i < debugLCDs.Count; i++)
        debugLCDs[i].WriteText("[" + tick.ToString().PadLeft(5) + "] " + message + "\n", true);
}

// ============================================================
// FORMATTERS
// ============================================================

string FormatMW(float mw)
{
    if (Math.Abs(mw) >= 1000f) return (mw / 1000f).ToString("0.00") + " GW";
    return mw.ToString("0.00") + " MW";
}

string FormatMWh(float mwh)
{
    if (Math.Abs(mwh) >= 1000f) return (mwh / 1000f).ToString("0.00") + " GWh";
    return mwh.ToString("0.00") + " MWh";
}

string FormatPct(float pct)
{
    return (pct * 100f).ToString("0.0") + "%";
}
