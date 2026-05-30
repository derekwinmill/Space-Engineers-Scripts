// Inventory Manager
// Version: 0.1 starter build
// Designed for Space Engineers programmable block
// Copy everything in this file into a Programmable Block.
//
// IMPORTANT:
// Mars Colony Inventory Manager.
// Start with DRY_RUN_MODE = true. Check debug/status LCDs first, then set it to false when ready.

// ====================
// USER CONFIGURATION
// ====================

const bool DRY_RUN_MODE = true;
const bool SPACE_MODE = false; 

const bool ENABLE_INVENTORY_SORTING = true;
const bool ENABLE_RESERVE_MONITORING = true;
const bool ENABLE_PRODUCTION_QUOTAS = true;
const bool ENABLE_SURPLUS_DISASSEMBLY = false;
const bool ENABLE_CONTRACT_SYSTEM = true;
const bool ENABLE_TRADE_SHIP_LOADING = false;
const bool ENABLE_LOADOUT_DOCK = false;
const bool ENABLE_SALVAGE_DOCK = false;
const bool ENABLE_REFINERY_ASSIGNMENT = true;
const bool ENABLE_STATUS_LIGHTS = true;
const bool ENABLE_BROADCASTS = true;
const bool ENABLE_DEBUG_LCD = true;

// ====================
// CROSS-SCRIPT INTEGRATION
// Tag the Programmable Blocks of the other scripts with these names
// so this script can send commands back to them.
// ====================

const string DEFENSE_PB_NAME   = "Base Defense PB";   
const string POWER_PB_NAME     = "Power Management PB"; 

// How much to multiply ammo quotas during combat. 2.0 = double normal quota.
const double COMBAT_AMMO_MULTIPLIER = 2.0;

// Runtime cadence. Keep these conservative for sim speed.
const int FULL_SCAN_INTERVAL_TICKS = 20;      // about every 2 seconds at Update10
const int SORT_INTERVAL_TICKS = 30;           // about every 3 seconds
const int PRODUCTION_INTERVAL_TICKS = 50;     // about every 5 seconds
const int LCD_INTERVAL_TICKS = 20;
const int RELAY_COOLDOWN_TICKS = 3000;        // about 5 minutes at Update10

const double ASSEMBLER_INPUT_DRAIN_THRESHOLD = 0.90;

// ====================
// BLOCK TAGS / GROUP NAMES
// ====================

const string TAG_ORE = "[Ore]";
const string TAG_INGOT = "[Ingot]";
const string TAG_COMPONENT = "[Component]";
const string TAG_AMMO = "[Ammo]";
const string TAG_TOOL = "[Tool]";
const string TAG_WEAPONS = "[Weapons]";
const string TAG_FUEL = "[Fuel]";
const string TAG_FOOD       = "[Food]";
const string TAG_IRRIGATION = "[Irrigation]"; 
const string TAG_EXPORT = "[Export]";
const string TAG_IMPORT = "[Import]";
const string TAG_RESERVE = "[Reserve]";
const string TAG_OVERFLOW = "[Overflow]";
const string TAG_QUARANTINE = "[Quarantine]";
const string TAG_DO_NOT_TRACK = "[DoNotTrack]";
const string TAG_NO_EXPORT = "[NoExport]";
const string TAG_NO_UNLOAD = "[NoUnload]";
const string TAG_NO_DRAIN = "[NoDrain]";
const string TAG_INPUT_ONLY = "[InputOnly]";
const string TAG_OUTPUT_ONLY = "[OutputOnly]";

const string TAG_TRADE_DOCK = "[TradeDock]";
const string TAG_TRADE_SHIP = "[TradeShip]";
const string TAG_SALVAGE_DOCK = "[SalvageDock]";
const string TAG_SALVAGE = "[Salvage]";
const string TAG_LOADOUT_DOCK = "[LoadoutDock]";
const string TAG_LOADOUT_SHIP = "[LoadoutShip]";
const string TAG_STAGED = "[Staged]";

const string TAG_CONTRACT_LCD = "[Contracts]";
const string TAG_CONTRACT_STATUS_LCD = "[ContractStatus]";
const string TAG_CONTRACT_HISTORY_LCD = "[ContractHistory]";
const string TAG_CONTRACT_RETURN = "[ContractReturn]";  
const string TAG_LOADOUT_LCD = "[Loadout]";
const string TAG_LOADOUT_STATUS_LCD = "[LoadoutStatus]";
const string TAG_STATUS_LCD = "[InventoryStatus]";
const string TAG_DEBUG_LCD = "[InventoryDebug]";
const string TAG_LEGEND_LCD = "[InventoryLegend]";
const string TAG_REFINERY_STATUS_LCD = "[RefineryStatus]";
const string TAG_PRODUCTION_SETTINGS_LCD = "[ProductionSettings]";

const string STATUS_LIGHT_GROUP_NAME = "InventoryStatusLights";
const string BROADCAST_BLOCK_TAG = "[InventoryBroadcast]"; 


const string IRON_ORE_LIGHT_GROUP = "IronOreStatusLights";
const string NICKEL_ORE_LIGHT_GROUP = "NickelOreStatusLights";
const string COBALT_ORE_LIGHT_GROUP = "CobaltOreStatusLights";
const string SILICON_ORE_LIGHT_GROUP = "SiliconOreStatusLights";
const string MAGNESIUM_ORE_LIGHT_GROUP = "MagnesiumOreStatusLights";

const string RELAY_IRON_ORE_NEEDED = "[Relay:Fe]";
const string RELAY_NICKEL_ORE_NEEDED = "[Relay:Ni]";
const string RELAY_COBALT_ORE_NEEDED = "[Relay:Co]";
const string RELAY_SILICON_ORE_NEEDED = "[Relay:Si]";
const string RELAY_MAGNESIUM_ORE_NEEDED = "[Relay:Mg]";

// ====================
// TRADE / LOADOUT SETTINGS
// ====================

const double TRADE_SHIP_FULL_THRESHOLD = 0.95;
const double LOADOUT_SHIP_FULL_THRESHOLD = 0.95;
const bool AUTO_CLEAR_CONTRACTS_ON_DEPARTURE = true;
const bool REQUIRE_READY_BEFORE_CLEARING_CONTRACTS = true;

// Rough items-per-second throughput used to estimate contract completion time.
// Adjust based on your assembler count and upgrade level.
const double ASSEMBLER_THROUGHPUT_PER_SECOND = 2.0;

// Fill target for loadout ship gas tanks when using loadout dock.
const double DEFAULT_LOADOUT_HYDROGEN_FILL = 0.80;
const double DEFAULT_LOADOUT_OXYGEN_FILL = 0.50;

// ====================
// RESERVE SETTINGS
// Item names should match common Space Engineers display names used in inventories.
// You can adjust these values freely.
// ====================

Dictionary<string, double> RESERVES = new Dictionary<string, double>()
{
    {"Steel Plate", 10000},
    {"Interior Plate", 5000},
    {"Construction Component", 3000},
    {"Motor", 1500},
    {"Computer", 1000},
    {"Large Steel Tube", 500},
    {"Small Steel Tube", 1000},
    {"Metal Grid", 500},
    {"Power Cell", 500},
    {"Display", 250},
    {"Medical Component", 100},
    {"Radio-communication Component", 50},

    {"Iron Ingot", 100000},
    {"Nickel Ingot", 25000},
    {"Cobalt Ingot", 15000},
    {"Silicon Wafer", 10000},
    {"Magnesium Powder", 5000},
    {"Silver Ingot", 3000},
    {"Gold Ingot", 3000},
    {"Platinum Ingot", 1000},
    {"Uranium Ingot", 500},

    {"Ice", 50000},
    {"Hydrogen Bottle", 10},
    {"Oxygen Bottle", 6},

    {"Food", 100},
    {"Seeds", 50},
};

// ====================
// PRODUCTION QUOTAS
// Minimum stock that assemblers should maintain.
// ====================

Dictionary<string, double> PRODUCTION_QUOTAS = new Dictionary<string, double>()
{
    {"Steel Plate", 15000},
    {"Interior Plate", 8000},
    {"Construction Component", 5000},
    {"Motor", 2500},
    {"Computer", 2000},
    {"Large Steel Tube", 1000},
    {"Small Steel Tube", 2000},
    {"Metal Grid", 1000},
    {"Power Cell", 1000},
    {"Display", 500},
    {"Medical Component", 200},
    {"Radio-communication Component", 150},
    {"Reactor Component", 150},
    {"Thrust Component", 400},
    {"Solar Cell", 300},
    {"Bulletproof Glass", 300},
    {"Girder", 500},

    {"5.56x45mm NATO magazine", 200},
    {"Missile 200mm", 50},
    {"Autocannon Magazine", 100},
    {"Assault Cannon Shell", 50},

    // Tools — tiers 1-4
    {"Welder", 4},
    {"Enhanced Welder", 0},
    {"Proficient Welder", 0},
    {"Elite Welder", 1},
    {"Grinder", 4},
    {"Enhanced Grinder", 0},
    {"Proficient Grinder", 0},
    {"Elite Grinder", 1},
    {"Hand Drill", 4},
    {"Enhanced Hand Drill", 0},
    {"Proficient Hand Drill", 0},
    {"Elite Hand Drill", 1},

    // Rifles
    {"Automatic Rifle", 4},
    {"Precise Automatic Rifle", 2},
    {"Rapid-Fire Automatic Rifle", 0},
    {"Elite Automatic Rifle", 0},

    // Pistols
    {"Semi-Auto Pistol", 0},
    {"Enhanced Semi-Auto Pistol", 0},
    {"Elite Pistol", 0},
    {"Full Auto Pistol", 0},
    {"Enhanced Full Auto Pistol", 0},

    // Launchers
    {"Basic Rocket Launcher", 0},
    {"Rocket Launcher", 0},

    // Additional ammo
    {"Artillery Shell", 0},
    {"Large Railgun Sabot", 0},
    {"Small Railgun Sabot", 0},
    {"S-10 Pistol Magazine", 0},
    {"MR-30E Pistol Magazine", 0},

    {"Hydrogen Bottle", 10},
    {"Oxygen Bottle", 6},
};

// ====================
// SURPLUS DISASSEMBLY MAXIMUMS
// Only items listed here will be automatically disassembled above the max.
// ====================

Dictionary<string, double> DISASSEMBLY_MAX = new Dictionary<string, double>()
{
    {"Steel Plate", 15000},
    {"Interior Plate", 8000},
    {"Construction Component", 5000},
    {"Motor", 2500},
    {"Computer", 2000},
    {"Large Steel Tube", 1000},
    {"Small Steel Tube", 2000},
    {"Metal Grid", 1000},
    {"Power Cell", 1000},
    {"Display", 500},
    {"Medical Component", 200},
    {"Radio-communication Component", 150},
    {"Reactor Component", 150},
    {"Thrust Component", 400},
    {"Solar Cell", 300},
    {"Bulletproof Glass", 300},
    {"Girder", 500},
};

// ====================
// ORE REFINERY SETTINGS
// Dedicated refineries: tag a refinery with one or more [Refine:X] tags.
//   e.g. "Refinery [Refine:Fe]" processes only iron.
//        "Refinery [Refine:Fe][Refine:Ni]" handles iron and nickel, choosing
//        whichever ingot is most below reserve at any given time.
// General refineries: no [Refine:] tag — automatically processes whichever
//   ore produces the ingot most below its reserve target. Unloads wrong ore
//   before switching so it responds to base needs dynamically.
// ====================

Dictionary<string, double> ORE_LOW_THRESHOLDS = new Dictionary<string, double>()
{
    {"Iron Ore", 10000},
    {"Nickel Ore", 5000},
    {"Cobalt Ore", 5000},
    {"Silicon Ore", 5000},
    {"Magnesium Ore", 2500},
    {"Silver Ore", 2500},
    {"Gold Ore", 2500},
    {"Platinum Ore", 1000},
    {"Uranium Ore", 1000},
};

Dictionary<string, string> REFINERY_TAG_BY_ORE = new Dictionary<string, string>()
{
    {"Iron Ore", "[Refine:Fe]"},
    {"Nickel Ore", "[Refine:Ni]"},
    {"Cobalt Ore", "[Refine:Co]"},
    {"Silicon Ore", "[Refine:Si]"},
    {"Magnesium Ore", "[Refine:Mg]"},
    {"Silver Ore", "[Refine:Ag]"},
    {"Gold Ore", "[Refine:Au]"},
    {"Platinum Ore", "[Refine:Pt]"},
    {"Uranium Ore", "[Refine:U]"},
};

// Maps ore types to the ingot they produce — used by general refineries to
// prioritise whichever ingot is furthest below its reserve target.
Dictionary<string, string> ORE_TO_INGOT = new Dictionary<string, string>()
{
    {"Iron Ore", "Iron Ingot"},
    {"Nickel Ore", "Nickel Ingot"},
    {"Cobalt Ore", "Cobalt Ingot"},
    {"Silicon Ore", "Silicon Wafer"},
    {"Magnesium Ore","Magnesium Powder"},
    {"Silver Ore", "Silver Ingot"},
    {"Gold Ore", "Gold Ingot"},
    {"Platinum Ore", "Platinum Ingot"},
    {"Uranium Ore", "Uranium Ingot"},
};

// ====================
// INTERNAL STATE
// ====================

