

using Animanure.API;

using Microsoft.Xna.Framework;

using Netcode;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;

using SObject = StardewValley.Object;

namespace Animanure;

internal sealed class AnimalManure : Mod {

    internal static Mod? instance;
    internal static ModConfig? Config { get; set; }

    internal static IMonitor? monitor;
    internal static IModHelper? modHelper;
    internal static IManifest? modManifest;

    private IJsonAssetsAPI? _jsonAssetsApi;
    private IContentPatcherAPI? _contentPatcherApi;

    internal static string GetFromi18n(string key) => instance!.Helper.Translation.Get(key);

    private List<FarmAnimal>? farmAnimals;

    private static readonly int minimumFullness = 255; //Completely full

    private bool hasRanForDay = false;
    private readonly int timeOfDayToRun = 730; //7:30 AM

    /// <summary>
    /// Barn animal leaves behind manure.
    /// </summary>
    /// <param name="animal">The animal to leave the manure behind.</param>
    public static void MakeDookie(FarmAnimal animal) {
        string animalName = animal.Name;
        GameLocation animalLocation = animal.currentLocation;

        //Only make the messes outdoors.
        if (animalLocation.IsOutdoors) {
            string itemName = "(O)zollernwolf.cp_manure";
            SObject @manureObj = ItemRegistry.Create<SObject>(itemName);
            Vector2 vec2 = animal.Tile;

            if (!animalLocation.tryPlaceObject(vec2, @manureObj)) {
                monitor!.Log("Unable to place manure item object.", LogLevel.Error);
            }
        } else {
            monitor!.Log($"{animalName} was not outdoors during check runs.", LogLevel.Warn);
        }
        monitor!.Log($"Barn Animal {animalName} dropped a dookie!", LogLevel.Debug);
    }

    /// <summary>
    /// Check fullness of the current barn animal.
    /// </summary>
    public void CheckAnimalFullness() => this.farmAnimals!.ForEach(action: animal => {
        NetInt fullness = animal.fullness;
        monitor!.Log($"Barn Animal {animal.Name} fullness: {fullness.Value}", LogLevel.Info);
        if (fullness.Value >= AnimalManure.minimumFullness) {
            AnimalManure.MakeDookie(animal);
        }
    });

    public override void Entry(IModHelper helper) {
        instance = this;
        Config = this.Helper.ReadConfig<ModConfig>();
        monitor = this.Monitor;
        modHelper = helper;
        modManifest = this.ModManifest;

        IModEvents modEvents = helper.Events;
        IGameLoopEvents gameLoop = modEvents.GameLoop;
        gameLoop.GameLaunched += this.OnGameLaunched!;
        gameLoop.OneSecondUpdateTicked += this.OnOneSecondUpdateTicked!;
        gameLoop.DayStarted += this.OnDayStarted!;
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
}