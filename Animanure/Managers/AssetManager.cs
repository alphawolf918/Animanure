using Microsoft.Xna.Framework.Graphics;

using StardewModdingAPI;

namespace Animanure.Managers;

internal class AssetManager {

    internal string? assetFolderPath;
    internal Dictionary<string, Texture2D> toolNames = new();

    private readonly Texture2D? _manureTexture;

    internal IContentPack? contentPack;

    internal IContentPack? localPack;

    public AssetManager(IModHelper helper) {
        this.assetFolderPath = helper.ModContent.GetInternalAssetName("assets").Name;
        this._manureTexture = helper.ModContent.Load<Texture2D>(Path.Combine(assetFolderPath, "animal-manure.png"));
        this.toolNames.Add("Manure", this._manureTexture);
    }

    internal IContentPack GetLocalPack(bool update = false) {
        if (localPack is null || update is true) {
            localPack = AnimalManure.modHelper!.ContentPacks.CreateTemporary(Path.Combine(AnimalManure.modHelper.DirectoryPath, "assets"), "ZollernWolf.AnimalManure", "ZW - Manure", "Local appearance pack for Animanure.", AnimalManure.modManifest!.Author, AnimalManure.modManifest.Version);
        }
        return localPack;
    }


    internal Texture2D GetManureTexture() => this._manureTexture!;
}