using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private LayerMask columnMask;
    private SpoolItem currentSpoolItem;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (GameManager.instance.CurrentGameState != GameState.Playing)
                return;
            PickSpool();
        }

        if (Input.GetMouseButton(0))
        {

        }

        if (Input.GetMouseButtonUp(0))
        {
        }
    }
    public bool PickSpool()
    {
        bool isHitColumn = false;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f, columnMask))
        {
            SpoolItem spoolItem = hit.transform.GetComponent<SpoolItem>();

            if (hit.transform.tag == "Spool")
            {
                if (spoolItem != null)
                {
                    spoolItem.StartMoving();
                    isHitColumn = true;
                }
            }
        }

        return isHitColumn;
    }
}
