using Animanure.API;

using Netcode;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;

namespace Animanure;

internal sealed class AnimalManure : Mod {

    internal static Mod? instance;
    internal static ModConfig? Config { get; set; }

    internal static IMonitor? monitor;
    internal static IModHelper? modHelper;

    private IJsonAssetsAPI? _jsonAssetsApi;
    private IContentPatcherAPI? _contentPatcherApi;

    internal static string GetFromi18n(string key) => instance!.Helper.Translation.Get(key);

    private static readonly int minimumFullness = 5;

    private static List<FarmAnimal>? farmAnimals;

    public static void MakeDookie(FarmAnimal animal) {
        string animalName = animal.Name;
        monitor!.Log($"Barn Animal {animalName} dropped a dookie!", LogLevel.Debug);
    }

    public static void CheckFullness() {
        AnimalManure.farmAnimals!.ForEach(action: animal => {
            NetInt fullness = animal.fullness;
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

        ManureData? model = modHelper.Data.ReadJsonFile<ManureData>("Items/manure.json") ?? new ManureData();

        IModEvents modEvents = helper.Events;
        IGameLoopEvents gameLoop = modEvents.GameLoop;
        gameLoop.GameLaunched += this.OnGameLaunched!;
        gameLoop.UpdateTicked += this.OnUpdateTicked!;
        gameLoop.OneSecondUpdateTicked += this.OnOneSecondUpdateTicked!;
        gameLoop.DayStarted += this.OnDayStarted!;
        gameLoop.SaveLoaded += this.OnSaveLoaded!;
        helper.Events.Input.ButtonsChanged += this.OnButtonsChanged!;
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

        //Content Packs
        foreach (IContentPack contentPack in this.Helper.ContentPacks.GetOwned()) {
            if (!contentPack.HasFile("content.json")) {
                monitor!.Log("content.json file missing for content pack!", LogLevel.Error);
            }
            this.Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
        }
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e) {
        if (!Context.IsPlayerFree) {
            return;
        }
    }

    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e) {
        //
    }

    private async void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e) {
        if (!Context.IsWorldReady) {
            return;
        }
        AnimalManure.CheckFullness();
    }

    private void OnDayStarted(object sender, DayStartedEventArgs e) {
        Farm farm = Game1.getFarm();
        AnimalManure.farmAnimals = farm.getAllFarmAnimals();
    }

    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e) {
        //
    }
}