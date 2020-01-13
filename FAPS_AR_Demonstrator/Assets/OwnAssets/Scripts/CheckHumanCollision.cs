using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckHumanCollision : MonoBehaviour
{
    public bool HumanIsInside;
    public UserGUI userGUI;
    public MachineScript machineScript;
    public bool ActivateDetection;
    public bool AllowEnter;
    public bool WallsLifted;

    public void Start()
    {
        WallsLifted = true;
        ActivateDetection = true;
        HumanIsInside = false;
        userGUI = GameObject.FindWithTag("Canvas").GetComponent<UserGUI>();
        StartCoroutine(CheckUpdate());
    }

    IEnumerator CheckUpdate()
    {
        while (true)
        {
            if (ActivateDetection && !AllowEnter)
            {

                Collider[] cols = Physics.OverlapBox(transform.position, GetComponent<Collider>().bounds.extents);
                bool isHumanThere = false;
                foreach (Collider c in cols)
                {
                    if (c.tag == "HumanCollider" || c.tag == "MainCamera")
                    {
                        //Debug.Log("Human there");
                        // userGUI.GUI_Debug("Human is inside");
                        if (!HumanIsInside)
                        {
                            userGUI.WarningSign.SetActive(true);
                            HumanIsInside = true;
                            // userGUI.GUI_Debug("Human is there");
                        }
                        isHumanThere = true;
                        break;
                    }
                }
                if (!isHumanThere && HumanIsInside)
                {
                    userGUI.WarningSign.SetActive(false);
                    HumanIsInside = false;
                    // userGUI.GUI_Debug("Human is gone");
                }
            }
            yield return null;
        }

    }
}