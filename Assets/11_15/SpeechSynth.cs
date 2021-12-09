
// This resource is published under the Creative Commons Zero license.
// https://creativecommons.org/publicdomain/zero/1.0/
//
// 2018 HKU University of the Arts Utrecht, Niels Keetels
// Attibution of the author's name is appreciated but not required.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechSynth : MonoBehaviour 
{
	private float clock = 0.0f;

	private const int sampleRate = 44100;
	private const float timeStep = 1.0f / sampleRate;
	private AudioClip clip = null;
	private AudioSource audioSource = null;
	
	BandPass band1 = new BandPass();
	BandPass band2 = new BandPass();
	
	DelayLine delay1 = new DelayLine();
	
	// See this table for formant frequenties, amplitudes and bandwidths ()
	// In this example code only F1, F2, A1 and A2 are used with 65Hz as a fixed (average) bandwidth
	// 
	// https://www.classes.cs.uchicago.edu/archive/1999/spring/CS295/Computing_Resources/Csound/CsManual3.48b1.HTML/Appendices/table3.html
	
	Formant A = new Formant(685.0f, 0.2f, 1227.0f, 0.2f, 200.0f, 0.0f);
	Formant I = new Formant(300.0f, 0.2f, 2200.0f, 0.3f, 70.0f, 0.0f);	
	Formant O = new Formant(422.0f, 0.2f, 837.0f, 0.2f, 70.0f, 0.0f);
	Formant U = new Formant(265.0f, 0.2f, 1227.0f, 0.2f, 70.0f, 0.0f);
	Formant Ah = new Formant(614.0f, 0.2f, 8.0f, 0.2f, 70.0f, 0.0f);
	Formant Uh = new Formant(368.0f, 0.2f, 1158.0f, 0.2f, 70.0f, 0.0f);
	Formant Ij = new Formant(637.0f, 0.2f, 2093.0f, 0.2f, 70.0f, 0.0f);
	Formant Eh = new Formant(560.0f, 0.2f, 1725.0f, 0.2f, 70.0f, 0.0f);
	
	Formant L = new Formant(460.0f, 0.2f, 1480.0f, 0.2f, 70.0f, 0.0f);
	Formant H = new Formant(700.0f, 0.2f, 1100.0f, 0.2f, 70.0f, 0.6f);	
	Formant R = new Formant(490.0f, 0.2f, 1180.0f, 0.2f, 150.0f, 0.0f);
	Formant S = new Formant(2630.0f, 0.01f, 800.0f, 0.01f, 700.0f, 1.0f);

	Formant N = new Formant(234.0f, 0.2f, 1189.0f, 0.2f, 400.0f, 0.0f);

	Formant Silence = new Formant(0.0f, 0.0f, 0.0f, 0.0f, 70.0f, 0.5f);	
	
	Formant currentFormant = null;

	int character = 0;
	float nextCharTime = 0.14f;
	float characterTimer = 0.0f;

	private float prevF1 = 0.0f;
	private float prevF2 = 0.0f;
	private float prevA1 = 0.0f;
	private float prevA2 = 0.0f;
	
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

	class BandPass
	{
		private float a1;
		private float b1;
		private float b2;
		private float x1 = 0.0f;
		private float x2 = 0.0f;
		
		float exp2(float x)
		{
			return Mathf.Pow(2.0f, x * 1.442695041f);
		}
		
		public void Initialize(float frequency, float q)
		{
			float bw = Mathf.PI * 2.0f * q / sampleRate;
			float c = Mathf.Pow(2.0f, -bw * 1.442695041f);
			
			b2 = - c*c;
			b1 = c * 2.0f * Mathf.Cos( 2.0f*Mathf.PI* frequency / sampleRate );
			a1 = 1.0f - b1 - b2;
		}
		
		public float Run(float s)
		{
			float y = a1 * s + b1 * x1 + b2 * x2;
			x2 = x1;
			x1 = y;
			return y;	
		}
	}
	
	float getFrequency(int midiNote)
	{
		return 440.0f * Mathf.Pow(2.0f, (midiNote - 69) / 12.0f);
	}	
		
	private static System.Random random = new System.Random();

	float GenerateSawtooth(float phase, float frequency) 
	{
		return (phase * frequency - Mathf.Floor(0.5f + phase * frequency)) * 2.0f;
	}	
	
	float GenerateNoise(float phase, float frequency) 
	{
		return (float)random.Next(65535) / 65535.0f;
	}		
	
	class Formant
	{
		public float frequency1;
		public float frequency2;
		public float amplitude1;
		public float amplitude2;
		public float transitionSpeed;
		public float air;
		
		public Formant(float frequency1, float amplitude1, float frequency2, float amplitude2, float transitionSpeed, float air)
		{
			this.frequency1 = frequency1;
			this.amplitude1 = amplitude1;
			this.frequency2 = frequency2;
			this.amplitude2 = amplitude2;
			this.transitionSpeed = transitionSpeed;
			this.air = air;
		}
	}

	void Start () 
	{
		audioSource = gameObject.AddComponent<AudioSource>();
		clip = AudioClip.Create("Synthesizer", 44100, 2, sampleRate, false, true, OnAudioFilterRead);

		audioSource.clip = clip;
		audioSource.loop = true;
		audioSource.Play();			
		
		// slapback echo
		delay1.Set(19200/2, 25600/2, 0.15f, 0.1f);

		currentFormant = H;
	}
	
	void OnAudioFilterRead(float[] samples) 
	{
		int index = 0;
		for (int i = 0; i < samples.Length / 2; i++) 
		{		
			samples[index+0] = 0.0f;
			samples[index+1] = 0.0f;	
		
			clock += timeStep;
			characterTimer += timeStep;

			if (characterTimer >= nextCharTime)
			{                                                           
				switch (character)
				{
					case 0:
					currentFormant = H;
					break;
					case 1:
					currentFormant = A;
					break;
					case 2:
					currentFormant = L;
					break;
					case 3:
					currentFormant = O;
					break;
					case 4:
					currentFormant = O;
					break;
					case 5:
					currentFormant = O;
					break;
					case 6:
					currentFormant = Silence;
					break;
					case 7:
					currentFormant = N;
					break;                   
					case 8:
					currentFormant = I;
					break;    
					case 9:
					currentFormant = L;
					break;    
					case 10:
					currentFormant = S;
					break;    
					case 11:
					currentFormant = Silence;
					break;
					case 12:
					currentFormant = Silence;
					break;
					case 13:
					currentFormant = Silence;
					break;
					case 14:
					currentFormant = Silence;
					break;
					case 15:
					currentFormant = Silence;
					break;
					case 16:
					currentFormant = Silence;                           
					break;                                  
				}

				character++;
				character %= 16;                                                           

				characterTimer = 0.0f;
			}

			if (currentFormant != null)
			{
				prevF1 += (currentFormant.frequency1 - prevF1) * timeStep * currentFormant.transitionSpeed;
				prevF2 += (currentFormant.frequency2 - prevF2) * timeStep * currentFormant.transitionSpeed;
				prevA1 += (currentFormant.amplitude1 - prevA1) * timeStep * currentFormant.transitionSpeed;
				prevA2 += (currentFormant.amplitude2 - prevA2) * timeStep * currentFormant.transitionSpeed;

				band1.Initialize(prevF1, 65.0f);
				band2.Initialize(prevF2, 65.0f);				
				
				float carrierFreq = getFrequency(43);
				float carrier = GenerateSawtooth(clock, carrierFreq) * (1.0f - currentFormant.air) + GenerateNoise(0.0f, 0.0f) * currentFormant.air;

				float formant = band1.Run(carrier * prevA1) + band2.Run(carrier * prevA2);
				
				formant /= 2.0f;
				
				samples[index+0] = formant;
				samples[index+1] = formant;	
			}
			
			index += 2;
		}
		
		delay1.Apply(samples, 0.25f);
	}	
}
