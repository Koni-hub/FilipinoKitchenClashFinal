using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BowlFunction : MonoBehaviour, IPointerClickHandler
{
    // SPRITE OF THE BOWL TO BE SPAWNED IN THE TRAY
    [SerializeField] private Image bowlImage;
    public string bowlName;
    // PARENT TRANSFORM FOR THE SPAWNED BOWL IMAGE
    private Transform spawnParent;
    // REFERENCE TO THE BOWL MENU AND BOWL MANAGER
    private GameObject bowlStorage;
    // REFERENCE TO THE BOWL MANAGER TO ACCESS THE SPAWN POSITION FUNCTION
    private BowlManager bowlManager;
    // SHADER TO SHOW THE SELECTED SLOT

    public GameObject selectedShader;
    int snapPointIndex;
    [SerializeField]private BowlSnapPoints[] snapPoints;

    void Start()
    {
        bowlStorage = GameObject.Find("BowlStoragee");
        // bowlManager = GameObject.Find("Canvas").GetComponent<BowlManager>();
        spawnParent = GameObject.Find("SpawnedBowls").transform;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        selectedShader.SetActive(true);
        StartCoroutine(SecondsDelay(0.1f));
    }

    private IEnumerator SecondsDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (BowlSnapPoints point in snapPoints)
        {
            if (!point.isOccupied)
            {
                // SPAWN THE ITEM IN THE INVENTORY
                Image spawnedImage = Instantiate(bowlImage, spawnParent);

                // CONVERTS THE POSITION OF SNAP POINT TO THAT OF SPAWN INGREDIENT
                RectTransform snapPointRect = point.GetComponent<RectTransform>();
                RectTransform spawnParentRect = spawnParent.GetComponent<RectTransform>(); // this is where it comes from
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, snapPointRect.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    spawnParentRect,
                    screenPoint,
                    null,
                    out Vector2 localPoint
                );

                spawnedImage.GetComponent<RectTransform>().anchoredPosition = localPoint;
                point.isOccupied = true;
                BowlDragDrop dragDrop = spawnedImage.GetComponent<BowlDragDrop>();
                if (dragDrop != null)
                {
                    dragDrop.bowlName = bowlName;
                    dragDrop.BSP = point;
                }
                bowlStorage.SetActive(false);
                selectedShader.SetActive(false);
                yield break;
            }
        }
    }
}
