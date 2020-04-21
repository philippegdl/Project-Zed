﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HostGame : MonoBehaviour
{

    [SerializeField] private uint roomSize = 6;
    [SerializeField] private string roomName;
    private NetworkManager networkManager;
    
    private void Start()
    {
        networkManager = NetworkManager.singleton;
        if (networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }
    }

    
    public void SetRoomName (string _name)
    {
        roomName = _name;
    }

    public void CreateRoom()
    {
        if (roomName != "" && roomName != null)
        {
            Debug.Log("Création de la partie : " + roomName + " avec " + roomSize + " slots.");
            //créer la partie
            networkManager.matchMaker.CreateMatch(roomName, roomSize, true, "", "", "", 0, 0,
                networkManager.OnMatchCreate);
        }
    }
}
