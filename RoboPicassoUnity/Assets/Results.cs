using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

// TODO add insults
public class Results : MonoBehaviour {

    double timer = 9;
    int round; 


    // Use this for initialization
    void Start () {
        //Resize center canvas
        // You -> DrawCanvas.
        transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = Sprite.Create(GameState.lastImg, new Rect(0,0,400,400),new Vector2(0.5f,0.5f));
        // Resize width.
        var h = transform.GetChild(1).GetChild(0).gameObject.GetComponent<RectTransform>().rect.height;
        var w = transform.GetChild(1).GetChild(0).gameObject.GetComponent<RectTransform>().rect.width;

        float descale_w_size = h / w;


        transform.GetChild(1).GetChild(0).gameObject.GetComponent<RectTransform>().anchorMin = new Vector2((descale_w_size/2), 0.4f);
        transform.GetChild(1).GetChild(0).gameObject.GetComponent<RectTransform>().anchorMax = new Vector2(1 - (descale_w_size/2), 0.9f);



        SimpleJSON.JSONNode result = null;
        while (result == null)
        {
            result = GameState.sock.TryRecv();
            if (result != null)
            {
                string p = result["prompt"];
                int ranking = result["ranking"];
                round = result["round"];
                string report = result["report"];
                int score = result["score"];

                //Get header
                transform.GetChild(0).gameObject.GetComponentInChildren<Text>().text =  (((round != 7) ? ("Round " + round +"/7 " ) : "FINAL ")) + " - " + p;

                //Get Rating
                transform.GetChild(1).GetChild(1).gameObject.GetComponent<Text>().text = "RoboPicasso's Rating: " + score;

                //Get report
                transform.GetChild(2).GetChild(0).gameObject.GetComponentInChildren<Text>().text = report;


                //Rank
                transform.GetChild(3).gameObject.GetComponent<Text>().text = "RANKING: " + Ordinal(ranking);
            }
        }
    }

    public static string Ordinal(int number)
    {
        string suffix = String.Empty;
        int ones = number % 10;
        int tens = (int)Math.Floor(number / 10M) % 10;

        if (tens == 1)
        {
            suffix = "th";
        }
        else
        {
            switch (ones)
            {
                case 1:
                    suffix = "st";
                    break;
                case 2:
                    suffix = "nd";
                    break;
                case 3:
                    suffix = "rd";
                    break;
                default:
                    suffix = "th";
                    break;
            }
        }
        return String.Format("{0}{1}", number, suffix);
    }

    // Update is called once per frame
    // In 10 seconds, move on to next round.
    void Update () {
        
	    timer -= Time.deltaTime;
        GameObject.FindGameObjectWithTag("Timer").GetComponent<Text>().text = "Next round in: " + ((int)(timer)).ToString();
        if((int)(timer) == 0)
        {
            if(round == 7)
            {
                GameState.sock.Close();
                Thread.Sleep(2);
                Application.LoadLevel("title");
            }
            Application.LoadLevel("drawcanvas");
        }
	}
}
