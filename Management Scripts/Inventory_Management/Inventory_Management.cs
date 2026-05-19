// Inventory Manager
// Version: Updated with ammo prioritization during attacks

// User Configuration for Attack Signal
const string ATTACK_SIGNAL = "under_attack";
const string CLEAR_ATTACK_SIGNAL = "clear_attack";

bool attackMode = false; // Tracks whether the base is under attack.

// ===============================================
// MAIN ENTRY POINT
// ===============================================
void Main(string argument, UpdateType updateSource)
{
    if (!string.IsNullOrWhiteSpace(argument))
    {
        HandleArgument(argument.Trim().ToLower());
    }

    // Existing periodic updates
    tick++;
    DecayCooldowns();

    // Standard runtime execution intervals...
    if (tick % FULL_SCAN_INTERVAL_TICKS == 1)
    {
        ScanBlocks();
        ScanInventories();
        ParseContractLCD();
        ParseLoadoutLCD();
    }

    if (ENABLE_INVENTORY_SORTING && tick % SORT_INTERVAL_TICKS == 2)
    {
        SortInventories();
    }

    if (ENABLE_PRODUCTION_QUOTAS && tick % PRODUCTION_INTERVAL_TICKS == 3)
    {
        ManageProductionQuotas();
    }

    if (ENABLE_RESERVE_MONITORING && tick % LCD_INTERVAL_TICKS == 4)
    {
        CheckReserves();
    }

    if (tick % LCD_INTERVAL_TICKS == 7)
    {
        UpdateLCDs();
        UpdateStatusLights();
    }

    Echo("Mars Colony Inventory Manager");
    Echo("Dry Run: " + (DRY_RUN_MODE ? "ON" : "OFF"));
    Echo("Status: " + currentStatus);
    Echo("Last Alert: " + lastAlert);
}

// ===============================================
// ARGUMENT HANDLING AND ATTACK MANAGEMENT
// ===============================================
void HandleArgument(string argument)
{
    if (argument == ATTACK_SIGNAL)
    {
        attackMode = true;
        LogMessage("Base under attack! Prioritizing ammo production.");
        PrioritizeAmmoProduction();
    }
    else if (argument == CLEAR_ATTACK_SIGNAL)
    {
        attackMode = false;
        LogMessage("Attack cleared. Restoring normal production.");
    }
    else
    {
        HandleCommand(argument); // Existing argument handler
    }
}

// ===============================================
// AMMO PRODUCTION PRIORITIZATION
// ===============================================
void PrioritizeAmmoProduction()
{
    var ammoBlueprints = new List<string>
    {
        "MyObjectBuilder_BlueprintDefinition/NATO_5p56x45mmMagazine",
        "MyObjectBuilder_BlueprintDefinition/Missile200mm",
        "MyObjectBuilder_BlueprintDefinition/AutocannonMagazine",
        "MyObjectBuilder_BlueprintDefinition/AssaultCannonShell"
    };

    foreach (var assembler in assemblers)
    {
        if (!assembler.IsQueueEmpty)
        {
            ClearAssemblerQueue(assembler);
        }

        foreach (var blueprint in ammoBlueprints)
        {
            assembler.AddQueueItem(MyDefinitionId.Parse(blueprint), 10); // Queue 10 units of each ammo type
        }

        LogMessage($"{assembler.CustomName}: Assigned to ammo production.");
    }
}

void RestoreNormalProduction()
{
    foreach (var assembler in assemblers)
    {
        if (!assembler.IsQueueEmpty)
        {
            ClearAssemblerQueue(assembler);
        }

        LogMessage($"{assembler.CustomName}: Restored to normal production.");
    }
}

void ClearAssemblerQueue(IMyAssembler assembler)
{
    while (!assembler.IsQueueEmpty)
    {
        assembler.RemoveQueueItem(0, assembler.GetQueue()[0].Amount);
    }
}

// ===============================================
// UTILITY FUNCTIONS
// ===============================================
void LogMessage(string message)
{
    var lcd = FindTextPanel(TAG_DEBUG_LCD);
    if (lcd != null)
    {
        lcd.WriteText(message + "\n", true);
    }
    Echo(message);
}

IMyTextPanel FindTextPanel(string tag)
{
    foreach (var panel in textPanels)
    {
        if (panel.CustomName.Contains(tag)) return panel;
    }
    return null;
}