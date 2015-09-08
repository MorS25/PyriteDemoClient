﻿using Pyrite;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
        Cards = GameObject.Find("cards_frame");
        NotConnectedCanvas = GameObject.Find("NotConnected");

        SetButtonStates(Connection.State);
        connectionAttempts = 0;
    }

    private GameObject Cards;
    private GameObject NotConnectedCanvas;

    private const int maximumConnectionAttempts = 5;
    private int connectionAttempts;

    // Update is called once per frame
    private void Update()
    {
        if (Connection.State != ConnectionState.Connected)
        {
            if (connectionAttempts >= maximumConnectionAttempts)
            {
                NotConnectedCanvas.GetComponentInChildren<Text>().text = "Failed to connect.";
            }
            else
            {
                StartCoroutine(Connection.CheckConnection());
                connectionAttempts++;
            }
        }
        SetButtonStates(Connection.State);
    }

    public void LaunchBegard()
    {
        AutoFade.LoadLevel("Begard", 0.5f, 0.5f, new Color(50, 9, 5));
    }

    public void LaunchPerth()
    {
        AutoFade.LoadLevel("Perth", 0.5f, 0.5f, new Color(50, 9, 5));
    }

	public void LaunchNashville()
	{
		AutoFade.LoadLevel("Nashville", 0.5f, 0.5f, new Color(50, 9, 5));
	}

    public void LaunchPyriteSite()
    {
        Application.OpenURL("http://www.pyrite3d.org");
    }

    /// <summary>
    /// Sets the button states states of the menu as appropriate for the current connection status
    /// </summary>
    /// <param name="connectionState">current connection state</param>
    private void SetButtonStates(ConnectionState connectionState)
    {
        if (connectionState != ConnectionState.Connected)
        {
            Cards.SetActive(false);
            NotConnectedCanvas.SetActive(true);
        }
        else
        {
            Cards.SetActive(true);
            NotConnectedCanvas.SetActive(false);
        }
    }
}