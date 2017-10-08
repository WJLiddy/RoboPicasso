
using System;
using System.Collections.Generic;
using UnityEngine;


public class DrawCanvas : MonoBehaviour
{

    SpriteRenderer canvas;
    Texture2D canvas_texture;
    bool last_point_set = false;
    Vector2 last_point;
    Color[,] undo_state;

    // Use this for initialization
    void Awake()
    {
        undo_state = new Color[400, 400];
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

    public void loadUndo()
    {
        Debug.Log("undo loaded");
        for (int x = 0; x != 400; x++)
        {
            for (int y = 0; y != 400; y++)
            {
                undo_state[x, y] = canvas_texture.GetPixel(x, y);
            }
        }
    }

    public void activateUndo()
    {
        Debug.Log("undo activated");
        for (int x = 0; x != 400; x++)
        {
            for (int y = 0; y != 400; y++)
            {
                canvas_texture.SetPixel(x, y, undo_state[x, y]);
            }
        }
        canvas_texture.Apply();
    }
    public void clearCanvas()
    {
        for(int x = 0; x != 400; x++)
        {
            for(int y = 0; y != 400; y++)
            {
                canvas_texture.SetPixel(x, y, Color.white);
            }
        }
    }

    Color indexToColor(int paint_index)
    {
        Color c;
        switch (paint_index)
        {
            case -1: c = new Color(255, 255, 255); break;
            case 0: c = new Color(1, 0, 0); break;
            case 1: c = new Color(1, 0.5f, 0); break;
            case 2: c = new Color(1, 1, 0); break;
            case 3: c = new Color(0, 1, 0); break;
            case 4: c = new Color(0, 0, 1); break;
            case 5: c = new Color(0.5f, 0.5f, 0.5f); break;
            default: c = new Color(0f, 0f, 0f); break;
        }
        return c;
    }

    void ColorOnDrawCanvas(int paint_index, int x, int y)
    {
        Color c = indexToColor(paint_index);
        canvas_texture.SetPixel(x, y, c);
    }

    void floodfill(int center_x, int center_y, Color recolor)
    {
        // First step: get all region that is the color selected
        Color region_color = canvas_texture.GetPixel(center_x, center_y);

        bool[,] is_checked = new bool[400, 400];
        for (int x = 0; x != 400; x++)
        {
            for (int y = 0; y != 400; y++)
            {
                is_checked[x, y] = false;
            }
        }


        Queue<int[]> q = new Queue<int[]>();
        List<int[]> to_recolor = new List<int[]>();

        var node = new int [2]{ center_x, center_y };
        q.Enqueue(node);

        while(q.Count > 0)
        {
            var denode = q.Dequeue();
            var delts = new List<int[]>();
            delts.Add(new int[2] { -1, 0 });
            delts.Add(new int[2] { 1, 0 });
            delts.Add(new int[2] { 0, -1 });
            delts.Add(new int[2] { 0, 1 });

            foreach(var delt in delts)
            {
                var next_node = new int[2] { denode[0] + delt[0], denode[1] + delt[1] };
                    if(next_node[0] < 0 || next_node[0] > 399 || next_node[1] < 0 || next_node[1] > 399)
                    {
                        continue;
                    }

                    if(is_checked[next_node[0],next_node[1]])
                    {
                        continue;
                    }

                    //Add it to the explore list.
                    is_checked[next_node[0], next_node[1]] = true;

                    //This is the same region, and it has not been explored.
                    if (canvas_texture.GetPixel(next_node[0],next_node[1]) == region_color)
                    {                 
                        q.Enqueue(next_node);
                        to_recolor.Add(next_node);
                    }
            }
        }
        

        foreach(int[] c in to_recolor)
        {
            canvas_texture.SetPixel(c[0], c[1], recolor);
        }
    }
    void plotPoint(int center_x, int center_y)
    {
        ToolsPanel t = GameObject.FindGameObjectWithTag("ToolsPanel").GetComponent<ToolsPanel>();

        if (t.erase_mode)
        {
            int BRUSH_SIZE = 15;
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
            floodfill(center_x, center_y, indexToColor(t.brush_selection));
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
                //Ignore OOB mouse presses.
                if(Input.mousePosition.y < (Screen.height - Screen.width))
                {
                    return;
                }

                // ALWAYS save state on first touch.
                if(!last_point_set)
                {
                    loadUndo();
                }
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
                //Ignore OOB mouse presses.
                if (Input.GetTouch(0).position.y < (Screen.height - Screen.width))
                {
                    return;
                }
                // ALWAYS save state on first touch.
                if (!last_point_set)
                {
                    loadUndo();
                }
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

    public string CanvasAsBase64()
    {
        return Convert.ToBase64String(canvas_texture.EncodeToPNG());
    }
}
