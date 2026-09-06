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

        isActive = true;
        isServed = false;

        UpdateMeterDisplay();

        gameObject.SetActive(true);
    }

    private void UpdateMeterDisplay()
    {
        if (meterRenderer == null) return;
        if (meterStages == null || meterStages.Length == 0) return;

        float timeRatio = patienceTimer / patienceTime;

        int stageIndex;
        if (timeRatio > 0.8f)
            stageIndex = 0;
        else if (timeRatio > 0.6f)
            stageIndex = 1;
        else if (timeRatio > 0.4f)
            stageIndex = 2;
        else if (timeRatio > 0.2f)
            stageIndex = 3;
        else if (timeRatio > 0.05f)
            stageIndex = 4;
        else
            stageIndex = meterStages.Length - 1;

        stageIndex = Mathf.Clamp(stageIndex, 0, meterStages.Length - 1);

        meterRenderer.sprite = meterStages[stageIndex];
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
    }
}
