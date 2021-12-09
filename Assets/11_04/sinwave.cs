using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sinwave : MonoBehaviour
{
    public AudioSource audioSource = null;
    public AudioClip audioClip = null;

    private float clock = 0.0f;
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioClip = AudioClip.Create("generated sinewave", 44100, 2, 44100, false, true, OnAudioFliterRead);

        audioSource.clip = audioClip;
        audioSource.loop = true; 
        audioSource.Play(); 
    }

    float GenerateSine(float amplitude, float phase, float frequency)
    {
        return amplitude * Mathf.Sin(frequency * phase * Mathf.PI * 2.0f);
    }

    void OnAudioFliterRead(float[] samples)
    {
        int index = 0;
        for (int i = 0; i < samples.Length / 2; i++)
        {
            float frequency = 220.0f;
            float samplerate = 44100.0f;
            float sine = 0.5f * Mathf.Sin(frequency * clock / samplerate * Mathf.PI * 2.0f);
            //가장 처음에 곱해주는 상수가 진폭의 역할

            samples[index + 0] = sine * 0.5f * Mathf.Sin(clock * 2.0f * Mathf.PI);
            samples[index + 1] = sine; //right channel

            clock += 1.0f;

            index += 2;
        }
    }
}