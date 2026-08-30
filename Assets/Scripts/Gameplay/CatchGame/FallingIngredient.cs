using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingIngredient : MonoBehaviour
{
    public string ingredientName;
    private float fallSpeed;
    private bool isCaught = false;

    public void Init(float speed)
    {
        fallSpeed = speed;

        if (CatchGameManager.Instance != null)
            CatchGameManager.Instance.RegisterIngredient(this);
    }

    void Update()
    {
        if (isCaught) return;

        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        if (transform.position.y < -3.5f)
        {
            CatchGameManager.Instance.MissedIngredient();
            Destroy(gameObject);
        }
    }

    public void Catch()
    {
        if (isCaught) return;
        isCaught = true;
        CatchGameManager.Instance.CatchIngredient(ingredientName);
        StartCoroutine(CatchEffect());
    }

    private IEnumerator CatchEffect()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color startColor = sr != null ? sr.color : Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, startScale * 1.5f, t);
            if (sr != null) sr.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
