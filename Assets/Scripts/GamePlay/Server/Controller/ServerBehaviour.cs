using System.Collections.Generic;
using Common.StateMachine;
using Common.StateMachine.Interfaces;
using GamePlay.Server.Controller.GameState;
using GamePlay.Server.Model;
using Mahjong.Model;
using Photon.Pun;
using PUNLobby;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using System.Linq; // <-- Add this
using Photon.Realtime; // <-- Add this
using ExitGames.Client.Photon; // <-- Add this

namespace GamePlay.Server.Controller
{
    /// <summary>
    /// This class only takes effect on server
    /// </summary>
    public class ServerBehaviour : MonoBehaviourPunCallbacks
    {
        public SceneField lobbyScene;
        [HideInInspector] public GameSetting GameSettings;
        public IStateMachine StateMachine { get; private set; }
        private MahjongSet mahjongSet;
        public ServerRoundStatus CurrentRoundStatus = null;
        public static ServerBehaviour Instance { get; private set; }

        // --- NEW VARIABLE ---
        private int botCount; // We will read this from room properties

        private void OnEnable()
        {
            base.OnEnable(); // Call the base method
            Debug.Log("[Server] ServerBehaviour.OnEnable() is called");
            if (!PhotonNetwork.IsConnected)
            {
                SceneManager.LoadScene(lobbyScene);
                return;
            }
            if (!PhotonNetwork.IsMasterClient) return;
                InitializeServerLogic();
        }

        /// <summary>
        /// This method contains the core logic to start the server's state machine.
        /// It's called by OnEnable() for the initial Master Client,
        /// and by OnMasterClientSwitched() for a new Master Client.
        /// </summary>
        private void InitializeServerLogic()
        {
            Debug.Log("[Server] Initializing Server Logic...");
            Instance = this;
            StateMachine = new StateMachine();
            ReadSetting();
            WaitForOthersLoading();
        }

        // --- NEW METHOD ---
        /// <summary>
        // This is the callback that Photon automatically runs on ALL clients
        // when the Master Client disconnects and a new one is assigned.
        /// </summary>
        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (newMasterClient == PhotonNetwork.LocalPlayer)
            {
                // I AM THE NEW MASTER CLIENT!
                Debug.LogWarning("[Server] Master Client switched! I am the new Master.");

                // I must take over the server responsibilities,
                // including running the AI for the bots.
                InitializeServerLogic();
            }
        }

        private void Start()
        {
            if (!PhotonNetwork.IsMasterClient)
                Destroy(gameObject);
        }

        private void Update()
        {
            StateMachine.UpdateState();
        }

        private void ReadSetting()
        {
            var room = PhotonNetwork.CurrentRoom;
            // var setting = (string)room.CustomProperties[SettingKeys.SETTING];
            // GameSettings = JsonUtility.FromJson<GameSetting>(setting);
            GameSettings = (GameSetting)room.CustomProperties[SettingKeys.SETTING];
            // Add this line to read the bot count we saved in the lobby
            botCount = (int)room.CustomProperties[SettingKeys.BOT_COUNT];

            Debug.Log($"[Server] Settings read. Bot count: {botCount}");
        }

        private void WaitForOthersLoading()
        {
            var waitingState = new WaitForLoadingState
            {
                TotalPlayers = GameSettings.MaxPlayer
            };
            StateMachine.ChangeState(waitingState);
        }

        public void GamePrepare()
        {
            // 1. Get the list of human players
            var humanPlayers = PhotonNetwork.PlayerList.ToList();

            // 2. Create the full list of participant names
            List<string> participantNames = new List<string>();
            foreach (var player in humanPlayers)
            {
                participantNames.Add(player.NickName);
            }

            // 3. Add bot names to the list
            for (int i = 0; i < botCount; i++)
            {
                // You can use any naming convention. "Bot 1", "Bot 2", etc. is simple.
                participantNames.Add($"Bot {i + 1}");
            }

            // 4. Convert to an array. This is our official "SeatOrder".
            string[] seatOrder = participantNames.ToArray();

            // (Optional: You could shuffle the 'seatOrder' list here if you want
            // random seating positions for humans and bots)

            // 5. Save this official order to the room properties
            // This is good practice so all clients can see the same list.
            var roomProps = new Hashtable
            {
                { SettingKeys.SEAT_ORDER, seatOrder }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
            Debug.Log($"[Server] Final seat order set: {string.Join(", ", seatOrder)}");

            // 6. !--- THIS IS THE CRITICAL CHANGE ---!
            // Your old code passed `PhotonNetwork.PlayerList`. We can't do that anymore
            // because it only contains humans.
            //
            // You MUST modify your `ServerRoundStatus` class to accept this new
            // `seatOrder` and the `humanPlayers` list.

            // The old code:
            // CurrentRoundStatus = new ServerRoundStatus(GameSettings, PhotonNetwork.PlayerList);

            // The NEW code (example):
            CurrentRoundStatus = new ServerRoundStatus(GameSettings, humanPlayers, seatOrder);

            // 7. The rest of the function continues as normal
            mahjongSet = new MahjongSet(GameSettings, GameSettings.GetAllTiles());
            var prepareState = new GamePrepareState
            {
                CurrentRoundStatus = CurrentRoundStatus,
            };
            StateMachine.ChangeState(prepareState);
        }

        public void GameAbort()
        {
            // todo -- implement abort logic here: at least one of the players cannot load into game, back to lobby scene
            Debug.LogError("The game aborted, this part is still under construction");
        }

        public void RoundStart(bool next, bool extra, bool keepSticks)
        {
            var startState = new RoundStartState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                MahjongSet = mahjongSet,
                NextRound = next,
                ExtraRound = extra,
                KeepSticks = keepSticks
            };
            StateMachine.ChangeState(startState);
        }

