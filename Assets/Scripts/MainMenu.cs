using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    AudioSource src;
    [SerializeField] AudioClip sfxClick;

    private void Awake()
    {
        src = GetComponent<AudioSource>();
    }

    public void Play()
    {
        src.PlayOneShot(sfxClick, 0.3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void PlayAgain()
    {
        src.PlayOneShot(sfxClick, 0.3f);
        SceneManager.LoadScene(1);
    }

    public void Quit()
    {
        src.PlayOneShot(sfxClick, 0.3f);
        Application.Quit();
    }
}
