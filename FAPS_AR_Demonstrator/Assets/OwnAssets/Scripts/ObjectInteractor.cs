using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInteractor : MonoBehaviour
{
    //Maximum support 5 Fingers
    RaycastHit2D hit;
    Vector2[] touches = new Vector2[5];

    public bool Functionenabled;
    public GameObject SelectedObject;
    public GameObject SelectedMachine;
    public bool SimulateWithMouse;

    public float MovementSensibility;
    public UserGUI userGUI;
    public bool allowMovement;

    public bool LockX_Axes = false;
    public bool LockZ_Axes = false;

    // Start is called before the first frame update
    void Start()
    {
        allowMovement = true;
        MovementSensibility = 3f;
        LockZ_Axes = true;
    }

    public void SetSelectedMachine(GameObject machine)
    {
        
        if (machine != null) {
            SelectedMachine = machine;
            // userGUI.GUI_Debug("Selected Machine");
        }
        else
        {
            // userGUI.GUI_Debug("Deselected Machine");
            SelectedMachine = null;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Functionenabled)
        {
            if (!SimulateWithMouse)
            {
                // Select Object with one touch
                if (GUIUtility.hotControl == 0 && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId) && Input.touchCount > 0 && Input.touchCount < 2 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                    RaycastHit hit;
                    if (Physics.Raycast(raycast, out hit))
                    {
                        if (SelectedObject != null && hit.collider.GetComponent<MotorScript>() == null)
                        {
                            SelectedObject.GetComponent<MotorScript>().OnDeactivated();
                        }
                        else if (hit.collider.GetComponent<MotorScript>() != null)
                        {
                            if (SelectedObject != null) SelectedObject.GetComponent<MotorScript>().OnDeactivated();
                            SelectedObject = hit.collider.GetComponent<MotorScript>().gameObject;
                            SelectedObject.GetComponent<MotorScript>().OnActivated();


                        }
                        else if (SelectedObject != null)
                        {
                            SelectedObject.GetComponent<MotorScript>().OnDeactivated();
                        }
                    }
                }
            }else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(raycast, out hit))
                    {
                        if (SelectedObject != null && hit.collider.GetComponent<MotorScript>() == null)
                        {
                            SelectedObject.GetComponent<MotorScript>().OnDeactivated();
                        }
                        else if (hit.collider.GetComponent<MotorScript>() != null)
                        {
                            if (SelectedObject != null) SelectedObject.GetComponent<MotorScript>().OnDeactivated();
                            SelectedObject = hit.collider.GetComponent<MotorScript>().gameObject;
                            SelectedObject.GetComponent<MotorScript>().OnActivated();


                        }
                        else if (SelectedObject != null)
                        {
                            SelectedObject.GetComponent<MotorScript>().OnDeactivated();
                        }
                    }
                }
            }
            // Move Object with 2 touches
            if (Input.touchCount > 1 && Input.touchCount < 3 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                if (SelectedMachine != null && userGUI.movementMode == 0)
                {
                    float amount_X = 0;
                    float amount_Z = 0;
                    float angleY = Vector3.SignedAngle(Camera.main.transform.right, SelectedMachine.transform.right, Vector3.up);
                    Vector2 posAmount = (Input.GetTouch(0).deltaPosition + Input.GetTouch(1).deltaPosition) / 2;

                    // userGUI.GUI_Debug("X: " +posAmount.x + ", Y: "+posAmount.y);

                    // Vorneansicht
                    if (Mathf.Abs(angleY)< 45)
                    {
                        if (!LockX_Axes) amount_X = posAmount.x;
                        if (!LockZ_Axes) amount_Z = posAmount.y;
                    }
                    // Hintenansicht
                    else if (Mathf.Abs(angleY) > 135)
                    {
                        if (!LockX_Axes) amount_X = -posAmount.x;
                        if (!LockZ_Axes) amount_Z = -posAmount.y;
                    }
                    // Linksansicht
                    else if (angleY >= 45 && angleY <= 135)
                    {
                        if (!LockX_Axes) amount_Z = posAmount.x;
                        if (!LockZ_Axes) amount_X = -posAmount.y;
                    }
                    // Rechtsansicht
                    else if (angleY <= -45 && angleY >= -135)
                    {
                        if (!LockX_Axes) amount_Z = -posAmount.x;
                        if (!LockZ_Axes) amount_X = posAmount.y;
                    }
                    if (allowMovement)
                    {
                        SelectedMachine.transform.Translate(amount_X * Time.deltaTime * 0.01f * MovementSensibility, 0, amount_Z * Time.deltaTime * 0.01f * MovementSensibility, Space.Self);
                        SelectedMachine.GetComponent<MachineScript>().ToImageOffset += new Vector3(amount_X * Time.deltaTime * 0.01f * MovementSensibility, 0, amount_Z * Time.deltaTime * 0.01f * MovementSensibility);
                    }

                }else if (SelectedObject!= null && userGUI.movementMode == 1)
                {
                    float amount_X = 0;
                    float amount_Z = 0;
                    float angleY = Vector3.SignedAngle(Camera.main.transform.right, SelectedObject.transform.right, Vector3.up);
                    Vector2 posAmount = (Input.GetTouch(0).deltaPosition + Input.GetTouch(1).deltaPosition) / 2;

                    // userGUI.GUI_Debug("X: " +posAmount.x + ", Y: "+posAmount.y);

                    // Vorneansicht
                    if (Mathf.Abs(angleY) < 45)
                    {
                        if (!LockX_Axes) amount_X = posAmount.x;
                        if (!LockZ_Axes) amount_Z = posAmount.y;
                    }
                    // Hintenansicht
                    else if (Mathf.Abs(angleY) > 135)
                    {
                        if (!LockX_Axes) amount_X = -posAmount.x;
                        if (!LockZ_Axes) amount_Z = -posAmount.y;
                    }
                    // Linksansicht
                    else if (angleY >= 45 && angleY <= 135)
                    {
                        if (!LockX_Axes) amount_Z = posAmount.x;
                        if (!LockZ_Axes) amount_X = -posAmount.y;
                    }
                    // Rechtsansicht
                    else if (angleY <= -45 && angleY >= -135)
                    {
                        if (!LockX_Axes) amount_Z = -posAmount.x;
                        if (!LockZ_Axes) amount_X = posAmount.y;
                    }
                    if (allowMovement)
                    {
                        SelectedObject.transform.Translate(amount_X * Time.deltaTime * 0.01f * MovementSensibility, 0, amount_Z * Time.deltaTime * 0.01f * MovementSensibility, Space.Self);
                    }

                }

            }
            // Rotate Object with 3 touches
            if (Input.touchCount > 2 && Input.touchCount < 4 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                // rot Amount detects how far and in which direction to rotate
                float rotAmount = (Input.GetTouch(0).deltaPosition.x + Input.GetTouch(1).deltaPosition.x + Input.GetTouch(2).deltaPosition.x) / 3;
                if (SelectedMachine != null && allowMovement && userGUI.movementMode == 0)
                {
                    SelectedMachine.transform.Rotate(0, - rotAmount * Time.deltaTime* 1 * MovementSensibility, 0);
                    SelectedMachine.GetComponent<MachineScript>().ToImageRotationOffset += -rotAmount * Time.deltaTime * 1 * MovementSensibility;
                }
                else if (SelectedObject != null && allowMovement && userGUI.movementMode == 1)
                {
                    SelectedObject.transform.Rotate(0, -rotAmount * Time.deltaTime * 1 * MovementSensibility, 0);
                }

            }
        }
    }
}
