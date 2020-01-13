using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
    public GameObject DebugText;
    public GameObject SceneControl;
    public GameObject DetectedPlaneVis;
    public GameObject PointCloudVis;
    public GameObject HelperText;
    public GameObject WarningSign;
    public GameObject ChatWindowContent;
    public GameObject MessageItemTemplateObject;
    public UnityEngine.UI.InputField InputField;

    public GameObject OptionsMenu;
    public UnityEngine.UI.Button ToggleOptionsMenuButton;
    public UnityEngine.UI.Dropdown SelectedMachineDropdown;
    public UnityEngine.UI.Button ToggleCADButton;
    public UnityEngine.UI.Button TogglePlaneMeshButton;
    public UnityEngine.UI.Button ChangeSensitivityButton;
    public UnityEngine.UI.Button ChangeLockMovementButton;
    public UnityEngine.UI.Button ChangeMovementModeButton;
    public UnityEngine.UI.Button ToggleSecurityModeButton;
    public GameObject CloudAnchorComponents;

    
    public static bool EnableCloud = false;
   

    public Material SelectedWhite;
    public Material SelectedRed;

    public GameObject LowInfoField;

    public GameObject MotorWindow;
    public GameObject PortalWindow;
    private List<GameObject> MotorWindowButtons;

    [InspectorName("Others")]
    public ObjectInteractor Interactor;

    private bool optionMenuEnabled = false;
    private bool cadModelEnabled = true;
    private bool planeMeshEnabled = true;
    private int moveAxesLockMode = 0; //0: Frei, 1: LockZ, 2: LockX, 3: LockXZ
    public int movementMode = 0; //0: Machine, 1: SelektiertesObjekt
    public LayerMask TransparentLayer;
    private List<GameObject> SelectableMachines;
    private string MessageInputString;
    
    public int amountDebugMsgs;
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
            Debug.Log("changed mode");
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
    List<string> displayTextDebug;

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
        displayTextDebug = new List<string>();
        ChangeSensitivityButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "\n \n \nSense: 3";


        MotorWindowButtons = new List<GameObject>();
        UnityEngine.UI.Button[] b = LowInfoField.GetComponentsInChildren<UnityEngine.UI.Button>();
        foreach (UnityEngine.UI.Button b1 in b)
        {
            MotorWindowButtons.Add(b1.gameObject);
        }
    }
    public void ButtonClickCallback(int ID)
    {
        switch (ID)
        {
            case 1:
                // toggle OptionsMenu Visibility
                ToggleOptionsMenuVisibility();
                break;
            case 2:
                // toggle CAD Model Visibility
                ToggleCADModelVisibility();
                break;
            case 3:
                // toggle DetectedPlane Mesh Visibility
                TogglePlaneMeshVisibility();
                break;
            case 4:
                // Reset Scene
                ResetScene();
                break;
            case 5:
                // Change movement Sensibility
                SetMovementSensibility();
                break;
            case 6:
                // Toggle Debug Window Visibility
                ToggleDebugWindowVisibility();
                break;
            case 7:
                // Deselect Machine
                DeselectMachine();
                break;
            case 8:
                // Set next Tipp
                SetNextTipp();
                GUI_Debug("Set new ID BUTTON!");
                break;
            case 9:
                // Lock ObjectManipulation to certain axes
                LockObjectInteractionAxes();
                break;
            case 10:
                // Save ObjectManipulation to PlayerPrefs
                SaveInPlayerPrefs();
                break;
            case 11:
                // Load ObjectManipulation from PlayerPrefs
                LoadFromPlayerPrefs();
                break;
            case 12:
                // Toggle MoveMachine/ MoveObject
                SetMovementMode();
                break;
            case 13:
                // Testzwecke 
                if (Interactor.SelectedMachine.GetComponent<MachineScript>() != null)
                {
                    Interactor.SelectedMachine.GetComponent<MachineScript>().MachineReady = !Interactor.SelectedMachine.GetComponent<MachineScript>().MachineReady;
                }
                break;
            case 14:
                // Add MessageItem to Motor
                // Invoking, that input string can be edited first
                Invoke ("SetNewMessageItem", 0.2f);
                break;
            case 15:
                // AR Cloud Menu enable
                if (!EnableCloud)
                {

                    ////////////////// (UN)COMMENT FOR CLOUD USE ////////////////

                    EnableCloud = !EnableCloud;
                    CloudAnchorComponents.SetActive(true);
                }
                break;
        }

    }

    /// <summary>
    /// Button functions
    /// </summary>
    
    void SetNewMessageItem()
    {
        if (Interactor.SelectedObject.GetComponent<MotorScript>() != null)
        {
            GUI_Debug("Added new Chat Message");
            MessageItem item = MessageItem.CreateInstance("MessageItem") as MessageItem;
            item.AddText(MessageInputString, UnityEngine.SystemInfo.deviceName);
            Interactor.SelectedObject.GetComponent<MotorScript>().Info.MessageItems.Add(item);

            // Add to current chat display this message
            GameObject go = GameObject.Instantiate(MessageItemTemplateObject, ChatWindowContent.transform);
            go.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = item.MessageText;
            go.transform.Find("SignatureText").GetComponent<UnityEngine.UI.Text>().text = item.SignatureText;

            // Send Message via Network
            GameObject g = GameObject.Find("LocalPlayer");
            if (g != null)
            {
                string[] param = new string[] { MessageInputString, UnityEngine.SystemInfo.deviceName , Interactor.SelectedObject.transform.parent.name};
                g.SendMessage("SendMessageViaNetwork", param, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private float SafetyButtonPressTime = 0;
    public void SetSafetyMode(int mode)
    {
        // Mode = 0: down, mode = 1: up
        if (mode == 0)
        {
            SafetyButtonPressTime = Time.realtimeSinceStartup;
        }
        else if (mode == 1)
        {
            float timeDelta = Time.realtimeSinceStartup - SafetyButtonPressTime;

            if (Interactor.SelectedMachine != null)
            {
                // kurz: Maschinenkontrolle aus
                if (timeDelta < 1.0f)
                {
                    // immer an   
                    if (SafetyMode == 0 || SafetyMode == 2)
                    {
                        SafetyMode = 1;
                        Interactor.SelectedMachine.GetComponent<MachineScript>().CheckHumanCollision.ActivateDetection = true;
                        GUI_Debug("Detection always on");
                        StartCoroutine(ToggleWall());

                    }
                    // immer aus
                    else if (SafetyMode == 1)
                    {
                        SafetyMode = 2;
                        Interactor.SelectedMachine.GetComponent<MachineScript>().CheckHumanCollision.ActivateDetection = false;
                        WarningSign.SetActive(false);
                        Interactor.SelectedMachine.GetComponent<MachineScript>().CheckHumanCollision.HumanIsInside = false;
                        GUI_Debug("Detection always off");
                        StartCoroutine(ToggleWall());
                    }
                }
                // lang: Maschinenkontrolle an
                else
                {
                    if (true)
                    {
                        SafetyMode = 0;
                        Interactor.SelectedMachine.GetComponent<MachineScript>().CheckHumanCollision.ActivateDetection = true;
                        GUI_Debug("Detection dependent on Machine Signal");
                        StartCoroutine(ToggleWall());

                    }
                }
            }
        }
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

    void ToggleOptionsMenuVisibility()
    {
        //GUI_Debug("Toggle Options Menu Visibility");
        if (optionMenuEnabled)
        {
            optionMenuEnabled = false;
            OptionsMenu.SetActive(false);
        } else
        {
            optionMenuEnabled = true;
            OptionsMenu.SetActive(true);
        }
    }
    void ToggleCADModelVisibility()
    {
        //GUI_Debug("Toggle CAD model visibility");
        if (cadModelEnabled && SelectedMachineDropdown.value != 0)
        {
            cadModelEnabled = false;
            Interactor.allowMovement = false;
            MeshRenderer[] rend = SelectableMachines[SelectedMachineDropdown.value - 1].GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer r in rend)
            {
                if (r.gameObject.name != "Fence")
                    r.enabled = false;
            }
        }
        else if (SelectedMachineDropdown.value != 0)
        {
            cadModelEnabled = true;
            Interactor.allowMovement = true;
            MeshRenderer[] rend = SelectableMachines[SelectedMachineDropdown.value - 1].GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer r in rend)
            {
                r.enabled = true;
            }
        }
    }

    private void SaveInPlayerPrefs()
    {
        if (Interactor.SelectedMachine != null)
        {
            Interactor.SelectedMachine.GetComponent<MachineScript>().SaveTransformManipulationValues();
        }
    }

    private void LoadFromPlayerPrefs()
    {
        if (Interactor.SelectedMachine != null)
        {
            Interactor.SelectedMachine.GetComponent<MachineScript>().ApplyOffsetFromPlayerPrefs();
        }
    }

    private void LockObjectInteractionAxes()
    {
        moveAxesLockMode++;
        if (moveAxesLockMode == 4)
        {
            moveAxesLockMode = 0;
        }

        switch (moveAxesLockMode)
        {
            case 0:
                ChangeLockMovementButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "\n \n \nLock: /";
                Interactor.LockX_Axes = false;
                Interactor.LockZ_Axes = false;
                break;
            case 1:
                ChangeLockMovementButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "\n \n \nLock: Z";
                Interactor.LockX_Axes = false;
                Interactor.LockZ_Axes = true;
                break;
            case 2:
                ChangeLockMovementButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "\n \n \nLock: X";
                Interactor.LockX_Axes = true;
                Interactor.LockZ_Axes = false;
                break;
            case 3:
                ChangeLockMovementButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "\n \n \nLock: X&Z";
                Interactor.LockX_Axes = true;
                Interactor.LockZ_Axes = true;
                break;

        }

    }

    // Callbackfunktion für Movement Mode
    private void SetMovementMode()
    {
        if (movementMode == 0)
        {
            movementMode = 1;
            ChangeMovementModeButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "\n \n \nMotor";
        }
        else if (movementMode == 1)
        {
            movementMode = 0;
            ChangeMovementModeButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "\n \n \nMachine";
        }
    }

    // Callbackfunktion für Movement Sensibility
    private void SetMovementSensibility()
    {
        //GUI_Debug("Change Movemement");
        if (Interactor.MovementSensibility < 5)
        {
            Interactor.MovementSensibility++;
        } else
        {
            Interactor.MovementSensibility = 1;
        }
        ChangeSensitivityButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "\n \n \nSense: " + Interactor.MovementSensibility;
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
            int counter = 0;
            foreach (Transform item in ChatWindowContent.transform.GetComponentInChildren<Transform>())
            {
                if (item.gameObject.name.Contains("Message"))
                {
                    Destroy(item.gameObject);
                }
            }

            // Message Items anzeigen
            foreach (MessageItem item in SelectedMotorInfo.MessageItems)
            {
                counter++;
                GameObject go = GameObject.Instantiate(MessageItemTemplateObject, ChatWindowContent.transform);
                go.transform.Find("Text").GetComponent<UnityEngine.UI.Text>().text = item.MessageText;
                go.transform.Find("SignatureText").GetComponent<UnityEngine.UI.Text>().text = item.SignatureText;
            }
           // GUI_Debug("Found " + counter + " Items");
        }
        else
        {
            // Neuer Motor angeklickt
        }
    }


    void TogglePlaneMeshVisibility()
    {
        //GUI_Debug("Toggles Plane Mesh Visibility");
        if (planeMeshEnabled)
        {
            PointCloudVis.SendMessage("EnableMesh", false, SendMessageOptions.DontRequireReceiver);
            DetectedPlaneVis.SendMessage("EnableMesh", false, SendMessageOptions.DontRequireReceiver);
            planeMeshEnabled = false;
        }else
        {
            PointCloudVis.SendMessage("EnableMesh", true, SendMessageOptions.DontRequireReceiver);
            DetectedPlaneVis.SendMessage("EnableMesh", true, SendMessageOptions.DontRequireReceiver);
            planeMeshEnabled = true;
        }
    }

    public void SetMachineSelectable()
    {
        SelectedMachineDropdown.RefreshShownValue();
        //GUI_Debug("Changed Value");
        if (SelectedMachineDropdown.value != 0)
        {
            GUI_Debug("Set Machine: " + SelectedMachineDropdown.value);
            Interactor.SetSelectedMachine(SelectableMachines[SelectedMachineDropdown.value - 1]);
            
        }else
        {
            Interactor.SetSelectedMachine(null);
        }
    }

    void ToggleDebugWindowVisibility()
    {
        //GUI_Debug("Toggles Debug Window");
        if (DebugText.activeInHierarchy)
        {
            DebugText.SetActive(false);
        }else
        {
            DebugText.SetActive(true);
        }
    }

    void DeselectMachine()
    {
        LowInfoField.SetActive(false);
        Interactor.SetSelectedMachine(null);
        //GUI_Debug("Deselected machine");
    }

    // Add more Machines to Scene
    void ResetScene()
    {
        /*if (SelectedMachineDropdown.value != 0)
        {
            SceneControl.SendMessage("ResetScene", SendMessageOptions.DontRequireReceiver);
            //Destroy(SelectableMachines[SelectedMachineDropdown.value - 1]);
        }*/
        SceneControl.SendMessage("ResetScene", SendMessageOptions.DontRequireReceiver);

    }
  
    public void AddMachineSelectable(GameObject selectable)
    {        
        SelectableMachines.Add(selectable);

        UnityEngine.UI.Dropdown.OptionData optionData = new UnityEngine.UI.Dropdown.OptionData
        {
            text = selectable.name
        };

        SelectedMachineDropdown.options.Add(optionData);
        SelectedMachineDropdown.RefreshShownValue();
        GUI_Debug("Adds: " + selectable.name);

    }

    void SetNextTipp()
    {
        int rnd = Random.Range(2, 9);
        switch (rnd)
        {
            case 2:
                Set_HelperMsg(HelperMsgsList[2].MsgInfo);
                break;
            case 3:
                Set_HelperMsg("The visibility of the machine's CAD Model can be toggled via the button CAD in the FAPS menu.");
                break;
            case 4:
                Set_HelperMsg("The sensitivity of the machine's transform manipulation can be edited via the Sense button in the FAPS menu.");
                break;
            case 5:
                Set_HelperMsg("Manipulating a machine is only possible, if the machine is beeing selected.");
                break;
            case 6:
                Set_HelperMsg("The surface plane visualization can be switched off in the FAPS menu with the Surface button.");
                break;
            case 7:
                Set_HelperMsg("Transform manipulation is only possible in the X-Axes. In order to move in the Y-Direction, walk around the physical machine.");
                break;
            case 8:
                Set_HelperMsg("Once the machines CAD model is adjusted, you can turn it off and still access the e.g. motor's data by clicking on the physical image of it.");
                break;
        }
    }

    public void Set_HelperMsg(string msg)
    {
        // HelperText.GetComponent<UnityEngine.UI.Text>().text = msg;
    }

    public void GUI_Debug (string text)
    {
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
        

    }
    public UnityEngine.UI.Text t;

    // Update is called once per frame
    void Update()
    {
       
        // Make sure Canvas is always last element that's beeing rendered (--> GUI overlay!)
        if (transform.GetSiblingIndex() < transform.hierarchyCount)
        {
            transform.SetAsLastSibling();
        }
    }

    public void OnMessageInputEnd()
    {
        MessageInputString = InputField.text;
        InputField.text = "";
    }
}
