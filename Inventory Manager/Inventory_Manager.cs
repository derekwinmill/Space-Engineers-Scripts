// Inventory Manager
// Version: 0.1 starter build
// Designed for Space Engineers programmable block
// Copy everything in this file into a Programmable Block.
//
// IMPORTANT:
// This is an original inventory-management script designed around the discussed Mars colony logistics setup.
// Start with DRY_RUN_MODE = true. Check debug/status LCDs first, then set it to false when ready.

// ============================================================
// USER CONFIGURATION
// ============================================================

const bool DRY_RUN_MODE = true;
const bool SPACE_MODE = false; // false = planetary/Mars priorities, true = space/asteroid priorities

const bool ENABLE_INVENTORY_SORTING = true;
const bool ENABLE_RESERVE_MONITORING = true;
const bool ENABLE_PRODUCTION_QUOTAS = true;
const bool ENABLE_SURPLUS_DISASSEMBLY = true;
const bool ENABLE_CONTRACT_SYSTEM = true;
const bool ENABLE_TRADE_SHIP_LOADING = true;
const bool ENABLE_LOADOUT_DOCK = true;
const bool ENABLE_SALVAGE_DOCK = true;
const bool ENABLE_REFINERY_ASSIGNMENT = true;
const bool ENABLE_STATUS_LIGHTS = true;
const bool ENABLE_BROADCASTS = true;
const bool ENABLE_DEBUG_LCD = true;

// Runtime cadence. Keep these conservative for sim speed.
const int FULL_SCAN_INTERVAL_TICKS = 20;      // about every 2 seconds at Update10
const int SORT_INTERVAL_TICKS = 30;           // about every 3 seconds
const int PRODUCTION_INTERVAL_TICKS = 50;     // about every 5 seconds
const int LCD_INTERVAL_TICKS = 20;
const int RELAY_COOLDOWN_TICKS = 3000;        // about 5 minutes at Update10

// ============================================================
// BLOCK TAGS / GROUP NAMES
// ============================================================

const string TAG_ORE = "[Ore]";
const string TAG_INGOT = "[Ingot]";
const string TAG_COMPONENT = "[Component]";
const string TAG_AMMO = "[Ammo]";
const string TAG_TOOL = "[Tool]";
const string TAG_WEAPONS = "[Weapons]";
const string TAG_FUEL = "[Fuel]";
const string TAG_FOOD = "[Food]";
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
const string TAG_LOADOUT_LCD = "[Loadout]";
const string TAG_LOADOUT_STATUS_LCD = "[LoadoutStatus]";
const string TAG_STATUS_LCD = "[InventoryStatus]";
const string TAG_DEBUG_LCD = "[InventoryDebug]";
const string TAG_LEGEND_LCD = "[InventoryLegend]";
const string TAG_REFINERY_STATUS_LCD = "[RefineryStatus]";

const string STATUS_LIGHT_GROUP_NAME = "InventoryStatusLights";
const string BROADCAST_BLOCK_TAG = "[InventoryBroadcast]"; // antenna, beacon, or text panel name tag

// Optional ore alert light groups and action relays. Relay blocks are triggered with ApplyAction("TriggerNow") if supported.
const string IRON_ORE_LIGHT_GROUP = "IronOreStatusLights";
const string NICKEL_ORE_LIGHT_GROUP = "NickelOreStatusLights";
const string COBALT_ORE_LIGHT_GROUP = "CobaltOreStatusLights";
const string SILICON_ORE_LIGHT_GROUP = "SiliconOreStatusLights";
const string MAGNESIUM_ORE_LIGHT_GROUP = "MagnesiumOreStatusLights";

const string RELAY_IRON_ORE_NEEDED = "[Relay:Iron]";
const string RELAY_NICKEL_ORE_NEEDED = "[Relay:Nickel]";
const string RELAY_COBALT_ORE_NEEDED = "[Relay:Cobalt]";
const string RELAY_SILICON_ORE_NEEDED = "[Relay:Silicon]";
const string RELAY_MAGNESIUM_ORE_NEEDED = "[Relay:Magnesium]";

