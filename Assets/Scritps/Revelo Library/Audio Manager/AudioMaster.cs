using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ReveloLibrary
{
    public class AudioMaster : MonoBehaviour
    {
        class ClipInfo
        {
            public AudioSource Source
            { 
                get; 
                set; 
            }

            public float OriginalVolume
            { 
                get; 
                set; 
            }

            public float currentVolume
            { 
                get; 
                set; 
            }

            public SoundDefinitions Definition
            { 
                get; 
                set; 
            }

            public bool StopFading
            { 
                get; 
                set; 
            }
        }

        public static AudioMaster instance = null;
        public float masterVolume = 1f;
        public List<GameSound> gameSoundList;
        private List<ClipInfo> activeAudioList;

        // Sounds marked to be remove
        private List<ClipInfo> trash;
        private Transform gameSoundsParent;
        // minimum volume while VoiceOver FX
        private float secondPlaneVolume;
        private float defaultVolume;
        private bool isVoiceOverActive;
        // The active music. Only 1 playing music loop
        private SoundDefinitions activeMusic;
        private AudioSource activeVoiceOverSound;

        public float OriginalVolumeBck
        {
            get;
            private set;
        }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            gameSoundsParent = transform;
            OriginalVolumeBck = masterVolume;
            defaultVolume = 1.0f;
            activeAudioList = new List<ClipInfo>();
            trash = new List<ClipInfo>();

            secondPlaneVolume = 0.1f;
            isVoiceOverActive = false;
            activeVoiceOverSound = null;
            activeMusic = SoundDefinitions.NONE;
        }

        void Update()
        {
            UpdateVoiceOverSounds();
            UpdateActiveAudio();
            CheckFaddingSoundsToStopIt();
        }

        #region "Unused methods"

        /*
        public void PlayUniqueSoundDefinitionType(SoundDefinitions soundDefinition)
        {
            bool alreadyPlaying = false;

            for (int i = 0; i < activeAudio.Count && alreadyPlaying != true; i++)
            {
                if (activeAudio[i].Definition == soundDefinition)
                    alreadyPlaying = true;
            }
			
            if (!alreadyPlaying)
                Play(soundDefinition);
        }

        /// <summary>
        /// Stops all sounds but the soundefinition specified with fadding
        /// </summary>
        /// <param name="soundDef">Sound def.</param>
        public void StopFaddingAllFxButThis(SoundDefinitions soundDef)
        {
            foreach (ClipInfo audioClip in activeAudioList)
            {
                if (audioClip.currentVolume > 0)
                if (!audioClip.Source.loop && audioClip.Definition != soundDef)
                    audioClip.StopFading = true;
            }
        }

		*/

        #endregion

        /// <summary>
        /// Play the specified sound.
        /// </summary>
        /// <param name="soundDefinition">Sound definition.</param>
        public AudioSource Play(SoundDefinitions soundDefinition)
        {
            //Create an empty game object
            GameObject soundObj = CreateSoundObject(soundDefinition + "_sfx");
            //Create the Audio source
            AudioSource source = soundObj != null ? soundObj.AddComponent<AudioSource>() : null;

            if (source != null)
            {
                //Configure the GameSound
                GameSound gs = GetTheSoundClip(soundDefinition);
                if (gs.TheSound != null)
                {
                    //Configure the AudioSource
                    SetSourceSettings(ref source, soundDefinition, gs.TheSound, gs.Volume);
                    if (source != null && source.clip != null)
                    {
                        //Play it
                        source.Play();
                        //Destroy it when stop

                        Destroy(soundObj, gs.TheSound.length);
                    }
                    //Set the source as active
                    if (activeAudioList != null && gameSoundList != null && gameSoundList.Count >= (int)soundDefinition)
                    {
                        activeAudioList.Add(new ClipInfo { Source = source, OriginalVolume = gs.Volume, currentVolume = gs.Volume * masterVolume, Definition = soundDefinition });
                    }
                }
				#if UNITY_EDITOR
				else
                {
                    Debug.Log(string.Format("The GameSound {0}.\n has not set any audio clip.", soundDefinition));
                }
                #endif
            }
            return source;
        }

        /// <summary>
        /// Play the specified soundDef and ignoreIfExist.
        /// </summary>
        /// <param name='soundDef'>
        /// Sound def.
        /// </param>
        /// <param name='ignoreIfExist'>
        /// if ignoreIfExist is 'True', play the dound even if if already playing another with same SoundDefinition
        /// Si ignoreIfExist es 'False', Stops the playing sound with same SounfDefinition and start over
        /// This function is used to prevent / allow several different sounds to overlap during the match
        /// </param>
        public AudioSource Play(SoundDefinitions soundDef, bool ignoreIfExist)
        {
            if (!ignoreIfExist)
            {
                StopSound(soundDef);        
            }

            return Play(soundDef);
        }

        /// <summary>
        /// Play a sound eith custom settings
        /// </summary>
        /// <returns>The play.</returns>
        /// <param name="soundDef">Sound def.</param>
        /// <param name="volume">Volume.</param>
        /// <param name="pitch">Pitch.</param>
        public AudioSource CustomPlay(SoundDefinitions soundDef, float volume, float pitch)
        {
            //Create an empty game object
            GameObject soundLoc = CreateSoundObject("Sound_" + soundDef);
            //Create the Audio source
            AudioSource source = soundLoc.AddComponent<AudioSource>();
            //Configure the GameSound
            GameSound gs = GetTheSoundClip(soundDef);
            if (gs.TheSound == null)
            {
                //Configure the AudioSource
                SetSourceSettings(ref source, soundDef, gs.TheSound, gs.Volume);
                source.pitch = (pitch < 0) ? 1 : pitch;
                source.volume = (volume < 0) ? gs.Volume : volume;
                //Play it
                source.Play();
                //Drstroy it when stop
                Destroy(soundLoc, gs.TheSound.length);			
                //Set the source as active
                activeAudioList.Add(new ClipInfo{ Source = source, OriginalVolume = gs.Volume, currentVolume = gs.Volume * masterVolume, Definition = soundDef });
            }
			#if UNITY_EDITOR
			else
            {
                Debug.Log(string.Format("No hay un Clip de audio asignado al GameSound definido como: {0}.\n" +
                        "Revisa el listado de definiciones en el prefab '", soundDef));
            }
            #endif
            return source;
        }

        // Reproduce un sonido, poniendo los demás en Fade, para que este suene por encima
        /// <summary>
        /// Plays a sound over the others lower others volume.
        /// </summary>
        /// <returns>The voice over.</returns>
        /// <param name="voiceOverDef">Voice over def.</param>
        public AudioSource PlayVoiceOver(SoundDefinitions voiceOverDef)
        {
            AudioSource source = Play(voiceOverDef);
            activeVoiceOverSound = source;
            isVoiceOverActive = true;
            return source;
        }
		
        // Reproduce un sonido en forma Loop
        public AudioSource PlayMusic(SoundDefinitions soundDef)
        {
            // Only 1 music at a time
            StopMusic();

            if (IsPlayingSoundDefinition(soundDef))
            {
                StopSound(soundDef);
            }

            GameObject soundObj = CreateSoundObject(soundDef.ToString() + "_music");

            //Create the audio source component
            AudioSource source = soundObj.AddComponent<AudioSource>();

            GameSound gs = GetTheSoundClip(soundDef);
            SetSourceSettings(ref source, soundDef, gs.TheSound, gs.Volume);
            source.loop = true;
            source.Play();

            //Set the source as active
            activeAudioList.Add(new ClipInfo{ Source = source, OriginalVolume = gs.Volume, currentVolume = gs.Volume, Definition = soundDef });
            activeMusic = soundDef;
            return source;
        }

        /// <summary>
        /// Stops a sound.
        /// </summary>
        /// <param name="sondDefinition">Sond definition to stop</param>
        /// <param name="stopFading">If set to <c>true</c> stop fading the sound.</param>
        public void StopSound(SoundDefinitions sondDefinition, bool stopFading = false)
        {
            foreach (ClipInfo ci in activeAudioList)
            {
                if (ci.Definition == sondDefinition)
                {
                    if (ci.currentVolume > 0)
                    {
                        ci.StopFading = true;
                    }
                    else
                    {
                        if (ci.Source)
                        {
                            Destroy(ci.Source.gameObject);
                        }
                        trash.Add(ci);
                    }
                }
            }
        }

        /// <summary>
        /// Stops all sounds.
        /// </summary>
        /// <param name="stopFading">If set to <c>true</c> stop fading.</param>
        public void StopAll(bool stopFading)
        {
            if (!stopFading)
            {
                foreach (ClipInfo ci in activeAudioList)
                {
                    if (ci.Source != null)
                    {
                        Destroy(ci.Source.gameObject);

                    }
                    trash.Add(ci);
                }
            }
            else
            {
                foreach (ClipInfo audioClip in activeAudioList)
                {
                    if (audioClip.currentVolume > 0)
                    {
                        audioClip.StopFading = true;
                    }
                }
            }
        }

        /// <summary>
        /// Stops fx sounds.
        /// </summary>
        /// <param name="stopFading">If set to <c>true</c> stop fading.</param>
        public void StopAllFx(bool stopFading)
        {
            if (!stopFading)
                foreach (ClipInfo ci in activeAudioList)
                {
                    if (!ci.Source.loop)
                    {
                        if (ci.Source)
                        {
                            Destroy(ci.Source.gameObject);
                        }
                        trash.Add(ci);
                    }
                }
            else
            {
                foreach (ClipInfo ci in activeAudioList)
                {
                    if (ci.currentVolume > 0)
                    {
                        if (!ci.Source.loop)
                        {
                            ci.StopFading = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Stops the current playing loop music.
        /// </summary>
        void StopMusic()
        {
            if (activeMusic != SoundDefinitions.NONE)
            {
                StopSound(activeMusic);
                activeMusic = SoundDefinitions.NONE;
            }
        }

        /// <summary>
        /// Reduce the volume of all soundsto 0.
        /// </summary>
        /// <param name="isMuted">If set to <c>true</c> is muted.</param>
        public void Mute(bool isMuted)
        {
            masterVolume = isMuted ? 0 : OriginalVolumeBck;
            foreach (ClipInfo clip in activeAudioList)
            {
                clip.currentVolume = clip.OriginalVolume * masterVolume;
            }
        }

        /// <summary>
        /// Pauses all Fx sound.
        /// </summary>
        /// <param name="pause">If set to <c>true</c> pause.</param>
        public void PauseFX(bool pause)
        { 
            foreach (ClipInfo audioClip in activeAudioList)
            {
                if (audioClip.Definition != activeMusic)
                {
                    if (pause)
                    {
                        audioClip.Source.Pause();
                    }
                    else
                    {
                        audioClip.Source.Play();
                    }
                }
            }
        }

        /// <summary>
        /// Creates the sound object and sets its parent.
        /// </summary>
        /// <returns>The sound object.</returns>
        /// <param name="name">Name.</param>
        private GameObject CreateSoundObject(string name)
        {
            //Create an empty game object
            GameObject soundObj = new GameObject(name);
            if (gameSoundsParent.position != Vector3.zero)
            {
                soundObj.transform.position = gameSoundsParent.position;
            }
            else
            {
                soundObj.transform.position = transform.position;
            }
			
            soundObj.transform.parent = gameSoundsParent;

            return soundObj;
        }

        /// <summary>
        /// Checks the fadding and send to trash listo to be remove if volume is 0 (or pretty near).
        /// </summary>
        void CheckFaddingSoundsToStopIt()
        {
            if (trash == null)
            {
                return;
            }
            
            foreach (ClipInfo ci in activeAudioList)
            {
                if (ci.StopFading)
                {
                    if (ci.currentVolume > 0.001f) //Si aún tienen volumen, se lo bajamos
                    {
                        ci.currentVolume -= ci.currentVolume * Time.deltaTime;
                    }
                    else
                    {
                        if (ci.Source)
                        {
                            Destroy(ci.Source.gameObject);
                        }
                        trash.Add(ci);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the sound clip.
        /// </summary>
        /// <returns>The the sound clip. Attention: Can't use more than once same sound definition because only the first defined will be return;</returns>
        /// <param name="soundDef">Sound definition</param>
        GameSound GetTheSoundClip(SoundDefinitions soundDef)
        {
            // Seleccionamos el clip definido como el parametro soundDef 
            GameSound gs = (from g in gameSoundList
                                     where g.SoundDef == soundDef
                                     select g
                           ).FirstOrDefault();

            if (gs != null)
            {
                gs.SetMainClip();
            }

            return gs;		
        }

        /// <summary>
        /// Sets the source settings.
        /// </summary>
        /// <param name="source">Source.</param>
        /// <param name="soundDef">Sound def.</param>
        /// <param name="clip">Clip.</param>
        /// <param name="volume">Volume.</param>
        private void SetSourceSettings(ref AudioSource source, SoundDefinitions soundDef, AudioClip clip, float volume)
        {
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.dopplerLevel = 0.2f;
            source.minDistance = 150;
            source.maxDistance = 1500;
            source.clip = clip;
            source.volume = volume * masterVolume;
            source.pitch = 1;
        }

        /// <summary>
        /// Determines whether this instance is playing sound definition the specified soundDef.
        /// </summary>
        /// <returns><c>true</c> if this instance is playing sound definition the specified soundDef; otherwise, <c>false</c>.</returns>
        /// <param name="soundDef">Sound def.</param>
        private bool IsPlayingSoundDefinition(SoundDefinitions soundDef)
        {
            bool isPlaying = false;
            foreach (ClipInfo clip in activeAudioList)
            {
                if (clip.Definition == soundDef)
                {
                    isPlaying = true;
                }
            }
            return isPlaying;
        }
		
        // Actualiza los sonidos que suenan por encima del resto, haciendo un Fade
        void UpdateVoiceOverSounds()
        {
            if (isVoiceOverActive && defaultVolume >= secondPlaneVolume)
            {
                defaultVolume -= 0.1f;
            }
            else if (!isVoiceOverActive && defaultVolume < 1.0f)
            {
                defaultVolume += 0.1f;
            }
        }
			
        // Actualiza los AudioSources activos, y los que ya no se reproduzcan los elimina de la lista
        private void UpdateActiveAudio()
        {
            if (trash == null)
            {
                return;
            }
            
            if (!activeVoiceOverSound)
            {
                isVoiceOverActive = false;
            }

            foreach (var audioClip in activeAudioList)
            {
                if (!audioClip.Source)
                {
                    trash.Add(audioClip);
                }
                else if (audioClip.Source != activeVoiceOverSound)
                {
                    audioClip.Source.volume = audioClip.currentVolume * defaultVolume * masterVolume;
                }
            }

            //cleanup
            foreach (var audioClip in trash)
            {
                activeAudioList.Remove(audioClip); 
            }
        }
    }
}
