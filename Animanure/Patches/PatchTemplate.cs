using StardewModdingAPI;

namespace Animanure.Patches;

internal class PatchTemplate {

    internal static IMonitor? _monitor;
    internal static IModHelper? _helper;

    internal PatchTemplate(IMonitor modMonitor, IModHelper modHelper) {
        _monitor = modMonitor;
        _helper = modHelper;
    }
}