using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchPlayer : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float minX = 2f;
    public float maxX = 6f;
    public float catchRadius = 1.2f;
    public Transform basketVisual;

    private bool isDragging = false;
    private float offset;

    void Update()
    {
        HandleTouch();
        CheckCatch();
    }

    void HandleTouch()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(touch.position);
            worldPos.z = 0f;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    float dist = Mathf.Abs(worldPos.x - transform.position.x);
                    if (dist < 2f)
                    {
                        isDragging = true;
                        offset = transform.position.x - worldPos.x;
                    }
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (isDragging)
                    {
                        float newX = worldPos.x + offset;
                        newX = Mathf.Clamp(newX, minX, maxX);
                        transform.position = new Vector3(newX, transform.position.y, 0f);

                        if (basketVisual != null)
                        {
                            basketVisual.position = new Vector3(newX, basketVisual.position.y, 0f);
                        }
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    break;
            }
        }

        // Editor testing with mouse
        #if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0f;
            float dist = Mathf.Abs(worldPos.x - transform.position.x);
            if (dist < 2f)
            {
                isDragging = true;
                offset = transform.position.x - worldPos.x;
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0f;
            float newX = worldPos.x + offset;
            newX = Mathf.Clamp(newX, minX, maxX);
            transform.position = new Vector3(newX, transform.position.y, 0f);

            if (basketVisual != null)
            {
                basketVisual.position = new Vector3(newX, basketVisual.position.y, 0f);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
        #endif
    }

    void CheckCatch()
    {
        for (int i = CatchGameManager.Instance.activeIngredients.Count - 1; i >= 0; i--)
        {
            FallingIngredient fi = CatchGameManager.Instance.activeIngredients[i];
            if (fi == null)
            {
                CatchGameManager.Instance.activeIngredients.RemoveAt(i);
                continue;
            }

            float distance = Vector2.Distance(transform.position, fi.transform.position);
            if (distance < catchRadius)
            {
                fi.Catch();
                CatchGameManager.Instance.activeIngredients.RemoveAt(i);
            }
        }
    }
}
