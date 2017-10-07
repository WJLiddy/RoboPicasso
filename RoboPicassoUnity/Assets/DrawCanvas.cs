using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCanvas : MonoBehaviour
{

    SpriteRenderer canvas;
    // Use this for initialization
    void Awake()
    {
        //Get canvas
        canvas = GetComponent<SpriteRenderer>();
        // Get origin and find device width in world units.
        var origin = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 topRightCorner = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));

        var deviceWidth = topRightCorner.x - origin.x;
        var deviceHeight = topRightCorner.y - origin.y;
        // Resize
        canvas.transform.localScale = new Vector2(deviceWidth / 4, deviceWidth / 4);

        // Move up. Center at .25 of height.
        canvas.transform.localPosition = new Vector2(0, topRightCorner.y - (deviceWidth / 4));

    }

    // Update is called once per frame
    void Update()
    {

    }
}
