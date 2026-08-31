using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class VoyageReturnTransition : MonoBehaviour
{
    public CanvasGroup fadePanel;
    public TMP_Text dockingText;

    public float dockingDelay = 1.5f;
    public float fadeDuration = 1f;

    private bool transitioning;


    private void Start()
    {
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
        }

        if (dockingText != null)
        {
            dockingText.gameObject.SetActive(false);
        }
    }


    public void BeginDocking()
    {
        if (transitioning)
        {
            return;
        }

        transitioning = true;

        StartCoroutine(
            DockingSequence()
        );
    }


    private IEnumerator DockingSequence()
    {
        if (dockingText != null)
        {
            dockingText.gameObject.SetActive(true);
        }


        yield return new WaitForSeconds(
            dockingDelay
        );


        if (fadePanel != null)
        {
            fadePanel.blocksRaycasts = true;

            float time = 0f;

            while (
                time < fadeDuration
            )
            {
                time += Time.deltaTime;

                fadePanel.alpha =
                    Mathf.Clamp01(
                        time / fadeDuration
                    );

                yield return null;
            }

            fadePanel.alpha = 1f;
        }


        SceneManager.LoadScene(
            "IslandScene"
        );
    }
}