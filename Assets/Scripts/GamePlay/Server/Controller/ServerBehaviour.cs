using System.Collections.Generic;
using Common.StateMachine;
using Common.StateMachine.Interfaces;
using GamePlay.Server.Controller.GameState;
using GamePlay.Server.Model;
using Mahjong.Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public interface IPlayer
{
    /// <summary>
    /// The unique ID for this player (e.g., 0, 1, 2, or 3)
    /// </summary>
    int PlayerId { get; }

    /// <summary>
    /// The name to display in the UI
    /// </summary>
    string Nickname { get; }

    /// <summary>
    /// Is this player controlled by AI?
    /// </summary>
    bool IsBot { get; }
}

public class LocalHumanPlayer : IPlayer
{
    public int PlayerId { get; private set; }
    public string Nickname { get; private set; }
    public bool IsBot => false; // This is the key difference

    public LocalHumanPlayer(int id, string name)
    {
        PlayerId = id;
        Nickname = name;
    }
}

public class BotPlayer : IPlayer
{
    public int PlayerId { get; private set; }
    public string Nickname { get; private set; }
    public bool IsBot => true; // This is the key difference

    public BotPlayer(int id, string name)
    {
        PlayerId = id;
        Nickname = name;
    }
}

namespace GamePlay.Server.Controller
{
    /// <summary>
    /// This class only takes effect on server
    /// </summary>
    public class ServerBehaviour : MonoBehaviour
    {
        public GameSetting LocalGameSettings; // Assign this in the Unity Inspector
        public SceneField lobbyScene;
        [HideInInspector] public GameSetting GameSettings;
        public IStateMachine StateMachine { get; private set; }
        private MahjongSet mahjongSet;
        public ServerRoundStatus CurrentRoundStatus = null;
        public static ServerBehaviour Instance { get; private set; }

        private void OnEnable()
        {
            Debug.Log("[Server] ServerBehaviour.OnEnable() is called");
            Instance = this;
            StateMachine = new StateMachine();
            ReadSetting();
            WaitForOthersLoading();
        }

        private void Start()
        {
            // You can completely remove the Start() method, 
            // as its only purpose was to destroy non-master-client instances.
        }

        private void Update()
        {
            StateMachine.UpdateState();
        }

        private void ReadSetting()
        {
            GameSettings = LocalGameSettings;
            if (GameSettings == null)
            {
                Debug.LogError("LocalGameSettings is not assigned in the Inspector!");
            }
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
            // You must create a new method to generate your local player + bot list
            var localPlayerList = CreateLocalPlayerList();

            // The ServerRoundStatus constructor will also need to be changed
            // to accept your new local player list instead of Photon.Player[]
            CurrentRoundStatus = new ServerRoundStatus(GameSettings, localPlayerList);
            mahjongSet = new MahjongSet(GameSettings, GameSettings.GetAllTiles());
            var prepareState = new GamePrepareState
            {
                CurrentRoundStatus = CurrentRoundStatus,
            };
            StateMachine.ChangeState(prepareState);
        }
        private IPlayer[] CreateLocalPlayerList()
        {
            IPlayer[] players = new IPlayer[4];

            // Player 0 is the human
            players[0] = new LocalHumanPlayer(0, "Player 1");

            // Players 1, 2, and 3 are bots
            players[1] = new BotPlayer(1, "Bot 1");
            players[2] = new BotPlayer(2, "Bot 2");
            players[3] = new BotPlayer(3, "Bot 3");

            return players;
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
