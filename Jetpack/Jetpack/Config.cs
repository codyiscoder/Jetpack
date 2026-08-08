using BepInEx.Configuration;

namespace Jetpack;

public static class Config
{
    public static ConfigEntry<float>
        Force,
        Volume;

    public static void Init()
    {
        Force = JetpackPlugin.Instance.Config.Bind(
            "Jetpack",
            "Force",
            10f);

        Volume = JetpackPlugin.Instance.Config.Bind(
            "Jetpack",
            "Sound Volume",
            0.5f);
    }
}