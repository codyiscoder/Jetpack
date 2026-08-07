using BepInEx;
using BepInEx.Configuration;
using GorillaLocomotion;
using UnityEngine;
using System.IO;
using System.Reflection;

namespace Jetpack
{
    [BepInPlugin(Constants.GUID, Constants.name, Constants.version)]
    public class Plugin : BaseUnityPlugin
    {
        private bool init;
        private bool playing;
        private bool last;

        private Transform head;
        private Rigidbody rb;
        private AudioSource src;
        private AudioClip rocket;

        private ConfigEntry<float> power;
        private ConfigEntry<float> vol;

        private Quaternion smooth = Quaternion.identity;
        private Quaternion target = Quaternion.identity;
        private Quaternion saved = Quaternion.identity;
        private Quaternion offset = Quaternion.identity;
        private Quaternion vel = Quaternion.identity;

        private void Awake()
        {
            power = Config.Bind("Jetpack", "Force", 10f);
            vol = Config.Bind("Jetpack", "Sound Volume", 0.5f);

            GorillaTagger.OnPlayerSpawned(() =>
            {
                init = true;

                head = GTPlayer.Instance.headCollider.transform;
                rb = GTPlayer.Instance.bodyCollider.attachedRigidbody;

                rocket = LoadEmbeddedWav("Jetpack.Resources.rocket.wav");

                GameObject pack = new GameObject("Jetpack");

                pack.transform.SetParent(
                    GorillaTagger.Instance.offlineVRRig.transform,
                    false);

                src = pack.AddComponent<AudioSource>();

                src.clip = rocket;
                src.playOnAwake = false;
                src.loop = true;
                src.spatialBlend = 0f;
                src.volume = 0f;
                src.Stop();
            });
        }

        private void LateUpdate()
        {
            if (!init)
                return;

            bool l = ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f;
            bool r = ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f;
            bool on = l || r;

            if (on)
            {
                if (!last)
                    offset = Quaternion.Inverse(head.rotation) * saved;

                sfx(true);

                float f = power.Value;

                if (l && r)
                    f *= 2f;

                Vector3 dir = head.rotation * Vector3.up;

                rb.AddForce(
                    dir.normalized * f,
                    ForceMode.Acceleration);

                float h = GorillaTagger.Instance.tapHapticStrength;

                GorillaTagger.Instance.StartVibration(
                    true,
                    h,
                    GorillaTagger.Instance.tapHapticDuration);

                GorillaTagger.Instance.StartVibration(
                    false,
                    h,
                    GorillaTagger.Instance.tapHapticDuration);

                target = head.rotation * offset;

                vel = target * Quaternion.Inverse(saved);

                saved = target;
            }
            else
            {
                sfx(false);

                if (GTPlayer.Instance.IsGroundedButt)
                {
                    saved = Quaternion.identity;
                    vel = Quaternion.identity;
                }
                else
                {
                    vel = Quaternion.Slerp(
                        vel,
                        Quaternion.identity,
                        2f * Time.deltaTime);

                    saved = vel * saved;
                }

                target = saved;
            }

            last = on;

            float t = 1f - Mathf.Exp(-5f * Time.deltaTime);

            smooth = Quaternion.Slerp(
                smooth,
                target,
                t);

            GTPlayerTransform.ApplyRotationOverride(
                smooth,
                Time.frameCount);
        }

        private void sfx(bool on)
        {
            if (src == null || rocket == null)
                return;

            if (on)
            {
                if (!playing)
                {
                    src.clip = rocket;
                    src.Play();
                    playing = true;
                }

                src.volume = Mathf.Lerp(
                    src.volume,
                    vol.Value,
                    10f * Time.deltaTime);

                return;
            }

            if (!playing)
                return;

            src.volume = Mathf.Lerp(
                src.volume,
                0f,
                5f * Time.deltaTime);

            if (src.volume <= 0.01f)
            {
                src.Stop();
                playing = false;
                src.volume = 0f;
            }
        }

        private AudioClip LoadEmbeddedWav(string resource)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resource))
            {
                if (stream == null)
                    return null;

                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);

                return WavUtility.ToAudioClip(data, "rocket");
            }
        }
    }
}