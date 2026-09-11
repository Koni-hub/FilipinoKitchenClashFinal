using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Customer : MonoBehaviour
{
    [Header("Customer Data")]
    public int customerID;
    public string dishOrder;
    public Sprite customerSprite;

    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public GameObject orderBubble;
    public TMP_Text orderText;

    [Header("Patience Meter")]
    public SpriteRenderer meterRenderer;
    public Sprite[] meterStages;
    public Vector2 meterOffset = new Vector2(1.17f, 1.61f);
    public float meterBaseScale = 0.29f;
    public Color meterColorFull = Color.green;
    public Color meterColorLow = Color.red;
    private float patienceTime = 60f;
    private float patienceTimer;
    private bool isActive = false;
    private bool isServed = false;

    public System.Action<Customer> OnCustomerLeft;
    public System.Action<Customer> OnCustomerServed;

    private void Update()
    {
        if (!isActive || isServed) return;

        patienceTimer -= Time.deltaTime;

        UpdateMeterDisplay();

        if (patienceTimer <= 0f)
        {
            CustomerLeave();
        }
    }

    public void Setup(int id, Sprite sprite, string order, Vector3 position, Sprite[] meters, float patience = 60f)
    {
        customerID = id;
        customerSprite = sprite;
        dishOrder = order;

        if (patience <= 0f)
            patience = 60f;

        patienceTime = patience;
        patienceTimer = patienceTime;

        transform.position = position;

        if (spriteRenderer != null)
            spriteRenderer.sprite = customerSprite;

        if (orderText != null)
            orderText.text = dishOrder;

        if (orderBubble != null)
            orderBubble.SetActive(true);

        if (meters != null && meters.Length > 0)
            meterStages = meters;

        if (meterRenderer == null)
            meterRenderer = GetComponentInChildren<SpriteRenderer>();

        if (meterRenderer != null)
        {
            meterRenderer.transform.position = transform.position + (Vector3)meterOffset;
            meterRenderer.transform.localScale = new Vector3(meterBaseScale, meterBaseScale, 1f);

            if (meterStages != null && meterStages.Length > 0)
                meterRenderer.sprite = meterStages[0];
        }

        isActive = true;
        isServed = false;

        UpdateMeterDisplay();

        gameObject.SetActive(true);
    }

    private void UpdateMeterDisplay()
    {
        if (meterRenderer == null) return;

        float timeRatio = Mathf.Clamp01(patienceTimer / patienceTime);

        meterRenderer.transform.localScale = new Vector3(meterBaseScale * timeRatio, meterBaseScale, 1f);

        meterRenderer.color = Color.Lerp(meterColorLow, meterColorFull, timeRatio);
    }

    public void ServeCustomer()
    {
        if (isServed || !isActive) return;

        isServed = true;
        isActive = false;

        if (orderBubble != null)
            orderBubble.SetActive(false);

        OnCustomerServed?.Invoke(this);

        StartCoroutine(ExitAnimation());
    }

    private void CustomerLeave()
    {
        if (isServed) return;

        isActive = false;

        if (orderBubble != null)
            orderBubble.SetActive(false);

        OnCustomerLeft?.Invoke(this);

        StartCoroutine(ExitAnimation());
    }

    private IEnumerator ExitAnimation()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        transform.localScale = Vector3.one;
        gameObject.SetActive(false);
    }

    public void ResetCustomer()
    {
        isActive = false;
        isServed = false;
        patienceTimer = patienceTime;
        gameObject.SetActive(false);
        transform.localScale = Vector3.one;

        if (meterRenderer != null)
        {
            meterRenderer.transform.localScale = new Vector3(meterBaseScale, meterBaseScale, 1f);
            meterRenderer.color = meterColorFull;
        }
    }
}
