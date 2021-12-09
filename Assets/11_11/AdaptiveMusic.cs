using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class Sample
{
    public float fadeInTime = -1.0f;
    public AudioClip clip = null;
    public string name = "empty sample";
}

[Serializable]
public class SubGroup
{
    public float minDuration = 0.0f;
    public float maxDuration = 0.0f;
    public float countDown = 0.0f;
    public string name = "empty subgroup";
    public List<Sample> sampleList = new List<Sample>();
}

[Serializable]
public class Group
{
    public string name = "empty group";
    public List<SubGroup> subgroupList = new List<SubGroup>();
    public List<Sample> transtitionList = new List<Sample>();
}

[Serializable]
public class SubGroupRecord
{
    public string name;
    public float minDuration;
    public float maxDuration;
}

public class AdaptiveMusic : MonoBehaviour
{
    public string NowPlayingSample;
    public float NowPlayingCountDown;
    public float minTime = 0.0f;
    public float maxTime = 0.0f;

    public List<Group> groupList = new List<Group>();
    public string startingGroup = "A";

    private AudioSource source1 = null;
    private AudioSource source2 = null;
    private bool oneIsPlaying = false;

    private bool SoundRunning = false;
    private bool Interrupted = false;

    public AudioListener ear = null;

    public int CurrentTrack = -1;
    public float FadeLength = 2.0f;
    
    bool isGroup(int index)
    {
        return index == 0;
    }

    bool isSubgroup(int index)
    {
        return index == 1;
    }

    bool isTransition(int index)
    {
        return index == 2;
    }

    void AddSample(string samplename, string groupname, int fadeTimeMS, AudioClip clip)
    {
        int index = 0;
        Group currentGroup = null;

        foreach (char c in groupname.ToCharArray())
        {
            if (isGroup(index))
            {
                bool foundGroup = false;

                foreach (Group g in groupList)
                {
                    if (g.name == c.ToString())
                    {
                        foundGroup = true;
                        currentGroup = g;
                    }
                }

                if (!foundGroup)
                {
                    Group newGroup = new Group();
                    newGroup.name = c.ToString();
                    currentGroup = newGroup;
                    groupList.Add(newGroup);
                }

                // foreach (var VARIABLE in COLLECTION) 여기서 막힘
                {
                    
                }
            }
        }
    }

    public List<SubGroupRecord> recordList = new List<SubGroupRecord>();
    private void Awake()
    {
        if (ear == null)
        {
            ear = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<AudioListener>();
        }

        if (source1 == null)
        {
            source1 = ear.gameObject.AddComponent<AudioSource>();
        }

        if (source2 == null)
        {
            source2 = source1.gameObject.AddComponent<AudioSource>();
        }

        source1.GetComponent<AudioSource>().volume = 1.0f;
        source2.GetComponent<AudioSource>().volume = 1.0f;

        // Initailize();

        StreamReader reader = new StreamReader("Asset/Resources/subgroups.txt");

        String line = "";
        while (!reader.EndOfStream)
        {
            line = reader.ReadLine();
            if (line.Contains("sec"))
            {
                string[] tokens = line.Split(' ', '\t');
                string subgroupNr = tokens[0];

                foreach (string s in tokens)
                {
                    if (s.Contains("-"))
                    {
                        string[] timings = s.Split('-');

                        SubGroupRecord rec = new SubGroupRecord();
                        rec.name = subgroupNr;
                        rec.minDuration = int.Parse(timings[0]) * 1.0f;
                        rec.minDuration = int.Parse(timings[1]) * 1.0f;
                        
                        recordList.Add(rec);
                    }
                }
            }
        }

        if (groupList.Count > 0)
        {
            foreach (Group g in groupList)
            {
                foreach (SubGroupRecord r in recordList)
                {
                    if (g.name == r.name[0].ToString())
                    {
                        foreach (SubGroup s in g.subgroupList)
                        {
                            if (s.name == r.name[1].ToString())
                            {
                                s.minDuration = r.minDuration;
                                s.maxDuration = r.maxDuration;
                            }
                        }
                    }
                }
            }
        }
    }

    [ContextMenu("Load Samples")]
    public void Initialize()
    {
        UnityEngine.Object[] soundObjects = Resources.LoadAll("BattleMusic", typeof(AudioClip));
        groupList.Clear();

        foreach (UnityEngine.Object clip in soundObjects)
        {
            string name = clip.name;
            string[] tokens = name.Split('_');
            
            if (tokens.Length < 3)
                continue;

            switch (tokens[0])
            {
                case "Battle":
                    AddSample(clip.name, tokens[1], int.Parse(tokens[3]), (AudioClip) clip);
                    break;
            }
        }

        int numGroups = groupList.Count;
        int numSamples = 0;
        int numTransitions = 0;
        foreach (Group g in groupList)
        {
            foreach (SubGroup s in g.subgroupList)
            {
                numSamples += s.sampleList.Count;
            }

            numSamples += g.transtitionList.Count;
            numTransitions += g.transtitionList.Count;
        }
        
        Debug.Log($"MusicServer added {numSamples.ToString()} samples in {numGroups} groups of which {numTransitions} are transitions");
    }

    int GetRandomIndex(int currentIndex, int length)
    {
        for (int i = 0; i < 5; i++)
        {
            int retVal = UnityEngine.Random.Range(0, length);
            if (retVal != currentIndex)
            {
                return retVal;
            }
        }

        return currentIndex;
    }

    private int nextGroupIndex = -1;

    public void SetGroup(string groupName)
    {
        Debug.Log($"Setting group {groupName}");
        for (int i = 0; i < groupList.Count; i++)
        {
            if (groupName == groupList[i].name)
            {
                nextGroupIndex = i;
            }
        }
    }
}