// ============================================================
// TRADE / LOADOUT SETTINGS
// ============================================================

const double TRADE_SHIP_FULL_THRESHOLD = 0.95;
const double LOADOUT_SHIP_FULL_THRESHOLD = 0.95;
const bool AUTO_CLEAR_CONTRACTS_ON_DEPARTURE = true;
const bool REQUIRE_READY_BEFORE_CLEARING_CONTRACTS = true;

// Fill target for loadout ship gas tanks when using loadout dock.
const double DEFAULT_LOADOUT_HYDROGEN_FILL = 0.80;
const double DEFAULT_LOADOUT_OXYGEN_FILL = 0.50;

// ============================================================
// RESERVE SETTINGS
// Item names should match common Space Engineers display names used in inventories.
// You can adjust these values freely.
// ============================================================

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

// ============================================================
// PRODUCTION QUOTAS
// Minimum stock that assemblers should maintain.
// ============================================================

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

    {"Welder", 4},
    {"Grinder", 4},
    {"Hand Drill", 4},
    {"Automatic Rifle", 4},
    {"Precise Automatic Rifle", 2},
    {"Elite Welder", 1},
    {"Elite Grinder", 1},
    {"Elite Hand Drill", 1},

    {"Hydrogen Bottle", 10},
    {"Oxygen Bottle", 6},
};

// ============================================================
// SURPLUS DISASSEMBLY MAXIMUMS
// Only items listed here will be automatically disassembled above the max.
// ============================================================

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

// ============================================================
// ORE REFINERY SETTINGS
// Refineries are assigned by name tag: [Refine:Iron], [Refine:Nickel], etc.
// The script loads only that ore type into that refinery.
// ============================================================

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
    {"Iron Ore", "[Refine:Iron]"},
    {"Nickel Ore", "[Refine:Nickel]"},
    {"Cobalt Ore", "[Refine:Cobalt]"},
    {"Silicon Ore", "[Refine:Silicon]"},
    {"Magnesium Ore", "[Refine:Magnesium]"},
    {"Silver Ore", "[Refine:Silver]"},
    {"Gold Ore", "[Refine:Gold]"},
    {"Platinum Ore", "[Refine:Platinum]"},
    {"Uranium Ore", "[Refine:Uranium]"},
};

// ============================================================
// INTERNAL STATE
// ============================================================

int tick = 0;
bool contractsPaused = false;
bool contractsReady = false;
bool tradeWasConnected = false;
bool loadoutPaused = false;
string lastCommand = "none";
string currentStatus = "Idle";
string lastAlert = "None";

Dictionary<string, double> totals = new Dictionary<string, double>();
Dictionary<string, double> baseTotals = new Dictionary<string, double>();
Dictionary<string, double> contractRequests = new Dictionary<string, double>();
Dictionary<string, double> loadoutRequests = new Dictionary<string, double>();
Dictionary<string, int> relayCooldowns = new Dictionary<string, int>();

