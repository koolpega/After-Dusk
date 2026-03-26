using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class splash : MonoBehaviour
{
    public RawImage splashImage;

    public float fadeInDuration = 5f;
    public float fadeOutDuration = 2f;

    private Vector3 startScale = Vector3.one * 0.9f;
    private Vector3 midScale = Vector3.one;
    private Vector3 endScale = Vector3.one * 1.1f;

    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    void Start()
    {
        splashImage.color = new Color(1, 1, 1, 0);
        splashImage.transform.localScale = startScale;

        StartCoroutine(SplashSequence());
    }

    IEnumerator SplashSequence()
    {
        yield return StartCoroutine(FadeScale(
            0f, 1f,
            startScale, midScale,
            fadeInDuration
        ));

        yield return StartCoroutine(FadeScale(
            1f, 0f,
            midScale, endScale,
            fadeOutDuration
        ));

        yield return new WaitForSeconds(0.1f);

        SceneManager.LoadSceneAsync("login");
    }

    IEnumerator FadeScale(
    float fromAlpha, float toAlpha,
    Vector3 fromScale, Vector3 toScale,
    float duration
)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float linear = Mathf.Clamp01(t / duration);
            float p = easeCurve.Evaluate(linear);

            Color c = splashImage.color;
            c.a = Mathf.Lerp(fromAlpha, toAlpha, p);
            splashImage.color = c;
    
            splashImage.transform.localScale =
                Vector3.Lerp(fromScale, toScale, p);

            yield return null;
        }
    }
}