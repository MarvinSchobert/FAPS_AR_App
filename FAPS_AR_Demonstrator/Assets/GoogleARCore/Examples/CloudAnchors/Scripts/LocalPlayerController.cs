//-----------------------------------------------------------------------
// <copyright file="LocalPlayerController.cs" company="Google">
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

namespace GoogleARCore.Examples.CloudAnchors
{
    using UnityEngine;
    using UnityEngine.Networking;
    using System.Collections.Generic;

    /// <summary>
    /// Local player controller. Handles the spawning of the networked Game Objects.
    /// </summary>
#pragma warning disable 618
    public class LocalPlayerController : NetworkBehaviour
#pragma warning restore 618
    {
        /// <summary>
        /// The Star model that will represent networked objects in the scene.
        /// </summary>
        public GameObject StarPrefab;

        /// <summary>
        /// The Anchor model that will represent the anchor in the scene.
        /// </summary>
        public GameObject AnchorPrefab;

        [System.Serializable]
        public struct SyncListMsgStrings
        {
            public string msg;
            public string sign;
            public string gName;
        }
        public class SyncListMessageItem : SyncListStruct<SyncListMsgStrings> { }


        public SyncListMessageItem MessageItems = new SyncListMessageItem();



        /// <summary>
        /// The Unity OnStartLocalPlayer() method.
        /// </summary>
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            
            // A Name is provided to the Game Object so it can be found by other Scripts, since this
            // is instantiated as a prefab in the scene.
            gameObject.name = "LocalPlayer";
        }
        void SynchronizeMessageItem(SyncListMsgStrings obj)
        {
            GameObject g = GameObject.Find(obj.gName);
            if (g != null)
            {
                // Motor ist vorhanden --> zum Motor Info eine neue Message kreieren!
                string[] messageParams = new string[2];
                messageParams[0] = obj.msg;
                messageParams[1] = obj.sign;
                g.GetComponentInChildren<MotorScript>().SendMessage("AddMessage", messageParams, SendMessageOptions.DontRequireReceiver);
            }
        }

        public void SendMessageOverNetwork(string[] parameter)
        {
            if (isLocalPlayer)
            {
                Debug.Log("GameObject: " + name + ", LocPlayer: " + isLocalPlayer + ", isServer: " + isServer);
                if (isLocalPlayer && !isServer)
                {
                    SyncListMsgStrings m = new SyncListMsgStrings();
                    m.msg = parameter[0];
                    m.sign = parameter[1];
                    m.gName = parameter[2];
                    SynchronizeMessageItem(m);
                    CmdDoTest(m);
                }
                if (isServer)
                {
                    SyncListMsgStrings m = new SyncListMsgStrings();
                    m.msg = parameter[0];
                    m.sign = parameter[1];
                    m.gName = parameter[2];
                    RpcTest(m);
                }

            }
        }

        void OnGUI()
        {
            if (isLocalPlayer)
            {
                if (GUILayout.Button("Test"))
                {
                    Debug.Log("GameObject: " + name + ", LocPlayer: " + isLocalPlayer + ", isServer: " + isServer);
                    if (isLocalPlayer && !isServer)
                    {
                        SyncListMsgStrings m = new SyncListMsgStrings();
                        m.msg = "Hallo Test " + Random.Range(0, 10);
                        m.sign = "ClientName_COMM";
                        m.gName = "Motor_Umsetzer_22";
                        SynchronizeMessageItem(m);
                        CmdDoTest(m);
                    }
                    if (isServer)
                    {
                        SyncListMsgStrings m = new SyncListMsgStrings();
                        m.msg = "Hallo Test " + Random.Range(0, 10);
                        m.sign = "ServerName_RPC";
                        m.gName = "Motor_Umsetzer_22";
                        RpcTest(m);
                    }
                }

                //GUILayout.Box(GameObject.Find("CloudAnchorsExampleController").GetComponent<CloudAnchorsExampleController>().Test.Count.ToString());
            }
        }

        [Command]
        void CmdDoTest(SyncListMsgStrings parameter)
        {
            SynchronizeMessageItem(parameter);

            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (g.GetComponent<LocalPlayerController>() != null)
                {
                    g.GetComponent<LocalPlayerController>().MessageItems.Add(parameter);
                }
            }
            
            Debug.Log("Cmd Test");

        }
        
        [ClientRpc]
        void RpcTest(SyncListMsgStrings parameter)
        {
            SynchronizeMessageItem(parameter);
            
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (g.GetComponent<LocalPlayerController>() != null)
                {
                    g.GetComponent<LocalPlayerController>().MessageItems.Add(parameter);
                }
            }
            
            Debug.Log("RPC Client Test");
        }
        /// <summary>
        /// Will spawn the origin anchor and host the Cloud Anchor. Must be called by the host.
        /// </summary>
        /// <param name="position">Position of the object to be instantiated.</param>
        /// <param name="rotation">Rotation of the object to be instantiated.</param>
        /// <param name="anchor">The ARCore Anchor to be hosted.</param>
        public void SpawnAnchor(Vector3 position, Quaternion rotation, Component anchor)
        {
            // Instantiate Anchor model at the hit pose.
            var anchorObject = Instantiate(AnchorPrefab, position, rotation);

            // Anchor must be hosted in the device.
            anchorObject.GetComponent<AnchorController>().HostLastPlacedAnchor(anchor);

            // Host can spawn directly without using a Command because the server is running in this
            // instance.
#pragma warning disable 618
            NetworkServer.Spawn(anchorObject);
#pragma warning restore 618
        }

        /// <summary>
        /// A command run on the server that will spawn the Star prefab in all clients.
        /// </summary>
        /// <param name="position">Position of the object to be instantiated.</param>
        /// <param name="rotation">Rotation of the object to be instantiated.</param>
#pragma warning disable 618
        [Command]
#pragma warning restore 618
        public void CmdSpawnStar(Vector3 position, Quaternion rotation)
        {
            // Instantiate Star model at the hit pose.
            var starObject = Instantiate(StarPrefab, position, rotation);

            // Spawn the object in all clients.
#pragma warning disable 618
            NetworkServer.Spawn(starObject);
#pragma warning restore 618
        }
    }
}
