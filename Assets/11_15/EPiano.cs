using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EPiano : MonoBehaviour 
{
	private float clock = 0.0f;

	private const int sampleRate = 44100;
	private const float timeStep = 1.0f / sampleRate;
	
	private AudioClip clip = null;
	private AudioSource audioSource = null;
	
	private static System.Random random = new System.Random();
	private float pluckDeltaTimer = 0.0f;
	
	int curnote = 0;
	int[] blues = {60, 63, 65, 66, 67, 70, 72, 70, 67, 66, 65, 63 };
			
	float getFrequency(int midiNote)
	{
		return 440.0f * Mathf.Pow(2.0f, (midiNote - 69) / 12.0f);
	}
	
	float oscillator(float freq, float offset, float scale, float t)
	{
			float carrier = Mathf.PI * 2.0f * (freq + offset) * t;
			float modulator = Mathf.Sin(Mathf.PI * 2.0f * freq * scale * t);
			
			return Mathf.Sin(carrier + modulator);
	}
	
	Vector2 generator(float freq, float t, float vel)
	{   
			float attenuation = Mathf.Exp(-5.0f * t) * vel;
			
			Vector2 body = new Vector2(oscillator(freq * 0.997f, 0.0f, 1.0f, t), oscillator(freq * 1.003f, 0.0f, 1.0f, t));
			Vector2 bell = new Vector2(oscillator(freq * 0.997f, 3.0f, 14.0f, t), oscillator(freq * 1.003f, 3.0f, 14.0f, t)) * attenuation;
			
			return Vector2.one * 0.7f * (body * 0.5f + bell * 0.5f) * Mathf.Exp(-1.0f * t);
	}
			
	void Start() 
	{		
		audioSource = gameObject.AddComponent<AudioSource>();
		clip = AudioClip.Create("Epiano", 44100, 2, sampleRate, false, true, OnAudioFilterRead);
	
		audioSource.clip = clip;
		audioSource.loop = true;
		audioSource.Play();		
	}
	
	float vel = 0.1f;
	int note = 60;
	int instrumentIndexL = 0;
	int instrumentIndexR = 0;
	void OnAudioFilterRead(float[] samples) 
	{			
		int index = 0;
		for (int i = 0; i < samples.Length / 2; i++) 
		{		
			clock += timeStep;	
			pluckDeltaTimer += timeStep;
	
			samples[index+0] = 0.0f;
			samples[index+1] = 0.0f;	
			
			Vector2 epiano = Vector2.zero;
			
			float timeToNextPluck = 1.7f;
			if (pluckDeltaTimer >= timeToNextPluck)
			{
				note = blues[curnote];
				
				pluckDeltaTimer = 0.0f;
				
				curnote = random.Next(blues.Length);
				vel = random.Next(5) / 10.0f + 0.1f;
			}
			
			epiano = generator(getFrequency(note), pluckDeltaTimer, vel);

			samples[index+0] += epiano.x;
			samples[index+1] += epiano.y;		
				
			index += 2;
			instrumentIndexL++;
			instrumentIndexR++;
		}
	}
}
