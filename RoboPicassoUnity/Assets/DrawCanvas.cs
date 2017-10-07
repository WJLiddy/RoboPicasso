
using UnityEngine;


public class DrawCanvas : MonoBehaviour
{

    SpriteRenderer canvas;
    Texture2D canvas_texture;
    bool last_point_set = false;
    Vector2 last_point;

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
            case -1: c = new Color(255, 255, 255); break;
            case 0: c = new Color(1, 0, 0); break;
            case 1: c = new Color(1, 0.5f, 0); break;
            case 2: c = new Color(1, 1, 0); break;
            case 3: c = new Color(0, 1, 0); break;
            case 4: c = new Color(0, 0, 1); break;
            case 5: c = new Color(0.5f, 0.5f, 0.5f); break;
            case 6: c = new Color(0f, 0f, 0f); break;
        }
        canvas_texture.SetPixel(x, y, c);
    }

    void floodfill(int center_x, int center_y)
    {
        /**
        Flood - fill(node, target - color, replacement - color):
 1.If target - color is equal to replacement-color, return.
 2.If color of node is not equal to target - color, return.
 3.Set Q to the empty queue.
 4.Add node to Q.
 5.For each element N of Q:
        6.Set w and e equal to N.
 7.Move w to the west until the color of the node to the west of w no longer matches target - color.
 8.Move e to the east until the color of the node to the east of e no longer matches target - color.
 9.For each node n between w and e:
        10.Set the color of n to replacement-color.
11.If the color of the node to the north of n is target - color, add that node to Q.
12.If the color of the node to the south of n is target - color, add that node to Q.
13.Continue looping until Q is exhausted.
14.Return.
    */
    }

    void plotPoint(int center_x, int center_y)
    {
        ToolsPanel t = GameObject.FindGameObjectWithTag("ToolsPanel").GetComponent<ToolsPanel>();

        if (t.erase_mode)
        {
            int BRUSH_SIZE = 10;
            //draw... 5 by 5?
            for (int x = 0; x != BRUSH_SIZE; x++)
            {
                for (int y = 0; y != BRUSH_SIZE; y++)
                {
                    ColorOnDrawCanvas(-1, center_x + x, center_y + y);
                }
            }
        }
        else if (t.floodfill)
        {
            floodfill(center_x, center_y);
        }
        else
        {
            int BRUSH_SIZE = 2;
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

    void processSingleTouchEvent(Vector2 touch_vec)
    {
        ToolsPanel t = GameObject.FindGameObjectWithTag("ToolsPanel").GetComponent<ToolsPanel>();
        //Add half of device width to offset from 0 and get ratio.
        float x_fraction = touch_vec.x / (float)Screen.width;
        // bias y with part of screen on bottom
        float y_bias = -((Screen.height - Screen.width));
        float y_fraction = (touch_vec.y + y_bias) / (float)Screen.width;


        int center_x = (int)(x_fraction * 400);
        int center_y = (int)(y_fraction * 400);

        plotPoint(center_x, center_y);

        //only map last points. 
        if (!t.floodfill)
        {
            if (!last_point_set)
            {
                //Do nothing
                last_point_set = true;
                last_point = new Vector2(center_x, center_y);
            }
            else
            {
                //last point was set.
                double diff = (new Vector2((center_x - last_point.x), (center_y - last_point.y))).magnitude;

                var dx = (center_x - last_point.x) / diff;
                var dy = (center_y - last_point.y) / diff;

                for (double step = 0; step < diff; step += 1)
                {
                    plotPoint((int)(last_point.x + (step*dx)), (int)(last_point.y + (step*dy))); 
                }
                last_point = new Vector2(center_x, center_y);
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
            } else
            {
                last_point_set = false;
            }
        }
        else
        {
            if(Input.touchCount >= 1)
            {
                processSingleTouchEvent(Input.GetTouch(0).position);
            } else
            {
                last_point_set = false;
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
