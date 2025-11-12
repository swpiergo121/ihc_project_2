using System.Collections.Generic;
using System.Linq;
using Mahjong.Model;
using Managers;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using ExitGames.Client.Photon; // <--- ADD THIS LINE

namespace PUNLobby.Room
{
    public class RoomPanelManager : MonoBehaviour
    {
        public Text roomTitleText;
        public RoomSlotPanel[] slots;
        public Button readyButton;
        public Button cancelButton;
        public Button startButton;
        public RulePanel rulePanel;
        public WarningPanel warningPanel;
        private IList<Player> players;

        private void Start()
        {
            CheckButtonForMaster();
        }

        private void Update()
        {
            ShowSlots();
        }

        public void SetTitle(string title)
        {
            roomTitleText.text = title;
        }

        public void SetPlayers(IList<Player> players)
        {
            this.players = players;
        }

        private void ShowSlots()
        {
            int length = 0;
            if (players != null) length = players.Count;
            for (int i = 0; i < length; i++)
            {
                slots[i].gameObject.SetActive(true);
                var player = players[i];
                var ready = player.GetCustomPropertyOrDefault<bool>(SettingKeys.READY, false);
                slots[i].Set(player.IsMasterClient, player.NickName, ready);
            }
            for (int i = length; i < slots.Length; i++)
            {
                slots[i].gameObject.SetActive(false);
            }
        }

        public void CheckButtonForMaster()
        {
            readyButton.interactable = !PhotonNetwork.IsMasterClient;
            startButton.interactable = PhotonNetwork.IsMasterClient;
        }

        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void CheckRule()
        {
            var currentRoom = PhotonNetwork.CurrentRoom;
            var gameSetting = (GameSetting)currentRoom.CustomProperties[SettingKeys.SETTING];
            rulePanel.Show(gameSetting);
        }

        public void OnStartButtonClicked()
        {
            var launcher = RoomLauncher.Instance;
            var room = PhotonNetwork.CurrentRoom;

            // 1. First, check if all HUMAN players are ready
            if (!CheckReadiness())
            {
                Debug.Log("Game cannot start, since some players are not ready");
                warningPanel.Show(400, 200, "Game cannot start, some players are not ready.");
                return;
            }

            // 2. All humans are ready. Now, calculate how many bots to add.
            // This replaces the old "Not enough players" check.
            int botsToSpawn = 0;
            if (room.PlayerCount < room.MaxPlayers)
            {
                botsToSpawn = room.MaxPlayers - room.PlayerCount;
                Debug.Log($"Room is not full. Starting game with {room.PlayerCount} players and {botsToSpawn} bots.");
            }
            else
            {
                Debug.Log($"Room is full. Starting game with {room.PlayerCount} players.");
            }

            // 3. Save the bot count to the Room's Custom Properties.
            // This is the critical step. All clients will receive this update
            // and know how many bots to spawn in the game scene.
            var roomProps = new Hashtable
            {
                { SettingKeys.BOT_COUNT, botsToSpawn } // You'll need to add BOT_COUNT to your SettingKeys class
            };
            room.SetCustomProperties(roomProps);

            // 4. Save settings locally (as before)
            var setting = (GameSetting)room.CustomProperties[SettingKeys.SETTING];
            SaveSettings(setting);

            // 5. Start the game (as before)
            Debug.Log("Game is starting");
            launcher.GameStart();
        }

        private bool CheckReadiness()
        {
            return players.All(p => p.IsMasterClient || p.GetCustomPropertyOrDefault<bool>(SettingKeys.READY, false));
        }

        private void SaveSettings(GameSetting gameSettings)
        {
            Debug.Log($"Save settings: {gameSettings}");
            ResourceManager.Instance.SaveSettings(gameSettings);
        }

        public void OnReadyButtonClicked()
        {
            Debug.Log("Set ready");
            var player = PhotonNetwork.LocalPlayer;
            player.SetCustomProperty(SettingKeys.READY, true);
            readyButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(true);
        }

        public void OnCancelButtonClicked()
        {
            Debug.Log("Cancel ready");
            var player = PhotonNetwork.LocalPlayer;
            player.SetCustomProperty(SettingKeys.READY, false);
            readyButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(false);
        }
    }
}
