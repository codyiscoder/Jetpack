using BepInEx;
using GorillaLocomotion;
using UnityEngine;
using static Jetpack.Constants;

namespace Jetpack;

[BepInPlugin(GUID, pluginName, version)]
public class JetpackPlugin : BaseUnityPlugin
{
    public static JetpackPlugin Instance;

    // i think multiple variable declaration is more readable than a bunch of single line declarations, if you dont like it, complain
    private bool
        init,
        playing,
        last,
        modEnabled = true,
        wasInModdedRoom;


    private Transform head;
    private Rigidbody body;
    public static AudioSource source;

    private Quaternion
        currROT = Quaternion.identity,
        targetROT = Quaternion.identity,
        referenceHeadRot = Quaternion.identity,
        inertiaDelta = Quaternion.identity,
        baseROT = Quaternion.identity;

    private bool isGrounded =>
        GTPlayer.Instance.IsGroundedButt ||
        GTPlayer.Instance.IsGroundedHand;

    private void Awake()
    {
        Instance = this;
        Jetpack.Config.Init();

        // made AssetLoader a monobehaviour so i dont have to call a method to load the jetpack prefab, plus it will be easier to load more assets in the future if needed
        new GameObject("Jetpack", typeof(AssetLoader));
        GorillaTagger.OnPlayerSpawned(PlayerSpawned);
    }

    private void PlayerSpawned()
    {
        init = true;
        head = GTPlayer.Instance.headCollider.transform;
        body = GTPlayer.Instance.bodyCollider.attachedRigidbody;
    }

    private void FixedUpdate()
    {
        if (!modEnabled || !init)
            return;

        bool inModdedRoom =
            NetworkSystem.Instance.InRoom &&
            NetworkSystem.Instance.GameModeString.Contains("MODDED");

        // according to skellon this is a better way of doing modded checks, im not sure whether its true or not, but we'll use it anyways
        // update it did NOT work, you could just use it in any lobby, AND it spammed logs with errors.
        //if (NetworkSystem.Instance is { InRoom: false } net && !net.GameModeString.Contains("MODDED"))
        if (!inModdedRoom)
        {
            if (wasInModdedRoom)
                CleanUp();

            wasInModdedRoom = false;
            return;
        }

        wasInModdedRoom = true;

        bool left = ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f;
        bool right = ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f;
        bool on = left || right;

        if (on && !last)
            referenceHeadRot = Quaternion.Inverse(head.rotation) * baseROT;

        float force = Jetpack.Config.Force.Value;

        if (left && right)
            force *= 2f;

        if (on)
        {
            body.AddForce((head.rotation * Vector3.up).normalized * force, ForceMode.Acceleration);

            sfx(true);

            GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tapHapticStrength, GorillaTagger.Instance.tapHapticDuration);
            GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength, GorillaTagger.Instance.tapHapticDuration);

            targetROT = head.rotation * referenceHeadRot;
            inertiaDelta = targetROT * Quaternion.Inverse(baseROT);
            baseROT = targetROT;
        }
        else
        {
            sfx(false);

            if (isGrounded)
            {
                baseROT = Quaternion.identity;
                inertiaDelta = Quaternion.identity;
            }
            else
            {
                inertiaDelta = Quaternion.Slerp(inertiaDelta, Quaternion.identity, 2f * Time.deltaTime);
                baseROT = inertiaDelta * baseROT;
            }

            targetROT = baseROT;
        }

        last = on;
        currROT = Quaternion.Slerp(currROT, targetROT, 1f - Mathf.Exp(-5f * Time.deltaTime));

        // dont remove the override until the rotation is almost back to normal, otherwise it will snap
        if (on || !isGrounded || Quaternion.Angle(currROT, Quaternion.identity) > 0.05f)
            GTPlayerTransform.ApplyRotationOverride(currROT, Time.frameCount);
        else
            currROT = Quaternion.identity;
    }

    // probably terrible, but it works and i dont care enough to make it better
    private void sfx(bool on)
    {
        if (source == null)
            return;

        if (on)
        {
            if (!playing)
            {
                source.Play();
                playing = true;
            }

            source.volume = Mathf.Lerp(source.volume, Jetpack.Config.Volume.Value, 10f * Time.deltaTime);

            return;
        }

        if (!playing)
            return;

        source.volume = Mathf.Lerp(source.volume, 0f, 5f * Time.deltaTime);

        if (source.volume <= 0.01f)
        {
            source.Stop();
            playing = false;
            source.volume = 0f;
        }
    }

    private void CleanUp()
    {
        if (source != null)
            source.Stop();

        playing = false;
        currROT = Quaternion.identity;
        targetROT = Quaternion.identity;
        referenceHeadRot = Quaternion.identity;
        inertiaDelta = Quaternion.identity;
        baseROT = Quaternion.identity;
        last = false;
        wasInModdedRoom = false;

        GTPlayerTransform.ApplyRotationOverride(Quaternion.identity, Time.frameCount);
    }

    private void OnEnable() => modEnabled = true;

    private void OnDisable()
    {
        modEnabled = false;
        CleanUp();
    }
}