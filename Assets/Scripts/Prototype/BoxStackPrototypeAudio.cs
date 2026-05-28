using UnityEngine;

/// <summary>
/// BoxStack 프로토타입의 작은 합성 효과음을 생성하고 재생합니다.
/// </summary>
public sealed class BoxStackPrototypeAudio : MonoBehaviour
{
    private const int SampleRate = 44100;
    private const float MasterVolume = 0.85f;
    private const float MiddleCFrequency = 261.6256f;
    private static readonly int[] MajorScaleSemitoneOffsets = { 0, 2, 4, 5, 7, 9, 11 };

    private AudioSource _audioSource;
    private AudioClip[] _placementScaleClips;
    private AudioClip _clearClip;
    private AudioClip _failureClip;

    private void Awake()
    {
        Initialize();
    }

    public void PlayPlacement(int placedBoxIndex)
    {
        Initialize();

        if (_placementScaleClips == null || _placementScaleClips.Length == 0)
        {
            return;
        }

        int clipIndex = Mathf.Clamp(placedBoxIndex, 0, _placementScaleClips.Length - 1);
        Play(_placementScaleClips[clipIndex], 0.45f, 1.0f);
    }

    public void PlayClear()
    {
        Play(_clearClip, 0.4f, 1.0f);
    }

    public void PlayFailure()
    {
        Play(_failureClip, 0.36f, 1.0f);
    }

    private void Initialize()
    {
        if (_audioSource != null)
        {
            return;
        }

        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
        _audioSource.spatialBlend = 0f;
        _audioSource.volume = MasterVolume;

        _placementScaleClips = CreatePlacementScaleClips(16);
        _clearClip = CreateToneClip("BoxStack Clear Chime", 0.16f, 660f, 990f, 0.72f);
        _failureClip = CreateToneClip("BoxStack Failure Thud", 0.13f, 150f, 95f, 0.84f);
    }

    private void Play(AudioClip clip, float volumeScale, float pitch)
    {
        Initialize();

        if (clip == null || _audioSource == null || AudioListener.pause)
        {
            return;
        }

        _audioSource.pitch = pitch;
        _audioSource.PlayOneShot(clip, volumeScale);
    }

    private static AudioClip[] CreatePlacementScaleClips(int count)
    {
        AudioClip[] clips = new AudioClip[Mathf.Max(1, count)];
        for (int i = 0; i < clips.Length; i++)
        {
            float frequency = GetMajorScaleFrequency(i);
            clips[i] = CreatePianoToneClip($"BoxStack Placement Scale {i + 1:00}", 0.18f, frequency);
        }

        return clips;
    }

    private static float GetMajorScaleFrequency(int scaleIndex)
    {
        int octave = scaleIndex / MajorScaleSemitoneOffsets.Length;
        int degree = scaleIndex % MajorScaleSemitoneOffsets.Length;
        int semitones = MajorScaleSemitoneOffsets[degree] + (octave * 12);
        return MiddleCFrequency * Mathf.Pow(2f, semitones / 12f);
    }

    private static AudioClip CreatePianoToneClip(string clipName, float durationSeconds, float frequency)
    {
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * durationSeconds));
        float[] samples = new float[sampleCount];
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float progress = sampleCount <= 1 ? 1f : i / (float)(sampleCount - 1);
            phase += frequency / SampleRate;

            float attack = Mathf.Clamp01(progress / 0.025f);
            float decay = Mathf.Exp(-progress * 8.5f);
            float envelope = attack * decay;
            float fundamental = Mathf.Sin(phase * Mathf.PI * 2f);
            float secondHarmonic = Mathf.Sin(phase * Mathf.PI * 4f) * 0.34f;
            float thirdHarmonic = Mathf.Sin(phase * Mathf.PI * 6f) * 0.13f;
            samples[i] = (fundamental + secondHarmonic + thirdHarmonic) * envelope;
        }

        var clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip CreateToneClip(
        string clipName,
        float durationSeconds,
        float startFrequency,
        float endFrequency,
        float decay)
    {
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * durationSeconds));
        float[] samples = new float[sampleCount];
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float progress = sampleCount <= 1 ? 1f : i / (float)(sampleCount - 1);
            float frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
            phase += frequency / SampleRate;

            float attack = Mathf.Clamp01(progress / 0.08f);
            float release = Mathf.Pow(1f - progress, decay);
            float envelope = attack * release;
            float fundamental = Mathf.Sin(phase * Mathf.PI * 2f);
            float softHarmonic = Mathf.Sin(phase * Mathf.PI * 4f) * 0.18f;
            samples[i] = (fundamental + softHarmonic) * envelope;
        }

        var clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