        public void DrawTile(int playerIndex, bool isLingShang = false, bool turnDoraAfterDiscard = false)
        {
            var drawState = new PlayerDrawTileState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = playerIndex,
                MahjongSet = mahjongSet,
                IsLingShang = isLingShang,
                TurnDoraAfterDiscard = turnDoraAfterDiscard
            };
            StateMachine.ChangeState(drawState);
        }

        public void DiscardTile(int playerIndex, Tile tile, bool isRichiing, bool discardLastDraw, int bonusTurnTime, bool turnDoraAfterDiscard)
        {
            var discardState = new PlayerDiscardTileState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = playerIndex,
                DiscardTile = tile,
                IsRichiing = isRichiing,
                DiscardLastDraw = discardLastDraw,
                BonusTurnTime = bonusTurnTime,
                MahjongSet = mahjongSet,
                TurnDoraAfterDiscard = turnDoraAfterDiscard
            };
            StateMachine.ChangeState(discardState);
        }

        public void TurnEnd(int playerIndex, Tile discardingTile, bool isRichiing, OutTurnOperation[] operations,
            bool isRobKong, bool turnDoraAfterDiscard)
        {
            var turnEndState = new TurnEndState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = playerIndex,
                DiscardingTile = discardingTile,
                IsRichiing = isRichiing,
                Operations = operations,
                MahjongSet = mahjongSet,
                IsRobKong = isRobKong,
                TurnDoraAfterDiscard = turnDoraAfterDiscard
            };
            StateMachine.ChangeState(turnEndState);
        }

        public void PerformOutTurnOperation(int newPlayerIndex, OutTurnOperation operation)
        {
            var operationPerformState = new OperationPerformState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = newPlayerIndex,
                DiscardPlayerIndex = CurrentRoundStatus.CurrentPlayerIndex,
                Operation = operation,
                MahjongSet = mahjongSet
            };
            StateMachine.ChangeState(operationPerformState);
        }

        public void Tsumo(int currentPlayerIndex, Tile winningTile, PointInfo pointInfo)
        {
            var tsumoState = new PlayerTsumoState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                TsumoPlayerIndex = currentPlayerIndex,
                WinningTile = winningTile,
                MahjongSet = mahjongSet,
                TsumoPointInfo = pointInfo
            };
            StateMachine.ChangeState(tsumoState);
        }

        public void Rong(int currentPlayerIndex, Tile winningTile, int[] rongPlayerIndices, PointInfo[] rongPointInfos)
        {
            var rongState = new PlayerRongState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = currentPlayerIndex,
                RongPlayerIndices = rongPlayerIndices,
                WinningTile = winningTile,
                MahjongSet = mahjongSet,
                RongPointInfos = rongPointInfos
            };
            StateMachine.ChangeState(rongState);
        }

        public void Kong(int playerIndex, OpenMeld kong)
        {
            var kongState = new PlayerKongState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = playerIndex,
                MahjongSet = mahjongSet,
                Kong = kong
            };
            StateMachine.ChangeState(kongState);
        }

        public void RoundDraw(RoundDrawType type)
        {
            var drawState = new RoundDrawState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                RoundDrawType = type
            };
            StateMachine.ChangeState(drawState);
        }

        public void BeiDora(int playerIndex)
        {
            var beiState = new PlayerBeiDoraState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                CurrentPlayerIndex = playerIndex,
                MahjongSet = mahjongSet
            };
            StateMachine.ChangeState(beiState);
        }

        public void PointTransfer(IList<PointTransfer> transfers, bool next, bool extra, bool keepSticks)
        {
            var transferState = new PointTransferState
            {
                CurrentRoundStatus = CurrentRoundStatus,
                NextRound = next,
                ExtraRound = extra,
                KeepSticks = keepSticks,
                PointTransferList = transfers
            };
            StateMachine.ChangeState(transferState);
        }

        public void GameEnd()
        {
            var gameEndState = new GameEndState
            {
                CurrentRoundStatus = CurrentRoundStatus
            };
            StateMachine.ChangeState(gameEndState);
        }
    }
}