int tick = 0;
bool contractsPaused = false;
bool contractsReady = false;
bool tradeWasConnected = false;
bool loadoutPaused = false;
string lastCommand = "none";
string currentStatus = "Idle";
string lastAlert = "None";

// ---- Combat Mode State ----
bool combatMode = false;

// ---- Multi-contract state ----
// Each entry is one named contract: name → (item → quantity).
// Contracts are parsed from all LCDs tagged [Contracts] or [Contract:Name].
// A plain [Contracts] LCD uses "Default" as the contract name.
List<ContractEntry> activeContracts = new List<ContractEntry>();
List<string> contractHistory = new List<string>();  

// Tracks what was loaded onto the ship this dock session per contract.
// Key = contractName, Value = (item → loaded amount).
Dictionary<string, Dictionary<string, double>> contractLoaded = new Dictionary<string, Dictionary<string, double>>();

// ---- LCD-configured production overrides ----
// Populated by ParseProductionSettingsLCD each scan cycle.
// Format on the [ProductionSettings] LCD: Item Name = min/max
//   min → production quota override, max → surplus-disassembly ceiling override.
//   Set either to 0 to disable that behaviour for the item.
Dictionary<string, double> lcdProductionMins  = new Dictionary<string, double>();
Dictionary<string, double> lcdProductionMaxes = new Dictionary<string, double>();

Dictionary<string, double> totals = new Dictionary<string, double>();
Dictionary<string, double> baseTotals = new Dictionary<string, double>();
Dictionary<string, double> loadoutRequests = new Dictionary<string, double>();
Dictionary<string, int> relayCooldowns = new Dictionary<string, int>();

// Populated by FindContainerForItem when an item's correct tag container is missing
// from the grid. Cleared and re-evaluated each LCD update cycle. Displayed on
// [InventoryDebug] so the player knows which storage containers need to be tagged.
HashSet<string> debugMissingTags = new HashSet<string>();

// ====================
// BLUEPRINT DICTIONARY
// Declared as a class-level field so it is allocated once at startup.
// BlueprintForItem() looks up entries here instead of rebuilding on every call.
// ====================
readonly Dictionary<string, string> BLUEPRINTS = new Dictionary<string, string>()
{
    // Components
    {"Steel Plate", "MyObjectBuilder_BlueprintDefinition/SteelPlate"},
    {"Interior Plate", "MyObjectBuilder_BlueprintDefinition/InteriorPlate"},
    {"Construction Component", "MyObjectBuilder_BlueprintDefinition/ConstructionComponent"},
    {"Motor", "MyObjectBuilder_BlueprintDefinition/MotorComponent"},
    {"Computer", "MyObjectBuilder_BlueprintDefinition/ComputerComponent"},
    {"Large Steel Tube", "MyObjectBuilder_BlueprintDefinition/LargeTube"},
    {"Small Steel Tube", "MyObjectBuilder_BlueprintDefinition/SmallTube"},
    {"Metal Grid", "MyObjectBuilder_BlueprintDefinition/MetalGrid"},
    {"Power Cell", "MyObjectBuilder_BlueprintDefinition/PowerCell"},
    {"Display", "MyObjectBuilder_BlueprintDefinition/Display"},
    {"Medical Component", "MyObjectBuilder_BlueprintDefinition/MedicalComponent"},
    {"Radio-communication Component", "MyObjectBuilder_BlueprintDefinition/RadioCommunicationComponent"},
    {"Reactor Component", "MyObjectBuilder_BlueprintDefinition/ReactorComponent"},
    {"Thrust Component", "MyObjectBuilder_BlueprintDefinition/ThrustComponent"},
    {"Solar Cell", "MyObjectBuilder_BlueprintDefinition/SolarCell"},
    {"Bulletproof Glass", "MyObjectBuilder_BlueprintDefinition/BulletproofGlass"},
    {"Girder", "MyObjectBuilder_BlueprintDefinition/Girder"},
    // Gas
    {"Hydrogen Bottle", "MyObjectBuilder_BlueprintDefinition/HydrogenBottle"},
    {"Oxygen Bottle", "MyObjectBuilder_BlueprintDefinition/OxygenBottle"},
    // Ammo
    {"5.56x45mm NATO magazine", "MyObjectBuilder_BlueprintDefinition/NATO_5p56x45mmMagazine"},
    {"Missile 200mm", "MyObjectBuilder_BlueprintDefinition/Missile200mm"},
    {"Autocannon Magazine", "MyObjectBuilder_BlueprintDefinition/AutocannonClip"},
    {"Assault Cannon Shell", "MyObjectBuilder_BlueprintDefinition/MediumCalibreAmmo"},
    {"Artillery Shell", "MyObjectBuilder_BlueprintDefinition/LargeCalibreAmmo"},
    {"Large Railgun Sabot", "MyObjectBuilder_BlueprintDefinition/LargeRailgunAmmo"},
    {"Small Railgun Sabot", "MyObjectBuilder_BlueprintDefinition/SmallRailgunAmmo"},
    {"S-10 Pistol Magazine", "MyObjectBuilder_BlueprintDefinition/SemiAutoPistolMagazine"},
    {"MR-30E Pistol Magazine", "MyObjectBuilder_BlueprintDefinition/FullAutoPistolMagazine"},
    // Tools — tiers 1-4
    {"Welder", "MyObjectBuilder_BlueprintDefinition/Welder"},
    {"Enhanced Welder", "MyObjectBuilder_BlueprintDefinition/Welder2"},
    {"Proficient Welder", "MyObjectBuilder_BlueprintDefinition/Welder3"},
    {"Elite Welder", "MyObjectBuilder_BlueprintDefinition/Welder4"},
    {"Grinder", "MyObjectBuilder_BlueprintDefinition/AngleGrinder"},
    {"Enhanced Grinder", "MyObjectBuilder_BlueprintDefinition/AngleGrinder2"},
    {"Proficient Grinder", "MyObjectBuilder_BlueprintDefinition/AngleGrinder3"},
    {"Elite Grinder", "MyObjectBuilder_BlueprintDefinition/AngleGrinder4"},
    {"Hand Drill", "MyObjectBuilder_BlueprintDefinition/HandDrill"},
    {"Enhanced Hand Drill", "MyObjectBuilder_BlueprintDefinition/HandDrill2"},
    {"Proficient Hand Drill", "MyObjectBuilder_BlueprintDefinition/HandDrill3"},
    {"Elite Hand Drill", "MyObjectBuilder_BlueprintDefinition/HandDrill4"},
    // Rifles
    {"Automatic Rifle", "MyObjectBuilder_BlueprintDefinition/AutomaticRifle"},
    {"Precise Automatic Rifle", "MyObjectBuilder_BlueprintDefinition/PreciseAutomaticRifle"},
    {"Rapid-Fire Automatic Rifle", "MyObjectBuilder_BlueprintDefinition/RapidFireAutomaticRifle"},
    {"Elite Automatic Rifle", "MyObjectBuilder_BlueprintDefinition/UltimateAutomaticRifle"},
    // Pistols
    {"Semi-Auto Pistol", "MyObjectBuilder_BlueprintDefinition/SemiAutoPistolItem"},
    {"Enhanced Semi-Auto Pistol", "MyObjectBuilder_BlueprintDefinition/EnhancedSemiAutoPistolItem"},
    {"Elite Pistol", "MyObjectBuilder_BlueprintDefinition/ElitePistolItem"},
    {"Full Auto Pistol", "MyObjectBuilder_BlueprintDefinition/FullAutoPistolItem"},
    {"Enhanced Full Auto Pistol", "MyObjectBuilder_BlueprintDefinition/EnhancedFullAutoPistolItem"},
    // Launchers
    {"Basic Rocket Launcher", "MyObjectBuilder_BlueprintDefinition/BasicHandHeldLauncherItem"},
    {"Rocket Launcher", "MyObjectBuilder_BlueprintDefinition/AdvancedHandHeldLauncherItem"},
};

List<IMyTerminalBlock> inventoryBlocks = new List<IMyTerminalBlock>();
List<IMyCargoContainer> cargoContainers = new List<IMyCargoContainer>();
List<IMyAssembler> assemblers = new List<IMyAssembler>();
List<IMyRefinery> refineries = new List<IMyRefinery>();
List<IMyShipConnector> connectors = new List<IMyShipConnector>();
List<IMyGasTank> gasTanks = new List<IMyGasTank>();
List<IMyTextPanel> textPanels = new List<IMyTextPanel>();
List<IMyLightingBlock> statusLights = new List<IMyLightingBlock>();
List<IMyTerminalBlock> broadcastBlocks = new List<IMyTerminalBlock>();

// ====================
// CONTRACT DATA STRUCTURE
// ====================

// Represents one named trade contract with its item list and metadata.
class ContractEntry
{
    public string Name;                          // e.g. "Alpha", "Default"
    public Dictionary<string, double> Items;     // item → quantity requested
    public string Deadline;                      // optional, parsed from # Deadline: comment
    public string Notes;                         // optional, parsed from # Notes: comment
    public bool Ready;                           // true when all items loaded onto ship
    public bool Paused;                          // per-contract pause

    public ContractEntry(string name)
    {
        Name    = name;
        Items   = new Dictionary<string, double>();
        Deadline = "";
        Notes    = "";
        Ready    = false;
        Paused   = false;
    }
}

Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    Echo("Mars Colony Inventory Manager loaded.");
}

void Save() { }

