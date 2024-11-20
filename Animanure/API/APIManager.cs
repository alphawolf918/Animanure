using StardewModdingAPI;

namespace Animanure.API;

public class APIManager {

    private static readonly IMonitor monitor = AnimalManure.monitor!;
    private static IContentPatcherAPI? contentPatcherApi;
    private static IJsonAssetsAPI? jsonAssetsApi;

    internal static bool HookIntoJsonAssets(IModHelper helper) {
        jsonAssetsApi = helper.ModRegistry.GetApi<IJsonAssetsAPI>("spacechase0.JsonAssets");

        if (jsonAssetsApi is null) {
            monitor.Log("Failed to hook into spacechase0.JsonAssets.", LogLevel.Error);
            return false;
        }

        monitor.Log("Successfully hooked into spacechase0.JsonAssets.", LogLevel.Debug);
        return true;
    }

    internal static bool HookIntoContentPatcher(IModHelper helper) {
        contentPatcherApi = helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");

        if (contentPatcherApi is null) {
            monitor.Log("Failed to hook into Pathoschild.ContentPatcher.", LogLevel.Error);
            return false;
        }

        monitor.Log("Successfully hooked into Pathoschild.ContentPatcher.", LogLevel.Debug);
        return true;
    }

    public static IContentPatcherAPI GetContentPatcherInterface() {
        return contentPatcherApi!;
    }

    internal static IJsonAssetsAPI GetJsonAssetsApi() {
        return jsonAssetsApi!;
    }
}