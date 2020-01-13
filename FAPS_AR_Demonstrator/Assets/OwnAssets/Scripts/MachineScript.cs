using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MachineScript : MonoBehaviour
{
    // Array to store all child meshRenderers
    private MeshRenderer[] meshRenderers;
    public UserGUI user_gui;
    public float surfacePlaneHeight = 0;
    private bool OffsetIsApplied = false;
    public GameObject ReferencePoint;
    public GoogleARCore.Trackable trackable;
    public List<GameObject> disable;
    public List<GameObject> Walls;
    public GoogleARCore.AugmentedImage TrackableImage;
    public Transform anchor;
    public MotorScript[] motors;
    public GoogleARCore.DetectedPlane Surface;
    public CheckHumanCollision CheckHumanCollision;

    // JSON Infos
    public JSONObject CloudInputInformation;
    public bool HasChangedMotorInputValues;
    public bool HasChangedPortalInputValues;
    public float LastMotorInfoChange;
    public float LastPortalInfoChange;

    // Für Testzwecke!!!!
    public bool MachineReady;

    // Initital Transform

    GameObject Init_Transform_GameObject;
    Transform Init_Transform;

    // New x & z Offset and rotation. 
    public float New_Offset_PosX = 0;
    public float New_Offset_PosZ = 0;
    public float New_Offset_RotY = 0;

    // "Originaloffset" zur Imageposition
    public Vector3 ToImageOffset;
    public float ToImageRotationOffset;
    bool LostImageTracking = false;
    
    public void Initialize()
    {
        CheckHumanCollision = GetComponentInChildren<CheckHumanCollision>();
        user_gui = GameObject.Find("PostPlacementInteractor").GetComponent<ObjectInteractor>().userGUI;
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        motors = GetComponentsInChildren<MotorScript>();
        
        // find lowest and highest point in order to determine y- Offset
        float lowest = float.PositiveInfinity;
        float highest = float.NegativeInfinity;
       
        foreach (MeshRenderer rend in meshRenderers)
        {
            if (rend.transform != null)
            {
                
                // check if lowest
                if (rend.bounds.min.y < lowest)
                {
                    lowest = rend.bounds.min.y;
                }
                // check if highest
                if (rend.transform.position.y + rend.bounds.extents.y > highest)
                {
                    highest = rend.transform.position.y + rend.bounds.extents.y;
                }
            }
            
        }
        user_gui.GUI_Debug("Translated y by: " + (transform.position.y - lowest));
        anchor = transform.parent;
        transform.position = new Vector3(transform.position.x, transform.position.y + (surfacePlaneHeight - lowest), transform.position.z);
        // GetComponentInChildren<CheckHumanCollision>().machineScript = this;

        Init_Transform_GameObject = new GameObject();
        Init_Transform_GameObject.transform.position = transform.position;
        Init_Transform_GameObject.transform.rotation = transform.rotation;
        Init_Transform = Init_Transform_GameObject.transform;

        InitAnchor = anchor.transform.position;

        
        c = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c.transform.position = anchor.transform.position;
        c.transform.rotation = anchor.rotation;
        c.transform.localScale *= 0.08f;
        Destroy(c.GetComponent<MeshRenderer>());
        Destroy(c.GetComponent<Collider>());
        transform.parent = c.transform;
        
        if (TrackableImage!=null) ToImageOffset = transform.position - TrackableImage.CenterPose.position;
        ToImageRotationOffset = 0;

        foreach (GameObject g in disable)
        {
            g.SetActive(false);
        }

        StartCoroutine(simulate());
        StartCoroutine(_Update());

        

    }

    GameObject c;

    public void ApplyOffsetFromPlayerPrefs()
    {
        if (!OffsetIsApplied && PlayerPrefs.HasKey(gameObject.name + "_settings"))
        {

            ////////////////////// NOCHMAL DAS MIT DER ROTATION ÜBERDENKEN! AUSRICHTUNG DES BILDES NUTZEN!!!

            user_gui.GUI_Debug("Used Offset Values from PlayerPrefs");
            

            float rotCamInit = PlayerPrefs.GetFloat(gameObject.name + "_rotationToCamFrameY");
            rotCamInit = rotCamInit - Vector3.SignedAngle(Vector3.forward, transform.forward, transform.up);

            user_gui.GUI_Debug("delta coordinate System: " + rotCamInit + ". Rotationdifference: "+ PlayerPrefs.GetFloat(gameObject.name + "_settings_Y_Rot"));




            //Vector3 OffsetTranslation = new Vector3(PlayerPrefs.GetFloat(gameObject.name + "_settings_X_Trans") * Mathf.Cos(rotCamInit) - PlayerPrefs.GetFloat(gameObject.name + "_settings_Z_Trans") * Mathf.Sin(rotCamInit), 0, PlayerPrefs.GetFloat(gameObject.name + "_settings_X_Trans") * Mathf.Sin(rotCamInit) + PlayerPrefs.GetFloat(gameObject.name + "_settings_Z_Trans") * Mathf.Cos(rotCamInit));

            Vector3 OffsetTranslation = new Vector3(PlayerPrefs.GetFloat(gameObject.name + "_settings_X_Trans"), 0, PlayerPrefs.GetFloat(gameObject.name + "_settings_Z_Trans"));

            user_gui.GUI_Debug("Offset " + OffsetTranslation.x + "/ " + OffsetTranslation.z);
            transform.Rotate(0, PlayerPrefs.GetFloat(gameObject.name + "_settings_Y_Rot"), 0);
            transform.position = new Vector3(Init_Transform.position.x, transform.position.y, Init_Transform.position.z) + Init_Transform.TransformDirection(Vector3.right) * OffsetTranslation.x + Init_Transform.TransformDirection(Vector3.forward) * OffsetTranslation.z;
            
            OffsetIsApplied = true;
            
        }
    }

    IEnumerator simulate()
    {
        yield return new WaitForSeconds(2.0f);
        while (true)
        {
            yield return new WaitForSeconds(2.5f);
            HasChangedMotorInputValues = true;
        }
    }
    Vector3 InitAnchor;
    // Update is called once per frame
    IEnumerator _Update()
    {
        while (true)
        {
            c.transform.position = new Vector3(anchor.transform.position.x, Surface.CenterPose.position.y, anchor.transform.position.z);


            // Wenn das ursprüngliche Bild wieder entdeckt wird, korrigiere Transform entsprechend
            if ((TrackableImage != null) && LostImageTracking && TrackableImage.TrackingMethod == GoogleARCore.AugmentedImageTrackingMethod.FullTracking)
            {
                transform.position = TrackableImage.CenterPose.position + ToImageOffset;

                if (Vector3.Angle(new Vector3(transform.position.x - TrackableImage.CenterPose.up.x * 3, transform.position.y, transform.position.z - TrackableImage.CenterPose.up.z * 3), Init_Transform.forward) > 10)
                {
                }
                else if (Vector3.Angle(new Vector3(transform.position.x - TrackableImage.CenterPose.up.x * 3, transform.position.y, transform.position.z - TrackableImage.CenterPose.up.z * 3), Init_Transform.forward) > 5)
                {
                }
                else
                {
                    transform.LookAt(new Vector3(transform.position.x - TrackableImage.CenterPose.up.x * 3, transform.position.y, transform.position.z - TrackableImage.CenterPose.up.z * 3));
                    // user_gui.GUI_Debug("AngleOffset is fine");
                }
                LostImageTracking = false;
            }
            else if ((TrackableImage != null) && !LostImageTracking && TrackableImage.TrackingMethod == GoogleARCore.AugmentedImageTrackingMethod.LastKnownPose)
            {
                LostImageTracking = true;
            }

            // Wenn der Anchor weg ist, setze einen neuen!


            /////////////////////////////////////////////////////// Update Info Values

            if (HasChangedMotorInputValues && Time.time - LastMotorInfoChange > 1f)
            {
                HasChangedMotorInputValues = false;
                LastMotorInfoChange = Time.time;
                StartCoroutine(EvaluateJsonObject(CloudInputInformation, 0));
            }
            if (HasChangedPortalInputValues && Time.time - LastPortalInfoChange > 1f)
            {
                HasChangedPortalInputValues = false;
                LastPortalInfoChange = Time.time;
                StartCoroutine(EvaluateJsonObject(CloudInputInformation, 1));
            }

            yield return null;
        }
    }

    public IEnumerator EvaluateJsonObject(JSONObject j, int mode)
    {
        //  Receive JSON message and convert string to a text that can be displayed on the GUI window
        //  ggf. über den string das machen

        // mode 0: Motoren, mode 1: Portal
        // Hier wird bereits vorgefiltert: 
        // Motoren der Conveyorbänder haben genau den gleichen Namen der Parameter
        // die Portalachse hat X/ Y/ Z im Namen!
        float timeStart = Time.realtimeSinceStartup;
        string msg = "";
        if (j != null)
        {
            msg = j.Print();
        }else if (mode == 0)
        {
            // Test-message übermitteln
            int rdValue1 = Random.Range(0, 10);
            int rdValue2 = Random.Range(0, 10);
            int rdValue3 = Random.Range(0, 10);

            msg = "\\\"Motor_Band_1_Velocity\\\": "+ rdValue1+ ",\\\"Motor_Band_1_Acceleration\\\": 0,\\\"Motor_Band_1_Power\\\": " + rdValue2 + ",\\\"Motor_Band_2_Velocity\\\": " + rdValue3 + ",\\\"Motor_Band_2_Acceleration\\\": 1,     " +
                "\\\"Motor_Band_2_Power\\\": " + rdValue2 + ",\\\"Motor_Band_3_Velocity\\\": 3,\\\"Motor_Band_3_Acceleration\\\": 0,\\\"Motor_Band_3_Power\\\": 0,\\\"Motor_Umsetzer_11_Velocity\\\": 6," +
                "\\\"Motor_Umsetzer_11_Acceleration\\\": " + rdValue3 + ",\\\"Motor_Umsetzer_11_Power\\\": 4,\\\"Motor_Umsetzer_12_Velocity\\\": 8, \\\"Motor_Umsetzer_12_Acceleration\\\": 0," +
                "\\\"Motor_Umsetzer_12_Power\\\": 0,\\\"Motor_Umsetzer_21_Velocity\\\": " + rdValue1 + ",\\\"Motor_Umsetzer_21_Acceleration\\\": " + rdValue3 + ",\\\"Motor_Umsetzer_21_Power\\\": 2," +
                "\\\"Motor_Umsetzer_22_Velocity\\\": " + rdValue2 + ",\\\"Motor_Umsetzer_22_Acceleration\\\": 0,\\\"Motor_Umsetzer_22_Power\\\": " + rdValue1 + ",\\\"Linearachse_Fertig\\\": " + MachineReady.ToString()+ "}";
            
        }
        
        List<string[,]> str = new List<string[,]>();
        // Manuell Json file auflösen in String
        int KeyCounter = -1; 
        for (int i = 0; i < msg.Length; i++)
        {

            if (msg[i].ToString() == "\\" && msg[++i].ToString() == "\"")
            {
                KeyCounter++;
                // Key name in array
                string[,] s = new string[1, 2];

                while (msg[++i].ToString() != "\\")
                {
                    s[0, 0] += msg[i].ToString();
                }
                i += 2;
                // Value in array
                while (msg[++i].ToString() != "," && msg[i].ToString() != "}")
                {
                    s[0, 1] += msg[i].ToString();
                }
                str.Add(s);
            }
        }        
        // user_gui.GUI_Debug("Zeit Schritt 1: " + (Time.realtimeSinceStartup - timeStart));
        // timeStart = Time.realtimeSinceStartup;
        // Conveyor Motor
        if (mode == 0)
        {
            // Jetzt noch für das Result filtern für die verschiedenen Typen: 
            string f = "";
            for (int x = 0; x < str.Count; x++)
            {
                f += str[x][0, 0] + ", ";
            }
            //user_gui.GUI_Debug(f);
            foreach (MotorScript m in motors)
            {
                List <string> s1 = new List<string>();
                string s2 = "";
                for (int i = 0; i < str.Count; i++)
                {
                    // Wenn der Motorname vorhanden ist
                    if (str[i][0, 0].Contains(m.transform.parent.name))
                    {
                        // Für Parameter
                        s2 += str[i][0, 0] + ": " + str[i][0, 1] + "\n";
                        // Update Canvas Parameter
                        if (str[i][0, 0].Contains("Acceleration"))
                        {
                            s1.Add ("Acceleration: " + str[i][0, 1]);
                        }
                        else if (str[i][0, 0].Contains("Power"))
                        {
                            s1.Add("Power: " + str[i][0, 1]);
                        }
                        else if (str[i][0, 0].Contains("Velocity"))
                        {
                            s1.Add("Velocity: " + str[i][0, 1]);
                        }
                    }
                    else if (str[i][0, 0].Contains("Linearachse_Fertig"))
                    {
                        // Sperrbereich darf ggf. betreten werden
                        if (str[i][0,1].Contains("True"))
                        {
                            if (!user_gui.AllowEnter)
                            {
                                user_gui.AllowEnter = true;
                                StartCoroutine(user_gui.ToggleWall());
                                // user_gui.GUI_Debug("Now detected Machine is ready");
                            }
                        }
                        // Sperrbereich darf ggf. nicht mehr betreten werden
                        else if (str[i][0, 1].Contains("False"))
                        {
                            if (user_gui.AllowEnter)
                            {
                                user_gui.AllowEnter = false;
                                StartCoroutine(user_gui.ToggleWall());
                                // user_gui.GUI_Debug("Now detected Machine not ready");
                            }
                        }else
                        {
                            // user_gui.GUI_Debug(str[i][0,0] + " "+ str[i][0,1]);
                        }
                    }
                }
            
                if (m.IsConveyorMotor)
                {
                    m.Info.Canvas_Parameter = s1;
                    m.Info.Parameter = s2;
                    if (m.activated)
                    {
                        // Update current information on sign                        
                        user_gui.ReplaceMotorInfo(m.Info, false);
                    }
                }
            }
        }
        // Portal Motor
        else if (mode == 1)
        {
            // Jetzt noch für das Result filtern für die verschiedenen Typen: 
            for (int i = 0; i < str.Count; i++)
            {
                foreach (MotorScript m in motors)
                {
                    // Wenn der Motorname vorhanden ist
                    if (str[i][0, 0].Contains("_x"))
                    {
                        // Für Parameter
                        m.Info.Parameter += str[i][0, 0] + ": " + str[i][0, 1] + "\n";
                        // Update Canvas Parameter
                        
                        break;
                    }
                }
            }
            

            foreach (MotorScript m in motors)
            {
                if (m.IsPortalAxes)
                {    
                    if (m.activated)
                    {
                        // Update current information on sign
                        user_gui.ReplaceMotorInfo(m.Info, false);
                    }
                }
            }

            
        }
        // user_gui.GUI_Debug("Zeit Schritt 2: " + (Time.realtimeSinceStartup - timeStart));
        yield return null;
    }



    public void SaveTransformManipulationValues()
    {
        user_gui.GUI_Debug("Stored Offset Values in PlayerPrefs");

        Vector3 LocalOffset = Init_Transform.InverseTransformDirection(transform.position - Init_Transform.position);

        New_Offset_PosX = LocalOffset.x;
        New_Offset_PosZ = LocalOffset.z;
        New_Offset_RotY = Vector3.SignedAngle(Init_Transform.forward, transform.forward, transform.up);

        // Relativrotation Initkoordinatensystem zu Maschinenkoordinatensystem bestimmen
        PlayerPrefs.SetFloat(gameObject.name + "_rotationToCamFrameY", Vector3.Angle(Vector3.forward, transform.forward));
        user_gui.GUI_Debug("Angle to machine: " + Vector3.SignedAngle(Vector3.forward, transform.forward, transform.up) + ". Offset: X: "+ New_Offset_PosX+"/ Z: "+ New_Offset_PosZ + "/ Rot: "+New_Offset_RotY);

        // Relativwerte einspeichern
        PlayerPrefs.SetString(gameObject.name + "_settings", "Marvin");
        PlayerPrefs.SetFloat(gameObject.name + "_settings_X_Trans", New_Offset_PosX);
        PlayerPrefs.SetFloat(gameObject.name + "_settings_Z_Trans", New_Offset_PosZ);
        PlayerPrefs.SetFloat(gameObject.name + "_settings_Y_Rot", New_Offset_RotY);
    }
}