void Main(string argument, UpdateType updateSource)
{
    tick++;
    DecayCooldowns();

    if (!string.IsNullOrWhiteSpace(argument))
    {
        HandleCommand(argument.Trim().ToLower());
    }

    if (tick % FULL_SCAN_INTERVAL_TICKS == 1)
    {
        ScanBlocks();
        ScanInventories();
        ParseContractLCDs();
        ParseLoadoutLCD();
        ParseProductionSettingsLCD();
    }

    if (ENABLE_INVENTORY_SORTING && tick % SORT_INTERVAL_TICKS == 2)
    {
        FillIrrigationBlocks();
        SortInventories();
    }

    if (ENABLE_REFINERY_ASSIGNMENT && tick % SORT_INTERVAL_TICKS == 5)
    {
        ManageRefineries();
    }

    if (ENABLE_SALVAGE_DOCK && tick % SORT_INTERVAL_TICKS == 8)
    {
        ManageSalvageDock();
    }

    if (ENABLE_CONTRACT_SYSTEM && ENABLE_TRADE_SHIP_LOADING && tick % SORT_INTERVAL_TICKS == 11)
    {
        ManageContractsAndTradeShip();
    }

    if (ENABLE_LOADOUT_DOCK && tick % SORT_INTERVAL_TICKS == 14)
    {
        ManageLoadoutDock();
    }

    if (ENABLE_PRODUCTION_QUOTAS && tick % PRODUCTION_INTERVAL_TICKS == 3)
    {
        ManageProductionQuotas();
    }

    if (tick % SORT_INTERVAL_TICKS == 17)
    {
        DrainAssemblerInputs();
    }

    if (ENABLE_SURPLUS_DISASSEMBLY && tick % PRODUCTION_INTERVAL_TICKS == 13)
    {
        ManageSurplusDisassembly();
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

// ====================
// COMMANDS
// ====================

void HandleCommand(string cmd)
{
    lastCommand = cmd;

    if (cmd == "force sort")
    {
        ScanBlocks();
        ScanInventories();
        FillIrrigationBlocks();
        SortInventories();
        currentStatus = "Force sort complete";
    }
    else if (cmd == "pause contracts")
    {
        contractsPaused = true;
        currentStatus = "Contracts paused";
    }
    else if (cmd == "resume contracts")
    {
        contractsPaused = false;
        currentStatus = "Contracts resumed";
    }
    else if (cmd.StartsWith("pause contract:"))
    {
        // Pause a single named contract: "pause contract:Alpha"
        string name = cmd.Substring(15).Trim();
        SetContractPause(name, true);
    }
    else if (cmd.StartsWith("resume contract:"))
    {
        string name = cmd.Substring(16).Trim();
        SetContractPause(name, false);
    }
    else if (cmd.StartsWith("complete contract:"))
    {
        // Manually mark a named contract as complete and archive it.
        string name = cmd.Substring(18).Trim();
        CompleteContract(name);
    }
    else if (cmd == "complete contracts")
    {
        // Mark all ready contracts as complete.
        for (int ci = 0; ci < activeContracts.Count; ci++)
        {
            if (activeContracts[ci].Ready)
                CompleteContract(activeContracts[ci].Name);
        }
    }
    else if (cmd == "pause loadout")
    {
        loadoutPaused = true;
        currentStatus = "Loadout paused";
    }
    else if (cmd == "resume loadout")
    {
        loadoutPaused = false;
        currentStatus = "Loadout resumed";
    }
    else if (cmd == "reset contracts")
    {
        activeContracts.Clear();
        contractLoaded.Clear();
        contractsReady = false;
        currentStatus = "Contracts reset";
    }
    else if (cmd == "clear alerts")
    {
        lastAlert = "None";
        currentStatus = "Alerts cleared";
    }
    else if (cmd == "status")
    {
        ScanBlocks();
        ScanInventories();
        UpdateLCDs();
        currentStatus = "Status refreshed";
    }
    // ---- Combat Mode Commands (sent by Base_Defense PB) ----
    else if (cmd == "combat_mode")
    {
        EnterCombatMode();
    }
    else if (cmd == "clear_combat")
    {
        ExitCombatMode();
    }
    // ---- Power Management Commands (sent by Power_Management PB) ----
    else if (cmd == "power_critical")
    {
        EnterPowerEmergency();
    }
    else if (cmd == "power_ok")
    {
        ExitPowerEmergency();
    }
    else
    {
        lastAlert = "Unknown command: " + cmd;
    }
}

void SetContractPause(string name, bool paused)
{
    for (int ci = 0; ci < activeContracts.Count; ci++)
    {
        if (activeContracts[ci].Name == name)
        {
            activeContracts[ci].Paused = paused;
            currentStatus = "Contract '" + name + "' " + (paused ? "paused" : "resumed");
            return;
        }
    }
    lastAlert = "Contract not found: " + name;
}

void CompleteContract(string name)
{
    for (int ci = activeContracts.Count - 1; ci >= 0; ci--)
    {
        ContractEntry c = activeContracts[ci];
        if (c.Name != name) continue;

        string entry = "Contract '" + c.Name + "' — manually completed";
        if (c.Deadline.Length > 0) entry += " | Deadline: " + c.Deadline;
        contractHistory.Add(entry);
        foreach (var item in c.Items)
            contractHistory.Add("  " + item.Key + " x" + FormatAmount(item.Value));
        contractHistory.Add("---");
        while (contractHistory.Count > 30) contractHistory.RemoveAt(0);

        ClearNamedContractLCD(c.Name);
        activeContracts.RemoveAt(ci);
        contractLoaded.Remove(name);
        UpdateContractHistoryLCD();
        currentStatus = "Contract '" + name + "' completed and archived";
        return;
    }
    lastAlert = "Contract not found: " + name;
}

// ====================
// COMBAT MODE
// Called by Base_Defense via TryRun("combat_mode") / TryRun("clear_combat")
// ====================

void EnterCombatMode()
{
    if (combatMode) return;

    combatMode = true;
    contractsPaused = true;
    loadoutPaused   = true;

    // The combat multiplier is applied inside ManageProductionQuotas while combatMode is true.
    currentStatus = "COMBAT MODE - Ammo priority active";
    lastAlert     = "Under attack! Ammo quotas boosted x" + COMBAT_AMMO_MULTIPLIER;
    Echo(lastAlert);
}

void ExitCombatMode()
{
    if (!combatMode) return;

    combatMode = false;
    contractsPaused = false;
    loadoutPaused   = false;

    lastAlert     = "None";
    currentStatus = "Combat cleared - normal operations resumed";
    Echo(currentStatus);
}

// ---- Power Emergency State ----
// Tracks whether contracts/loadout were paused specifically by a power_critical signal,
// so power_ok can restore them without overriding user-initiated pauses.
bool powerPausedContracts = false;
bool powerPausedLoadout   = false;

void EnterPowerEmergency()
{
    if (!contractsPaused) { contractsPaused = true; powerPausedContracts = true; }
    if (!loadoutPaused)   { loadoutPaused   = true; powerPausedLoadout   = true; }

    lastAlert     = "Power critical - operations suspended by Power Management";
    currentStatus = "POWER EMERGENCY - contracts and loadout paused";
    Echo(lastAlert);
}

void ExitPowerEmergency()
{
    if (powerPausedContracts) { contractsPaused = false; powerPausedContracts = false; }
    if (powerPausedLoadout)   { loadoutPaused   = false; powerPausedLoadout   = false; }

    lastAlert     = "None";
    currentStatus = "Power restored - operations resumed";
    Echo(currentStatus);
}

bool IsAmmoItem(string itemName)
{
    return itemName.Contains("magazine") ||
           itemName.Contains("Magazine") ||
           itemName.Contains("Missile")  ||
           itemName.Contains("Shell")    ||
           itemName.Contains("Sabot")    ||
           itemName.Contains("Autocannon");
}

// Send a command string to another script's programmable block by name.
void SendToPB(string pbName, string command)
{
    IMyProgrammableBlock pb = GridTerminalSystem.GetBlockWithName(pbName) as IMyProgrammableBlock;
    if (pb == null)
    {
        Echo("WARNING: PB not found: " + pbName);
        return;
    }
    if (!DRY_RUN_MODE)
    {
        pb.TryRun(command);
    }
    else
    {
        Echo("[DryRun] Would send '" + command + "' to " + pbName);
    }
}

// ====================
// SCANNING
// ====================

void ScanBlocks()
{
    inventoryBlocks.Clear();
    cargoContainers.Clear();
    assemblers.Clear();
    refineries.Clear();
    connectors.Clear();
    gasTanks.Clear();
    textPanels.Clear();
    broadcastBlocks.Clear();
    statusLights.Clear();

    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(inventoryBlocks, b => b.HasInventory && IsUsableBlock(b));
    GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargoContainers, b => IsUsableBlock(b));
    GridTerminalSystem.GetBlocksOfType<IMyAssembler>(assemblers, b => IsUsableBlock(b));
    // Survival kits share IMyAssembler but cannot accept production queue items.
    for (int i = assemblers.Count - 1; i >= 0; i--)
    {
        if (IsExcludedAssembler(assemblers[i]))
            assemblers.RemoveAt(i);
    }
    GridTerminalSystem.GetBlocksOfType<IMyRefinery>(refineries, b => IsUsableBlock(b));
    GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(connectors, b => IsUsableBlock(b));
    GridTerminalSystem.GetBlocksOfType<IMyGasTank>(gasTanks, b => IsUsableBlock(b));
    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(textPanels, b => IsUsableBlock(b));
    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(broadcastBlocks, b => b.CustomName.Contains(BROADCAST_BLOCK_TAG));

    IMyBlockGroup lightGroup = GridTerminalSystem.GetBlockGroupWithName(STATUS_LIGHT_GROUP_NAME);
    if (lightGroup != null) lightGroup.GetBlocksOfType<IMyLightingBlock>(statusLights);
}

bool IsUsableBlock(IMyTerminalBlock b)
{
    if (b == null) return false;
    if (b.CustomName.Contains(TAG_DO_NOT_TRACK)) return false;
    return true;
}

void ScanInventories()
{
    totals.Clear();
    baseTotals.Clear();

    for (int i = 0; i < inventoryBlocks.Count; i++)
    {
        IMyTerminalBlock block = inventoryBlocks[i];
        bool excludedFromBase = IsExcludedFromAvailability(block);

        for (int invIndex = 0; invIndex < block.InventoryCount; invIndex++)
        {
            IMyInventory inv = block.GetInventory(invIndex);
            List<MyInventoryItem> items = new List<MyInventoryItem>();
            inv.GetItems(items);
            for (int j = 0; j < items.Count; j++)
            {
                string name = ItemDisplayName(items[j]);
                double amount = (double)items[j].Amount;
                AddAmount(totals, name, amount);
                if (!excludedFromBase) AddAmount(baseTotals, name, amount);
            }
        }
    }
}

bool IsExcludedFromAvailability(IMyTerminalBlock block)
{
    string n = block.CustomName;
    return n.Contains(TAG_TRADE_SHIP)   || n.Contains(TAG_LOADOUT_SHIP) ||
           n.Contains(TAG_STAGED)       || n.Contains(TAG_SALVAGE)      ||
           n.Contains(TAG_EXPORT)       || n.Contains(TAG_DO_NOT_TRACK) ||
           n.Contains(TAG_IRRIGATION);
}

void AddAmount(Dictionary<string, double> dict, string key, double amount)
{
    if (!dict.ContainsKey(key)) dict[key] = 0;
    dict[key] += amount;
}

string ItemDisplayName(MyInventoryItem item)
{
    string subtype = item.Type.SubtypeId;
    string type    = item.Type.TypeId;

    // TypeId in the SE PB API is the full type name e.g. "MyObjectBuilder_Ingot".
    // It does NOT contain a slash, so EndsWith("/Ingot") always fails.
    // Use Contains on the unqualified type name suffix instead.
    if (type.Contains("Ore"))
        return subtype + " Ore";

    if (type.Contains("Ingot"))
    {
        if (subtype == "Silicon")   return "Silicon Wafer";
        if (subtype == "Magnesium") return "Magnesium Powder";
        return subtype + " Ingot";
    }

    // Food and consumables — TypeId is MyObjectBuilder_ConsumableItem.
    if (type.Contains("ConsumableItem"))
    {
        if (subtype == "Organic")    return "Organic";
        if (subtype == "SpaceWheat") return "Space Wheat";
        if (subtype == "Mushroom")   return "Mushroom";
        if (subtype == "Redbeet")    return "Red Beet";
        if (subtype == "Carrot")     return "Carrot";
        if (subtype == "Tomato")     return "Tomato";
        if (subtype == "CookedMeat") return "Cooked Meat";
        if (subtype == "RawMeat")    return "Raw Meat";
        if (subtype == "Egg")        return "Egg";
        if (subtype == "Bread")      return "Bread";
        if (subtype == "Flour")      return "Flour";
        if (subtype == "Wheat")      return "Wheat";
        // Any other unmapped consumable still goes to food storage.
        return subtype;
    }

    // Component subtypes — matched by subtype name, no TypeId ambiguity here.
    if (subtype == "SteelPlate")           return "Steel Plate";
    if (subtype == "InteriorPlate")        return "Interior Plate";
    if (subtype == "Construction")         return "Construction Component";
    if (subtype == "MetalGrid")            return "Metal Grid";
    if (subtype == "LargeTube")            return "Large Steel Tube";
    if (subtype == "SmallTube")            return "Small Steel Tube";
    if (subtype == "Motor")                return "Motor";
    if (subtype == "Computer")             return "Computer";
    if (subtype == "Display")              return "Display";
    if (subtype == "Medical")              return "Medical Component";
    if (subtype == "RadioCommunication")   return "Radio-communication Component";
    if (subtype == "PowerCell")            return "Power Cell";
    if (subtype == "SolarCell")            return "Solar Cell";
    if (subtype == "BulletproofGlass")     return "Bulletproof Glass";
    if (subtype == "Girder")               return "Girder";
    if (subtype == "ReactorComponent")     return "Reactor Component";
    if (subtype == "ThrustComponent")      return "Thrust Component";
    if (subtype == "Explosives")           return "Explosives";
    if (subtype == "Detector")             return "Detector Components";
    if (subtype == "Canvas")               return "Canvas";
    if (subtype == "GravityGenerator")     return "Gravity Generator Components";
    if (subtype == "ZoneChip")             return "Zone Chip";
    if (subtype == "EngineerPlushie")      return "Engineer Plushie";
    if (subtype == "SabiroidPlushie")      return "Saberoid Plushie";
    // Gas bottles
    if (subtype == "HydrogenBottle")       return "Hydrogen Bottle";
    if (subtype == "OxygenBottle")         return "Oxygen Bottle";
    // Seeds — subtype match covers both ConsumableItem and GasContainerObject versions.
    if (subtype == "WheatSeeds")           return "Wheat Seeds";
    if (subtype == "MushroomSeeds")        return "Mushroom Seeds";
    if (subtype == "CarrotSeeds")          return "Carrot Seeds";
    if (subtype == "TomatoSeeds")          return "Tomato Seeds";
    if (subtype == "RedbeetSeeds")         return "Redbeet Seeds";
    // Ammo
    if (subtype == "NATO_5p56x45mm")          return "5.56x45mm NATO magazine";
    if (subtype == "NATO_25x184mm")           return "25x184mm NATO magazine";
    if (subtype == "Missile200mm")            return "Missile 200mm";
    if (subtype == "AutocannonClip")          return "Autocannon Magazine";
    if (subtype == "MediumCalibreAmmo")       return "Assault Cannon Shell";
    if (subtype == "LargeCalibreAmmo")        return "Artillery Shell";
    if (subtype == "LargeRailgunAmmo")        return "Large Railgun Sabot";
    if (subtype == "SmallRailgunAmmo")        return "Small Railgun Sabot";
    if (subtype == "SemiAutoPistolMagazine")  return "S-10 Pistol Magazine";
    if (subtype == "FullAutoPistolMagazine")  return "MR-30E Pistol Magazine";
    if (subtype == "AutocannonClipSm")        return "Small Autocannon Magazine";
    // Tools
    if (subtype == "Welder")               return "Welder";
    if (subtype == "Welder2")              return "Enhanced Welder";
    if (subtype == "Welder3")              return "Proficient Welder";
    if (subtype == "Welder4")              return "Elite Welder";
    if (subtype == "AngleGrinder")         return "Grinder";
    if (subtype == "AngleGrinder2")        return "Enhanced Grinder";
    if (subtype == "AngleGrinder3")        return "Proficient Grinder";
    if (subtype == "AngleGrinder4")        return "Elite Grinder";
    if (subtype == "HandDrill")            return "Hand Drill";
    if (subtype == "HandDrill2")           return "Enhanced Hand Drill";
    if (subtype == "HandDrill3")           return "Proficient Hand Drill";
    if (subtype == "HandDrill4")           return "Elite Hand Drill";
    // Rifles
    if (subtype == "AutomaticRifleGun" || subtype == "AutomaticRifle") return "Automatic Rifle";
    if (subtype == "PreciseAutomaticRifleGun" || subtype == "PreciseAutomaticRifle") return "Precise Automatic Rifle";
    if (subtype == "RapidFireAutomaticRifleGun" || subtype == "RapidFireAutomaticRifle") return "Rapid-Fire Automatic Rifle";
    if (subtype == "UltimateAutomaticRifleGun" || subtype == "UltimateAutomaticRifle") return "Elite Automatic Rifle";
    // Pistols
    if (subtype == "SemiAutoPistolItem")          return "Semi-Auto Pistol";
    if (subtype == "EnhancedSemiAutoPistolItem")  return "Enhanced Semi-Auto Pistol";
    if (subtype == "ElitePistolItem")             return "Elite Pistol";
    if (subtype == "FullAutoPistolItem")          return "Full Auto Pistol";
    if (subtype == "EnhancedFullAutoPistolItem")  return "Enhanced Full Auto Pistol";
    // Launchers
    if (subtype == "BasicHandHeldLauncherItem")    return "Basic Rocket Launcher";
    if (subtype == "AdvancedHandHeldLauncherItem") return "Rocket Launcher";
    return subtype;
}

// ====================
// LCD PARSING
// ====================

// Scans all text panels for contract LCDs.
// Supports two formats:
//   [Contracts]          — treated as a single contract named "Default"
//   [Contract:Name]      — named contract, e.g. [Contract:Alpha]
//
// LCD text format:
//   Steel Plate = 5000
//   Iron Ore = 20000
//   # Deadline: Before Sunset
//   # Notes: Priority delivery to Outpost 3
//   # Lines starting with # or // are comments, except Deadline/Notes which are parsed.
//
// Multiple named contract LCDs can be active simultaneously.
// Each is loaded independently to the trade ship in priority order (top to bottom within each).
void ParseContractLCDs()
{
    activeContracts.Clear();

    for (int i = 0; i < textPanels.Count; i++)
    {
        IMyTextPanel lcd = textPanels[i];
        string n = lcd.CustomName;

        string contractName = null;
        if (n.Contains(TAG_CONTRACT_LCD))
        {
            contractName = "Default";
        }
        else
        {
            // Scan for [Contract:Name] pattern
            int start = n.IndexOf("[Contract:");
            if (start >= 0)
            {
                int end = n.IndexOf("]", start);
                if (end > start)
                    contractName = n.Substring(start + 10, end - start - 10).Trim();
            }
        }

        if (contractName == null) continue;

        ContractEntry entry = new ContractEntry(contractName);
        string[] lines = lcd.GetText().Split('\n');
        for (int j = 0; j < lines.Length; j++)
        {
            string line = lines[j].Trim();
            if (line.Length == 0) continue;

            // Parse metadata comments
            if (line.StartsWith("# Deadline:") || line.StartsWith("// Deadline:"))
            {
                entry.Deadline = line.Substring(line.IndexOf(':') + 1).Trim();
                continue;
            }
            if (line.StartsWith("# Notes:") || line.StartsWith("// Notes:"))
            {
                entry.Notes = line.Substring(line.IndexOf(':') + 1).Trim();
                continue;
            }
            if (line.StartsWith("#") || line.StartsWith("//")) continue;

            int eq = line.IndexOf('=');
            if (eq < 0) continue;
            string item = line.Substring(0, eq).Trim();
            string qtyText = line.Substring(eq + 1).Trim().Replace(",", "");
            double qty;
            if (item.Length > 0 && double.TryParse(qtyText, out qty) && qty > 0)
                entry.Items[item] = qty;
        }

        if (entry.Items.Count > 0)
            activeContracts.Add(entry);
    }
}

void ParseLoadoutLCD()
{
    loadoutRequests.Clear();
    IMyTextPanel lcd = FindTextPanel(TAG_LOADOUT_LCD);
    if (lcd == null) return;
    ParseRequestText(lcd.GetText(), loadoutRequests);
}

// Reads a [ProductionSettings]-tagged LCD to configure per-item production min and
// disassembly max at runtime without recompiling the script.
// LCD format (one item per line):
//   Item Name = min/max
//   Welder = 8/20         (produce until 8, disassemble above 20)
//   Elite Automatic Rifle = 2/5
//   Autocannon Magazine = 0/500  (don't produce, but disassemble above 500)
// Lines starting with # or // are comments and are ignored.
void ParseProductionSettingsLCD()
{
    lcdProductionMins.Clear();
    lcdProductionMaxes.Clear();
    IMyTextPanel lcd = FindTextPanel(TAG_PRODUCTION_SETTINGS_LCD);
    if (lcd == null) return;

    string[] lines = lcd.GetText().Split('\n');
    for (int i = 0; i < lines.Length; i++)
    {
        string line = lines[i].Trim();
        if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("//")) continue;
        int eq = line.IndexOf('=');
        if (eq < 0) continue;
        string item = line.Substring(0, eq).Trim();
        if (item.Length == 0) continue;
        string vals = line.Substring(eq + 1).Trim();
        int slash = vals.IndexOf('/');
        if (slash < 0) continue;
        double min, max;
        string minPart = vals.Substring(0, slash).Trim();
        string maxPart = vals.Substring(slash + 1).Trim();
        if (minPart.Length > 0 && double.TryParse(minPart, out min))
            lcdProductionMins[item] = min;
        if (maxPart.Length > 0 && double.TryParse(maxPart, out max))
            lcdProductionMaxes[item] = max;
    }
}

