using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorScript : MonoBehaviour
{
    public MachineScript Machine;
    MeshRenderer mesh;
    public GameObject Initcanvas;
    private UserGUI user_gui;
    public GameObject Canvas;

    // Every Subwindow of a motor that should be visualized:
    public bool VisibleWindowsChanged;

    public bool IsConveyorMotor;
    public bool IsPortalAxes;

    

    public bool TestString;
    
    public UserGUI.MotorInfo Info = new UserGUI.MotorInfo();
    
    public bool activated;
    public void Start()
    {
       
        Info.IsMotor = IsConveyorMotor;
        Info.IsPortalAxes = IsPortalAxes;
        Info.MotorName = transform.parent.name;
        Info.VisibleWindows = new List<UserGUI.InfoVisible>();
        Info.Canvas_Parameter = new List<string>();
        Info.MessageItems = new List<MessageItem>();
        if (IsPortalAxes)
        {
            UserGUI.InfoVisible i1 = new UserGUI.InfoVisible
            {
                InfoType = "Speed",
                IsVisible = true
            };
            UserGUI.InfoVisible i2 = new UserGUI.InfoVisible
            {
                InfoType = "Speed",
                IsVisible = true
            };
            UserGUI.InfoVisible i3 = new UserGUI.InfoVisible
            {
                InfoType = "Speed",
                IsVisible = true
            };
            UserGUI.InfoVisible i4 = new UserGUI.InfoVisible
            {
                InfoType = "Speed",
                IsVisible = true
            };
            UserGUI.InfoVisible i5 = new UserGUI.InfoVisible
            {
                InfoType = "Speed",
                IsVisible = true
            };
        }
        else if (IsConveyorMotor)
        {
            UserGUI.InfoVisible i1 = new UserGUI.InfoVisible
            {
                InfoType = "Velocity",
                IsVisible = true
            };
            UserGUI.InfoVisible i2 = new UserGUI.InfoVisible
            {
                InfoType = "Acceleration",
                IsVisible = true
            };
            UserGUI.InfoVisible i3 = new UserGUI.InfoVisible
            {
                InfoType = "Power",
                IsVisible = true
            };

            Info.VisibleWindows.Add(i1);
            Info.VisibleWindows.Add(i2);
            Info.VisibleWindows.Add(i3);
        }
        user_gui = GameObject.Find("PostPlacementInteractor").GetComponent<ObjectInteractor>().userGUI;

        StartCoroutine(_Update());
       
    }
    public void AddMessage(string[] parameter)
    {
        Debug.Log("Adding");
        user_gui.GUI_Debug("Added Message Item");
        MessageItem item = MessageItem.CreateInstance("MessageItem") as MessageItem;
        item.AddText(parameter[0], parameter[1]);
        Info.MessageItems.Add(item);
    }
    public void HideMesh()
    {
        mesh = GetComponent<MeshRenderer>();
        mesh.enabled = false;
    }
    
    public IEnumerator _Update()
    {
        while (true)
        {
            // Canvas immer auf Kamera ausrichten und Werte darin aktualisieren!
            Canvas.transform.LookAt(Canvas.transform.position + (Canvas.transform.position - Camera.main.transform.position));

            string s = "";
            if (Info.Canvas_Parameter.Count > 0) {s = Info.Canvas_Parameter[0]; }
            else
            {
                s = Info.Parameter;
            }
           
            /*for (int i = 0; i < Info.Canvas_Parameter.Count; i++)
            {
                foreach (UserGUI.InfoVisible inf in Info.VisibleWindows)
                {
                    if (Info.Canvas_Parameter[i].Contains(inf.GetInfoType()) && inf.GetIsVisible())
                    {
                        s += Info.Canvas_Parameter[i]+"\n";
                        break;
                    }
                    else if (!inf.GetIsVisible())
                    {
                        Debug.Log("is false");
                    }
                }
            }*/
            Canvas.GetComponentInChildren<UnityEngine.UI.Text>().text = s;
            if (activated)
            {
                if (VisibleWindowsChanged)
                {
                    // user_gui.GUI_Debug("Changed Window Visibility");
                    VisibleWindowsChanged = false;
                }
            }
            yield return null;
        }
    }
    public void OnActivated()
    {
        user_gui.ReplaceMotorInfo(Info, true);

        GetComponent<MeshRenderer>().material = user_gui.SelectedRed;
        Canvas.GetComponent<UnityEngine.Canvas>().sortingOrder = 1;
        Canvas.GetComponentInChildren<UnityEngine.UI.Image>().color = new Color(Canvas.GetComponentInChildren<UnityEngine.UI.Image>().color.r, Canvas.GetComponentInChildren<UnityEngine.UI.Image>().color.g, Canvas.GetComponentInChildren<UnityEngine.UI.Image>().color.b, 1);
        user_gui.GUI_Debug("is activated");
        activated = true;
    }
    public void OnDeactivated()
    {
        
        if (IsConveyorMotor)
        {
            user_gui.MotorWindow.SetActive(false);
        }
        if (IsPortalAxes)
        {
            user_gui.PortalWindow.SetActive(false);
        }

        user_gui.LowInfoField.SetActive(false);
        GetComponent<MeshRenderer>().material = user_gui.SelectedWhite;
        Canvas.GetComponentInChildren<UnityEngine.UI.Image>().color = new Color(Canvas.GetComponentInChildren<UnityEngine.UI.Image>().color.r, Canvas.GetComponentInChildren<UnityEngine.UI.Image>().color.g, Canvas.GetComponentInChildren<UnityEngine.UI.Image>().color.b, 0.5f);
        Canvas.GetComponent<UnityEngine.Canvas>().sortingOrder = 0;
        activated = false;
    }
}
