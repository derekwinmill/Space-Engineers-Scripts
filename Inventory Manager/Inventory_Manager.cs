/* Inventory Manager
 * Version: 0.2 - Added prioritize ammo functionality.
 * Designed for Space Engineers programmable block.
 */

// ============================================================
// USER CONFIGURATION
// ============================================================

const bool DRY_RUN_MODE = true; // Toggle for testing without real actions
const string TAG_PRIORITY_AMMO = "[PriorityAmmo]"; // Group or tag to prioritize ammo

// RUNTIME SETTINGS
List<IMyTerminalBlock> ammoPriorityBlocks = new List<IMyTerminalBlock>(); // Blocks with priority ammo
Dictionary<string, double> PRIORITIZED_AMMO = new Dictionary<string, double>()
{
    { "5.56x45mm NATO magazine", 500 },
    { "Missile 200mm", 50 },
    { "Autocannon Magazine", 200 },
};

// Program Initialization
public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
    Echo("Inventory Manager: Initialized with prioritize ammo.");
    UpdatePriorityAmmoBlocks();
}

public void Save() {}

// Main Execution Loop
public void Main(string argument, UpdateType updateSource)
{
    if (!string.IsNullOrWhiteSpace(argument))
    {
        HandleCommand(argument);
    }

    if (tick % FULL_SCAN_INTERVAL_TICKS == 1)
    {
        ScanBlocks();
        ScanInventories();
    }

    if (tick % SORT_INTERVAL_TICKS == 2)
    {
        PrioritizeAmmo();
        SortInventories();
    }
}

// ============================================================
// PRIORITIZE AMMO FUNCTIONALITY
// ============================================================

void UpdatePriorityAmmoBlocks()
{
    ammoPriorityBlocks.Clear();
    GridTerminalSystem.GetBlocksOfType(ammoPriorityBlocks, b => b.CustomName.Contains(TAG_PRIORITY_AMMO) && b.HasInventory);
    Echo($"Ammo Priority blocks found: {ammoPriorityBlocks.Count}");
}

void PrioritizeAmmo()
{
    if (DRY_RUN_MODE)
    {
        Echo("Dry Run: Prioritize Ammo simulated");
        return;
    }

    foreach (var ammo in PRIORITIZED_AMMO)
    {
        string ammoName = ammo.Key;
        double desiredAmount = ammo.Value;
        double currentAmount = CountInBlocks(ammoPriorityBlocks, ammoName);

        if (currentAmount < desiredAmount)
        {
            double shortfall = desiredAmount - currentAmount;
            Echo($"Prioritizing {ammoName}: Need {shortfall}");
            MoveItemToBlocks(ammoName, shortfall, ammoPriorityBlocks, true);
        }
    }
}

// ============================================================
// HELPER FUNCTIONS
// ============================================================

// Count a specific item in a given list of blocks
double CountInBlocks(List<IMyTerminalBlock> blocks, string itemName)
{
    double total = 0;
    foreach (var block in blocks)
    {
        for (int i = 0; i < block.InventoryCount; i++)
        {
            IMyInventory inv = block.GetInventory(i);
            List<MyInventoryItem> items = new List<MyInventoryItem>();
            inv.GetItems(items);
            total += items.Where(item => ItemDisplayName(item) == itemName).Sum(item => (double)item.Amount);
        }
    }
    return total;
}

// Transfer items to prioritized blocks
bool MoveItemToBlocks(string itemName, double amount, List<IMyTerminalBlock> targets, bool respectReserve)
{
    double remaining = amount;
    foreach (var source in inventoryBlocks)
    {
        if (remaining <= 0) break;
        if (!CanDrainFrom(source)) continue;

        for (int j = 0; j < source.InventoryCount; j++)
        {
            var sourceInv = source.GetInventory(j);
            var items = new List<MyInventoryItem>();
            sourceInv.GetItems(items);

            foreach (var item in items)
            {
                if (ItemDisplayName(item) != itemName) continue;
                double transferAmount = Math.Min(remaining, (double)item.Amount);
                foreach (var target in targets)
                {
                    var targetInv = target.GetInventory(0);
                    if (sourceInv.TransferItemTo(targetInv, item, null, true, (VRage.MyFixedPoint)transferAmount))
                    {
                        remaining -= transferAmount;
                        if (remaining <= 0) return true;
                    }
                }
            }
        }
    }
    return remaining <= 0;
}

string ItemDisplayName(MyInventoryItem item)
{
    string subtype = item.Type.SubtypeId;
    if (item.Type.TypeId.EndsWith("/Ammo")) return subtype;
    return subtype;
}

// ============================================================
// END
// ============================================================