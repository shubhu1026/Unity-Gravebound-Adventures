using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEnd : MonoBehaviour
{
    [SerializeField] AudioClip sfxRoundEnd;
    [SerializeField] float loadNextLevelDelay = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            //other.GetComponent<PlayerMovement>().isUpsideDown = false;
            other.GetComponent<PlayerMovement>().StopPlayer();
            other.GetComponent<AudioSource>().PlayOneShot(sfxRoundEnd, 0.3f);
            other.GetComponent<PlayerMovement>().SetGravity();
            StartCoroutine("LoadNextLevel");
            //other.GetComponent<PlayerMovement>().canMove = true;
        }
    }

    private IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(loadNextLevelDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
