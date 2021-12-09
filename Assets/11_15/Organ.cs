using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Organ : MonoBehaviour 
{
	private float clock = 0.0f;	

	private const int sampleRate = 44100;
	private const float timeStep = 1.0f / sampleRate;

	private AudioClip clip = null;
	private AudioSource audioSource = null;
	private static System.Random random = new System.Random();
	private float pluckDeltaTimer = 0.0f;
	
	DelayLine delay1 = new DelayLine();
	DelayLine delay2 = new DelayLine();
	DelayLine delay3 = new DelayLine();	
	
	Envelope env = new Envelope();
	
	int curnote = 0;
	int[] blues = {60, 63, 65, 66, 67, 70, 72, 70, 67, 66, 65, 63 };
	
		public class Envelope 
	{
		public float a = 0.01f;
		public float s = 0.5f;
		public float r = 1.0f;
		
		private float time = 0.0f;
		
		public void Initialize(float attack, float sustain, float release)
		{
			a = attack;
			s = sustain;
			r = release;
			
			time = 0.0f;
		}
		
		public Vector2 Apply()
		{
			time += timeStep;
			
			float v = 1.0f;
			if (time < a)
			{
				v = time / a;
			}
			else if (time >= a + s)
			{
			  v -= (time - a - s) / r;
			}			
			
			v = Mathf.Max(0.0f, v);
			
			return new Vector2(v, v);
		}			
	};
			
	float getFrequency(int midiNote)
	{
		return 440.0f * Mathf.Pow(2.0f, (midiNote - 69) / 12.0f);
	}
	
	float GenerateSine(float phase, float frequency) 
	{
		return Mathf.Sin(frequency * phase * Mathf.PI * 2.0f);
	}

	Vector2 oscillator(float freq, float offset, float scale, float t)
	{
			float sample = GenerateSine(t, freq);
			
			sample = Mathf.Pow(sample, 3.0f);
			
			float amp = scale * Mathf.Sqrt(1000.0f / freq);
			
			return new Vector3(sample * amp, sample * amp);
	}
	
	Vector2 generator(float freq, float t, float vel)
	{   
		Vector2 sound = Vector2.zero;
		
		sound += oscillator(freq, 0.0f, 0.3f * vel, t);
		sound += oscillator(freq * 2.0f, 0.0f, 0.1f * vel, t);
		sound += oscillator(freq * 4.0f, 0.0f, 0.04f * vel, t);
		sound += oscillator(freq * 0.5f, 0.0f, 0.08f * vel, t);
		
		return sound;
	}
			
	void Start() 
	{		
		audioSource = gameObject.AddComponent<AudioSource>();
		clip = AudioClip.Create("Epiano", 44100, 2, sampleRate, false, true, OnAudioFilterRead);
	
		audioSource.clip = clip;
		audioSource.loop = true;
		audioSource.Play();		
		
		delay1.Set(19200/3, 25600/3, 0.55f, 0.25f);
		delay2.Set(25600/2, 19200/2, 0.55f, 0.25f);
		delay3.Set(23000, 22000, 0.55f, 0.25f);	
	}
	
	public class DelayLine
	{
		const int bufferSize = sampleRate * 8;
		
		private float[] l = new float[bufferSize];
		private float[] r = new float[bufferSize];

		private int delayLengthL = 1024;
		private int delayLengthR = 1024;
		
		public float feedback = 0.5f;
		public float crossFeedback = 0.25f;
		
		private int cursorL = 0;
		private int cursorR = 0;
		
		public void Set(int delayLengthL, int delayLengthR, float feedback, float crossFeedback)
		{
			this.delayLengthL = Mathf.Clamp(delayLengthL, 1, bufferSize);
			this.delayLengthR = Mathf.Clamp(delayLengthR, 1, bufferSize);
			this.feedback = Mathf.Clamp(feedback, 0.0f, 1.0f);
			this.crossFeedback = Mathf.Clamp(crossFeedback, 0.0f, 1.0f);			
		}
		
		public void Apply(float[] samples, float wet)
		{
			int index = 0;
			Vector2 sample = Vector2.zero;
			for (int i = 0; i < samples.Length/2; i++)
			{
				sample.x = l[cursorL] * feedback + r[cursorR] * crossFeedback + samples[index+0];
				sample.y = r[cursorR] * feedback + l[cursorL] * crossFeedback + samples[index+1];
				
				samples[index+0] += (sample.x - samples[index+0]) * wet;
				samples[index+1] += (sample.y - samples[index+1]) * wet;

				sample.x = Mathf.Clamp(sample.x, -1.0f, 1.0f);
				sample.y = Mathf.Clamp(sample.y, -1.0f, 1.0f);

				l[cursorL] = sample.x;
				r[cursorR] = sample.y;		
				
				cursorL = (cursorL + 1) % delayLengthL;
				cursorR = (cursorR + 1) & delayLengthR;
		
				index += 2;
			}			
		}
	};
	
		
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
			
			Vector2 organ = Vector2.zero;
			
			float timeToNextPluck = 1.7f;
			if (pluckDeltaTimer >= timeToNextPluck)
			{
				note = blues[curnote];
				
				pluckDeltaTimer = 0.0f;
				
				curnote = random.Next(blues.Length);
				
				env.Initialize(0.05f, 0.9f, 0.5f);
			}
			
			organ = generator(getFrequency(note), clock, vel);
			/*
			organ += generator(getFrequency(note-12), clock + 0.001f, vel);
			organ += generator(getFrequency(note-24), clock + 0.0012f, vel);
**/
			organ *= env.Apply();

			samples[index+0] += organ.x;
			samples[index+1] += organ.y;		

			index += 2;
			instrumentIndexL++;
			instrumentIndexR++;
		}
		
			delay1.Apply(samples, 0.45f);
			delay2.Apply(samples, 0.35f);	
			delay3.Apply(samples, 0.25f);	
	}
}
