// Inventory Manager
// Version: Updated to correctly prioritize ammo during attacks while maintaining component production.

// User Configuration for Attack Signal
const string ATTACK_SIGNAL = "under_attack";
const string CLEAR_ATTACK_SIGNAL = "clear_attack";

bool attackMode = false; // Tracks whether the base is under attack.
List<IMyAssembler> ammoAssemblers = new List<IMyAssembler>();
List<IMyAssembler> componentAssemblers = new List<IMyAssembler>();

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

    if (ENABLE_PRODUCTION_QUOTAS && tick % PRODUCTION_INTERVAL_TICKS == 3)
    {
        ManageProductionQuotas(); // Handle normal production quotas.
        if (attackMode)
        {
            ManageAmmoProduction(); // Handle ammo production during attack mode.
        }
    }

    UpdateLCDs();
    UpdateStatusLights();
}

// ===============================================
// ARGUMENT HANDLING
// ===============================================
void HandleArgument(string argument)
{
    if (argument == ATTACK_SIGNAL)
    {
        attackMode = true;
        LogMessage("Base under attack! Prioritizing ammo production.");
        AllocateAssemblersForAmmo();
    }
    else if (argument == CLEAR_ATTACK_SIGNAL)
    {
        attackMode = false;
        LogMessage("Attack cleared. Resuming full component production.");
        RestoreAssemblerAllocation();
    }
    else
    {
        HandleCommand(argument); // Existing argument handler
    }
}

// ===============================================
// AMMO PRODUCTION MANAGEMENT
// ===============================================
void AllocateAssemblersForAmmo()
{
    ammoAssemblers.Clear();
    componentAssemblers.Clear();

    // Divide assemblers: some for ammo, others for components.
    int splitPoint = assemblers.Count / 2; // 50% for ammo, 50% for components.
    for (int i = 0; i < assemblers.Count; i++)
    {
        if (i < splitPoint)
        {
            ammoAssemblers.Add(assemblers[i]);
        }
        else
        {
            componentAssemblers.Add(assemblers[i]);
        }
    }
    LogMessage("Assemblers allocated: Ammo = " + ammoAssemblers.Count + ", Components = " + componentAssemblers.Count);
}

void ManageAmmoProduction()
{
    var ammoBlueprints = new List<string>
    {
        "MyObjectBuilder_BlueprintDefinition/NATO_5p56x45mmMagazine",
        "MyObjectBuilder_BlueprintDefinition/Missile200mm",
        "MyObjectBuilder_BlueprintDefinition/AutocannonMagazine",
        "MyObjectBuilder_BlueprintDefinition/AssaultCannonShell"
    };

    foreach (var assembler in ammoAssemblers)
    {
        if (!assembler.IsQueueEmpty)
        {
            ClearAssemblerQueue(assembler); // Clear ongoing queues for dedicated ammo assemblers.
        }

        foreach (var blueprint in ammoBlueprints)
        {
            assembler.AddQueueItem(MyDefinitionId.Parse(blueprint), 10); // Queue 10 units of each ammo type.
        }
        LogMessage(assembler.CustomName + ": Producing ammo.");
    }
}

void RestoreAssemblerAllocation()
{
    // Clear ammo assemblers and return them to general use.
    foreach (var assembler in ammoAssemblers)
    {
        ClearAssemblerQueue(assembler);
        componentAssemblers.Add(assembler);
    }
    ammoAssemblers.Clear();
    LogMessage("Assembler allocation restored. All assemblers back to component production.");
}

void ClearAssemblerQueue(IMyAssembler assembler)
{
    while (!assembler.IsQueueEmpty)
    {
        assembler.RemoveQueueItem(0, assembler.GetQueue()[0].Amount);
    }
}

// ===============================================
// COMPONENT PRODUCTION MANAGEMENT
// ===============================================
void ManageProductionQuotas()
{
    foreach (var q in PRODUCTION_QUOTAS)
    {
        double have = GetTotal(baseTotals, q.Key);
        if (have < q.Value)
        {
            QueueProduction(q.Key, q.Value - have);
        }
    }

    foreach (var assembler in componentAssemblers)
    {
        // Ensure non-ammo assemblers continue with component production.
        LogMessage(assembler.CustomName + ": Assigned to component production.");
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