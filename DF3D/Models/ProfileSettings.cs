namespace DF3D.Models;

/// <summary>
/// A named preset combining overlay and crosshair settings.
/// </summary>
public class ProfileSettings
{
    public string Name { get; set; } = "Default";
    public OverlaySettings Overlay { get; set; } = new();
    public CrosshairSettings Crosshair { get; set; } = new();
    public bool IsBuiltIn { get; set; }

    public ProfileSettings Clone()
    {
        return new ProfileSettings
        {
            Name = Name,
            Overlay = Overlay.Clone(),
            Crosshair = Crosshair.Clone(),
            IsBuiltIn = IsBuiltIn
        };
    }

    /// <summary>
    /// Creates the built-in default profiles.
    /// </summary>
    public static List<ProfileSettings> CreateDefaults()
    {
        return new List<ProfileSettings>
        {
            new()
            {
                Name = "默认",
                IsBuiltIn = true,
                Overlay = new OverlaySettings
                {
                    Shape = VignetteShape.Box,
                    Size = VignetteSize.M,
                    Length = VignetteLength.Plus2,
                    Color = OverlayColor.Black,
                    Opacity = 0.7
                },
                Crosshair = new CrosshairSettings
                {
                    Shape = CrosshairShape.Cross,
                    Size = CrosshairSize.M,
                    Color = OverlayColor.Green,
                    Opacity = 0.8
                }
            },
            new()
            {
                Name = "FPS 游戏",
                IsBuiltIn = true,
                Overlay = new OverlaySettings
                {
                    Shape = VignetteShape.Dome,
                    Size = VignetteSize.S,
                    Length = VignetteLength.Plus3,
                    Color = OverlayColor.Black,
                    Opacity = 0.6
                },
                Crosshair = new CrosshairSettings
                {
                    Shape = CrosshairShape.Circle,
                    Size = CrosshairSize.S,
                    Color = OverlayColor.Green,
                    Opacity = 0.9
                }
            },
            new()
            {
                Name = "赛车游戏",
                IsBuiltIn = true,
                Overlay = new OverlaySettings
                {
                    Shape = VignetteShape.Flag,
                    Size = VignetteSize.L,
                    Length = VignetteLength.Plus4,
                    Color = OverlayColor.Black,
                    Opacity = 0.8
                },
                Crosshair = new CrosshairSettings
                {
                    IsVisible = false,
                    Shape = CrosshairShape.Diamond,
                    Size = CrosshairSize.L,
                    Color = OverlayColor.Yellow,
                    Opacity = 0.7
                }
            },
            new()
            {
                Name = "轻度防护",
                IsBuiltIn = true,
                Overlay = new OverlaySettings
                {
                    Shape = VignetteShape.Box,
                    Size = VignetteSize.XS,
                    Length = VignetteLength.Plus1,
                    Color = OverlayColor.Black,
                    Opacity = 0.5
                },
                Crosshair = new CrosshairSettings
                {
                    Shape = CrosshairShape.Cross,
                    Size = CrosshairSize.XS,
                    Color = OverlayColor.Green,
                    Opacity = 0.6
                }
            }
        };
    }
}
