using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ToolsPanel : MonoBehaviour {

    public bool erase_mode = false;
    public bool floodfill = false;

    public int brush_selection = 6;
    List<GameObject> paint_buttons = new List<GameObject>();
    List<GameObject> mode_buttons = new List<GameObject>();

    double timer = 10;
    // Use this for initialization
    void Start () {

        SimpleJSON.JSONNode prompt = null;
        while (prompt == null)
        {
            prompt = GameState.sock.TryRecv();
            if (prompt != null)
            {
                string p = prompt["prompt"];
                bool an = (p[0] == 'a') || (p[0] == 'e') || (p[0] == 'i') || (p[0] == 'o') || (p[0] == 'u');
                transform.GetChild(10).gameObject.GetComponent<Text>().text = "Draw " + (an ? "an " : "a ") + p;
                break;
            }
        }
        
        // Get origin and find device width in world units.
        var origin = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 topRightCorner = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));
        var deviceH = topRightCorner.y - origin.y;
        var deviceW = topRightCorner.x - origin.x;
        var pct = (deviceH - deviceW) / deviceH;
        //Resize this based on space.
        GetComponent<RectTransform>().anchorMax = new Vector2(1, pct);

        //Get ROYGBgb btns.
        for (int i = 0; i != 7; i++)
        {
            paint_buttons.Add(transform.GetChild(i).GetChild(0).gameObject);
        }
        for (int i = 7; i != 10; i++)
        {
            mode_buttons.Add(transform.GetChild(i).gameObject);
        }
        brushSelection(6);
    }
	
	// Update is called once per frame
	void Update () {
        timer -= Time.deltaTime;
        GameObject.FindGameObjectWithTag("Timer").GetComponent<Text>().text = ((int)(timer)).ToString();
        if((int)(timer) == 0)
        {
            //send results
            var j = SimpleJSON.JSON.Parse("{}");
            j["picture"] = GameObject.FindGameObjectWithTag("DrawCanvas").GetComponent<DrawCanvas>().CanvasAsBase64();
            GameState.lastImg = GameObject.FindGameObjectWithTag("DrawCanvas").GetComponent<SpriteRenderer>().sprite.texture;
            GameState.sock.Submit(j.ToString());
            Application.LoadLevel("rating");
        }

	}

    public void brushSelection(int i)
    {
        if(erase_mode)
        {
            return;
        }

        brush_selection = i;
        for(int j = 0; j != 7; j++)
        {
            if(j != brush_selection)
            {
                //disable paint icon
                paint_buttons[j].GetComponentInChildren<Image>().color = new Color(0, 0, 0, 0);
            } else
            {
                //enable paint icon
                paint_buttons[j].GetComponentInChildren<Image>().color = new Color(255, 255, 255, 255);
            }
        }
    }

    public void clearBrush()
    {
        for (int j = 0; j != 7; j++)
        {
            paint_buttons[j].GetComponentInChildren<Image>().color = new Color(0, 0, 0, 0);
        }
    }

    public void bolden(int i )
    {
        for(int j = 0; j != 3; j++)
        {
            if( i == j)
            {
                mode_buttons[j].GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
            }
            else
            {
                mode_buttons[j].GetComponentInChildren<Text>().fontStyle = FontStyle.Normal;
            }
        }
    }

    public void PaintMode()
    {
        erase_mode = false;
        floodfill = false;
        brushSelection(brush_selection);
        bolden(0);
    }

    public void EraseMode()
    {
        erase_mode = true;
        floodfill = false;
        clearBrush();
        bolden(1);
    }

    public void FloodFillMode()
    {
        erase_mode = false;
        floodfill = true;
        brushSelection(brush_selection);
        bolden(2);
    }
}
