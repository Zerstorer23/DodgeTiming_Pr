using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_TouchPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
/*, IPointerClickHandler,
IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler,
*/
{
    // Start is called before the first frame update
    public static bool isTouching = false;
    public static Vector2 touchVector;

    public void HandleTouch() {
        if (Input.touchCount <= 0) return;

       // Debug.Log("touch count " + Input.touchCount);
        for (int i = 0; i < Input.touchCount; ++i)
        {
            var touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                 //   Debug.Log(i + ": Mousedown " + touch.position + " touch " + EventSystem.current.IsPointerOverGameObject(touch.fingerId));
                    isTouching = true;
                    touchVector = touch.position;
                    return;
                }
                
            }
            else if (touch.phase == TouchPhase.Canceled
               || touch.phase == TouchPhase.Ended)
            {
                isTouching = false;
                return;
            }
            else 
            {
                if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    touchVector = touch.position;
                    return;
                }
            }
        }

    }
/*    private void Update()
    {
        HandleTouch();
    }*/
 public void OnBeginDrag(PointerEventData eventData)
    {
        StartTouch(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        StartTouch(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndTouch(eventData);
    }
    
    /*
    public void OnPointerClick(PointerEventData eventData)
    {
     //   Debug.Log("Clicked: " + eventData.pointerCurrentRaycast.gameObject.name);
    }*/

    public void OnPointerDown(PointerEventData eventData)
    {
        StartTouch(eventData);
    }

/*    public void OnPointerEnter(PointerEventData eventData)
    {
     //   Debug.Log("Mouse Enter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
       // Debug.Log("Mouse Exit");
    }*/

    public void OnPointerUp(PointerEventData eventData)
    {
        EndTouch(eventData);
    }
    public void StartTouch(PointerEventData eventData) {

        isTouching = true;
        touchVector = eventData.position;
       // Debug.Log("Mouse Down: " + touchVector);
    }
    public void EndTouch(PointerEventData eventData) {
        isTouching = false;
        touchVector = eventData.position;
    }
}
