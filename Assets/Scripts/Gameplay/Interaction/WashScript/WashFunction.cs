using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class WashFunction : MonoBehaviour, IDropHandler
{

    // This variable is used to store a reference to the GameObject that is currently being dragged 
    public GameObject currentItem; 
    // This array is used to store references to the Image components of the washed objects
    public Image[] washedObjects;

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop called on ItemSlot");
        Debug.Log("pointerDrag is: " + eventData.pointerDrag);
        if (eventData.pointerDrag != null)
        {
            eventData.pointerDrag.GetComponent<DragDrop>().snapPoint = null;
            // This line sets the position of the object dropped into the center of the sink
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
            // This line sets the currentItem variable to the GameObject that is dropped
            currentItem = eventData.pointerDrag;
            Debug.Log("currentItem set to: " + currentItem.name);

            // This switch statement checks the tag of the currentItem and updates its sprite and tag to the washed version
            switch (currentItem.tag)
            {
                case "LaurelLeaves":
                    currentItem.GetComponent<Image>().sprite = washedObjects[0].sprite;
                    currentItem.tag = "WashedLaurelLeaves";
                    break;
                case "Pork":
                    currentItem.GetComponent<Image>().sprite = washedObjects[1].sprite;
                    currentItem.tag = "WashedPork";
                    break;
            }

            // This line stops the water animation after 5 seconds
            StartCoroutine(StopWaterAfterDelay(0.50f));
        }
    
    }

    private IEnumerator StopWaterAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
    }
}
