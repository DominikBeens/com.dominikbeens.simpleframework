using UnityEngine;

namespace DB.SimpleFramework.SimpleAudioManager {

    public class SimpleAudioSource : MonoBehaviour {

        private AudioSource source;
        private Transform target;

        private bool hasStarted;
        private bool isPaused;
        private float defaultVolume;

        private float startFadeDuration;
        private float endFadeDuration;

        public bool IsInitialized { get; private set; }
        public bool IsFinished => hasStarted && !source.isPlaying && !isPaused;

        private void Awake() {
            source = gameObject.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = SimpleAudioManager.MixerMasterGroup;
        }

        public void Initialize(SimpleAudioSettings settings, Vector3 position = default) {
            transform.position = position;
            settings.ApplyToAudioSource(source);
            IsInitialized = true;
            Play();
        }

        public void Deinitialize() {
            Stop();
            IsInitialized = false;
            hasStarted = false;
            isPaused = false;
            source.clip = null;
            startFadeDuration = 0f;
            endFadeDuration = 0f;
        }

        public void Tick() {
            HandleFollowTarget();
            HandleVolumeFade();
        }

        public SimpleAudioSource SetMute(bool state) {
            if (!Validate()) { return this; }
            source.mute = state;
            return this;
        }

        public SimpleAudioSource SetPause(bool state) {
            if (!Validate()) { return this; }
            isPaused = state;
            if (state) {
                source.Pause();
            } else {
                source.UnPause();
            }
            return this;
        }

        public SimpleAudioSource SetLoop(bool loop) {
            if (!Validate()) { return this; }
            source.loop = loop;
            return this;
        }

        public SimpleAudioSource SetTarget(Transform target) {
            if (!Validate()) { return this; }
            this.target = target;
            return this;
        }

        public SimpleAudioSource SetFade(float startDuration, float endDuration) {
            if (!Validate()) { return this; }
            defaultVolume = source.volume;
            startFadeDuration = startDuration;
            endFadeDuration = endDuration;
            return this;
        }

        private void Play() {
            hasStarted = true;
            source.Play();

            SetMute(SimpleAudioManager.IsMuted);
            SetPause(SimpleAudioManager.IsPaused);
        }

        public void Stop() {
            if (!Validate()) { return; }
            source.Stop();
        }

        private void HandleFollowTarget() {
            if (!target) { return; }
            transform.position = target.position;
        }

        private void HandleVolumeFade() {
            if (startFadeDuration > 0) {
                if (source.time < startFadeDuration) {
                    source.volume = source.time / startFadeDuration * defaultVolume;
                }
                if (source.time >= startFadeDuration && source.volume != defaultVolume) {
                    source.volume = defaultVolume;
                }
            }
            if (endFadeDuration > 0) {
                if (source.time > source.clip.length - endFadeDuration) {
                    source.volume = (source.clip.length - source.time) / endFadeDuration * defaultVolume;
                }
                if (source.time >= source.clip.length && source.volume != 0f) {
                    source.volume = 0f;
                }
            }
        }

        private bool Validate(bool throwError = true) {
            if (!IsInitialized && throwError) {
                Debug.LogError("Tried accessing functionality on an uninitialized SimpleAudioSource. Please check the property IsInitialized before using a cached SimpleAudioSource.");
            }
            return IsInitialized;
        }
    }
}
