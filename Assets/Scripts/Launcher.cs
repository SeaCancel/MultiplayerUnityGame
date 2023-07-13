using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;

namespace Con.IgorGuriev.FPSMultiplayer
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        public InputField roomnameField;
        public Slider maxPlayerSlider;
        public Text maxPlayersValue;

        public GameObject tabMain;
        public GameObject tabRooms;
        public GameObject tabCreate;

        public GameObject buttonRoom;

        private List<RoomInfo> roomList;

      public void Awake()
      {
        PhotonNetwork.AutomaticallySyncScene = true;
        Connect();
      }

      public override void OnConnectedToMaster()
      {
            Debug.Log("Connected");

            PhotonNetwork.JoinLobby();
            base.OnConnectedToMaster();
      }

      public override void OnJoinedRoom()
      {
        StartGame();

        base.OnJoinedRoom();
      }

      public override void OnJoinRandomFailed(short returnCode, string message)
      {
            Create();

            base.OnJoinRandomFailed(returnCode, message);
      }
        
      public void Connect ()
      {
            Debug.Log("Trying to Connect...");
            PhotonNetwork.GameVersion = "0.0.0";
            PhotonNetwork.ConnectUsingSettings();
      }

      public void Join ()
      {
            PhotonNetwork.JoinRandomRoom();
      }

      public void Create ()
      {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = (byte)maxPlayerSlider.value;

            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
            properties.Add("map", 0);
            options.CustomRoomProperties = properties;

            PhotonNetwork.CreateRoom(roomnameField.text, options);
      }

      public void ChangeMap ()
      {

      }

      public void ChangeMaxPlayersSlider (float t_vatue)
      {
            maxPlayersValue.text = Mathf.RoundToInt(t_vatue).ToString();
      }

      public void TabCloseAll()
      {
            tabMain.SetActive(false);
            tabRooms.SetActive(false);
            tabCreate.SetActive(false);
      }
      
      public void TabOpenMain ()
      {
            TabCloseAll();
            tabMain.SetActive(true);
      }

      public void TabOpenRooms ()
      {
            TabCloseAll();
            tabRooms.SetActive(true);
      }

      public void TabOpenCreate ()
      {
            TabCloseAll();
            tabCreate.SetActive(true);

            
        }

      private void ClearRoomList ()
      {
            Transform content = tabRooms.transform.Find("ScrollView/Viewport/Content");
            foreach (Transform a in content) Destroy(a.gameObject);
      }

        public override void OnRoomListUpdate(List<RoomInfo> p_list)
        {
            roomList = p_list;
            ClearRoomList();

            Transform content = tabRooms.transform.Find("ScrollView/Viewport/Content");

            foreach (RoomInfo a in roomList)
            {
                GameObject newRoomButton = Instantiate(buttonRoom, content) as GameObject;

                newRoomButton.transform.Find("Name").GetComponent<Text>().text = a.Name;
                newRoomButton.transform.Find("Players").GetComponent<Text>().text = a.PlayerCount + " / " + a.MaxPlayers;

                newRoomButton.GetComponent<Button>().onClick.AddListener(delegate { JoinRoom(newRoomButton.transform); });
            }

            base.OnRoomListUpdate(roomList);
        }

        public void JoinRoom(Transform p_button)
        {

            Debug.Log("Joining Room @ " + Time.time);
            string t_roomName = p_button.Find("Name").GetComponent<Text>().text;

            RoomInfo roomInfo = null;
            Transform buttonParent = p_button.parent;
            for (int i = 0; i < buttonParent.childCount; i++)
            {
                if (buttonParent.GetChild(i).Equals(p_button))
                {
                    roomInfo = roomList[i];
                    break;
                }
            }

            if (roomInfo != null)
            { 
                PhotonNetwork.JoinRoom(t_roomName);
            }
        }

        public void StartGame ()
      {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                PhotonNetwork.LoadLevel(1);
            }
      }
    
    }
}
    