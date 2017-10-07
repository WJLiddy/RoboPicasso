
using UnityEngine;


public class DrawCanvas : MonoBehaviour
{

    SpriteRenderer canvas;
    Texture2D canvas_texture;
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

        // Center 
        canvas.transform.localPosition = new Vector2(0, (deviceHeight / 2) - (deviceWidth / 2));

        canvas_texture = GetComponent<SpriteRenderer>().sprite.texture;
        clearCanvas();
    }

    void clearCanvas()
    {
        for(int x = 0; x != 400; x++)
        {
            for(int y = 0; y != 400; y++)
            {
                canvas_texture.SetPixel(x, y, Color.white);
            }
        }
    }
    void ColorOnDrawCanvas(int paint_index, int x, int y)
    {
        
        Color c = new Color(0,0,0);
        switch(paint_index)
        {
            case -1: c = new Color(0, 0, 0); break;
            case 0: c = new Color(1, 0, 0); break;
            case 1: c = new Color(1, 0.5f, 0); break;
            case 2: c = new Color(1, 1, 0); break;
            case 3: c = new Color(0, 1, 0); break;
            case 4: c = new Color(0, 0, 1); break;
            case 5: c = new Color(0.5f, 0.5f, 0.5f); break;
            case 6: c = new Color(0f, 0f, 0f); break;
        }
        Debug.Log(c);
        canvas_texture.SetPixel(x, y, c);
    }

    void processSingleTouchEvent(Vector2 touch_vec)
    {
        //Add half of device width to offset from 0 and get ratio.
        float x_fraction = touch_vec.x / (float)Screen.width;
        // bias y with part of screen on bottom
        float y_bias = -((Screen.height - Screen.width));
        float y_fraction = (touch_vec.y + y_bias) / (float)Screen.width;

        ToolsPanel t = GameObject.FindGameObjectWithTag("ToolsPanel").GetComponent<ToolsPanel>();

        int center_x = (int)(x_fraction * 400);
        int center_y = (int)(y_fraction * 400);
        Debug.Log(center_y);


        Debug.Log(center_x);
        Debug.Log(center_y);

        int BRUSH_SIZE = 2;
        if (t.erase_mode)
        {
            //draw... 5 by 5?
            for (int x = 0; x != BRUSH_SIZE; x++)
            {
                for (int y = 0; y != BRUSH_SIZE; y++)
                {
                    ColorOnDrawCanvas(-1, x, y);
                }
            }
        }
        else if (t.floodfill)
        {

        }
        else
        {
            //draw... 5 by 5?
            for (int x = -BRUSH_SIZE; x != BRUSH_SIZE; x++)
            {
                for (int y = -BRUSH_SIZE; y != BRUSH_SIZE; y++)
                {
                    ColorOnDrawCanvas(t.brush_selection, center_x + x, center_y + y);
                }
            }
        }
    }
    void processTouchInputs()
    {
        if (!Application.isMobilePlatform)
        {
            if (Input.GetMouseButton(0))
            {
                processSingleTouchEvent(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
            }
        }
        else
        {
            for (var i = 0; i < Input.touchCount; ++i)
            {
                processSingleTouchEvent(Input.GetTouch(i).position);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        processTouchInputs();
        canvas_texture.Apply();
    }
}