List<IMyTerminalBlock> inventoryBlocks = new List<IMyTerminalBlock>();
List<IMyCargoContainer> cargoContainers = new List<IMyCargoContainer>();
List<IMyAssembler> assemblers = new List<IMyAssembler>();
List<IMyRefinery> refineries = new List<IMyRefinery>();
List<IMyShipConnector> connectors = new List<IMyShipConnector>();
List<IMyGasTank> gasTanks = new List<IMyGasTank>();
List<IMyTextPanel> textPanels = new List<IMyTextPanel>();
List<IMyLightingBlock> statusLights = new List<IMyLightingBlock>();
List<IMyTerminalBlock> broadcastBlocks = new List<IMyTerminalBlock>();

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
        ParseContractLCD();
        ParseLoadoutLCD();
    }

    if (ENABLE_INVENTORY_SORTING && tick % SORT_INTERVAL_TICKS == 2)
    {
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

// ============================================================
// COMMANDS
// ============================================================

void HandleCommand(string cmd)
{
    lastCommand = cmd;

    if (cmd == "force sort")
    {
        ScanBlocks();
        ScanInventories();
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
        if (!DRY_RUN_MODE) ClearContractLCD();
        contractRequests.Clear();
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
    else
    {
        lastAlert = "Unknown command: " + cmd;
    }
}

// ============================================================
// SCANNING
// ============================================================

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
    return n.Contains(TAG_TRADE_SHIP) || n.Contains(TAG_LOADOUT_SHIP) || n.Contains(TAG_STAGED) || n.Contains(TAG_SALVAGE) || n.Contains(TAG_EXPORT) || n.Contains(TAG_DO_NOT_TRACK);
}

void AddAmount(Dictionary<string, double> dict, string key, double amount)
{
    if (!dict.ContainsKey(key)) dict[key] = 0;
    dict[key] += amount;
}

string ItemDisplayName(MyInventoryItem item)
{
    string subtype = item.Type.SubtypeId;
    string type = item.Type.TypeId;

    if (type.EndsWith("/Ore")) return subtype + " Ore";
    if (type.EndsWith("/Ingot"))
    {
        if (subtype == "Silicon") return "Silicon Wafer";
        if (subtype == "Magnesium") return "Magnesium Powder";
        return subtype + " Ingot";
    }

    string friendly = subtype;
    friendly = friendly.Replace("SteelPlate", "Steel Plate");
    friendly = friendly.Replace("InteriorPlate", "Interior Plate");
    friendly = friendly.Replace("Construction", "Construction Component");
    friendly = friendly.Replace("Computer", "Computer");
    friendly = friendly.Replace("Motor", "Motor");
    friendly = friendly.Replace("Display", "Display");
    friendly = friendly.Replace("Medical", "Medical Component");
    friendly = friendly.Replace("RadioCommunication", "Radio-communication Component");
    friendly = friendly.Replace("LargeTube", "Large Steel Tube");
    friendly = friendly.Replace("SmallTube", "Small Steel Tube");
    friendly = friendly.Replace("MetalGrid", "Metal Grid");
    friendly = friendly.Replace("PowerCell", "Power Cell");
    friendly = friendly.Replace("NATO_5p56x45mm", "5.56x45mm NATO magazine");
    friendly = friendly.Replace("Missile200mm", "Missile 200mm");
    friendly = friendly.Replace("HydrogenBottle", "Hydrogen Bottle");
    friendly = friendly.Replace("OxygenBottle", "Oxygen Bottle");
    return friendly;
}

// ============================================================
// LCD PARSING
// ============================================================

void ParseContractLCD()
{
    contractRequests.Clear();
    IMyTextPanel lcd = FindTextPanel(TAG_CONTRACT_LCD);
    if (lcd == null) return;
    ParseRequestText(lcd.GetText(), contractRequests);
}

void ParseLoadoutLCD()
{
    loadoutRequests.Clear();
    IMyTextPanel lcd = FindTextPanel(TAG_LOADOUT_LCD);
    if (lcd == null) return;
    ParseRequestText(lcd.GetText(), loadoutRequests);
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

// ============================================================
// INVENTORY SORTING
// ============================================================

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
    if (n.Contains(TAG_RESERVE)) return false;
    if (n.Contains(TAG_INPUT_ONLY)) return false;
    if (n.Contains(TAG_TRADE_SHIP)) return false;
    if (n.Contains(TAG_LOADOUT_SHIP)) return false;
    if (n.Contains(TAG_STAGED)) return false;
    if (n.Contains(TAG_EXPORT)) return false;
    if (n.Contains(TAG_FOOD)) return false;
    return true;
}

IMyCargoContainer FindContainerForItem(string itemName, IMyTerminalBlock source)
{
    string tag = TagForItem(itemName);
    IMyCargoContainer target = FindCargoWithTag(tag);
    if (target != null) return target;
    target = FindCargoWithTag(TAG_OVERFLOW);
    if (target != null) return target;
    return FindCargoWithTag(TAG_QUARANTINE);
}

string TagForItem(string itemName)
{
    if (itemName.EndsWith(" Ore") || itemName == "Stone") return TAG_ORE;
    if (itemName.EndsWith(" Ingot") || itemName == "Silicon Wafer" || itemName == "Magnesium Powder") return TAG_INGOT;
    if (itemName.Contains("Bottle") || itemName == "Ice") return TAG_FUEL;
    if (itemName.Contains("magazine") || itemName.Contains("Missile") || itemName.Contains("Shell")) return TAG_AMMO;
    if (itemName.Contains("Welder") || itemName.Contains("Grinder") || itemName.Contains("Drill")) return TAG_TOOL;
    if (itemName.Contains("Rifle")) return TAG_WEAPONS;
    if (itemName == "Food" || itemName == "Seeds") return TAG_FOOD;
    return TAG_COMPONENT;
}

IMyCargoContainer FindCargoWithTag(string tag)
{
    for (int i = 0; i < cargoContainers.Count; i++)
    {
        IMyCargoContainer c = cargoContainers[i];
        if (c.CustomName.Contains(tag) && !c.CustomName.Contains(TAG_OUTPUT_ONLY)) return c;
    }
    return null;
}

// ============================================================
// CONTRACTS / TRADE SHIP
// ============================================================

void ManageContractsAndTradeShip()
{
    IMyShipConnector dock = FindConnector(TAG_TRADE_DOCK);
    bool connected = dock != null && dock.Status == MyShipConnectorStatus.Connected;

    if (tradeWasConnected && !connected)
    {
        if (AUTO_CLEAR_CONTRACTS_ON_DEPARTURE && (!REQUIRE_READY_BEFORE_CLEARING_CONTRACTS || contractsReady))
        {
            if (!DRY_RUN_MODE) ClearContractLCD();
            contractRequests.Clear();
            contractsReady = false;
            Broadcast("Trade ship departed. Contract LCD reset.");
        }
    }
    tradeWasConnected = connected;

    if (contractsPaused)
    {
        currentStatus = "Contracts paused";
        return;
    }
    if (!connected)
    {
        currentStatus = "Awaiting trade ship";
        return;
    }
    if (contractRequests.Count == 0)
    {
        currentStatus = "No active contracts";
        return;
    }

    List<IMyTerminalBlock> shipBlocks = GetConnectedInventoryBlocks(dock, TAG_TRADE_SHIP);
    if (shipBlocks.Count == 0)
    {
        currentStatus = "Trade ship connected, no [TradeShip] cargo found";
        return;
    }

    bool allLoaded = true;
    foreach (var req in contractRequests)
    {
        double loaded = CountInBlocks(shipBlocks, req.Key);
        double needed = req.Value - loaded;
        if (needed <= 0) continue;
        allLoaded = false;

        if (GetAverageCargoFill(shipBlocks) >= TRADE_SHIP_FULL_THRESHOLD)
        {
            lastAlert = "Trade ship full before contracts complete";
            Broadcast(lastAlert);
            break;
        }

        double available = AvailableForUse(req.Key);
        if (available <= 0)
        {
            QueueProduction(req.Key, needed);
            continue;
        }

        double moveAmount = Math.Min(needed, available);
        MoveItemToBlocks(req.Key, moveAmount, shipBlocks, true);
    }

    if (allLoaded)
    {
        contractsReady = true;
        currentStatus = "Trade contracts loaded";
        Broadcast("Trade ship loaded. Acquisition contracts ready for delivery.");
    }
    else
    {
        contractsReady = false;
        currentStatus = "Fulfilling trade contracts";
    }
}

void ClearContractLCD()
{
    IMyTextPanel lcd = FindTextPanel(TAG_CONTRACT_LCD);
    if (lcd == null) return;
    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
    lcd.WriteText("# Enter acquisition contracts below\n# Format: Item Name = Quantity\n\nSteel Plate = \nConstruction Component = \nMotor = \nComputer = \n");
}

// ============================================================
// LOADOUT DOCK
// ============================================================

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

// ============================================================
// SALVAGE DOCK
// ============================================================

void ManageSalvageDock()
{
    IMyShipConnector dock = FindConnector(TAG_SALVAGE_DOCK);
    if (dock == null || dock.Status != MyShipConnectorStatus.Connected) return;

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

// ============================================================
// REFINERIES
// ============================================================

void ManageRefineries()
{
    foreach (var kv in REFINERY_TAG_BY_ORE)
    {
        string oreName = kv.Key;
        string refineryTag = kv.Value;
        IMyRefinery refinery = FindRefinery(refineryTag);
        if (refinery == null) continue;

        double oreAvailable = GetTotal(baseTotals, oreName);
        double threshold = ORE_LOW_THRESHOLDS.ContainsKey(oreName) ? ORE_LOW_THRESHOLDS[oreName] : 0;
        if (oreAvailable < threshold)
        {
            TriggerOreAlert(oreName);
            continue;
        }

        if (!DRY_RUN_MODE)
        {
            MoveItemToInventory(oreName, 10000, refinery.GetInventory(0), false);
        }
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

// ============================================================
// PRODUCTION / DISASSEMBLY
// ============================================================

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

    foreach (var c in contractRequests)
    {
        double have = GetTotal(baseTotals, c.Key);
        double available = AvailableForUse(c.Key);
        if (available < c.Value) QueueProduction(c.Key, c.Value - available);
    }

    foreach (var l in loadoutRequests)
    {
        double available = AvailableForUse(l.Key);
        if (available < l.Value) QueueProduction(l.Key, l.Value - available);
    }
}

void QueueProduction(string itemName, double amount)
{
    if (amount <= 0) return;
    IMyAssembler assembler = GetPrimaryAssembler(false);
    if (assembler == null) return;

    string blueprint = BlueprintForItem(itemName);
    if (blueprint == "") return;

    if (!DRY_RUN_MODE)
    {
        try
        {
            assembler.AddQueueItem(MyDefinitionId.Parse(blueprint), (VRage.MyFixedPoint)amount);
        }
        catch
        {
            lastAlert = "Could not queue production for " + itemName;
        }
    }
}

void ManageSurplusDisassembly()
{
    IMyAssembler disassembler = GetPrimaryAssembler(true);
    if (disassembler == null) return;

    foreach (var max in DISASSEMBLY_MAX)
    {
        double have = GetTotal(baseTotals, max.Key);
        if (have <= max.Value) continue;
        double excess = have - max.Value;
        string blueprint = BlueprintForItem(max.Key);
        if (blueprint == "") continue;
        if (!DRY_RUN_MODE)
        {
            try
            {
                disassembler.Mode = MyAssemblerMode.Disassembly;
                disassembler.AddQueueItem(MyDefinitionId.Parse(blueprint), (VRage.MyFixedPoint)excess);
            }
            catch
            {
                lastAlert = "Could not queue disassembly for " + max.Key;
            }
        }
    }
}

IMyAssembler GetPrimaryAssembler(bool disassembly)
{
    for (int i = 0; i < assemblers.Count; i++)
    {
        if (assemblers[i].CustomName.Contains(TAG_DO_NOT_TRACK)) continue;
        if (disassembly || assemblers[i].Mode != MyAssemblerMode.Disassembly) return assemblers[i];
    }
    return null;
}

string BlueprintForItem(string itemName)
{
    // Blueprint names vary across updates/mods. These are common vanilla blueprint ids.
    Dictionary<string, string> bp = new Dictionary<string, string>()
    {
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
        {"5.56x45mm NATO magazine", "MyObjectBuilder_BlueprintDefinition/NATO_5p56x45mmMagazine"},
        {"Missile 200mm", "MyObjectBuilder_BlueprintDefinition/Missile200mm"},
        {"Hydrogen Bottle", "MyObjectBuilder_BlueprintDefinition/HydrogenBottle"},
        {"Oxygen Bottle", "MyObjectBuilder_BlueprintDefinition/OxygenBottle"},
        {"Welder", "MyObjectBuilder_BlueprintDefinition/Welder"},
        {"Grinder", "MyObjectBuilder_BlueprintDefinition/AngleGrinder"},
        {"Hand Drill", "MyObjectBuilder_BlueprintDefinition/HandDrill"},
        {"Automatic Rifle", "MyObjectBuilder_BlueprintDefinition/AutomaticRifle"},
    };
    if (bp.ContainsKey(itemName)) return bp[itemName];
    return "";
}

// ============================================================
// INVENTORY MOVEMENT HELPERS
// ============================================================

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

double AvailableForUse(string itemName)
{
    double have = GetTotal(baseTotals, itemName);
    double reserve = RESERVES.ContainsKey(itemName) ? RESERVES[itemName] : 0;
    double assignedContracts = contractRequests.ContainsKey(itemName) ? 0 : 0;
    double available = have - reserve - assignedContracts;
    return Math.Max(0, available);
}

double GetTotal(Dictionary<string, double> dict, string key)
{
    return dict.ContainsKey(key) ? dict[key] : 0;
}

// ============================================================
// RESERVE CHECKS / STATUS
// ============================================================

void CheckReserves()
{
    foreach (var r in RESERVES)
    {
        double have = GetTotal(baseTotals, r.Key);
        if (have < r.Value)
        {
            lastAlert = "Reserve low: " + r.Key + " " + FormatAmount(have) + " / " + FormatAmount(r.Value);
            return;
        }
    }
}

// ============================================================
// LCD / LIGHT / BROADCAST OUTPUT
// ============================================================

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
    sb.AppendLine("Paused: " + (contractsPaused ? "YES" : "NO"));
    sb.AppendLine("Ready: " + (contractsReady ? "YES" : "NO"));
    IMyShipConnector dock = FindConnector(TAG_TRADE_DOCK);
    sb.AppendLine("Trade Dock: " + ConnectorStatusText(dock));
    sb.AppendLine();
    foreach (var req in contractRequests)
    {
        double have = GetTotal(baseTotals, req.Key);
        double available = AvailableForUse(req.Key);
        sb.AppendLine(req.Key + ": need " + FormatAmount(req.Value) + ", base " + FormatAmount(have) + ", avail " + FormatAmount(available));
    }
    if (contractRequests.Count == 0) sb.AppendLine("No active contracts.");
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
    sb.AppendLine("Last Command: " + lastCommand);
    sb.AppendLine("Last Alert: " + lastAlert);
    sb.AppendLine();
    sb.AppendLine("BLOCKS FOUND");
    sb.AppendLine("Inventory Blocks: " + inventoryBlocks.Count);
    sb.AppendLine("Cargo Containers: " + cargoContainers.Count);
    sb.AppendLine("Assemblers: " + assemblers.Count);
    sb.AppendLine("Refineries: " + refineries.Count);
    sb.AppendLine("Connectors: " + connectors.Count);
    sb.AppendLine("Status Lights: " + statusLights.Count);
    sb.AppendLine();
    sb.AppendLine("DOCK DETECTION");
    sb.AppendLine("Trade Connector: " + ConnectorFoundText(trade));
    sb.AppendLine("Trade Ship Connected: " + ConnectedYesNo(trade));
    sb.AppendLine("Transport/Loadout Connector: " + ConnectorFoundText(loadout));
    sb.AppendLine("Transport/Loadout Rover Connected: " + ConnectedYesNo(loadout));
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

// ============================================================
// TEXT HELPERS
// ============================================================

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