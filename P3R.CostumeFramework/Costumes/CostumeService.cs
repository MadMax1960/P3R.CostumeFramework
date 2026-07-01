using P3R.CostumeFramework.Configuration;
using P3R.CostumeFramework.Hooks;
using P3R.CostumeFramework.Hooks.Animations;
using P3R.CostumeFramework.Hooks.Costumes;
using P3R.CostumeFramework.Hooks.Services;
using p3rpc.asyncassetloader.Interfaces;
using UE.Toolkit.Interfaces;
using Unreal.ObjectsEmitter.Interfaces;
using IDataTables = Unreal.ObjectsEmitter.Interfaces.IDataTables;

namespace P3R.CostumeFramework.Costumes;

internal unsafe class CostumeService
{
    private readonly CostumeHooks costumeHooks;
    private readonly CostumeManager costumeManager;
    private readonly ItemCountHook itemCountHook;
    private readonly CostumeNameHook costumeNameHook;
    private readonly ItemEquip itemEquip;
    private readonly CostumeAlloutService alloutService;
    private readonly CostumeBustupPhysicsService bustupPhysicsService;
    private readonly CostumeMontageService montageService;
    private readonly CostumeSceneMontageService sceneMontageService;
    private readonly CostumeCritCameraService critCameraService;
    private readonly CostumeCylinderService cylinderService;
    private readonly CostumeVisualTableService visualTableService;
    private readonly CostumeWeaponService weaponService;
    private readonly CostumeTableService costumeTable;
    private readonly CostumeShellService costumeShells;
    private readonly CostumeAnimsService costumeAnims;
    private readonly CostumeHeadPanelService costumeHeadPanel;
    private readonly CostumeTheurgiaService theurgiaService;
    private readonly CostumeFaceAnimService faceAnimService;
    private readonly CostumeResultService resultService;
    private readonly CostumeAlloutSequencerService alloutSequencerService;
    private readonly CostumeCameraService cameraService;

    public CostumeService(
        IUObjects uobjs,
        IUnreal unreal,
        IDataTables dt,
        CostumeRegistry registry,
        CostumeOverridesRegistry overrides,
        CostumeDescService costumeDesc,
        CostumeMusicService costumeMusic,
        CostumeRyoService costumeAudio,
        bool useFemcPlayer,
        IUnrealMemory toolkitMemory,
        IUnrealClasses toolkitClasses,
        IUnrealObjects toolkitObjects,
        IUnrealSpawning toolkitSpawning,
        IAssetLoader assetLoader
    )
    {
        this.itemEquip = new(registry);
        this.itemCountHook = new(registry);
        this.costumeTable = new(dt, unreal, registry, useFemcPlayer);
        this.costumeShells = new(dt, this.costumeTable, useFemcPlayer);
        this.costumeAnims = new(uobjs, unreal, toolkitClasses, this.costumeTable);
        this.costumeHooks = new(uobjs, unreal, registry, overrides, costumeDesc, costumeMusic, costumeAudio, this.costumeShells, this.itemEquip, this.itemCountHook);
        this.costumeManager = new(this.costumeHooks);
        this.alloutService = new(dt, unreal, this.costumeManager);
        this.bustupPhysicsService = new(dt, unreal, this.costumeManager, this.costumeHooks);
        this.montageService = new(dt, unreal, this.costumeManager, this.costumeHooks);
        this.sceneMontageService = new(dt, unreal, this.costumeManager, this.costumeHooks);
        this.critCameraService = new(dt, unreal, this.costumeManager, this.costumeHooks);
        this.cylinderService = new(dt, unreal, this.costumeManager, this.costumeHooks);
        this.visualTableService = new(dt, unreal, this.costumeManager, this.costumeHooks);
        this.weaponService = new(dt, unreal, this.costumeManager, this.costumeHooks);
        this.costumeNameHook = new(uobjs, unreal, registry);
        this.costumeHeadPanel = new(this.costumeManager, toolkitMemory, toolkitObjects, toolkitSpawning, assetLoader);
        this.theurgiaService = new(dt, unreal, this.costumeManager, this.costumeHooks);
        this.theurgiaService = new(dt, unreal, this.costumeManager, this.costumeHooks);
        this.resultService = new(dt, unreal, this.costumeManager, this.costumeHooks);
        this.faceAnimService = new(uobjs, this.costumeManager, this.costumeHooks);
        this.alloutSequencerService = new(dt, unreal, this.costumeManager, this.costumeHooks);
        this.cameraService = new(dt, this.costumeManager, this.costumeHooks);

        this.costumeHooks.OnCostumeChanged += costume =>
        {
            costumeMusic.Refresh(costume);
            costumeAudio.Refresh(costume);
            this.costumeHeadPanel.Refresh(costume);
        };

        this.costumeHooks.OnCostumeDataReady += () =>
        {
            this.costumeNameHook.RefreshNames();
        };
    }

    public void SetConfig(Config config)
    {
        this.costumeHooks.SetRandomizeCostumes(config.RandomizeCostumes);
        this.costumeHooks.SetOverworldCostumes(config.OverworldCostumes);
    }
}