using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] AudioClip sfxDie;

    PlayerMovement playerMovementScript;
    AudioSource src;

    int count = 0;

    //Reload level Time
    [SerializeField] private float levelRestartDelay = 0.5f;

    private void Awake()
    {
        playerMovementScript = GetComponent<PlayerMovement>();
        src = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!playerMovementScript.isUpsideDown)
        {
            if (transform.position.y < 0)
            {
                if (count == 0)
                {
                    StartCoroutine("ReloadCurrentLevel");
                    count++;
                }
            }
        }
        else
        {
            if (transform.position.y > 0)
            {
                if (count == 0)
                {
                    StartCoroutine("ReloadCurrentLevel");
                    count++;
                }
            }
        }
    }

    private IEnumerator ReloadCurrentLevel()
    {
        src.PlayOneShot(sfxDie, 0.3f);
        playerMovementScript.isUpsideDown = false;
        yield return new WaitForSeconds(levelRestartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        playerMovementScript.SetGravity();
        count = 0;
    }
}
