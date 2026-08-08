using System.IO;
using System.Reflection;
using UnityEngine;

namespace Jetpack;

public class AssetLoader : MonoBehaviour
{
    public static GameObject Jetpack;

    private void Awake() => GorillaTagger.OnPlayerSpawned(LoadJetpack);

    private void LoadJetpack()
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Jetpack.Resources.jetpack");

        byte[] data = new byte[stream.Length];
        stream.Read(data, 0, data.Length);

        AssetBundle bundle = AssetBundle.LoadFromMemory(data);
        GameObject prefab = bundle.LoadAsset<GameObject>("jetpack");

        Jetpack = Instantiate(prefab, GorillaTagger.Instance.offlineVRRig.transform);
        JetpackPlugin.source = Jetpack.GetComponent<AudioSource>();
    }
}