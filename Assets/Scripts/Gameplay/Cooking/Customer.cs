using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    [Header("Customer Data")]
    public int customerID;
    public string dishOrder;
    public Sprite customerSprite;
    private bool orderTaken = false;

    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public CustomerPatienceMeter patienceMeter;

    [Header("Patience Meter")]
    public SpriteRenderer meterRenderer;
    public Sprite[] meterStages;
    private float patienceTime = 60f;
    private float patienceTimer;
    private bool isActive = false;
    private bool isServed = false;

    public System.Action<Customer> OnCustomerLeft;
    public System.Action<Customer> OnCustomerServed;

    private Vector3 originalScale;

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

    public void Setup(int id, Sprite sprite, string order, Sprite dishSpriteParam, Vector3 position, Sprite[] meters, float patience = 60f)
    {
        customerID = id;
        customerSprite = sprite;
        dishOrder = order;
        orderTaken = false;

        if (patience <= 0f)
            patience = 60f;

        patienceTime = patience;
        patienceTimer = patienceTime;

        transform.position = position;
        originalScale = transform.localScale;

        if (spriteRenderer != null)
            spriteRenderer.sprite = customerSprite;

        if (meters != null && meters.Length > 0)
            meterStages = meters;

        if (meterRenderer == null)
            meterRenderer = GetComponentInChildren<SpriteRenderer>();

        if (patienceMeter != null)
        {
            patienceMeter.Setup(this, dishOrder, dishSpriteParam);
        }

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

        OnCustomerServed?.Invoke(this);

        StartCoroutine(ExitAnimation());
    }

    public void OnOrderTaken()
    {
        orderTaken = true;

        Debug.Log($"[Customer] Order '{dishOrder}' taken for customer {customerID}.");
    }

    public bool IsOrderTaken()
    {
        return orderTaken;
    }

    private void CustomerLeave()
    {
        if (isServed) return;

        isActive = false;

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

        transform.localScale = originalScale;
        gameObject.SetActive(false);
    }

    public void ResetCustomer()
    {
        isActive = false;
        isServed = false;
        orderTaken = false;
        patienceTimer = patienceTime;
        gameObject.SetActive(false);
        transform.localScale = originalScale;

        if (patienceMeter != null)
            patienceMeter.ResetMeter();
    }
}