void ParseRequestText(string text, Dictionary<string, double> output)
{
    string[] lines = text.Split('\n');
    for (int i = 0; i < lines.Length; i++)
    {
        string line = lines[i].Trim();
        if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("//")) continue;
        int eq = line.IndexOf('=');
        if (eq < 0) continue;
        string item = line.Substring(0, eq).Trim();
        string qtyText = line.Substring(eq + 1).Trim().Replace(",", "");
        double qty;
        if (double.TryParse(qtyText, out qty) && qty > 0)
        {
            output[item] = qty;
        }
    }
}

IMyTextPanel FindTextPanel(string tag)
{
    for (int i = 0; i < textPanels.Count; i++)
    {
        if (textPanels[i].CustomName.Contains(tag)) return textPanels[i];
    }
    return null;
}

// ====================
// IRRIGATION PRIORITY FILL
// ====================

// Fills [Irrigation]-tagged blocks with food/seeds before general sorting.
// Priority: [Food] containers first, then any drainable source.
// Irrigation blocks are excluded from CanDrainFrom so items stay put.
void FillIrrigationBlocks()
{
    if (DRY_RUN_MODE) return;

    List<IMyTerminalBlock> irrigBlocks = new List<IMyTerminalBlock>();
    for (int i = 0; i < inventoryBlocks.Count; i++)
        if (inventoryBlocks[i].CustomName.Contains(TAG_IRRIGATION))
            irrigBlocks.Add(inventoryBlocks[i]);

    if (irrigBlocks.Count == 0) return;

    // Collect all drainable food-type items across the base into a transfer list.
    // We pull from [Food] containers first, then any other drainable source.
    for (int bi = 0; bi < irrigBlocks.Count; bi++)
    {
        IMyTerminalBlock irrigBlock = irrigBlocks[bi];
        IMyInventory dest = irrigBlock.GetInventory(0);
        if (dest == null) continue;

        double freeVol = (double)(dest.MaxVolume - dest.CurrentVolume);
        if (freeVol <= 0) continue;

        // Try food-tagged containers first, then general drainable sources.
        for (int pass = 0; pass < 2 && freeVol > 0; pass++)
        {
            for (int si = 0; si < inventoryBlocks.Count && freeVol > 0; si++)
            {
                IMyTerminalBlock src = inventoryBlocks[si];
                if (src == irrigBlock) continue;

                // Pass 0: only pull from [Food] containers.
                // Pass 1: pull from any drainable source that has food items.
                bool isFoodContainer = src.CustomName.Contains(TAG_FOOD) ||
                                       src.CustomName.Contains("[Foods]");
                if (pass == 0 && !isFoodContainer) continue;
                if (pass == 1 && isFoodContainer)  continue;
                if (pass == 1 && !CanDrainFrom(src)) continue;

                for (int invIdx = 0; invIdx < src.InventoryCount && freeVol > 0; invIdx++)
                {
                    IMyInventory srcInv = src.GetInventory(invIdx);
                    List<MyInventoryItem> items = new List<MyInventoryItem>();
                    srcInv.GetItems(items);
                    for (int j = items.Count - 1; j >= 0 && freeVol > 0; j--)
                    {
                        if (TagForItem(ItemDisplayName(items[j])) != TAG_FOOD) continue;
                        srcInv.TransferItemTo(dest, j);
                        freeVol = (double)(dest.MaxVolume - dest.CurrentVolume);
                    }
                }
            }
        }
    }
}

// ====================
// INVENTORY SORTING
// ====================

void SortInventories()
{
    if (DRY_RUN_MODE)
    {
        currentStatus = "Dry run: sorting simulated";
        return;
    }

    for (int i = 0; i < inventoryBlocks.Count; i++)
    {
        IMyTerminalBlock source = inventoryBlocks[i];
        if (!CanDrainFrom(source)) continue;

        for (int invIndex = 0; invIndex < source.InventoryCount; invIndex++)
        {
            IMyInventory srcInv = source.GetInventory(invIndex);
            List<MyInventoryItem> items = new List<MyInventoryItem>();
            srcInv.GetItems(items);
            for (int j = items.Count - 1; j >= 0; j--)
            {
                MyInventoryItem item = items[j];
                string name = ItemDisplayName(item);
                IMyCargoContainer target = FindContainerForItem(name, source);
                if (target == null || target == source) continue;
                srcInv.TransferItemTo(target.GetInventory(0), j);
            }
        }
    }
}

bool CanDrainFrom(IMyTerminalBlock block)
{
    string n = block.CustomName;
    if (n.Contains(TAG_DO_NOT_TRACK)) return false;
    if (n.Contains(TAG_RESERVE))      return false;
    if (n.Contains(TAG_INPUT_ONLY))   return false;
    if (n.Contains(TAG_TRADE_SHIP))   return false;
    if (n.Contains(TAG_LOADOUT_SHIP)) return false;
    if (n.Contains(TAG_STAGED))       return false;
    if (n.Contains(TAG_EXPORT))       return false;
    // Do not drain food storage or irrigation blocks — food/seeds should only flow in.
    if (n.Contains(TAG_FOOD) || n.Contains("[Foods]")) return false;
    if (n.Contains(TAG_IRRIGATION)) return false;
    // Never drain weapon blocks — turrets, gatling guns, missile launchers, interior
    // turrets, and hand-held weapons held in suit/character inventory. Draining these
    // disarms the base defences every sort cycle.
    // IMyUserControllableGun covers all turret and fixed-weapon block types.
    if (block is IMyUserControllableGun) return false;
    return true;
}

IMyCargoContainer FindContainerForItem(string itemName, IMyTerminalBlock source)
{
    string tag = TagForItem(itemName);
    if (tag != null)
    {
        IMyCargoContainer target = FindCargoWithTag(tag);
        if (target != null) return target;
        // The correct tag container exists in the script but no container on the
        // grid has that tag. Log it so the player can see which tag is missing
        // rather than silently dumping into overflow.
        if (ENABLE_DEBUG_LCD)
            debugMissingTags.Add(tag + " (for " + itemName + ")");
    }
    // Fall through to overflow, then quarantine.
    IMyCargoContainer overflow = FindCargoWithTag(TAG_OVERFLOW);
    if (overflow != null) return overflow;
    return FindCargoWithTag(TAG_QUARANTINE);
}

string TagForItem(string itemName)
{
    if (itemName.EndsWith(" Ore") || itemName == "Stone")                               return TAG_ORE;
    if (itemName.EndsWith(" Ingot") || itemName == "Silicon Wafer"
                                    || itemName == "Magnesium Powder")                  return TAG_INGOT;
    if (itemName.Contains("Bottle") || itemName == "Ice")                               return TAG_FUEL;
    if (itemName.Contains("magazine") || itemName.Contains("Magazine") ||
        itemName.Contains("Missile")  || itemName.Contains("Shell")
                                      || itemName.Contains("Sabot"))                    return TAG_AMMO;
    if (itemName.Contains("Welder") || itemName.Contains("Grinder")
                                    || itemName.Contains("Drill"))                      return TAG_TOOL;
    if (itemName.Contains("Rifle")  || itemName.Contains("Pistol")
                                    || itemName.Contains("Launcher"))                   return TAG_WEAPONS;

    // Food — match all vanilla consumable items, seeds, and organic matter by name.
    // ItemDisplayName maps all ConsumableItem subtypes to their display names above,
    // so we match on those display names here.
    if (itemName == "Organic"     || itemName == "Space Wheat"  || itemName == "Wheat"    ||
        itemName == "Mushroom"    || itemName == "Red Beet"     || itemName == "Carrot"   ||
        itemName == "Tomato"      || itemName == "Cooked Meat"  || itemName == "Raw Meat" ||
        itemName == "Egg"         || itemName == "Bread"        || itemName == "Flour"    ||
        itemName == "Wheat Seeds" || itemName == "Mushroom Seeds" ||
        itemName == "Carrot Seeds"|| itemName == "Tomato Seeds" || itemName == "Redbeet Seeds" ||
        itemName == "Seeds"       || itemName == "Food")                                return TAG_FOOD;

    // Known component subtypes — explicit list prevents unrecognised items from
    // silently landing in [Component] storage. If you add modded components,
    // add their display names here.
    if (itemName == "Steel Plate"                  || itemName == "Interior Plate"          ||
        itemName == "Construction Component"       || itemName == "Metal Grid"              ||
        itemName == "Large Steel Tube"             || itemName == "Small Steel Tube"        ||
        itemName == "Motor"                        || itemName == "Computer"                ||
        itemName == "Display"                      || itemName == "Medical Component"       ||
        itemName == "Radio-communication Component"                                         ||
        itemName == "Power Cell"                   || itemName == "Solar Cell"              ||
        itemName == "Bulletproof Glass"            || itemName == "Girder"                  ||
        itemName == "Reactor Component"            || itemName == "Thrust Component"        ||
        itemName == "Explosives"                   || itemName == "Detector Components"     ||
        itemName == "Canvas"                       || itemName == "Gravity Generator Components" ||
        itemName == "Zone Chip"                    || itemName == "Engineer Plushie"        ||
        itemName == "Saberoid Plushie")                                                     return TAG_COMPONENT;

    // Unrecognised item — return null so FindContainerForItem logs it and routes
    // to [Overflow] rather than silently dumping into [Component].
    return null;
}

