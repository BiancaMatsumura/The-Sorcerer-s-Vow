using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMixerController : MonoBehaviour
{
    public AudioMixer audioMixer;

    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    private const string MASTER_KEY = "MasterVol";
    private const string MUSIC_KEY = "MusicVol";
    private const string SFX_KEY = "EffectsVol";

    void Start()
    {
        // 🔹 Carrega volumes salvos (ou define padrão)
        float masterValue = PlayerPrefs.GetFloat(MASTER_KEY, 0.75f);
        float musicValue = PlayerPrefs.GetFloat(MUSIC_KEY, 0.75f);
        float sfxValue = PlayerPrefs.GetFloat(SFX_KEY, 0.75f);

        // 🔹 Define valores iniciais dos sliders
        masterSlider.value = masterValue;
        musicSlider.value = musicValue;
        sfxSlider.value = sfxValue;

        // 🔹 Aplica volumes no AudioMixer
        SetVolume(MASTER_KEY, masterValue);
        SetVolume(MUSIC_KEY, musicValue);
        SetVolume(SFX_KEY, sfxValue);

        // 🔹 Eventos de mudança — já salvam automaticamente
        masterSlider.onValueChanged.AddListener((v) => ChangeVolume(MASTER_KEY, v));
        musicSlider.onValueChanged.AddListener((v) => ChangeVolume(MUSIC_KEY, v));
        sfxSlider.onValueChanged.AddListener((v) => ChangeVolume(SFX_KEY, v));
    }

    private void ChangeVolume(string key, float value)
    {
        SetVolume(key, value);
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    private void SetVolume(string parameterName, float value)
    {
        if (value <= 0.001f)
        {
            audioMixer.SetFloat(parameterName, -80f); // Mudo total
        }
        else
        {
            audioMixer.SetFloat(parameterName, Mathf.Log10(value) * 20);
        }
    }
}
