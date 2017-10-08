using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

// TODO add insults
public class Results : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //Await results.
        Thread.Sleep(1);
        var result = GameState.sock.TryRecv();
        if (result != null)
        {
            string p = result["prompt"];
            int score = result["score"];
            int ranking = result["ranking"];
            int round = result["round"];

            //Get header
            transform.GetChild(0).gameObject.GetComponent<Text>().text = "Round " + round + " - " + p;

            //Get Rating
            transform.GetChild(1).GetChild(1).gameObject.GetComponent<Text>().text = "Rating: " + score;

            //TODO comments.

            //Rank
            transform.GetChild(3).gameObject.GetComponent<Text>().text = "RANKING: " + Ordinal(ranking);



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
    void Update () {
		
	}
}