// Returns the tagged cargo container with the most remaining free space.
// Selecting by free space rather than list order means items naturally
// distribute across all same-tagged containers as they fill up, and a
// container carrying multiple tags (e.g. [Ammo][Tool][Weapons]) is eligible
// for every item type it declares and will receive whichever type has the
// most pressure at any given moment.
// Also accepts common plural variants: [Ingots] matches [Ingot], [Components]
// matches [Component], so minor naming differences don't silently break routing.
IMyCargoContainer FindCargoWithTag(string tag)
{
    // Build a list of acceptable tag variants for this tag.
    // Singular [Ingot] also accepts [Ingots], [Component] accepts [Components], etc.
    string tagAlt = null;
    if      (tag == TAG_INGOT)     tagAlt = "[Ingots]";
    else if (tag == TAG_COMPONENT) tagAlt = "[Components]";
    else if (tag == TAG_ORE)       tagAlt = "[Ores]";
    else if (tag == TAG_AMMO)      tagAlt = "[Ammos]";
    else if (tag == TAG_TOOL)      tagAlt = "[Tools]";
    else if (tag == TAG_FOOD)      tagAlt = "[Foods]";

    IMyCargoContainer best = null;
    double bestFree = -1;
    for (int i = 0; i < cargoContainers.Count; i++)
    {
        IMyCargoContainer c = cargoContainers[i];
        string n = c.CustomName;
        if (!n.Contains(tag) && (tagAlt == null || !n.Contains(tagAlt))) continue;
        if (n.Contains(TAG_OUTPUT_ONLY)) continue;
        IMyInventory inv = c.GetInventory(0);
        double free = (double)(inv.MaxVolume - inv.CurrentVolume);
        if (free > bestFree)
        {
            bestFree = free;
            best = c;
        }
    }
    return best;
}

// ====================
// CONTRACTS / TRADE SHIP
// ====================

void ManageContractsAndTradeShip()
{
    IMyShipConnector dock = FindConnector(TAG_TRADE_DOCK);
    bool connected = dock != null && dock.Status == MyShipConnectorStatus.Connected;

    // On disconnect: archive completed contracts, clear ready state, optionally wipe LCDs.
    if (tradeWasConnected && !connected)
    {
        HandleTradeShipDeparture();
    }
    tradeWasConnected = connected;

    if (contractsPaused)
    {
        currentStatus = "Contracts paused";
        return;
    }
    if (!connected)
    {
        contractLoaded.Clear();
        currentStatus = "Awaiting trade ship";
        return;
    }
    if (activeContracts.Count == 0)
    {
        currentStatus = "No active contracts";
        return;
    }

    List<IMyTerminalBlock> shipBlocks = GetConnectedInventoryBlocks(dock, TAG_TRADE_SHIP);
    if (shipBlocks.Count == 0)
    {
        currentStatus = "Trade ship connected — no [TradeShip] cargo found";
        return;
    }

    // On fresh dock, also unload any [ContractReturn] blocks (payment/reward items from last mission).
    UnloadContractReturn(dock);

    bool anyIncomplete = false;

    // Process each contract in order. Each gets a fair share of the ship's remaining space.
    for (int ci = 0; ci < activeContracts.Count; ci++)
    {
        ContractEntry contract = activeContracts[ci];
        if (contract.Paused) continue;

        if (!contractLoaded.ContainsKey(contract.Name))
            contractLoaded[contract.Name] = new Dictionary<string, double>();

        bool contractComplete = true;

        foreach (var req in contract.Items)
        {
            string item = req.Key;
            double needed = req.Value;

            double loaded = CountInBlocks(shipBlocks, item);
            double remaining = needed - loaded;

            // Track loaded amount for status display.
            contractLoaded[contract.Name][item] = loaded;

            if (remaining <= 0) continue;

            contractComplete = false;
            anyIncomplete = true;

            if (GetAverageCargoFill(shipBlocks) >= TRADE_SHIP_FULL_THRESHOLD)
            {
                lastAlert = "Trade ship full — contracts incomplete";
                Broadcast(lastAlert);
                goto doneLoading;
            }

            double available = AvailableForContract(item);
            if (available <= 0)
            {
                // Not enough in stock — queue production if it's a producible item.
                QueueProduction(item, remaining);
                continue;
            }

            MoveItemToBlocks(item, Math.Min(remaining, available), shipBlocks, false);
        }

        contract.Ready = contractComplete;
    }

    doneLoading:
    bool allReady = !anyIncomplete && activeContracts.Count > 0;
    contractsReady = allReady;
    if (allReady)
    {
        currentStatus = "All contracts loaded — ready for departure";
        Broadcast("Trade ship loaded. All contracts fulfilled and ready.");
    }
    else
    {
        currentStatus = "Loading contracts (" + GetContractProgressSummary() + ")";
    }
}

// Calculates how much of an item is available for contract loading:
// base stock minus reserves minus what's already committed to other active contracts.
double AvailableForContract(string itemName)
{
    double have = GetTotal(baseTotals, itemName);
    double reserve = RESERVES.ContainsKey(itemName) ? RESERVES[itemName] : 0;

    // Subtract quantities committed to other active contracts for this item.
    double committed = 0;
    for (int ci = 0; ci < activeContracts.Count; ci++)
    {
        ContractEntry c = activeContracts[ci];
        if (!c.Items.ContainsKey(itemName)) continue;
        // Only count other contracts' committed amounts, not the current one being evaluated.
        // We use the loaded tracking to know what's already on the ship.
        if (contractLoaded.ContainsKey(c.Name) && contractLoaded[c.Name].ContainsKey(itemName))
            committed += contractLoaded[c.Name][itemName];
    }

    return Math.Max(0, have - reserve - committed);
}

// Returns a short progress string like "2/3 contracts, 74% loaded" for the status LCD.
string GetContractProgressSummary()
{
    int readyCount = 0;
    double totalNeeded = 0;
    double totalLoaded = 0;

    for (int ci = 0; ci < activeContracts.Count; ci++)
    {
        ContractEntry c = activeContracts[ci];
        if (c.Ready) readyCount++;
        foreach (var req in c.Items)
        {
            totalNeeded += req.Value;
            if (contractLoaded.ContainsKey(c.Name) && contractLoaded[c.Name].ContainsKey(req.Key))
                totalLoaded += contractLoaded[c.Name][req.Key];
        }
    }

    int pct = totalNeeded > 0 ? (int)(totalLoaded / totalNeeded * 100) : 0;
    return readyCount + "/" + activeContracts.Count + " contracts, " + pct + "% loaded";
}

// When the trade ship undocks, archive completed contracts and optionally clear their LCDs.
void HandleTradeShipDeparture()
{
    for (int ci = 0; ci < activeContracts.Count; ci++)
    {
        ContractEntry c = activeContracts[ci];
        if (!c.Ready) continue;

        // Archive to history.
        string timestamp = "Contract '" + c.Name + "' — departed";
        if (c.Deadline.Length > 0) timestamp += " | Deadline: " + c.Deadline;
        contractHistory.Add(timestamp);
        foreach (var item in c.Items)
            contractHistory.Add("  " + item.Key + " x" + FormatAmount(item.Value));
        contractHistory.Add("---");

        // Trim history to last 30 entries.
        while (contractHistory.Count > 30)
            contractHistory.RemoveAt(0);

        UpdateContractHistoryLCD();
    }

    if (AUTO_CLEAR_CONTRACTS_ON_DEPARTURE)
    {
        // Only clear LCDs for contracts that were fully ready.
        // Incomplete contracts are left so you can finish them next trip.
        bool anyClear = false;
        for (int ci = 0; ci < activeContracts.Count; ci++)
        {
            if (!activeContracts[ci].Ready) continue;
            if (!REQUIRE_READY_BEFORE_CLEARING_CONTRACTS || activeContracts[ci].Ready)
            {
                ClearNamedContractLCD(activeContracts[ci].Name);
                anyClear = true;
            }
        }
        if (anyClear)
            Broadcast("Trade ship departed. Completed contract LCDs reset.");
    }

    contractLoaded.Clear();
    contractsReady = false;
}

// Unloads items from [ContractReturn]-tagged blocks on the connected construct into base storage.
// This handles incoming payment, reward items, or return cargo from the last mission.
void UnloadContractReturn(IMyShipConnector dock)
{
    if (DRY_RUN_MODE) return;
    List<IMyTerminalBlock> returnBlocks = GetConnectedInventoryBlocks(dock, TAG_CONTRACT_RETURN);
    if (returnBlocks.Count == 0) return;

    for (int i = 0; i < returnBlocks.Count; i++)
    {
        IMyTerminalBlock b = returnBlocks[i];
        for (int invIndex = 0; invIndex < b.InventoryCount; invIndex++)
        {
            IMyInventory inv = b.GetInventory(invIndex);
            List<MyInventoryItem> items = new List<MyInventoryItem>();
            inv.GetItems(items);
            for (int j = items.Count - 1; j >= 0; j--)
            {
                IMyCargoContainer target = FindContainerForItem(ItemDisplayName(items[j]), b);
                if (target != null) inv.TransferItemTo(target.GetInventory(0), j);
            }
        }
    }
}

// Clears the LCD for the named contract and writes back the blank template.
void ClearNamedContractLCD(string contractName)
{
    for (int i = 0; i < textPanels.Count; i++)
    {
        IMyTextPanel lcd = textPanels[i];
        string n = lcd.CustomName;
        bool isDefault = contractName == "Default" && n.Contains(TAG_CONTRACT_LCD);
        bool isNamed   = n.Contains("[Contract:" + contractName + "]");
        if (!isDefault && !isNamed) continue;
        if (!DRY_RUN_MODE)
            lcd.WriteText("# Contract: " + contractName + "\n# Deadline: \n# Notes: \n# Format: Item Name = Quantity\n\n");
    }
}

// ====================
// LOADOUT DOCK
// ====================

void ManageLoadoutDock()
{
    if (loadoutPaused) return;
    IMyShipConnector dock = FindConnector(TAG_LOADOUT_DOCK);
    if (dock == null || dock.Status != MyShipConnectorStatus.Connected) return;
    if (loadoutRequests.Count == 0) return;

    List<IMyTerminalBlock> shipBlocks = GetConnectedInventoryBlocks(dock, TAG_LOADOUT_SHIP);
    if (shipBlocks.Count == 0) return;

    foreach (var req in loadoutRequests)
    {
        double loaded = CountInBlocks(shipBlocks, req.Key);
        double needed = req.Value - loaded;
        if (needed <= 0) continue;
        if (GetAverageCargoFill(shipBlocks) >= LOADOUT_SHIP_FULL_THRESHOLD) break;
        double available = AvailableForUse(req.Key);
        if (available <= 0)
        {
            QueueProduction(req.Key, needed);
            continue;
        }
        MoveItemToBlocks(req.Key, Math.Min(needed, available), shipBlocks, true);
    }

    FillLoadoutGasTanks(dock);
}

void FillLoadoutGasTanks(IMyShipConnector dock)
{
    if (DRY_RUN_MODE) return;
    List<IMyTerminalBlock> sameConstruct = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyGasTank>(sameConstruct, b => b.CubeGrid == dock.CubeGrid || b.IsSameConstructAs(dock));
    for (int i = 0; i < sameConstruct.Count; i++)
    {
        IMyGasTank tank = sameConstruct[i] as IMyGasTank;
        if (tank == null || tank.CustomName.Contains(TAG_NO_DRAIN)) continue;
        if (tank.CustomName.Contains(TAG_LOADOUT_SHIP)) tank.Stockpile = true;
    }
}

// ====================
// SALVAGE DOCK
// ====================

