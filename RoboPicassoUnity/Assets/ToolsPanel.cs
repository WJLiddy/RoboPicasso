using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolsPanel : MonoBehaviour {

	// Use this for initialization
	void Start () {
        // Get origin and find device width in world units.
        var origin = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 topRightCorner = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));
        var deviceH = topRightCorner.y - origin.y;
        var deviceW = topRightCorner.x - origin.x;
        Debug.Log(deviceW);
        Debug.Log(deviceH);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
