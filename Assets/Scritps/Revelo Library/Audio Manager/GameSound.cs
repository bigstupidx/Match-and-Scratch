using UnityEngine;
using System.Collections;

namespace ReveloLibrary
{
    /// <summary>
    /// A Game sound. Data structure with a sound  and all the stuff that needs
    /// </summary>
    [System.Serializable]
    public class GameSound
    {
        [SerializeField]
        public SoundDefinitions SoundDef;
		
        [SerializeField]
        public float Volume = 1f;
		
        [SerializeField]		
        public AudioClip[] Audios;
		
        private AudioClip mSound;

        public AudioClip TheSound
        {
            get
            { 
                return mSound; 
            }
            set
            { 
                mSound = value; 
            }
        }

        private int mCurrenTrack = -1;

        public int CurrentTrack
        {
            get
            { 
                return mCurrenTrack; 
            }
            set
            { 
                mCurrenTrack = value; 
            }
        }

        public  GameSound()
        {
        }

        public void SetMainClip()
        {
            int i = Random.Range(0, Audios.Length);
            TheSound = Audios[i];	
        }
    }
}