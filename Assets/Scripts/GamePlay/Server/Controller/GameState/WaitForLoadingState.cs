using System.Collections.Generic;
using ExitGames.Client.Photon;
using GamePlay.Server.Model;
using Photon.Pun;
using Photon.Realtime;
using GamePlay.Server.Model.Events;
using UnityEngine;


namespace GamePlay.Server.Controller.GameState
{
    /// <summary>
    /// When server is in this state, the server waits for ReadinessMessage from every player.
    /// When the server gets enough ReadinessMessages, the server transfers to GamePrepareState.
    /// Otherwise the server will resend the messages to not-responding clients until get enough responds or time out.
    /// When time out, the server transfers to GameAbortState.
    /// </summary>
    public class WaitForLoadingState : ServerState, IOnEventCallback
    {
        public int TotalPlayers;
        private ISet<int> responds;
        private float firstTime;
        public float serverTimeOut;

        public override void OnServerStateEnter()
        {
            PhotonNetwork.AddCallbackTarget(this);
            responds = new HashSet<int>();
            firstTime = Time.time;
            serverTimeOut = ServerConstants.ServerWaitForLoadingTimeOut;
        }

        public override void OnServerStateExit()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        // --- MODIFIED METHOD ---
        public override void OnStateUpdate()
        {
            // The Master Client is PlayerCount = 1.
            // We are waiting for all *other* human clients.
            int expectedHumanClients = PhotonNetwork.CurrentRoom.PlayerCount - 1;

            if (responds.Count == expectedHumanClients)
            {
                Debug.Log($"All {responds.Count} human clients are ready, game start");
                ServerBehaviour.Instance.GamePrepare();
                return;
            }

            if (Time.time - firstTime > serverTimeOut)
            {
                Debug.Log($"Time out: Expected {expectedHumanClients} responses but only got {responds.Count}");
                ServerBehaviour.Instance.GameAbort();
                return;
            }
        }

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == EventMessages.LoadCompleteEvent)
            {
                Debug.Log($"Received event code: {photonEvent.Code} with content {photonEvent.CustomData}");
                responds.Add((int)photonEvent.CustomData);
            }
        }
    }
}