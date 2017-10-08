using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;

public class Title : MonoBehaviour {

    public string name = "";
    public string serv_state = "";
	// Use this for initialization
	void Start () {
        GameState.sock = new Sock("25.83.129.56",39181);
    }
	
	// Update is called once per frame
	void Update () {

        //Testing only
        if (GameState.sock != null)
        {
            SimpleJSON.JSONNode n = GameState.sock.TryRecv();
            if (n != null)
            {
                serv_state = n["status"];
                transform.GetChild(3).GetComponent<Text>().text = serv_state;
                Debug.Log(serv_state);
            }
        }
        if(name != "" && serv_state == "Ready")
        {
            SimpleJSON.JSONNode n = JSONNode.Parse("{}");
            n["uname"] = name;
            GameState.sock.Submit(n.ToString());
            Application.LoadLevel("drawcanvas");
        }
	}

    public void SetName(string s)
    {
        name = s;
    }
}
