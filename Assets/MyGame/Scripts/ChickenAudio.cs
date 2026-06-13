using UnityEngine;

public class ChickenAudio : MonoBehaviour
{
    [SerializeField] private AudioSource chickenAudioSource;
    // Credit: chickenCluckClip - ElevenLabs_Gegacker.mp3, generated with ElevenLabs; keep project/user credentials and license notes with the audio asset.
    [SerializeField] private AudioClip chickenCluckClip;
    [SerializeField, Range(0f, 1f)] private float chickenCluckVolume = 0.7f;
    [SerializeField] private Vector2 cluckIntervalRange = new Vector2(4f, 10f);

    private float cluckTimer;

    private void Awake()
    {
        ResolveAudioSource();
        ScheduleNextCluck();
    }

    private void Update()
    {
        if (chickenCluckClip == null || chickenAudioSource == null)
        {
            return;
        }

        cluckTimer -= Time.deltaTime;
        if (cluckTimer > 0f)
        {
            return;
        }

        chickenAudioSource.PlayOneShot(chickenCluckClip, chickenCluckVolume);
        ScheduleNextCluck();
    }

    private void ResolveAudioSource()
    {
        if (chickenAudioSource == null)
        {
            chickenAudioSource = GetComponent<AudioSource>();
            if (chickenAudioSource == null)
            {
                chickenAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        chickenAudioSource.playOnAwake = false;
        chickenAudioSource.spatialBlend = 1f;
    }

    private void ScheduleNextCluck()
    {
        cluckTimer = Random.Range(cluckIntervalRange.x, cluckIntervalRange.y);
    }
}
