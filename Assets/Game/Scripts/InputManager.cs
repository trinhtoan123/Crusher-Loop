using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private LayerMask columnMask;
 private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // if (GameManager.instance.currentGameState != GameManager.GAME_STATE.PLAYING)
            //     return;
            // if (currentHexaColumn != null) return;
            //Debug.Log("Working");

            PickHexaColumn();
        }


        if (Input.GetMouseButton(0))
        {


        }

        if (Input.GetMouseButtonUp(0))
        {

        }
    }
        public bool PickHexaColumn()
    {
        bool isHitColumn = false;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, 100.0f, columnMask))
        {
            if (hit.transform.tag == "CellColumn")
            {
                // Debug.Log("You selected the " + hit.transform.name);

                if (hit.transform.GetComponent<SpoolItem>() != null)
                {
                   hit.transform.GetComponent<SpoolItem>().MoveToConveyor();
                }
            }

        }

        return isHitColumn;
    }
}
