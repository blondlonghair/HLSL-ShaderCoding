// This resource is published under the Creative Commons Zero license.
// https://creativecommons.org/publicdomain/zero/1.0/
//
// 2018 HKU University of the Arts Utrecht, Niels Keetels
// Attibution of the author's name is appreciated but not required.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Celeste : MonoBehaviour 
{
	private static float clock = 0.0f;

	private const int sampleRate = 44100;
	private const float timeStep = 1.0f / sampleRate;
	private const int beatsPerMinute = 90;
	private const float samplesPerTick = (60.0f / 4.0f / beatsPerMinute) * sampleRate;
	private const float ticksPerStep = (60.0f / 4.0f / beatsPerMinute);	
	
	private float stepTicks = 0.0f;
	private int currentStep = -1;
	private int patternLength = 32;
	private int currentPattern = -1;
	
	private AudioClip clip = null;
	private AudioSource audioSource = null;
	
	private const int numGuitarVoices = 5;
	private int curGuitarVoiceIndex = 0;
	private KS[] guitar = new KS[numGuitarVoices]; 
	private KS[] guitarLow = new KS[numGuitarVoices]; 
	
	private const int numSawVoices = 5;
	private int curSawVoiceIndex = 0;
	private SawVoice[] sawVoice = new SawVoice[numSawVoices];
	
	private int baseNote = 88;
	private int[] pianoArpeggio = {0, -5, -9, -12};
	private int pianoArpNoteIndex = 0;

	private int[] sawArpeggio = {-12, -9, -5, 0};
	private int sawArpNoteIndex = 0;
	
	private int[] bassNotes = {52, 59, 50, 57};
	private int bassNoteIndex = 0;
	
	private int[] melodyNotes = {64, 64, 59, 66, 68, 71, 73, 74, 76, 74, 73, 71, 69, 73};
	private int melodyIndex = 0;
	
	private DrumMachine drums = new DrumMachine();
	private SwooshVoice swoosh = new SwooshVoice();
	private BassVoice bassVoice = new BassVoice();
	
	private int currentBassIndex = 0;
	
	private DelayLine delay1 = new DelayLine();
	private DelayLine delay2 = new DelayLine();
		
	private bool enableGuitar = true;
	private bool enableSaw = true;
	private bool enableSwoosh = true;
	private bool enableDrums = true;
	private bool enableBass = true;
	private bool enableMelody = true;
	
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
		
		public float Apply()
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
			
			return Mathf.Max(0.0f, v);
		}			
	};
		
	
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
	
	public enum FilterType 
	{
		LOWPASS,
		HIGHPASS,
		BANDPASS,
		NONE
	};

	// source of this algorithm?
	public class StateVariableFilter 
	{
		private float y0, y1, y2, y3;
		
		public void Initialize()
		{	
			y0 = y1 = y2 = y3 = 0.0f;
		}

		// 2-pole (12dB per octaaf) state variable filter
		public float Apply(FilterType type, float y, float cutoff, float q) 
		{
			float f = 2.0f * Mathf.Sin(Mathf.PI * cutoff / sampleRate);
			float feedback = q + q / (1.0f - f);
			
			y0 = y0 + f * (y - y0);
			y0 = y0 + feedback * (y0 - y1);
			y1 = y1 + f * (y0 - y1);
			y2 = y2 + f * (y1 - y2);
			y3 = y3 + f * (y2 - y3);

			switch (type) 
			{
        case FilterType.LOWPASS:
            return y3;
        case FilterType.HIGHPASS:
            return y - y3;
        case FilterType.BANDPASS:
            return y0 - y3;
			}
			
			return 0.0f;
		}
	};		
	
	public class KS
	{
		const int maxBufferSize = 500;
		public float[] buffer = new float[maxBufferSize];
		int buffersize = 0;
		bool isInitialized = false;
		
		public void pluck(float frequency, int durationMs)
		{
			buffersize = (int)Mathf.Ceil(sampleRate / frequency);
			if (buffersize >= maxBufferSize)
				buffersize = maxBufferSize;
			
			for (int i = 0; i < buffersize; i++)
			{
				buffer[i] = (float)random.Next(65536) / 32768.0f - 1.0f;
			}		

			isInitialized = true;
		}
		
		int index = 0;
		public float sample()
		{
			if (!isInitialized)
				return 0.0f;
	
			float avg = 0.996f * .5f * (buffer[index] + buffer[(index+1) % buffersize]);			
			buffer[index] = avg;
			index++;
			index %= buffersize;
			return avg;
		}
	};	
	
		public float Foldback(float sample, float threshold )
		{
		float s = sample;
			if (Mathf.Abs(sample) > threshold) 
			{
				s = Mathf.Abs(Mathf.Abs(Mathf.Repeat(sample - threshold, threshold * 4.0f)) - 		threshold * 2.0f) - threshold;
			}
return s;
}	
	
	class DrumMachine
	{
		private float kickRange = 400.0f;
		private float kickSlope = 10.7f;
		private float kickLength = 1.0f;
		private float kickTime = 0.0f;
		private float kickAmp = 1.0f;
		
		private Envelope kickEnvelope = new Envelope();
		
		private float snareRange = 440.0f;
		private float snareSlope = 14.0f;
		private float snareLength = 0.6f;
		private float snareTime = 0.0f;
		private float snareAmp = 1.0f;

		private Envelope snareEnvelope = new Envelope();		
		private StateVariableFilter snareFilter = new StateVariableFilter();
		
		private float hihatLength = 0.0f;
		private float hihatTime = 0.0f;
		private float hihatAmp = 1.0f;
		
		private Envelope hihatEnvelope = new Envelope();		
		private StateVariableFilter hihatFilter = new StateVariableFilter();		
		
		private bool snareOn = false;
		private bool kickOn = false;
		private bool hihatOn = false;
		

		
		private float time;
		
		public void Initialize()
		{
			snareFilter.Initialize();
		}
		
		public void Kick(float amplitude)
		{
			kickEnvelope.Initialize(0.0f, 0.0f, kickLength);
			kickTime = 0.0f;
			kickAmp = amplitude;
			kickOn = true;
		}
		
		public void Snare(float amplitude)
		{	
			snareEnvelope.Initialize(0.0f, 0.0f, snareLength);
			snareTime = 0.0f;
			snareAmp = amplitude;
			snareOn = true;
		}
		
		public void Hihat(float amplitude)
		{
			hihatEnvelope.Initialize(0.0f, 0.0f, 0.1f);
			hihatTime = 0.0f;
			hihatAmp = amplitude;
			hihatOn = true;
		}
		
		public float Apply()
		{
			// kick
			kickTime += timeStep;			
			float kickFreq = (kickLength / (kickTime * kickSlope + 1.0f)) * kickRange;
			float kick = GenerateSine(kickTime, kickFreq) * kickEnvelope.Apply();
			
			// snare
			snareTime += timeStep;
			float rattle = 0.66f;
			float snareFreq = (snareLength / (snareTime * snareSlope + 1.0f)) * snareRange;
			float snare = (1.0f - rattle) * GenerateSine(snareTime, snareFreq);
			float snareEnv = snareEnvelope.Apply();
			snare = rattle * snareFilter.Apply(FilterType.HIGHPASS, snare + GenerateNoise(0.0f, 0.0f), 3000.0f, 0.0f);
			snare += snareFilter.Apply(FilterType.LOWPASS, snare + GenerateNoise(0.0f, 0.0f), 5000.0f, 0.0f);
			snare *= snareEnv * snareEnv * snareEnv;
			
			// hihat
			float hihatEnv = hihatEnvelope.Apply();
			float hihat = GenerateNoise(0.0f, 0.0f) * hihatEnv * hihatEnv;
			
			// mix kick + snare + hihat
			float total = (kickOn ? kick * kickAmp : 0.0f) + (snareOn ? snare * snareAmp : 0.0f) + (hihatOn ? hihat * hihatAmp : 0.0f);
			
			return Mathf.Clamp(total, -1.0f, 1.0f);
		}
	};
	
	class SawVoice
	{
		private Envelope amplitudeEnvelope = new Envelope();
		private Envelope filterEnvelope = new Envelope();
		private StateVariableFilter filter = new StateVariableFilter();
		
		private float time;
		private float frequency;
		
		public void Initialize(int midiNote)
		{
			time = 0.0f;
			frequency = getFrequency(midiNote);
			amplitudeEnvelope.Initialize(0.01f, 0.01f, 0.2f);
			filterEnvelope.Initialize(0.01f, 0.01f, 0.2f);			
			filter.Initialize();
		}
		
		public Vector2 Apply()
		{
			time += timeStep;
			float detune = frequency * 0.0006f;
			float waveForm1 = GenerateSawtooth(time, frequency + detune);
			float waveForm2 = GenerateSawtooth(time, frequency - detune);
			float cutoff = 3000 + Mathf.Cos(clock * (1.0f / 1.5f)) * 2400.0f * filterEnvelope.Apply();
			waveForm1 = filter.Apply(FilterType.HIGHPASS, waveForm1, cutoff, 0.0f) * 0.2f;
			waveForm2 = filter.Apply(FilterType.HIGHPASS, waveForm2, cutoff, 0.0f) * 0.2f;
			float amplitude = amplitudeEnvelope.Apply();
			
			Vector2 sample = Vector2.zero;
			sample.x = (waveForm1 * 0.75f + waveForm2 * 0.25f) * amplitude;
			sample.y = (waveForm1 * 0.25f + waveForm2 * 0.75f) * amplitude;
			
			return sample;
		}
	};
	
	class SwooshVoice
	{
		private float time = 0.0f;
		private StateVariableFilter filter = new StateVariableFilter();
		private Envelope env = new Envelope();
		
		public void Initialize()
		{
			time = 0.0f;
			env.Initialize(1.001f, 4.0f, 1.001f);
			filter.Initialize();
		}
		
		public Vector2 Apply()
		{
			Vector2 s;
			
			time += timeStep;
			
			float shape = env.Apply();
			float LFO = GenerateSine(time, 0.25f / 4.0f);
			float noise = GenerateNoise(0.0f, 0.0f) * 0.3f;
			noise += 0.22f * LFO * noise;
			noise = filter.Apply(FilterType.HIGHPASS, noise, 2500.0f, 0.0f);
			noise = filter.Apply(FilterType.LOWPASS, noise, 3000.0f - 1000.0f * LFO, 0.0f);			
			
			s.x = noise * shape;
			
			noise = GenerateNoise(0.0f, 0.0f) * 0.3f;
			noise += 0.2f * LFO * noise;			
			noise = filter.Apply(FilterType.HIGHPASS, noise, 2500.0f, 0.0f);
			noise = filter.Apply(FilterType.LOWPASS, noise, 3000.0f - 1000.0f * LFO, 0.0f);			
			
			s.y = noise * shape;
			
			return s;
		}
	};
	
	class BassVoice
	{
		private float time = 0.0f;
		private float frequency = 220.0f;
		private float lastFrequency;
		private StateVariableFilter filter = new StateVariableFilter();
		private Envelope env = new Envelope();
		private bool firstNote = true;
		private float legatoCoeff = 1000.0f;
		
		public void Initialize(int midiNote)
		{
			time = 0.0f;
			frequency = getFrequency(midiNote);
			env.Initialize(0.1f, 0.0f, 1.2f);
			if (firstNote)
			{
				lastFrequency = frequency;
				firstNote = false;
			}
		}
		
		
		public float Apply()
		{
			float s = 0.0f;
			time += timeStep;
			
			lastFrequency += (frequency - lastFrequency) * timeStep * legatoCoeff;

			float detune = 0.0003f * lastFrequency;
			s = GenerateTriangle(time, lastFrequency + detune) + GenerateTriangle(time, lastFrequency - detune);

			return s * env.Apply();
		}		
	};

	static float getFrequency(int midiNote)
	{
		return 440.0f * Mathf.Pow(2.0f, (midiNote - 69) / 12.0f);
	}	
	
	private static System.Random random = new System.Random();
	public static float GenerateSine(float phase, float frequency) 
	{
		return Mathf.Sin (frequency * phase * Mathf.PI * 2.0f);
	}
	
	public static float GenerateSawtooth(float phase, float frequency) 
	{
		return (phase * frequency - Mathf.Floor(0.5f + phase * frequency)) * 2.0f;
	}
	
	public static float GenerateTriangle(float phase, float frequency) 
	{
		return Mathf.Abs(GenerateSawtooth(phase, frequency)) * 2.0f - 1.0f;
	}
	
	public static float GenerateSquare(float phase, float frequency) 
	{
		return Mathf.Sin (frequency * phase * Mathf.PI * 2.0f) < 0 ? -1.0f : 1.0f;
	}
	
	public static float GenerateNoise(float phase, float frequency) 
	{
		return (float)random.Next(65535) / 65535.0f;
	}
	
	void Start() 
	{	
		audioSource = gameObject.AddComponent<AudioSource>();
		clip = AudioClip.Create("Synthesizer", 44100, 2, sampleRate, false, true, OnAudioFilterRead);

		audioSource.clip = clip;
		audioSource.loop = true;
		audioSource.Play();		
		
		delay1.Set(19200, 25600, 0.55f, 0.25f);
		delay2.Set(25600/2, 19200/2, 0.25f, 0.25f);
		
		drums.Initialize();
		
		for (int g = 0; g < numGuitarVoices; g++)
		{
			guitar[g] = new KS();
			guitarLow[g] = new KS();
		}
		
		for (int w = 0; w < numSawVoices; w++)
		{
			sawVoice[w] = new SawVoice();
		}
	}
	
	void OnAudioFilterRead(float[] samples) 
	{
		
		int index = 0;
		for (int i = 0; i < samples.Length / 2; i++) 
		{		
			samples[index+0] = 0.0f;
			samples[index+1] = 0.0f;	
		
			clock += timeStep;
			stepTicks += timeStep;			

			bool step = false;
			if (stepTicks >= ticksPerStep) 
			{
				currentStep++;
				step = true;
				stepTicks = 0.0f;
			}
			
			if (currentStep >= patternLength)
			{
				currentStep = 0;
				currentPattern++;
				Debug.Log("step = " + currentStep + "    pattern = " + currentPattern);
			}

			// early exit point
			if (currentPattern < 0 && currentStep < 0)
			{
				index += 2;
				continue;
			}

			// Give the game time to load before playing music
			if (currentPattern >= 0)
			{			
				if (step && guitar[0] != null  && currentPattern >= 2)
				{
					float guitarFrequency = getFrequency(baseNote + pianoArpeggio[pianoArpNoteIndex]);
					guitar[curGuitarVoiceIndex].pluck(guitarFrequency, 5000);
					guitarLow[curGuitarVoiceIndex].pluck(guitarFrequency / 2.0f, 5000);
					curGuitarVoiceIndex++;
					pianoArpNoteIndex++;
					
					curGuitarVoiceIndex %= numGuitarVoices;
					pianoArpNoteIndex %= pianoArpeggio.Length;
				}
				
				if (step && sawVoice[0] != null)
				{
					sawVoice[sawArpNoteIndex].Initialize(baseNote - 12 + sawArpeggio[sawArpNoteIndex]);
					sawArpNoteIndex++;
					sawArpNoteIndex %= sawArpeggio.Length;
				}
			
				// piano/guitar
				if (enableGuitar && guitar[0] != null)
				{
					for (int g = 0; g < numGuitarVoices; g++)
					{
						samples[index+0] += guitar[g].sample() * 0.15f;
						samples[index+1] += guitarLow[g].sample() * 0.15f;
					}					
				}
			
				// Saw
				if (enableSaw && sawVoice[0] != null && currentPattern >= 0)
				{
					for (int w = 0; w < numSawVoices; w++)
					{
						Vector2 voice = sawVoice[w].Apply();
						samples[index+0] += voice.x * 0.5f;
						samples[index+1] += voice.y * 0.5f;
					}
				}
				
				// bass
				if (enableBass && bassVoice != null && currentPattern >= 4)
				{
					if (step && (currentStep == 0 || currentStep == 6 || currentStep == 18 || currentStep == 22))
					{
						bassVoice.Initialize(bassNotes[currentBassIndex]-12);
						
						currentBassIndex++;
						currentBassIndex %= bassNotes.Length;
					}	 
					
					float bass = bassVoice.Apply();
					
					samples[index+0] += bass * 0.4f;
					samples[index+1] += bass * 0.4f;						
				}
				
				// swoosh at start of pattern
				if (step && currentStep == 0 && currentPattern == 7)
				{
					swoosh.Initialize();
				}
				
				if (enableSwoosh && currentPattern == 7)
				{
					Vector2 sw = swoosh.Apply();
					samples[index+0] += sw.x * 0.2f;
					samples[index+1] += sw.y * 0.2f;	
				}			
				
				if (step && currentPattern >= 0)
				{
					// drums
					if (step && (currentStep == 28))
					{
						drums.Snare(0.7f);
					}
					
					if (step && (currentStep == 0 || currentStep == 6 || currentStep == 18 || currentStep == 22))
					{
						drums.Kick(0.3f);
					}
					
					// one bar: [-,-,2,3]
					if ((currentStep % 4) == 2 || (currentStep % 4) == 3)
					{
						drums.Hihat(0.5f);
					}	
				}
				
				if (enableDrums && currentPattern >= 4)
				{
					float drum = drums.Apply();			
					samples[index+0] += drum * 0.5f;
					samples[index+1] += drum * 0.5f;			
				}			
		}
	
			index += 2;
		}	
		
		// global effects
		delay1.Apply(samples, 0.1f);
		delay2.Apply(samples, 0.05f);
		
	}	
}
