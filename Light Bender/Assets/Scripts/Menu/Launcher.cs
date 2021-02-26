﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Random = UnityEngine.Random;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;
    
    [SerializeField]  TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text RoomNameText;
    [SerializeField] Transform roomListContentR;
    [SerializeField] Transform roomListContentB;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject PlayerListItemPrefab;
    [SerializeField]  GameObject StartGamebutton;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start() // tu te connectes au jeu
    {
        Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings();
    }

    
    public override void OnConnectedToMaster() // se connecter au host
    {
        Debug.Log("Joined to Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu("Mainmenu");
        Debug.Log("Joined Lobby");
        PhotonNetwork.NickName = "Player" + Random.Range(0, 1000).ToString("0000"); // donne un nom random au joueur de 0 a 1000
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text)) // creation de room
        {
            return;
        }
        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.Instance.OpenMenu("Loading");
    }

    public override void OnJoinedRoom()
    {
       
        MenuManager.Instance.OpenMenu("room");
        RoomNameText.text = PhotonNetwork.CurrentRoom.Name; // donne le nom de la room actuelle avec celle tape precedemment

        Player[] players = PhotonNetwork.PlayerList; // liste des joueurs

        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
            for (int i = 0; i < players.Length; i++)
            {
                Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(players[i]); // setup des joueurs
            }

            StartGamebutton.SetActive(PhotonNetwork.IsMasterClient); // instancie le bouton start uniquement pour le master du jeu
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        StartGamebutton.SetActive(PhotonNetwork.IsMasterClient); // switch de master quand le precedent est parti
    }

    public override void OnCreateRoomFailed(short returnCode,string message)
    {
        errorText.text = "Room Creation Failed" + message;
        MenuManager.Instance.OpenMenu("error");
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(1) ; // index de la scene
    }

    public void LeaveRoom() // leave room
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("loading");
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("loading");
        
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("Mainmenu");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform trans in roomListContentB)
        {
            Destroy(trans.gameObject); // clear la liste a chaque fois qu'on update
        }
        foreach (Transform trans in roomListContentR)
        {
            Destroy(trans.gameObject); 
        }
        
        for (int i = 0; i < roomList.Count; i++) // quand tu quittes la room on enleve le joueur
        {
            if (roomList[i].RemovedFromList)
                continue;
            Instantiate(roomListItemPrefab,roomListContentB).GetComponent<RoomListItem>().Setup(roomList[i]);
            Instantiate(roomListItemPrefab,roomListContentR).GetComponent<RoomListItem>().Setup(roomList[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(PlayerListItemPrefab,playerListContent).GetComponent<PlayerListItem>().Setup(newPlayer); // instancie un  nouveau player
    }
}
