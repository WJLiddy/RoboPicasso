using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

// TODO add insults
public class Results : MonoBehaviour {

    double timer = 10;

	// Use this for initialization
	void Start () {
        //Await results.
        SimpleJSON.JSONNode result = null;
        //while (result == null)
        Thread.Sleep(8000);
        {
            result = GameState.sock.TryRecv();
            if (result != null)
            {
                string p = result["prompt"];
                int score = result["score"];
                int ranking = result["ranking"];
                int round = result["round"];

                //Get header
                transform.GetChild(0).gameObject.GetComponentInChildren<Text>().text = "Round " + round + " - " + p;

                //Get Rating
                transform.GetChild(1).GetChild(1).gameObject.GetComponent<Text>().text = "RoboPicasso's Rating: " + score;

                //TODO comments.

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
        GameObject.FindGameObjectWithTag("Timer").GetComponent<Text>().text = ((int)(timer)).ToString();
        /**
        if((int)(timer) == 0)
        {
            //send results
            var j = SimpleJSON.JSON.Parse("{}");
            j["picture"] = GameObject.FindGameObjectWithTag("DrawCanvas").GetComponent<DrawCanvas>().CanvasAsBase64();
            GameState.lastImg = GameObject.FindGameObjectWithTag("DrawCanvas").GetComponent<SpriteRenderer>().sprite.texture;
            GameState.sock.Submit(j.ToString());
            Application.LoadLevel("rating");
        }
    */
	}
}