void ManageSalvageDock()
{
    IMyShipConnector dock = FindConnector(TAG_SALVAGE_DOCK);
    if (dock == null || dock.Status != MyShipConnectorStatus.Connected) return;

    // Multi-dock guard: if the connected construct has [TradeShip] or [LoadoutShip]
    // cargo then this connector is currently serving another dock role and the
    // salvage unload must not run. This protects setups where one connector
    // carries multiple dock tags, and also catches accidental docking of a trade
    // or loadout ship to a salvage connector.
    List<IMyTerminalBlock> tradeCheck = GetConnectedInventoryBlocks(dock, TAG_TRADE_SHIP);
    List<IMyTerminalBlock> loadoutCheck = GetConnectedInventoryBlocks(dock, TAG_LOADOUT_SHIP);
    if (tradeCheck.Count > 0 || loadoutCheck.Count > 0)
    {
        currentStatus = "Salvage dock: trade/loadout ship detected — unload skipped";
        return;
    }

    List<IMyTerminalBlock> salvageBlocks = GetConnectedInventoryBlocks(dock, "");
    if (!DRY_RUN_MODE)
    {
        for (int i = 0; i < salvageBlocks.Count; i++)
        {
            IMyTerminalBlock b = salvageBlocks[i];
            if (b.CustomName.Contains(TAG_DO_NOT_TRACK) || b.CustomName.Contains(TAG_NO_UNLOAD)) continue;
            for (int invIndex = 0; invIndex < b.InventoryCount; invIndex++)
            {
                IMyInventory inv = b.GetInventory(invIndex);
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                inv.GetItems(items);
                for (int j = items.Count - 1; j >= 0; j--)
                {
                    IMyCargoContainer target = FindContainerForItem(ItemDisplayName(items[j]), b);
                    if (target != null) inv.TransferItemTo(target.GetInventory(0), j);
                }
            }
        }

        // Drain salvage tanks by putting salvage-grid tanks on stockpile and base tanks off stockpile.
        List<IMyGasTank> tanks = new List<IMyGasTank>();
        GridTerminalSystem.GetBlocksOfType<IMyGasTank>(tanks, t => t.IsSameConstructAs(dock));
        for (int i = 0; i < tanks.Count; i++)
        {
            IMyGasTank t = tanks[i];
            if (t.CustomName.Contains(TAG_NO_DRAIN)) continue;
            if (t.CubeGrid == dock.CubeGrid) t.Stockpile = false;
            else t.Stockpile = true;
        }
    }

    currentStatus = "Salvage dock active";
}

// ====================
// REFINERIES
// ====================

void ManageRefineries()
{
    for (int i = 0; i < refineries.Count; i++)
    {
        IMyRefinery r = refineries[i];
        List<string> dedicatedOres = GetDedicatedOres(r);
        if (dedicatedOres.Count > 0)
            ManageDedicatedRefinery(r, dedicatedOres);
        else
            ManageGeneralRefinery(r);
    }
}

// Returns the list of ore types this refinery is dedicated to (from its [Refine:X] tags).
// An empty list means the refinery is general-purpose.
List<string> GetDedicatedOres(IMyRefinery r)
{
    List<string> result = new List<string>();
    foreach (var kv in REFINERY_TAG_BY_ORE)
    {
        if (r.CustomName.Contains(kv.Value))
            result.Add(kv.Key);
    }
    return result;
}

// Dedicated refinery: picks the most-needed ore from its allowed set and loads it.
// Also ejects any ore that is not in this refinery's allowed set (e.g. manually dumped
// wrong ore) so it doesn't clog the input or get processed by the wrong machine.
void ManageDedicatedRefinery(IMyRefinery r, List<string> allowedOres)
{
    string bestOre = GetHighestNeedOre(allowedOres);

    if (!DRY_RUN_MODE)
    {
        // Always clean out any ore that doesn't belong in this refinery first,
        // regardless of whether we have a best ore to load. This catches manual
        // mis-dumps and wrong-ore contamination from conveyors.
        UnloadRefineryInputExceptAllowed(r, allowedOres);
    }

    if (bestOre == null) return;

    double threshold = ORE_LOW_THRESHOLDS.ContainsKey(bestOre) ? ORE_LOW_THRESHOLDS[bestOre] : 0;
    if (GetTotal(baseTotals, bestOre) < threshold)
    {
        TriggerOreAlert(bestOre);
        return;
    }

    if (!DRY_RUN_MODE)
        MoveItemToInventory(bestOre, 10000, r.GetInventory(0), false);
}

// General refinery: picks globally most-needed ore, unloads any other ore in the
// input first so the refinery switches immediately rather than burning through the queue.
void ManageGeneralRefinery(IMyRefinery r)
{
    string bestOre = GetHighestNeedOre(null);
    if (bestOre == null) return;

    if (!DRY_RUN_MODE)
    {
        UnloadRefineryInputExcept(r, bestOre);
        MoveItemToInventory(bestOre, 10000, r.GetInventory(0), false);
    }
}

// Picks the ore whose corresponding ingot is furthest below its RESERVES target,
// considering only ores above their ORE_LOW_THRESHOLDS minimum.
// Pass null for allowedOres to consider all ore types.
string GetHighestNeedOre(List<string> allowedOres)
{
    string best = null;
    double bestRatio = double.MaxValue;

    foreach (var kv in ORE_TO_INGOT)
    {
        string oreName  = kv.Key;
        string ingotName = kv.Value;

        if (allowedOres != null && !allowedOres.Contains(oreName)) continue;

        double oreHave = GetTotal(baseTotals, oreName);
        double oreMin  = ORE_LOW_THRESHOLDS.ContainsKey(oreName) ? ORE_LOW_THRESHOLDS[oreName] : 0;
        if (oreHave <= oreMin) continue; // not enough ore

        double ingotHave   = GetTotal(baseTotals, ingotName);
        double ingotTarget = RESERVES.ContainsKey(ingotName) ? RESERVES[ingotName] : 1;
        if (ingotTarget <= 0) ingotTarget = 1;
        double ratio = ingotHave / ingotTarget;

        if (ratio < bestRatio)
        {
            bestRatio = ratio;
            best = oreName;
        }
    }
    return best;
}

// Moves all ore types OTHER than keepOreName out of the refinery input and back to ore storage.
void UnloadRefineryInputExcept(IMyRefinery r, string keepOreName)
{
    IMyInventory input = r.GetInventory(0);
    List<MyInventoryItem> items = new List<MyInventoryItem>();
    input.GetItems(items);
    IMyCargoContainer oreStorage = FindCargoWithTag(TAG_ORE);
    for (int j = items.Count - 1; j >= 0; j--)
    {
        if (ItemDisplayName(items[j]) == keepOreName) continue;
        if (oreStorage != null)
            input.TransferItemTo(oreStorage.GetInventory(0), j);
    }
}

// Dedicated-refinery variant: ejects any ore NOT in the allowed set back to ore storage.
// Used to clean up manually mis-dumped or conveyor-fed wrong ore types.
void UnloadRefineryInputExceptAllowed(IMyRefinery r, List<string> allowedOres)
{
    IMyInventory input = r.GetInventory(0);
    List<MyInventoryItem> items = new List<MyInventoryItem>();
    input.GetItems(items);
    IMyCargoContainer oreStorage = FindCargoWithTag(TAG_ORE);
    for (int j = items.Count - 1; j >= 0; j--)
    {
        if (allowedOres.Contains(ItemDisplayName(items[j]))) continue;
        if (oreStorage != null)
            input.TransferItemTo(oreStorage.GetInventory(0), j);
    }
}

IMyRefinery FindRefinery(string tag)
{
    for (int i = 0; i < refineries.Count; i++)
    {
        if (refineries[i].CustomName.Contains(tag)) return refineries[i];
    }
    return null;
}

void TriggerOreAlert(string oreName)
{
    string msg = oreName + " low at refinery/outpost. Mine additional ore.";
    lastAlert = msg;
    Broadcast(msg);

    if (oreName == "Iron Ore") SetLightGroup(IRON_ORE_LIGHT_GROUP, Color.Red, true);
    else if (oreName == "Nickel Ore") SetLightGroup(NICKEL_ORE_LIGHT_GROUP, Color.Red, true);
    else if (oreName == "Cobalt Ore") SetLightGroup(COBALT_ORE_LIGHT_GROUP, Color.Red, true);
    else if (oreName == "Silicon Ore") SetLightGroup(SILICON_ORE_LIGHT_GROUP, Color.Red, true);
    else if (oreName == "Magnesium Ore") SetLightGroup(MAGNESIUM_ORE_LIGHT_GROUP, Color.Red, true);

    TriggerRelayForOre(oreName);
}

void TriggerRelayForOre(string oreName)
{
    string tag = "";
    if (oreName == "Iron Ore") tag = RELAY_IRON_ORE_NEEDED;
    else if (oreName == "Nickel Ore") tag = RELAY_NICKEL_ORE_NEEDED;
    else if (oreName == "Cobalt Ore") tag = RELAY_COBALT_ORE_NEEDED;
    else if (oreName == "Silicon Ore") tag = RELAY_SILICON_ORE_NEEDED;
    else if (oreName == "Magnesium Ore") tag = RELAY_MAGNESIUM_ORE_NEEDED;

    if (tag == "") return;
    if (relayCooldowns.ContainsKey(tag) && relayCooldowns[tag] > 0) return;

    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.SearchBlocksOfName(tag, blocks);
    for (int i = 0; i < blocks.Count; i++)
    {
        if (!DRY_RUN_MODE) blocks[i].ApplyAction("TriggerNow");
    }
    relayCooldowns[tag] = RELAY_COOLDOWN_TICKS;
}

void DecayCooldowns()
{
    List<string> keys = new List<string>(relayCooldowns.Keys);
    for (int i = 0; i < keys.Count; i++)
    {
        relayCooldowns[keys[i]] = Math.Max(0, relayCooldowns[keys[i]] - 1);
    }
}

// ====================
// ASSEMBLER INPUT DRAINING
// ====================

// Drains the input inventory (slot 0) of each assembler back to appropriate cargo when:
//   (a) the assembler is idle — clears leftover ingredients from a cancelled or
//       completed job so the next job can pull fresh components cleanly, or
//   (b) the input is >= ASSEMBLER_INPUT_DRAIN_THRESHOLD full — prevents the input
//       from becoming so packed that the assembler can no longer pull components
//       for the next queued item.
// Items are routed through FindContainerForItem so they land in the correct tagged
// cargo container (components → [Component], ingots → [Ingot], etc.).
// Assemblers tagged [DoNotTrack] are skipped entirely.
void DrainAssemblerInputs()
{
    if (DRY_RUN_MODE)
    {
        for (int i = 0; i < assemblers.Count; i++)
        {
            IMyAssembler a = assemblers[i];
            if (a.CustomName.Contains(TAG_DO_NOT_TRACK)) continue;
            if (IsExcludedAssembler(a)) continue;
            IMyInventory input = a.GetInventory(0);
            List<MyProductionItem> queue = new List<MyProductionItem>();
            a.GetQueue(queue);
            bool idle = queue.Count == 0;
            double fill = (double)input.CurrentVolume / (double)input.MaxVolume;
            if (idle || fill >= ASSEMBLER_INPUT_DRAIN_THRESHOLD)
                Echo("[DryRun] Would drain assembler input: " + a.CustomName +
                     " (idle=" + idle + ", fill=" + (fill * 100).ToString("0") + "%)");
        }
        return;
    }

    for (int i = 0; i < assemblers.Count; i++)
    {
        IMyAssembler a = assemblers[i];
        if (a.CustomName.Contains(TAG_DO_NOT_TRACK)) continue;
        if (IsExcludedAssembler(a)) continue;

        IMyInventory input = a.GetInventory(0);

        List<MyProductionItem> queue = new List<MyProductionItem>();
        a.GetQueue(queue);
        bool idle = queue.Count == 0;

        double fill = (double)input.CurrentVolume / (double)input.MaxVolume;

            if (!idle && fill < ASSEMBLER_INPUT_DRAIN_THRESHOLD) continue;

        List<MyInventoryItem> items = new List<MyInventoryItem>();
        input.GetItems(items);

        for (int j = items.Count - 1; j >= 0; j--)
        {
            string name = ItemDisplayName(items[j]);
            IMyCargoContainer target = FindContainerForItem(name, a);
            if (target == null) continue;
            input.TransferItemTo(target.GetInventory(0), j);
        }
    }
}

// ====================
// PRODUCTION / DISASSEMBLY
// ====================

void ManageProductionQuotas()
{
    // Build the union of hardcoded + LCD-configured item names to check.
    HashSet<string> items = new HashSet<string>(PRODUCTION_QUOTAS.Keys);
    foreach (var k in lcdProductionMins.Keys) items.Add(k);

    foreach (string item in items)
    {
        double quota = GetEffectiveProductionMin(item);
        if (quota <= 0) continue;

        // Scale ammo quotas during combat.
        if (combatMode && IsAmmoItem(item))
            quota *= COMBAT_AMMO_MULTIPLIER;

        double have = GetTotal(baseTotals, item);
        if (have < quota) QueueProduction(item, quota - have);
    }

    foreach (var c in activeContracts)
    {
        if (c.Paused) continue;
        foreach (var req in c.Items)
        {
            double available = AvailableForUse(req.Key);
            if (available < req.Value) QueueProduction(req.Key, req.Value - available);
        }
    }

    foreach (var l in loadoutRequests)
    {
        double available = AvailableForUse(l.Key);
        if (available < l.Value) QueueProduction(l.Key, l.Value - available);
    }
}

// LCD value takes precedence over hardcoded default.
double GetEffectiveProductionMin(string item)
{
    if (lcdProductionMins.ContainsKey(item)) return lcdProductionMins[item];
    if (PRODUCTION_QUOTAS.ContainsKey(item))  return PRODUCTION_QUOTAS[item];
    return 0;
}

double GetEffectiveDisassemblyMax(string item)
{
    if (lcdProductionMaxes.ContainsKey(item)) return lcdProductionMaxes[item];
    if (DISASSEMBLY_MAX.ContainsKey(item))     return DISASSEMBLY_MAX[item];
    return 0;
}

