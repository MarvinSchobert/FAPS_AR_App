//-----------------------------------------------------------------------
// <copyright file="AugmentedImageExampleController.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------


namespace GoogleARCore.Examples.AugmentedImage
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using System.Collections;

    /// <summary>
    /// Controller for AugmentedImage example.
    /// </summary>
    /// <remarks>
    /// In this sample, we assume all images are static or moving slowly with
    /// a large occupation of the screen. If the target is actively moving,
    /// we recommend to check <see cref="AugmentedImage.TrackingMethod"/> and
    /// render only when the tracking method equals to
    /// <see cref="AugmentedImageTrackingMethod"/>.<c>FullTracking</c>.
    /// See details in <a href="https://developers.google.com/ar/develop/c/augmented-images/">
    /// Recognize and Augment Images</a>
    /// </remarks>
    /// 

    public class SceneControl : MonoBehaviour
    {
        /// <summary>
        /// A prefab for visualizing an AugmentedImage.
        /// </summary>
        /// 

        [System.Serializable]
        public struct ImageNameMatch
        {
            public string ImageName;
            public GameObject InstantiateObject;
        }
        public AugmentedImageVisualizer AugmentedImageVisualizerPrefab;
        public UserGUI User_GUI;
        public ObjectInteractor interactor;

        public bool SimulateWithMouse;
        public List<ImageNameMatch> ImageNameMatches = new List<ImageNameMatch>();
        /// <summary>
        /// The overlay containing the fit to scan user guide.
        /// </summary>
        public GameObject FitToScanOverlay;
        public ARCoreSessionConfig config;

        public Trackable image_;

        private Dictionary<int, AugmentedImageVisualizer> m_Visualizers
            = new Dictionary<int, AugmentedImageVisualizer>();

        private bool isfinished;
        public Trackable FloorSurface_Machine;
        public TrackableHit hit;
        private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();
        private AugmentedImageDatabase database;

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            // Enable ARCore to target 60fps camera capture frame rate on supported devices.
            // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
            Application.targetFrameRate = 60;
            database = config.AugmentedImageDatabase;


        }

        public void OnApplicationQuit()
        {
            config.AugmentedImageDatabase = database;
        }

        public void ResetScene()
        {
            // FloorSurface_Machine = null;
            m_Visualizers.Clear();
            isfinished = false;
            User_GUI.GUI_Debug("Reseted Scene");
        }
        /// <summary>
        /// The Unity Update method.
        /// </summary>
        private List<DetectedPlane> m_NewPlanes = new List<DetectedPlane>();
        public void Update()
        {
            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (!isfinished)
            {
                // Debug.Log("Is not finished " + Time.realtimeSinceStartup);
                // Get updated augmented images for this frame.
                Session.GetTrackables<AugmentedImage>(
                    m_TempAugmentedImages, TrackableQueryFilter.Updated);

                // Create visualizers and anchors for updated augmented images that are tracking and do
                // not previously have a visualizer. Remove visualizers for stopped images.
                if (!SimulateWithMouse)
                {
                    foreach (var image in m_TempAugmentedImages)
                    {

                        AugmentedImageVisualizer visualizer = null;
                        GameObject fapsMachine = null;
                        m_Visualizers.TryGetValue(image.DatabaseIndex, out visualizer);
                        if ((FloorSurface_Machine != null) && (image.TrackingState == TrackingState.Tracking && visualizer == null))
                        {


                            // Create FAPS Machine GameObject and the image fully tracked
                            if (fapsMachine == null && image.TrackingMethod == AugmentedImageTrackingMethod.FullTracking)
                            {
                                User_GUI.GUI_Debug("Found Image now");
                                // Create an anchor to ensure that ARCore keeps tracking this augmented image.
                                Anchor anchor = image.CreateAnchor(image.CenterPose);
                                visualizer = (AugmentedImageVisualizer)Instantiate(
                                    AugmentedImageVisualizerPrefab, anchor.transform);
                                visualizer.Image = image;
                                m_Visualizers.Add(image.DatabaseIndex, visualizer);

                                StartCoroutine(InstantiateWithDelay(image));
                                isfinished = true;
                                User_GUI.GUI_Debug("Image: " + database[image.DatabaseIndex].Name);
                            }
                            else
                            {
                                User_GUI.GUI_Debug("Image isnt tracked right");
                            }


                        }
                        else if (image.TrackingState == TrackingState.Stopped && visualizer != null)
                        {
                            User_GUI.GUI_Debug("Destroying");
                            m_Visualizers.Remove(image.DatabaseIndex);
                            GameObject.Destroy(visualizer.gameObject);
                        }
                        // else User_GUI.GUI_Debug("Hit surface first");
                    }
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    GameObject fapsMachine = null;
                    if (FloorSurface_Machine != null && fapsMachine == null)
                    {
                        StartCoroutine(InstantiateWithDelay(null));
                        isfinished = true;
                        FitToScanOverlay.SetActive(false);
                    }

                }

                // Show the fit-to-scan overlay if there are no images that are Tracking.
                foreach (var visualizer in m_Visualizers.Values)
                {
                    if (visualizer.Image.TrackingState == TrackingState.Tracking)
                    {
                        FitToScanOverlay.SetActive(false);
                        return;
                    }
                }

                if (!FitToScanOverlay.activeInHierarchy && FloorSurface_Machine != null)
                {
                    User_GUI.Set_HelperMsg("Now Scan your reference image in order to auto - create the CAD machine");
                    FitToScanOverlay.SetActive(true);
                }
            }


            ///////////////////////////////// Handling of Touch Interactions

            if (!isfinished && !SimulateWithMouse)
            {
                Touch touch;
                if ((Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began))
                {
                    return;
                }

                // Should not handle input if the player is pointing on UI.
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    return;
                }

                // Raycast against the location the player touched to search for planes.

                TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.FeaturePointWithSurfaceNormal;

                if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
                {
                    // Use hit pose and camera pose to check if hittest is from the
                    // back of the plane, if it is, no need to create the anchor.
                    if ((hit.Trackable is DetectedPlane))
                    {
                        if (FloorSurface_Machine == null)
                        {


                        }

                        FloorSurface_Machine = hit.Trackable;
                        User_GUI.GUI_Debug("Hit detected Plane");
                    }
                    else
                    {
                        User_GUI.GUI_Debug("Hit something");
                    }

                }
            }
            else 
            {
                if (!isfinished && Input.GetMouseButtonDown(0))
                {
                    TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.FeaturePointWithSurfaceNormal;

                    if (Frame.Raycast(Input.mousePosition.x, Input.mousePosition.z, raycastFilter, out hit))
                    {
                        if ((hit.Trackable is DetectedPlane))
                        {
                            if (FloorSurface_Machine == null)
                            {


                            }

                            FloorSurface_Machine = hit.Trackable;
                            User_GUI.GUI_Debug("Hit detected Plane");
                        }
                        else
                        {
                            User_GUI.GUI_Debug("Hit something");
                        }

                    }
                }
            }
        }
        IEnumerator InstantiateWithDelay(AugmentedImage image)
        {
            yield return new WaitForSeconds(1.0f);
            // Find suitable Machine Prefab
            GameObject FAPS_Machine = null;
            foreach (ImageNameMatch match in ImageNameMatches)
            {
                if (match.ImageName == image.Name)
                {
                    FAPS_Machine = match.InstantiateObject;
                    break;
                }
            }
            if (FAPS_Machine != null)
            {
                // Maschine wird offline instanziert
                if (image != null) image_ = image;
                GameObject fapsMachine = null;
                User_GUI.GUI_Debug("Image is fully tracked");
                Anchor anchorMachine = FloorSurface_Machine.CreateAnchor(hit.Pose);
                fapsMachine = Instantiate(FAPS_Machine);

                
                

                fapsMachine.transform.parent = anchorMachine.transform;
                if (fapsMachine.GetComponent<MachineScript>() != null)
                {
                    fapsMachine.GetComponent<MachineScript>().trackable = FloorSurface_Machine;
                    if (image != null) fapsMachine.GetComponent<MachineScript>().TrackableImage = image;
                }
                fapsMachine.transform.rotation = hit.Pose.rotation;

                if (image != null) fapsMachine.transform.LookAt(new Vector3(fapsMachine.transform.position.x - image.CenterPose.up.x * 3, fapsMachine.transform.position.y, fapsMachine.transform.position.z - image.CenterPose.up.z * 3));

                // Position so anpassen, dass Referencepunkt auf der ImagePosition sitzt
                if ((image != null) && fapsMachine.GetComponent<MachineScript>() != null)
                {
                    Vector3 referencePosition = fapsMachine.GetComponent<MachineScript>().ReferencePoint.transform.position - image.CenterPose.position;
                    fapsMachine.transform.position = new Vector3((fapsMachine.transform.position - referencePosition).x, hit.Pose.position.y, (fapsMachine.transform.position - referencePosition).z);
                }
                if (image != null) User_GUI.GUI_Debug("Instantiate Rotation Angle: " + Vector3.Angle(Vector3.forward, -image.CenterPose.up) + " vs " + Vector3.Angle(Vector3.forward, fapsMachine.transform.forward));

                if (fapsMachine.GetComponent<MachineScript>() != null)
                {

                    User_GUI.AddMachineSelectable(fapsMachine);
                    Session.GetTrackables<DetectedPlane>(m_NewPlanes, TrackableQueryFilter.All);
                    for (int i = 0; i < m_NewPlanes.Count; i++)
                    {
                        if (FloorSurface_Machine == m_NewPlanes[i])
                        {
                            fapsMachine.GetComponent<MachineScript>().Surface = m_NewPlanes[i];
                            fapsMachine.GetComponent<MachineScript>().surfacePlaneHeight = m_NewPlanes[i].CenterPose.position.y;
                            break;
                        }
                    }
                    fapsMachine.GetComponent<MachineScript>().Initialize();

                }




                isfinished = true;

                // Jetzt kein Image Tracking mehr!
                config.AugmentedImageDatabase = null;
                interactor.Functionenabled = true;

                User_GUI.Set_HelperMsg("Select your machine via the FAPS menu in order to:\nRotate and translate the object by 2-/3" +
                    " finger swiping\nGet machine data by clicking on e.g. the motors\nCheck out the FAPS menu for further possibilities.");
            }
        }      
        
    }
}
