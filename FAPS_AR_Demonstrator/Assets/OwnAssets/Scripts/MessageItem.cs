using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MessageItem : ScriptableObject
{
    
    public string MessageText;
    public string SignatureText;
    
    public void AddText(string Text, string UserName)
    {
        MessageText = Text;
        SignatureText= System.DateTime.Now.ToString() +", "+ UserName;
    }

}
