﻿using Microsoft.Xna.Framework;

namespace Animanure.Models;

public class ItemModel {

    public string? Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public int Price { get; set; }
    public float InventoryScale { get; set; } = 1f;
    public Position? SpritePosition { get; set; }
    public Size? SpriteSize { get; set; }

    internal bool IsValid() => SpritePosition is not null && SpriteSize is not null;

    internal Rectangle GetSpriteRectangle() {
        return new Rectangle(SpritePosition!.X, SpritePosition.Y, SpriteSize!.Width, SpriteSize.Length);
    }
}