void QueueProduction(string itemName, double amount)
{
    if (amount <= 0) return;

    string blueprint = BlueprintForItem(itemName);
    if (blueprint == "")
    {
        lastAlert = "No blueprint for: " + itemName;
        return;
    }

    if (DRY_RUN_MODE) return;

    MyDefinitionId blueprintId;
    try { blueprintId = MyDefinitionId.Parse(blueprint); }
    catch { lastAlert = "Blueprint parse failed for: " + itemName; return; }

    string blueprintSubtype = blueprint.Contains("/") ?
        blueprint.Substring(blueprint.LastIndexOf('/') + 1) : blueprint;

    double alreadyQueued = 0;
    for (int i = 0; i < assemblers.Count; i++)
    {
        IMyAssembler a = assemblers[i];
        if (a.CustomName.Contains(TAG_DO_NOT_TRACK)) continue;
        if (a.CooperativeMode) continue;
        if (IsExcludedAssembler(a)) continue;
        List<MyProductionItem> queue = new List<MyProductionItem>();
        a.GetQueue(queue);
        for (int q = 0; q < queue.Count; q++)
            if (string.Equals(queue[q].BlueprintId.SubtypeName, blueprintSubtype,
                              StringComparison.OrdinalIgnoreCase))
                alreadyQueued += (double)queue[q].Amount;
    }

    double stillNeeded = amount - alreadyQueued;
    if (stillNeeded <= 0) return;

    IMyAssembler assembler = GetBestAssemblerForProduction();
    if (assembler == null) { lastAlert = "No assembler available for: " + itemName; return; }

    try { assembler.AddQueueItem(blueprintId, (VRage.MyFixedPoint)stillNeeded); }
    catch { lastAlert = "Could not queue: " + itemName; }
}

IMyAssembler GetBestAssemblerForProduction(bool disassembly = false)
{
    IMyAssembler best = null;
    int bestQueueDepth = int.MaxValue;

    for (int i = 0; i < assemblers.Count; i++)
    {
        IMyAssembler a = assemblers[i];
        if (a.CustomName.Contains(TAG_DO_NOT_TRACK)) continue;
        if (IsExcludedAssembler(a)) continue;

        if (disassembly)
        {
            if (a.Mode != MyAssemblerMode.Disassembly) continue;
        }
        else
        {
            if (a.Mode == MyAssemblerMode.Disassembly) continue;
            if (a.CooperativeMode) continue;
        }

        List<MyProductionItem> queue = new List<MyProductionItem>();
        a.GetQueue(queue);
        if (queue.Count < bestQueueDepth)
        {
            bestQueueDepth = queue.Count;
            best = a;
        }
    }
    return best;
}

// Returns true for assembler-interface blocks that are not valid for production
// queuing (survival kits, food processors, etc.). Only standard assembler subtypes pass.
bool IsExcludedAssembler(IMyAssembler a)
{
    string sub = a.BlockDefinition.SubtypeName;
    return sub != "LargeAssembler" &&
           sub != "BasicAssembler" &&
           sub != "LargeAssemblerIndustrial";
}

IMyAssembler GetPrimaryAssembler(bool disassembly)
{
    return GetBestAssemblerForProduction(disassembly);
}

void ManageSurplusDisassembly()
{
    if (combatMode) return;

    IMyAssembler disassembler = GetPrimaryAssembler(true);
    if (disassembler == null) return;

    // Union of hardcoded + LCD-configured items.
    HashSet<string> items = new HashSet<string>(DISASSEMBLY_MAX.Keys);
    foreach (var k in lcdProductionMaxes.Keys) items.Add(k);

    foreach (string item in items)
    {
        double maxVal = GetEffectiveDisassemblyMax(item);
        if (maxVal <= 0) continue; // 0 = don't auto-disassemble

        double have = GetTotal(baseTotals, item);
        if (have <= maxVal) continue;
        double excess = have - maxVal;
        string blueprint = BlueprintForItem(item);
        if (blueprint == "") continue;
        if (!DRY_RUN_MODE)
        {
            try
            {
                MyDefinitionId blueprintId = MyDefinitionId.Parse(blueprint);
                disassembler.Mode = MyAssemblerMode.Disassembly;
                disassembler.AddQueueItem(blueprintId, (VRage.MyFixedPoint)excess);
            }
            catch
            {
                lastAlert = "Could not queue disassembly for: " + item;
            }
        }
    }
}

// Blueprint lookup
string BlueprintForItem(string itemName)
{
    return BLUEPRINTS.ContainsKey(itemName) ? BLUEPRINTS[itemName] : "";
}

// ====================
// INVENTORY MOVEMENT HELPERS
// ====================

bool MoveItemToBlocks(string itemName, double amount, List<IMyTerminalBlock> targets, bool respectReserve)
{
    if (DRY_RUN_MODE) return true;
    double remaining = amount;

    for (int i = 0; i < inventoryBlocks.Count && remaining > 0; i++)
    {
        IMyTerminalBlock src = inventoryBlocks[i];
        if (!CanDrainFrom(src)) continue;
        if (respectReserve && AvailableForUse(itemName) <= 0) break;

        for (int invIndex = 0; invIndex < src.InventoryCount && remaining > 0; invIndex++)
        {
            IMyInventory inv = src.GetInventory(invIndex);
            List<MyInventoryItem> items = new List<MyInventoryItem>();
            inv.GetItems(items);
            for (int j = items.Count - 1; j >= 0 && remaining > 0; j--)
            {
                if (ItemDisplayName(items[j]) != itemName) continue;
                double move = Math.Min((double)items[j].Amount, remaining);
                for (int t = 0; t < targets.Count && move > 0; t++)
                {
                    IMyInventory targetInv = targets[t].GetInventory(0);
                    bool ok = inv.TransferItemTo(targetInv, j, null, true, (VRage.MyFixedPoint)move);
                    if (ok)
                    {
                        remaining -= move;
                        move = 0;
                    }
                }
            }
        }
    }
    return remaining <= 0;
}

bool MoveItemToInventory(string itemName, double amount, IMyInventory target, bool respectReserve)
{
    if (DRY_RUN_MODE) return true;
    double remaining = amount;
    for (int i = 0; i < inventoryBlocks.Count && remaining > 0; i++)
    {
        IMyTerminalBlock src = inventoryBlocks[i];
        if (!CanDrainFrom(src)) continue;
        for (int invIndex = 0; invIndex < src.InventoryCount && remaining > 0; invIndex++)
        {
            IMyInventory inv = src.GetInventory(invIndex);
            List<MyInventoryItem> items = new List<MyInventoryItem>();
            inv.GetItems(items);
            for (int j = items.Count - 1; j >= 0 && remaining > 0; j--)
            {
                if (ItemDisplayName(items[j]) != itemName) continue;
                double move = Math.Min((double)items[j].Amount, remaining);
                if (respectReserve) move = Math.Min(move, AvailableForUse(itemName));
                if (move <= 0) continue;
                bool ok = inv.TransferItemTo(target, j, null, true, (VRage.MyFixedPoint)move);
                if (ok) remaining -= move;
            }
        }
    }
    return remaining <= 0;
}

List<IMyTerminalBlock> GetConnectedInventoryBlocks(IMyShipConnector dock, string requiredTag)
{
    List<IMyTerminalBlock> result = new List<IMyTerminalBlock>();
    if (dock == null || dock.Status != MyShipConnectorStatus.Connected) return result;

    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(result, b =>
        b.HasInventory &&
        b.IsSameConstructAs(dock) &&
        b != dock &&
        !b.CustomName.Contains(TAG_DO_NOT_TRACK) &&
        (requiredTag == "" || b.CustomName.Contains(requiredTag))
    );
    return result;
}

IMyShipConnector FindConnector(string tag)
{
    for (int i = 0; i < connectors.Count; i++)
    {
        if (connectors[i].CustomName.Contains(tag)) return connectors[i];
    }
    return null;
}

double CountInBlocks(List<IMyTerminalBlock> blocks, string itemName)
{
    double total = 0;
    for (int i = 0; i < blocks.Count; i++)
    {
        for (int invIndex = 0; invIndex < blocks[i].InventoryCount; invIndex++)
        {
            IMyInventory inv = blocks[i].GetInventory(invIndex);
            List<MyInventoryItem> items = new List<MyInventoryItem>();
            inv.GetItems(items);
            for (int j = 0; j < items.Count; j++)
            {
                if (ItemDisplayName(items[j]) == itemName) total += (double)items[j].Amount;
            }
        }
    }
    return total;
}

double GetAverageCargoFill(List<IMyTerminalBlock> blocks)
{
    double current = 0;
    double max = 0;
    for (int i = 0; i < blocks.Count; i++)
    {
        for (int invIndex = 0; invIndex < blocks[i].InventoryCount; invIndex++)
        {
            IMyInventory inv = blocks[i].GetInventory(invIndex);
            current += (double)inv.CurrentVolume;
            max += (double)inv.MaxVolume;
        }
    }
    if (max <= 0) return 0;
    return current / max;
}

// Returns how much of an item is freely available: base stock minus reserve minus
// all quantities already committed across every active contract.
// Used by loadout, production quotas, and reserve checks to avoid over-counting stock.
double AvailableForUse(string itemName)
{
    double have = GetTotal(baseTotals, itemName);
    double reserve = RESERVES.ContainsKey(itemName) ? RESERVES[itemName] : 0;

    // Subtract the total contracted quantity across all active contracts so the
    // loadout dock and assembler quotas don't treat contracted stock as free.
    double contractCommitted = 0;
    for (int ci = 0; ci < activeContracts.Count; ci++)
    {
        ContractEntry c = activeContracts[ci];
        if (c.Items.ContainsKey(itemName))
            contractCommitted += c.Items[itemName];
    }

    return Math.Max(0, have - reserve - contractCommitted);
}

double GetTotal(Dictionary<string, double> dict, string key)
{
    return dict.ContainsKey(key) ? dict[key] : 0;
}

// ====================
// RESERVE CHECKS / STATUS
// ====================

void CheckReserves()
{
    // Accumulate all low reserves instead of stopping at the first one.
    // lastAlert shows the most critical item (furthest below reserve as a ratio),
    // but the status LCD WriteStatusLCD shows the full picture per item.
    string worstItem = null;
    double worstRatio = double.MaxValue;

    foreach (var r in RESERVES)
    {
        double have = GetTotal(baseTotals, r.Key);
        if (r.Value <= 0) continue;
        double ratio = have / r.Value;
        if (ratio < 1.0 && ratio < worstRatio)
        {
            worstRatio = ratio;
            worstItem  = r.Key;
        }
    }

    if (worstItem != null)
    {
        double have    = GetTotal(baseTotals, worstItem);
        double reserve = RESERVES[worstItem];
        string severity = worstRatio <= 0.25 ? "CRITICAL" : "LOW";
        lastAlert = "Reserve " + severity + ": " + worstItem +
                    " " + FormatAmount(have) + " / " + FormatAmount(reserve);
    }
    else if (lastAlert.StartsWith("Reserve"))
    {
        // All reserves recovered — clear the stale reserve alert.
        lastAlert = "None";
    }
}

// ====================
// LCD / LIGHT / BROADCAST OUTPUT
// ====================

void UpdateLCDs()
{
    WriteStatusLCD();
    WriteContractStatusLCD();
    WriteLoadoutStatusLCD();
    WriteDebugLCD();
    WriteLegendLCD();
    WriteRefineryStatusLCD();
}

void WriteStatusLCD()
{
    IMyTextPanel lcd = FindTextPanel(TAG_STATUS_LCD);
    if (lcd == null) return;
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("MARS COLONY INVENTORY");
    sb.AppendLine("====================");
    sb.AppendLine("Dry Run: " + (DRY_RUN_MODE ? "ON" : "OFF"));
    sb.AppendLine("Mode: " + (SPACE_MODE ? "SPACE" : "PLANETARY"));
    sb.AppendLine("Combat Mode: " + (combatMode ? "*** ACTIVE ***" : "Normal"));
    sb.AppendLine("Status: " + currentStatus);
    sb.AppendLine("Alert: " + lastAlert);
    sb.AppendLine();
    sb.AppendLine("KEY RESERVES");
    AppendReserveLine(sb, "Iron Ingot");
    AppendReserveLine(sb, "Nickel Ingot");
    AppendReserveLine(sb, "Cobalt Ingot");
    AppendReserveLine(sb, "Silicon Wafer");
    AppendReserveLine(sb, "Magnesium Powder");
    AppendReserveLine(sb, "Platinum Ingot");
    AppendReserveLine(sb, "Uranium Ingot");
    AppendReserveLine(sb, "Ice");
    AppendReserveLine(sb, "Steel Plate");
    AppendReserveLine(sb, "Construction Component");
    AppendReserveLine(sb, "Motor");
    AppendReserveLine(sb, "Computer");
    WritePanel(lcd, sb.ToString());
}

void AppendReserveLine(StringBuilder sb, string item)
{
    double have = GetTotal(baseTotals, item);
    double reserve = RESERVES.ContainsKey(item) ? RESERVES[item] : 0;
    string state = have >= reserve ? "OK" : (have <= reserve * 0.25 ? "CRIT" : "LOW");
    sb.AppendLine(item + ": " + state + " " + FormatAmount(have) + " / " + FormatAmount(reserve));
}

