using Animanure.API;
using Animanure.Managers;

using Netcode;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.GameData.FarmAnimals;

namespace Animanure;

internal sealed class AnimalManure : Mod {

    internal static Mod? instance;
    internal static ModConfig? Config { get; set; }

    internal static IMonitor? monitor;
    internal static IModHelper? modHelper;
    internal static IManifest? modManifest;

    private IJsonAssetsAPI? _jsonAssetsApi;
    private IContentPatcherAPI? _contentPatcherApi;

    internal static AssetManager? assetManager;

    internal static string GetFromi18n(string key) => instance!.Helper.Translation.Get(key);

    private static readonly int minimumFullness = 255; //Completely full

    private List<FarmAnimal>? farmAnimals;

    private bool hasRanForDay = false;
    private readonly int timeOfDayToRun = 800; //8 AM

    /// <summary>
    /// Barn animal leaves behind manure.
    /// </summary>
    /// <param name="animal">The animal to leave the manure behind.</param>
    public static void MakeDookie(FarmAnimal animal) {
        string animalName = animal.Name;
        GameLocation animalLocation = animal.currentLocation;
        if (animalLocation is not null) {

        }
        monitor!.Log($"Barn Animal {animalName} dropped a dookie!", LogLevel.Debug);
    }

    public void CheckAnimalFullness() {
        this.farmAnimals!.ForEach(action: animal => {
            NetInt fullness = animal.fullness;
            FarmAnimalData data = animal.GetAnimalData();

            monitor!.Log($"Barn Animal {animal.Name} fullness: {fullness.Value}", LogLevel.Info);
            if (fullness.Value >= AnimalManure.minimumFullness) {
                AnimalManure.MakeDookie(animal);
            }
        });
    }

    public override void Entry(IModHelper helper) {
        instance = this;
        Config = this.Helper.ReadConfig<ModConfig>();
        monitor = this.Monitor;
        modHelper = helper;
        modManifest = this.ModManifest;
        assetManager = new(modHelper);

        IModEvents modEvents = helper.Events;
        IGameLoopEvents gameLoop = modEvents.GameLoop;
        gameLoop.GameLaunched += this.OnGameLaunched!;
        gameLoop.OneSecondUpdateTicked += this.OnOneSecondUpdateTicked!;
        gameLoop.DayStarted += this.OnDayStarted!;
        helper.Events.Multiplayer.ModMessageReceived += this.OnModMessageReceived!;

        this.LoadContentPacks();
    }

    private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e) {
        //
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {
        //Generic Mod Config Menu
        IGenericModConfigMenuAPI? configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
        if (configMenu == null) {
            this.Monitor.Log("Generic Mod Config Menu not installed!", LogLevel.Warn);
            return;
        }
        this.Monitor.Log("Successfully hooked into spacechase0.GenericModConfigMenu.", LogLevel.Debug);

        //Config
        configMenu.Register(this.ModManifest, reset: delegate {
            Config = new();
        }, save: delegate {
            this.Helper.WriteConfig(Config!);
        });

        //Content Patcher
        if (Helper.ModRegistry.IsLoaded("Pathoschild.ContentPatcher") && APIManager.HookIntoContentPatcher(Helper)) {
            _contentPatcherApi = APIManager.GetContentPatcherInterface();
        }

        //JSON Assets
        if (Helper.ModRegistry.IsLoaded("spacechase0.JsonAssets") && APIManager.HookIntoJsonAssets(Helper)) {
            _jsonAssetsApi = APIManager.GetJsonAssetsApi();
        }
    }

    private void LoadContentPacks(bool silent = false, string? packId = null) {
        IContentPackHelper? cPacks = Helper.ContentPacks!;
        IEnumerable<IContentPack> ownedPacks = cPacks!.GetOwned();
        List<IContentPack>? contentPacks = ownedPacks.Where(c => String.IsNullOrEmpty(packId) is true || c.Manifest.UniqueID.Equals(packId, StringComparison.OrdinalIgnoreCase)).ToList();
        contentPacks.Add(assetManager!.GetLocalPack(update: true));
    }

    private void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e) {
        if (!Context.IsWorldReady) {
            return;
        }
        if (Game1.timeOfDay >= this.timeOfDayToRun && !this.hasRanForDay) {
            this.CheckAnimalFullness();
            this.hasRanForDay = true;
        }
    }

    private void OnDayStarted(object sender, DayStartedEventArgs e) {
        Farm farm = Game1.getFarm();
        this.farmAnimals = farm.getAllFarmAnimals();
        this.hasRanForDay = false;
    }

    private void EnsureKeyExists(string key) {
        if (!Game1.player.modData.ContainsKey(key)) {
            Game1.player.modData[key] = null;
        }
    }
}