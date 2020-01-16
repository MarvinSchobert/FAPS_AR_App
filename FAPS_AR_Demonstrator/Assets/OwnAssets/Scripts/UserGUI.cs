using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using GoogleARCore.Examples.Common;
public class MyObject
{
    public int value;
    public float value2;
    public string value3;
}
#pragma warning disable 618
public class UserGUI : MonoBehaviour
#pragma warning restore 618
{

    [InspectorName("GUI Components")]
    public GameObject MotorInfoText;
    public GameObject SceneControl;
    public GameObject HelperText;
    public GameObject WarningSign;
    public GameObject MessageItemTemplateObject;
    
   

    public Material SelectedWhite;
    public Material SelectedRed;

    public GameObject LowInfoField;

    public GameObject MotorWindow;
    public GameObject PortalWindow;

    [InspectorName("Others")]
    public ObjectInteractor Interactor;
    public DetectedPlaneGenerator DetectedPlaneGen;

    public int movementMode = 0; //0: Machine, 1: SelektiertesObjekt
    public LayerMask TransparentLayer;
    private List<GameObject> SelectableMachines;
    
    public struct HelperMsgs
    {
        public string MsgInfo;
        public string MsgName;

    }
    public struct InfoVisible
    {
        public string InfoType;
        public bool IsVisible;
        public void SetIsVisible(bool mode)
        {
            IsVisible = mode;
        }
        public bool GetIsVisible()
        {
            return IsVisible;
        }
        public string GetInfoType()
        {
            return InfoType;
        }
    }
    public struct MotorInfo
    {
        /* 
         * hier werden die Daten der Cloud reingespeichert
         */
        public List<InfoVisible> VisibleWindows;

        // Das muss letztlich übers Netzwerk aktualisiert werden
        public List<MessageItem> MessageItems;
       

        public string MotorName;

        public bool IsMotor;
        public bool IsPortalAxes;

        // Filtered Text with parameters
        public string Parameter;

        // Filtered Text with parameters for small Canvas in Scene
        public List<string> Canvas_Parameter;
    }

    public MotorInfo SelectedMotorInfo = new MotorInfo();
    

    List<HelperMsgs> HelperMsgsList = new List<HelperMsgs>();

    int SafetyMode = 0; // 0: Maschine bestimmt ob an/ aus, 1: immer an, 2: immer aus
    public bool AllowEnter = false; // 0: Maschine ist nicht in Bewegung, es darf also betreten werden, 1: Maschine ist in Bewegung, Zutritt nicht erlaubt
    

    // Start is called before the first frame update
    void Start()
    {
        // Helper Messages Definieren
        HelperMsgs h1 = new HelperMsgs();
        h1.MsgInfo = "First Scan the floor and click on it in order to make a floor reference.";
        h1.MsgName = "Floor reference";
        HelperMsgsList.Add(h1);
        HelperMsgs h2 = new HelperMsgs();
        h2.MsgInfo = "Now Scan your reference image in order to auto-create the CAD machine";
        h2.MsgName = "Image reference";
        HelperMsgsList.Add(h2);
        HelperMsgs h3 = new HelperMsgs();
        h3.MsgInfo = "Select your machine via the FAPS menu in order to:\nRotate and translate the object by 2-/3 finger swiping\nGet machine data by clicking on e.g. the motors\nCheck out the FAPS menu for further possibilities.";
        h3.MsgName = "Interact reference";
        HelperMsgsList.Add(h3);

        SelectedMotorInfo.VisibleWindows = new List<InfoVisible>();
        SelectedMotorInfo.Canvas_Parameter = new List<string>();
        SelectedMotorInfo.MessageItems = new List<MessageItem>();
        Set_HelperMsg(h1.MsgInfo);

        SelectableMachines = new List<GameObject>();

        
        UnityEngine.UI.Button[] b = LowInfoField.GetComponentsInChildren<UnityEngine.UI.Button>();
    }
    

    

    
    public IEnumerator ToggleWall()
    {
        if (Interactor.SelectedMachine.GetComponent<MachineScript>() == null)
        {

        }
        else
        {
            int mode = SafetyMode;

            // Maschine kontrolliert
            if (mode == 0)
            {
                if (!AllowEnter)
                {
                    // Wand hochfahren wenn nicht Lifted
                    if (!Interactor.SelectedMachine.GetComponent<MachineScript>().GetComponent<MachineScript>().CheckHumanCollision.WallsLifted)
                    {
                        Interactor.SelectedMachine.GetComponent<MachineScript>().CheckHumanCollision.WallsLifted = true;

                        int steps = 30;
                        for (int i = 0; i < steps; i++)
                        {
                            foreach (GameObject g in Interactor.SelectedMachine.GetComponent<MachineScript>().Walls)
                            {
                                g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y + 0.2f / steps, g.transform.localScale.z);
                                g.transform.position = new Vector3(g.transform.position.x, (Interactor.SelectedMachine.GetComponent<MachineScript>().surfacePlaneHeight + g.GetComponent<MeshRenderer>().bounds.extents.y), g.transform.position.z);

                            }
                            yield return null;
                        }
                    }
                }
                else
                {
                    // Wand runterfahren wenn Lifted
                    if (Interactor.SelectedMachine.GetComponent<MachineScript>().CheckHumanCollision.WallsLifted)
                    {
                        Interactor.SelectedMachine.GetComponent<MachineScript>().CheckHumanCollision.WallsLifted = true;

                        int steps = 30;
                        for (int i = 0; i < steps; i++)
                        {
                            foreach (GameObject g in Interactor.SelectedMachine.GetComponent<MachineScript>().Walls)
                            {
                                g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y - 0.2f / steps, g.transform.localScale.z);
                                g.transform.position = new Vector3(g.transform.position.x, (Interactor.SelectedMachine.GetComponent<MachineScript>().surfacePlaneHeight + g.GetComponent<MeshRenderer>().bounds.extents.y), g.transform.position.z);
                            }
                            yield return null;
                        }

                    }
                }
            }
            // Immer an
            else if (mode == 1)
            {
                // Wand hochfahren wenn nicht Lifted
                if (!Interactor.SelectedMachine.GetComponent<MachineScript>().CheckHumanCollision.WallsLifted)
                {
                    Interactor.SelectedMachine.GetComponent<MachineScript>().CheckHumanCollision.WallsLifted = true;

                    int steps = 30;
                    for (int i = 0; i < steps; i++)
                    {
                        foreach (GameObject g in Interactor.SelectedMachine.GetComponent<MachineScript>().Walls)
                        {
                            g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y + 0.2f / steps, g.transform.localScale.z);
                            g.transform.position = new Vector3(g.transform.position.x, (Interactor.SelectedMachine.GetComponent<MachineScript>().surfacePlaneHeight + g.GetComponent<MeshRenderer>().bounds.extents.y), g.transform.position.z);
                        }
                        yield return null;
                    }

                }
            }
            // Immer aus
            else if (mode == 2)
            {
                // Wand runterfahren wenn Lifted
                if (Interactor.SelectedMachine.GetComponent<MachineScript>().CheckHumanCollision.WallsLifted)
                {
                    Interactor.SelectedMachine.GetComponent<MachineScript>().CheckHumanCollision.WallsLifted = false;

                    int steps = 30;
                    for (int i = 0; i < steps; i++)
                    {
                        foreach (GameObject g in Interactor.SelectedMachine.GetComponent<MachineScript>().Walls)
                        {
                            g.transform.localScale = new Vector3(g.transform.localScale.x, g.transform.localScale.y - 0.2f / steps, g.transform.localScale.z);
                            g.transform.position = new Vector3(g.transform.position.x, (Interactor.SelectedMachine.GetComponent<MachineScript>().surfacePlaneHeight + g.GetComponent<MeshRenderer>().bounds.extents.y), g.transform.position.z);
                        }
                        yield return null;
                    }

                }
            }

            yield return null;
        }
    }
    
    

    // Callbackfunktion, wenn ein Motor selektiert ist und ein Menüpunkt angeklickt wurde
    // --> GUI Sprechblase personalisieren
    public void SetMotorWindowButtonCallback(int buttonID)
    {
        // Falls die Linearachse selektiert wurde
        if (SelectedMotorInfo.IsPortalAxes)
        {
            // Interactor.SelectedObject.GetComponent<MotorScript>().VisibleWindows[buttonID] = !Interactor.SelectedObject.GetComponent<MotorScript>().VisibleWindows[buttonID];
            Interactor.SelectedObject.GetComponent<MotorScript>().VisibleWindowsChanged = true;
            bool finished = false;
            foreach (Transform t in PortalWindow.transform.GetComponentInChildren<Transform>())
            {
                if (t.parent == PortalWindow.transform && t.GetSiblingIndex() == buttonID)
                {
                    string s = t.GetComponentInChildren<UnityEngine.UI.Text>().text;
                    for (int inf = 0; inf < SelectedMotorInfo.VisibleWindows.Count; inf++)
                    {
                        if (s.Contains(SelectedMotorInfo.VisibleWindows[inf].InfoType))
                        {
                            if (SelectedMotorInfo.VisibleWindows[inf].IsVisible)
                            {
                                Debug.Log("Setting False");
                                SelectedMotorInfo.VisibleWindows[inf].SetIsVisible(false);
                                Interactor.SelectedObject.GetComponent<MotorScript>().Info.VisibleWindows[inf].SetIsVisible(false);
                                t.GetComponentInChildren<UnityEngine.UI.Text>().text = s.Replace("Show", "Hide");
                            }
                            else
                            {
                                Debug.Log("Setting true");
                                SelectedMotorInfo.VisibleWindows[inf].SetIsVisible(true);
                                Interactor.SelectedObject.GetComponent<MotorScript>().Info.VisibleWindows[inf].SetIsVisible(true);
                                t.GetComponentInChildren<UnityEngine.UI.Text>().text = s.Replace("Hide", "Show");
                            }
                            finished = true;
                            break;
                        }
                    }
                }
                if (finished) { break; }
            }

        }
        // Falls ein Conveyor-Motor selektiert wurde
        else if (SelectedMotorInfo.IsMotor)
        {
            Interactor.SelectedObject.GetComponent<MotorScript>().VisibleWindowsChanged = true;
            bool finished = false;
            foreach (Transform t in MotorWindow.transform.GetComponentInChildren<Transform>())
            {
                if (t.parent == MotorWindow.transform && t.GetSiblingIndex() == buttonID)
                {
                    string s = t.GetComponentInChildren<UnityEngine.UI.Text>().text;
                    for (int inf = 0; inf < SelectedMotorInfo.VisibleWindows.Count; inf++)
                    {
                        if (s.Contains(SelectedMotorInfo.VisibleWindows[inf].InfoType))
                        {
                            if (SelectedMotorInfo.VisibleWindows[inf].IsVisible)
                            {
                                InfoVisible iv = Interactor.SelectedObject.GetComponent<MotorScript>().Info.VisibleWindows[inf];
                                iv.IsVisible = false;
                                SelectedMotorInfo.VisibleWindows[inf] = iv;
                                Interactor.SelectedObject.GetComponent<MotorScript>().Info.VisibleWindows[inf] = iv;
                                Debug.Log("Setting now is: " + Interactor.SelectedObject.GetComponent<MotorScript>().Info.VisibleWindows[inf].IsVisible);
                                t.GetComponentInChildren<UnityEngine.UI.Text>().text = s.Replace("Hide", "Show");
                            }
                            else
                            {
                                InfoVisible iv = Interactor.SelectedObject.GetComponent<MotorScript>().Info.VisibleWindows[inf];
                                iv.IsVisible = true;
                                SelectedMotorInfo.VisibleWindows[inf] = iv;
                                Interactor.SelectedObject.GetComponent<MotorScript>().Info.VisibleWindows[inf] = iv;
                                Debug.Log("Setting now is: " + Interactor.SelectedObject.GetComponent<MotorScript>().Info.VisibleWindows[inf].IsVisible);
                                t.GetComponentInChildren<UnityEngine.UI.Text>().text = s.Replace("Show", "Hide");
                            }
                            finished = true;
                            break;
                        }
                    }
                }
                if (finished) { break; }
            }
        }


    }

    // Methode, um alle Informationen aus der Cloud zu Synchronisieren mit der GUI und Daten in das GUI Fenster zu schreiben
    public void ReplaceMotorInfo(MotorInfo info, bool change)
    {
        SelectedMotorInfo.IsMotor = info.IsMotor;
        SelectedMotorInfo.IsPortalAxes = info.IsPortalAxes;
        SelectedMotorInfo.Parameter = info.Parameter;
        SelectedMotorInfo.MotorName = info.MotorName;
        SelectedMotorInfo.VisibleWindows = info.VisibleWindows;
        SelectedMotorInfo.MessageItems = info.MessageItems;
        LowInfoField.SetActive(true);
        if (change)
        {
            // Motor selektiert
            if (SelectedMotorInfo.IsMotor)
            {
                MotorInfoText.GetComponent<UnityEngine.UI.Text>().text = SelectedMotorInfo.MotorName + "\n" + SelectedMotorInfo.Parameter;
                MotorWindow.SetActive(true);


                foreach (Transform t in MotorWindow.transform.GetComponentInChildren<Transform>())
                {
                    if (t.parent == MotorWindow.transform)
                    {
                        int idx = t.GetSiblingIndex();
                        if (idx == 0) // Acceleration
                        {
                            for (int x = 0; x < SelectedMotorInfo.VisibleWindows.Count; x++)
                            {
                                if (SelectedMotorInfo.VisibleWindows[x].GetInfoType() == "Acceleration")
                                {
                                    if (SelectedMotorInfo.VisibleWindows[x].GetIsVisible())
                                    {
                                        t.GetComponentInChildren<UnityEngine.UI.Text>().text = "Hide Acceleration";
                                    }
                                    else
                                    {
                                        t.GetComponentInChildren<UnityEngine.UI.Text>().text = "Show Acceleration";
                                    }
                                    break;
                                }
                            }
                        }
                        else if (idx == 1) // Velocity
                        {
                            for (int x = 0; x < SelectedMotorInfo.VisibleWindows.Count; x++)
                            {
                                if (SelectedMotorInfo.VisibleWindows[x].GetInfoType() == "Velocity")
                                {
                                    if (SelectedMotorInfo.VisibleWindows[x].GetIsVisible())
                                    {
                                        t.GetComponentInChildren<UnityEngine.UI.Text>().text = "Hide Velocity";
                                    }
                                    else
                                    {
                                        t.GetComponentInChildren<UnityEngine.UI.Text>().text = "Show Velocity";
                                    }
                                    break;
                                }
                            }
                        }
                        else if (idx == 2) // Power
                        {
                            for (int x = 0; x < SelectedMotorInfo.VisibleWindows.Count; x++)
                            {
                                if (SelectedMotorInfo.VisibleWindows[x].GetInfoType() == "Power")
                                {
                                    if (SelectedMotorInfo.VisibleWindows[x].GetIsVisible())
                                    {
                                        t.GetComponentInChildren<UnityEngine.UI.Text>().text = "Hide Power";
                                    }
                                    else
                                    {
                                        t.GetComponentInChildren<UnityEngine.UI.Text>().text = "Show Power";
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                PortalWindow.SetActive(false);
            }

            // Linearachse selektiert
            else if (SelectedMotorInfo.IsPortalAxes)
            {
                MotorInfoText.GetComponent<UnityEngine.UI.Text>().text = SelectedMotorInfo.MotorName + "\n" + SelectedMotorInfo.Parameter;
                PortalWindow.SetActive(true);
                //Die Buttons richtig mit Hide/ Show beschriften
                foreach (Transform t in PortalWindow.transform.GetComponentInChildren<Transform>())
                {
                    if (t.parent == PortalWindow.transform)
                    {

                        int idx = t.GetSiblingIndex();
                        if (idx == 0) // Position
                        {
                            for (int x = 0; x < SelectedMotorInfo.VisibleWindows.Count; x++)
                            {
                                if (SelectedMotorInfo.VisibleWindows[x].InfoType == "Position")
                                {
                                    if (SelectedMotorInfo.VisibleWindows[x].IsVisible)
                                    {
                                        t.GetComponentInChildren<UnityEngine.UI.Text>().text = "Hide Position";
                                    }
                                    else
                                    {
                                        t.GetComponentInChildren<UnityEngine.UI.Text>().text = "Show Position";
                                    }
                                    break;
                                }
                            }
                        }
                        else if (idx == 1) // Speed
                        {
                            for (int x = 0; x < SelectedMotorInfo.VisibleWindows.Count; x++)
                            {
                                if (SelectedMotorInfo.VisibleWindows[x].InfoType == "Speed")
                                {
                                    if (SelectedMotorInfo.VisibleWindows[x].IsVisible)
                                    {
                                        t.GetComponentInChildren<UnityEngine.UI.Text>().text = "Hide Speed";
                                    }
                                    else
                                    {
                                        t.GetComponentInChildren<UnityEngine.UI.Text>().text = "Show Speed";
                                    }
                                    break;
                                }
                            }
                        }
                        else if (idx == 2) // Force
                        {
                            for (int x = 0; x < SelectedMotorInfo.VisibleWindows.Count; x++)
                            {
                                if (SelectedMotorInfo.VisibleWindows[x].InfoType == "Force")
                                {
                                    if (SelectedMotorInfo.VisibleWindows[x].IsVisible)
                                    {
                                        t.GetComponentInChildren<UnityEngine.UI.Text>().text = "Hide Force";
                                    }
                                    else
                                    {
                                        t.GetComponentInChildren<UnityEngine.UI.Text>().text = "Show Force";
                                    }
                                    break;
                                }
                            }
                        }
                        else if (idx == 3) // Current
                        {
                            for (int x = 0; x < SelectedMotorInfo.VisibleWindows.Count; x++)
                            {
                                if (SelectedMotorInfo.VisibleWindows[x].InfoType == "Current")
                                {
                                    if (SelectedMotorInfo.VisibleWindows[x].IsVisible)
                                    {
                                        t.GetComponentInChildren<UnityEngine.UI.Text>().text = "Hide Current";
                                    }
                                    else
                                    {
                                        t.GetComponentInChildren<UnityEngine.UI.Text>().text = "Show Current";
                                    }
                                    break;
                                }
                            }
                        }
                        else if (idx == 4) // Power
                        {
                            for (int x = 0; x < SelectedMotorInfo.VisibleWindows.Count; x++)
                            {
                                if (SelectedMotorInfo.VisibleWindows[x].InfoType == "Power")
                                {
                                    if (SelectedMotorInfo.VisibleWindows[x].IsVisible)
                                    {
                                        t.GetComponentInChildren<UnityEngine.UI.Text>().text = "Hide Power";
                                    }
                                    else
                                    {
                                        t.GetComponentInChildren<UnityEngine.UI.Text>().text = "Show Power";
                                    }
                                    break;
                                }
                            }
                        }

                    }
                }

                MotorWindow.SetActive(false);

            }           
        }
        else
        {
            // Neuer Motor angeklickt
        }
    }
    
    void DeselectMachine()
    {
        LowInfoField.SetActive(false);
        Interactor.SetSelectedMachine(null);
        //GUI_Debug("Deselected machine");
    }
    
  
    public void AddMachineSelectable(GameObject selectable)
    {        
        SelectableMachines.Add(selectable);
        Interactor.SetSelectedMachine(selectable);
        DetectedPlaneGen.ToggleMeshVisibility(false);
}

    
    public void Set_HelperMsg(string msg)
    {
        HelperText.GetComponent<UnityEngine.UI.Text>().text = msg;
    }

    public void GUI_Debug (string text)
    {
        /*
         * DISABLED FOR VERSION 1.0
         * 
        displayTextDebug.Add(text);
        int removeItems = displayTextDebug.Count - amountDebugMsgs;
        if (removeItems > 0)
        {
            displayTextDebug.RemoveRange(0, removeItems);
        }
        DebugText.GetComponentInChildren<UnityEngine.UI.Text>().text = "";
        foreach (string s in displayTextDebug)
        {
            DebugText.GetComponentInChildren<UnityEngine.UI.Text>().text += s + "\n";
        }
        */
    }
    // Update is called once per frame
    void Update()
    {
       
        // Make sure Canvas is always last element that's beeing rendered (--> GUI overlay!)
        if (transform.GetSiblingIndex() < transform.hierarchyCount)
        {
            transform.SetAsLastSibling();
        }
    }    
}