void WriteContractStatusLCD()
{
    IMyTextPanel lcd = FindTextPanel(TAG_CONTRACT_STATUS_LCD);
    if (lcd == null) return;

    StringBuilder sb = new StringBuilder();
    sb.AppendLine("CONTRACT STATUS");
    sb.AppendLine("===============");
    sb.AppendLine("Paused:  " + (contractsPaused ? "YES" : "NO"));
    sb.AppendLine("Overall: " + (contractsReady ? "READY" : "Loading"));

    IMyShipConnector dock = FindConnector(TAG_TRADE_DOCK);
    sb.AppendLine("Dock:    " + ConnectorStatusText(dock));
    sb.AppendLine();

    if (activeContracts.Count == 0)
    {
        sb.AppendLine("No active contracts.");
        WritePanel(lcd, sb.ToString());
        return;
    }

    // Per-contract breakdown with progress bars and ETA.
    for (int ci = 0; ci < activeContracts.Count; ci++)
    {
        ContractEntry c = activeContracts[ci];
        sb.AppendLine("--- " + c.Name + (c.Ready ? " [READY]" : c.Paused ? " [PAUSED]" : "") + " ---");
        if (c.Deadline.Length > 0) sb.AppendLine("Deadline: " + c.Deadline);
        if (c.Notes.Length > 0)    sb.AppendLine("Notes: " + c.Notes);

        double totalNeeded = 0;
        double totalLoaded = 0;
        double totalDeficit = 0; // items that need to be produced (not yet in stock)

        foreach (var req in c.Items)
        {
            string item   = req.Key;
            double needed = req.Value;
            double loaded = 0;
            if (contractLoaded.ContainsKey(c.Name) && contractLoaded[c.Name].ContainsKey(item))
                loaded = contractLoaded[c.Name][item];

            double inStock   = GetTotal(baseTotals, item);
            double available = AvailableForContract(item);
            double remaining = needed - loaded;
            double deficit   = Math.Max(0, needed - inStock);

            totalNeeded  += needed;
            totalLoaded  += loaded;
            totalDeficit += deficit;

            // Progress bar: 20 chars wide.
            int barWidth = 20;
            int filled   = (int)(loaded / needed * barWidth);
            string bar   = "[" + new string('#', filled) + new string('.', barWidth - filled) + "]";

            string itemStatus;
            if (remaining <= 0)
                itemStatus = "DONE";
            else if (available <= 0 && deficit > 0)
                itemStatus = "PRODUCING";
            else
                itemStatus = "Loading";

            sb.AppendLine(item);
            sb.AppendLine("  " + bar + " " + FormatAmount(loaded) + "/" + FormatAmount(needed) + " " + itemStatus);
            if (deficit > 0)
                sb.AppendLine("  Need to produce: " + FormatAmount(deficit));
        }

        // Contract-level progress and ETA.
        int pct = totalNeeded > 0 ? (int)(totalLoaded / totalNeeded * 100) : 100;
        sb.AppendLine("Progress: " + pct + "%");

        if (totalDeficit > 0)
        {
            double etaSeconds = totalDeficit / ASSEMBLER_THROUGHPUT_PER_SECOND;
            sb.AppendLine("Est. production: " + FormatDuration(etaSeconds));
        }

        sb.AppendLine();
    }

    WritePanel(lcd, sb.ToString());
}

// Formats a duration in seconds to a human-readable string like "1h 23m" or "45m 10s".
string FormatDuration(double seconds)
{
    int h = (int)(seconds / 3600);
    int m = (int)((seconds % 3600) / 60);
    int s = (int)(seconds % 60);
    if (h > 0) return h + "h " + m + "m";
    if (m > 0) return m + "m " + s + "s";
    return s + "s";
}

// Writes the contract history LCD with the last 30 archived contracts.
void UpdateContractHistoryLCD()
{
    IMyTextPanel lcd = FindTextPanel(TAG_CONTRACT_HISTORY_LCD);
    if (lcd == null) return;
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("CONTRACT HISTORY");
    sb.AppendLine("================");
    for (int i = contractHistory.Count - 1; i >= 0; i--)
        sb.AppendLine(contractHistory[i]);
    WritePanel(lcd, sb.ToString());
}

void WriteLoadoutStatusLCD()
{
    IMyTextPanel lcd = FindTextPanel(TAG_LOADOUT_STATUS_LCD);
    if (lcd == null) return;
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("OUTPOST LOADOUT");
    sb.AppendLine("===============");
    sb.AppendLine("Paused: " + (loadoutPaused ? "YES" : "NO"));
    IMyShipConnector dock = FindConnector(TAG_LOADOUT_DOCK);
    sb.AppendLine("Loadout Dock: " + ConnectorStatusText(dock));
    sb.AppendLine();
    foreach (var req in loadoutRequests)
    {
        sb.AppendLine(req.Key + ": need " + FormatAmount(req.Value) + ", avail " + FormatAmount(AvailableForUse(req.Key)));
    }
    if (loadoutRequests.Count == 0) sb.AppendLine("No active loadout.");
    WritePanel(lcd, sb.ToString());
}

void WriteDebugLCD()
{
    if (!ENABLE_DEBUG_LCD) return;
    IMyTextPanel lcd = FindTextPanel(TAG_DEBUG_LCD);
    if (lcd == null) return;

    IMyShipConnector trade = FindConnector(TAG_TRADE_DOCK);
    IMyShipConnector salvage = FindConnector(TAG_SALVAGE_DOCK);
    IMyShipConnector loadout = FindConnector(TAG_LOADOUT_DOCK);

    StringBuilder sb = new StringBuilder();
    sb.AppendLine("INVENTORY DEBUG");
    sb.AppendLine("===============");
    sb.AppendLine("Dry Run: " + (DRY_RUN_MODE ? "ENABLED" : "DISABLED"));
    sb.AppendLine("Space Mode: " + (SPACE_MODE ? "YES" : "NO"));
    sb.AppendLine("Combat Mode: " + (combatMode ? "ACTIVE" : "OFF"));
    sb.AppendLine("Last Command: " + lastCommand);
    sb.AppendLine("Last Alert: " + lastAlert);
    sb.AppendLine();

    // ---- Storage tag validation ----
    // Check that every required storage tag has at least one cargo container on the grid.
    // A missing tag means items of that type have nowhere to go and will land in overflow
    // or quarantine instead.
    string[] requiredTags = new string[]
    {
        TAG_ORE, TAG_INGOT, TAG_COMPONENT, TAG_AMMO, TAG_TOOL,
        TAG_WEAPONS, TAG_FUEL, TAG_FOOD, TAG_OVERFLOW
    };
    bool anyMissingStorage = false;
    sb.AppendLine("STORAGE TAG CHECK");
    for (int i = 0; i < requiredTags.Length; i++)
    {
        bool found = FindCargoWithTag(requiredTags[i]) != null;
        if (!found) anyMissingStorage = true;
        sb.AppendLine("  " + requiredTags[i] + ": " + (found ? "OK" : "*** MISSING ***"));
    }
    if (!anyMissingStorage) sb.AppendLine("  All storage tags present.");
    sb.AppendLine();

    // ---- Items routing to wrong container this cycle ----
    if (debugMissingTags.Count > 0)
    {
        sb.AppendLine("ROUTING WARNINGS (sent to overflow)");
        foreach (string entry in debugMissingTags)
            sb.AppendLine("  No container for: " + entry);
        sb.AppendLine();
    }
    debugMissingTags.Clear();

    sb.AppendLine("BLOCKS FOUND");
    sb.AppendLine("Inventory Blocks: " + inventoryBlocks.Count);
    sb.AppendLine("Cargo Containers: " + cargoContainers.Count);
    sb.AppendLine("Assemblers: " + assemblers.Count);
    sb.AppendLine("Refineries: " + refineries.Count);
    sb.AppendLine("Connectors: " + connectors.Count);
    sb.AppendLine("Status Lights: " + statusLights.Count);
    sb.AppendLine();
    sb.AppendLine("ASSEMBLER DETAIL");
    for (int i = 0; i < assemblers.Count; i++)
    {
        IMyAssembler a = assemblers[i];
        sb.AppendLine(a.CustomName);
        sb.AppendLine("  Sub=" + a.BlockDefinition.SubtypeName +
                      " Coop=" + a.CooperativeMode +
                      " Mode=" + a.Mode +
                      " Excl=" + IsExcludedAssembler(a));
    }
    sb.AppendLine();
    sb.AppendLine("DOCK DETECTION");
    sb.AppendLine("Trade Connector: " + ConnectorFoundText(trade));
    sb.AppendLine("Trade Ship Connected: " + ConnectedYesNo(trade));
    sb.AppendLine("Loadout Connector: " + ConnectorFoundText(loadout));
    sb.AppendLine("Loadout Rover Connected: " + ConnectedYesNo(loadout));
    sb.AppendLine("Salvage Connector: " + ConnectorFoundText(salvage));
    sb.AppendLine("Salvage Rover Connected: " + ConnectedYesNo(salvage));
    sb.AppendLine();
    sb.AppendLine("LCDS");
    sb.AppendLine("Contracts LCD: " + (FindTextPanel(TAG_CONTRACT_LCD) != null ? "FOUND" : "MISSING"));
    sb.AppendLine("Status LCD: " + (FindTextPanel(TAG_STATUS_LCD) != null ? "FOUND" : "MISSING"));
    sb.AppendLine("Debug LCD: FOUND");
    WritePanel(lcd, sb.ToString());
}

void WriteLegendLCD()
{
    IMyTextPanel lcd = FindTextPanel(TAG_LEGEND_LCD);
    if (lcd == null) return;
    string text = "INVENTORY STATUS LIGHTS\n" +
        "=======================\n" +
        "Green  = Ready / healthy\n" +
        "Yellow = Producing / sorting\n" +
        "Blue   = Docked / loading\n" +
        "Red    = Blocked / missing materials\n" +
        "Purple = Contracts or loadout paused\n" +
        "White  = Idle / waiting\n" +
        "Off    = Script stopped or no light group\n";
    WritePanel(lcd, text);
}

void WriteRefineryStatusLCD()
{
    IMyTextPanel lcd = FindTextPanel(TAG_REFINERY_STATUS_LCD);
    if (lcd == null) return;
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("REFINERY STATUS");
    sb.AppendLine("===============");
    foreach (var ore in ORE_LOW_THRESHOLDS)
    {
        double have = GetTotal(baseTotals, ore.Key);
        string state = have >= ore.Value ? "OK" : "LOW";
        sb.AppendLine(ore.Key + ": " + state + " " + FormatAmount(have) + " / " + FormatAmount(ore.Value));
    }
    WritePanel(lcd, sb.ToString());
}

void WritePanel(IMyTextPanel lcd, string text)
{
    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
    lcd.WriteText(text);
}

void UpdateStatusLights()
{
    if (!ENABLE_STATUS_LIGHTS || statusLights.Count == 0 || DRY_RUN_MODE) return;
    Color color = Color.White;
    bool on = true;

    if (lastAlert != "None") color = Color.Red;
    else if (contractsPaused || loadoutPaused) color = new Color(128, 0, 255);
    else if (contractsReady) color = Color.Green;
    else if (currentStatus.Contains("loading") || currentStatus.Contains("Fulfilling") || currentStatus.Contains("Salvage")) color = Color.Blue;
    else if (currentStatus.Contains("Producing") || currentStatus.Contains("sorting")) color = Color.Yellow;
    else color = Color.White;

    for (int i = 0; i < statusLights.Count; i++)
    {
        statusLights[i].Enabled = on;
        statusLights[i].Color = color;
        statusLights[i].BlinkIntervalSeconds = 1f;
        statusLights[i].BlinkLength = color == Color.Red ? 50f : 0f;
    }
}

void SetLightGroup(string groupName, Color color, bool enabled)
{
    IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(groupName);
    if (group == null) return;
    List<IMyLightingBlock> lights = new List<IMyLightingBlock>();
    group.GetBlocksOfType<IMyLightingBlock>(lights);
    if (DRY_RUN_MODE) return;
    for (int i = 0; i < lights.Count; i++)
    {
        lights[i].Enabled = enabled;
        lights[i].Color = color;
    }
}

void Broadcast(string message)
{
    if (!ENABLE_BROADCASTS) return;
    if (DRY_RUN_MODE) return;
    for (int i = 0; i < broadcastBlocks.Count; i++)
    {
        IMyRadioAntenna ant = broadcastBlocks[i] as IMyRadioAntenna;
        if (ant != null)
        {
            ant.HudText = message;
            ant.EnableBroadcasting = true;
            continue;
        }
        IMyBeacon beacon = broadcastBlocks[i] as IMyBeacon;
        if (beacon != null)
        {
            beacon.HudText = message;
            beacon.Enabled = true;
            continue;
        }
        IMyTextPanel panel = broadcastBlocks[i] as IMyTextPanel;
        if (panel != null) panel.WriteText(message);
    }
}

// ====================
// TEXT HELPERS
// ====================

string ConnectorFoundText(IMyShipConnector c)
{
    return c == null ? "MISSING" : "FOUND";
}

string ConnectedYesNo(IMyShipConnector c)
{
    return c != null && c.Status == MyShipConnectorStatus.Connected ? "YES" : "NO";
}

string ConnectorStatusText(IMyShipConnector c)
{
    if (c == null) return "MISSING";
    return c.Status.ToString();
}

string FormatAmount(double amount)
{
    if (amount >= 1000000) return (amount / 1000000d).ToString("0.0") + "M";
    if (amount >= 1000) return (amount / 1000d).ToString("0.0") + "k";
    return amount.ToString("0");
}