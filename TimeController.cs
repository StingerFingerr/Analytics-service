using System.Collections;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    [SerializeField] private float _slowdownFactor = 0.1f;
    [SerializeField] private float _slowdownLength = 3f;

    private void Start()
    {
        DoSlowMotion();
    }

    private IEnumerator SlowMotion()
    {
        yield return new WaitForSeconds(0.6f);
        Time.timeScale = _slowdownFactor;
        yield return new WaitForSeconds(Time.timeScale += (1f / _slowdownLength) * Time.unscaledDeltaTime);
        Time.timeScale = 1;
    }

    private void DoSlowMotion()
    {
        StartCoroutine(SlowMotion());
    }
}
