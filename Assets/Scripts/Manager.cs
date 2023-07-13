using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace Con.IgorGuriev.FPSMultiplayer
{
    public enum GameState
    {
        Waiting = 0,
        Starting = 1,
        Playing = 2,
        Ending = 3
    }


    public class Manager : MonoBehaviourPunCallbacks
    {
        public int mainmenu = 0;
        public int killcount = 3;
        public bool perpetual = false;

        public string player_prefab;
        public Transform[] spawn_points;

        private Transform ui_endgame;

        private GameState state = GameState.Waiting;

        public enum EventCodes : byte
        {
            NewPlayer,
            UpdatePlayers,
            ChangeStat,
            NewMatch
        }

        private void Start()
        {
            ValidateConnection();
            InitializeUI();
            Spawn();
        }

        private void Update()
        {
            if (state == GameState.Ending)
            {
                return;
            }
        }

        public override void OnLeftRoom ()
        {
            base.OnLeftRoom();
            SceneManager.LoadScene(mainmenu);
        }

        public void Spawn ()
        {
            Transform t_spawn = spawn_points[Random.Range(0, spawn_points.Length)];
            PhotonNetwork.Instantiate(player_prefab, t_spawn.position, t_spawn.rotation);
        }
        
        private void InitializeUI ()
        {
            ui_endgame = GameObject.Find("Canvas").transform.Find("EndGame").transform;
        }

        private void ValidateConnection ()
        {
            if (PhotonNetwork.IsConnected) return;
            SceneManager.LoadScene(mainmenu);
        }

        private void StateCheck ()
        {
            if (state == GameState.Ending)
            {
                EndGame();
            }
        }

        private void EndGame()
        {
            state = GameState.Ending;

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.DestroyAll();

                if (!perpetual)
                {
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                }
            }

            ui_endgame.gameObject.SetActive(true);

            StartCoroutine(End(6f));
        }

        public void NewMatch_S ()
        {
            PhotonNetwork.RaiseEvent(
                (byte)EventCodes.NewMatch,
                null,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                new SendOptions { Reliability = true }
            );
        }

        public void NewMatch_R ()
        {
            // set game state to waiting
            state = GameState.Waiting;

            // hide end game ui
            ui_endgame.gameObject.SetActive(false);

            // spawn
            Spawn();
        }

        private IEnumerator End (float p_wait)
        {
            yield return new WaitForSeconds(p_wait);

            if(perpetual)
            {
                if(PhotonNetwork.IsMasterClient)
                {
                    NewMatch_S();
                }
            }
            else
            {
                PhotonNetwork.AutomaticallySyncScene = false;
                PhotonNetwork.LeaveRoom();
            }
        }
    }
}

