using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace DB.SimpleFramework.SimpleAudioManager {

    public enum SpatialMode { TwoDimensional, ThreeDimensional }

    [Serializable]
    public class SimpleAudioSettings {

        public AudioClip Clip;
        [Space]
        [Range(0f, 1f)] public float Volume = 1f;
        [Space]
        [Range(-3f, 3f)] public float PitchMin = 1f;
        [Range(-3f, 3f)] public float PitchMax = 1f;
        [Space]
        public SpatialMode SpatialMode = SpatialMode.ThreeDimensional;
        [Space]
        [Min(0f)] public float DistanceMin = 1f;
        [Min(0f)] public float DistanceMax = 500f;

        public bool IsValid => Clip != null && Volume > 0;

        public SimpleAudioSettings(AudioClip clip) {
            Clip = clip;
        }

        public void ApplyToAudioSource(AudioSource source) {
            source.clip = Clip;
            source.volume = Volume;
            source.pitch = PitchMin == PitchMax ? PitchMin : Random.Range(PitchMin, PitchMax);
            source.spatialBlend = SpatialMode == SpatialMode.TwoDimensional ? 0f : 1f;
            source.minDistance = DistanceMin;
            source.maxDistance = DistanceMax;
            source.playOnAwake = false;
        }
    }
}
