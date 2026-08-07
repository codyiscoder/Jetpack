using UnityEngine;
using System;

public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] wavFile, string name)
    {
        int channels = BitConverter.ToInt16(wavFile, 22);
        int sampleRate = BitConverter.ToInt32(wavFile, 24);

        int dataOffset = 12;

        while (dataOffset < wavFile.Length)
        {
            string chunk = System.Text.Encoding.UTF8.GetString(
                wavFile,
                dataOffset,
                4);

            int size = BitConverter.ToInt32(
                wavFile,
                dataOffset + 4);

            if (chunk == "data")
            {
                dataOffset += 8;

                int samples = size / 2;

                float[] audioData = new float[samples];

                for (int i = 0; i < samples; i++)
                {
                    short value = BitConverter.ToInt16(
                        wavFile,
                        dataOffset + i * 2);

                    audioData[i] = value / 32768f;
                }

                AudioClip clip = AudioClip.Create(
                    name,
                    samples / channels,
                    channels,
                    sampleRate,
                    false);

                clip.SetData(audioData, 0);

                return clip;
            }

            dataOffset += 8 + size;
        }

        return null;
    }
}