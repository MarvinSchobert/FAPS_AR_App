#region License
/*
 * TestSocketIO.cs
 *
 * The MIT License
 *
 * Copyright (c) 2014 Fabio Panettieri
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

using System.Collections;
using UnityEngine;
using SocketIO;

public class TestSocketIO : MonoBehaviour
{
	private SocketIOComponent socket;

    public string url = "ws://cloud.faps.uni-erlangen.de:8095/socket.io/?EIO=4&transport=websocket";
    public bool autoConnect = true;
    public int reconnectDelay = 5;
    public float ackExpirationTime = 1800f;
    public float pingInterval = 25f;
    public float pingTimeout = 60f;

    float lastCheckMotor;
    float lastCheckPortal;

    public void Start() 
	{
		GameObject go = GameObject.Find("SocketIO");
		socket = go.GetComponent<SocketIOComponent>();
        lastCheckMotor = Time.time;
        lastCheckPortal = Time.time;
        //if (socket != null)
        //{
        //    socket.url = this.url;
        //    socket.autoConnect = true;
        //    socket.reconnectDelay = 5;
        //    socket.ackExpirationTime = 1800f;
        //    socket.pingInterval = 25f;
        //    socket.pingTimeout = 60f;
        //    socket.Connect();
        //}

        socket.On("open", TestOpen);
		socket.On("boop", TestBoop);
		socket.On("error", TestError);
		socket.On("close", TestClose);

        // Concrete Data events
        socket.On("AMQPMachineData", PortalRobotDataMsg);
        socket.On("ConveyorData", ConveyorDataMsg);

       // StartCoroutine("BeepBoop");
	}

	private IEnumerator BeepBoop()
	{
		// wait 1 seconds and continue
		yield return new WaitForSeconds(1);
		
		socket.Emit("beep");
		
		// wait 3 seconds and continue
		yield return new WaitForSeconds(3);
		
		socket.Emit("beep");
		
		// wait 2 seconds and continue
		yield return new WaitForSeconds(2);
		
		socket.Emit("beep");
		
		// wait ONE FRAME and continue
		yield return null;
		
		socket.Emit("beep");
		socket.Emit("beep");
	}

	public void TestOpen(SocketIOEvent e)
	{
		Debug.Log("[SocketIO] Open received: " + e.name + " " + e.data);
	}
	
	public void TestBoop(SocketIOEvent e)
	{
		Debug.Log("[SocketIO] Boop received: " + e.name + " " + e.data);

		if (e.data == null) { return; }

		Debug.Log(
			"#####################################################" +
			"THIS: " + e.data.GetField("this").str +
			"#####################################################"
		);
	}
	
	public void TestError(SocketIOEvent e)
	{
		Debug.Log("[SocketIO] Error received: " + e.ToString());
	}
	
	public void TestClose(SocketIOEvent e)
	{	
		Debug.Log("[SocketIO] Close received: " + e.name + " " + e.data);
	}

    public void ConveyorDataMsg(SocketIOEvent e)
    {
        if (Time.time - lastCheckMotor > 0.5f)
        {
            JSONObject val = e.data;
            foreach (MachineScript m in GameObject.FindObjectsOfType<MachineScript>())
            {
                m.CloudInputInformation = val;
                m.HasChangedMotorInputValues = true;
            }            
            Debug.Log("[SocketIO] Msg received: " + e.name + " " + e.data);
            lastCheckMotor = Time.time;
        }
    }
    public void PortalRobotDataMsg(SocketIOEvent e)
    {
        if (Time.time - lastCheckPortal > 0.5f)
        {
            JSONObject val = e.data;
            foreach (MachineScript m in GameObject.FindObjectsOfType<MachineScript>())
            {
                m.CloudInputInformation = val;
                m.HasChangedPortalInputValues = true;
            }
            Debug.Log("[SocketIO] Msg received: " + e.name + " " + val.GetField("encoder_values_x").str);
            lastCheckPortal = Time.time;
        }
    }
}
