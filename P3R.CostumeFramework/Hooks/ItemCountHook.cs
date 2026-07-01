using P3R.CostumeFramework.Costumes;
using P3R.CostumeFramework.Costumes.Models;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X64;

namespace P3R.CostumeFramework.Hooks;

internal class ItemCountHook
{
    [Function(CallingConventions.Microsoft)]
    private delegate int FUN_14c15cad0(int itemId);
    private IHook<FUN_14c15cad0>? hook;

    private delegate byte IsAstrea();
    private IsAstrea? isAstrea;

    private readonly CostumeRegistry registry;

    private const int COSTUME_ITEM_BASE = 0x8000;

    private const int DLC_ITEM_PHANTOM = 110;
    private const int DLC_ITEM_SHUJIN = 100;
    private const int DLC_ITEM_YASOGAMI = 90;
    private const int DLC_ITEM_VELVET = 212;

    private static string[] GetItemNumCandidates =
    [
        "49 89 E3 48 81 EC 88 00 00 00 48 8B 05 ?? ?? ?? ?? 48 31 E0",
        "4C 8B DC 48 81 EC 88 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 48 8D 05 ?? ?? ?? ??"
    ];
    private readonly object GetItemNumLock = new();
    private int GetItemNumSignaturesScanned;

    public ItemCountHook(CostumeRegistry registry)
    {
        this.registry = registry;
        foreach (var (Index, Candidate) in GetItemNumCandidates.Select((x, i) => (i, x)))
        {
            Project.Scans.AddScanHook($"GET_ITEM_NUM[{Index}]", Candidate, (result, hooks) =>
            {
                lock (GetItemNumLock)
                {
                    GetItemNumSignaturesScanned++;
                    this.hook ??= hooks.CreateHook<FUN_14c15cad0>(this.Hook, result).Activate();
                }
            },
            () =>
            {
                lock (GetItemNumLock)
                {
                    GetItemNumSignaturesScanned++;
                    if (GetItemNumSignaturesScanned == GetItemNumCandidates.Length && this.hook == null)
                    {
                        Log.Error($"Failed to find a pattern for GET_ITEM_NUM.");
                    }
                    else
                    {
                        Log.Debug($"No matching pattern for GET_ITEM_NUM[{Index}].");
                    }
                }
            });
        }

        Project.Scans.AddScanHook(nameof(IsAstrea),
            "48 83 EC 28 E8 ?? ?? ?? ?? 48 85 C0 74 ?? E8 ?? ?? ?? ?? 48 8B C8 E8 ?? ?? ?? ?? 3C 01 0F 94 C0 48 83 C4 28 C3 48 83 C4 28 C3",
            (result, hooks) => isAstrea = hooks.CreateWrapper<IsAstrea>(result, out _));
    }

    private int Hook(int itemId)
    {
        if (this.registry.TryGetCostumeByItemId(itemId, out var costume))
        {
            // Found you fucker 
            var isRandomizedCostume = costume.CostumeId == GameCostumes.RANDOMIZED_COSTUME_ID;

            if (!isRandomizedCostume &&
                (costume.OwnerModId == null || costume.CostumeId < GameCostumes.BASE_MOD_COSTUME_ID))
            {
                return this.hook!.OriginalFunction(itemId);
            }

            if (costume.Character == Character.AigisReal && isAstrea!() == 0)
            {
                return 0;
            }

            // Dlc check do NOT delete 
            if (!this.OwnsRequiredDlc(costume))
            {
                return 0;
            }

            return 1;
        }

        return this.hook!.OriginalFunction(itemId);
    }

    private bool OwnsRequiredDlc(Costume costume)
    {
        var dlc = costume.Config.Dlc;
        if (dlc == null)
        {
            return true;
        }

        if (dlc.Phantom && !this.OwnsDlc(DLC_ITEM_PHANTOM)) return false;
        if (dlc.Shujin && !this.OwnsDlc(DLC_ITEM_SHUJIN)) return false;
        if (dlc.Yasogami && !this.OwnsDlc(DLC_ITEM_YASOGAMI)) return false;
        if (dlc.Velvet && !this.OwnsDlc(DLC_ITEM_VELVET)) return false;

        return true;
    }

    public bool CanUseForRandomizer(Costume costume)
    {
        if (costume.CostumeId == GameCostumes.RANDOMIZED_COSTUME_ID)
        {
            return false;
        }

        if (costume.Character == Character.AigisReal && isAstrea?.Invoke() == 0)
        {
            return false;
        }

        if (costume.OwnerModId != null && costume.CostumeId >= GameCostumes.BASE_MOD_COSTUME_ID)
        {
            return this.OwnsRequiredDlc(costume);
        }

        if (costume.CostumeItemId == default)
        {
            return false;
        }

        return this.hook?.OriginalFunction(COSTUME_ITEM_BASE + costume.CostumeItemId) > 0;
    }

    private bool OwnsDlc(int costumeIndex)
    {
        var count = this.hook!.OriginalFunction(COSTUME_ITEM_BASE + costumeIndex);
        Log.Information($"[DLC] GET_ITEM_NUM(0x{COSTUME_ITEM_BASE + costumeIndex:X}) = {count}"); // remember to remove count
        return count > 0;
    }
}