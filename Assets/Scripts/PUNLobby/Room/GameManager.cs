using System.Collections.Generic;
using System.Linq;
using Mahjong.Model;
using Managers;
using Photon.Pun;
using Photon.Realtime;
using PUNLobby;
using UnityEngine;
using Utils; // Make sure you have this for SettingKeys

public class GameManager : MonoBehaviourPunCallbacks
{
    // Assign your Bot Prefab in the Inspector
    public GameObject botPrefab;

    // A list to hold all participants (humans and bots)
    // You will need a simple Bot class/struct or a string to represent them
    // For now, let's just use Player and object
    private List<Player> humanPlayers;
    private int botCount;

    // This is called when the scene loads
    private void Start()
    {
        // ALL clients need to get the latest room info
        humanPlayers = PhotonNetwork.PlayerList.ToList();
        botCount = (int)PhotonNetwork.CurrentRoom.CustomProperties[SettingKeys.BOT_COUNT];

        // But ONLY the Master Client does the setup.
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log($"I am the Master Client. Setting up game with {humanPlayers.Count} humans and {botCount} bots.");
            SetupGame();
        }
        else
        {
            Debug.Log("I am a client. Waiting for Master Client to set up the game.");
        }
    }

    private void SetupGame()
    {
        // 1. Get the total player count (e.g., 4)
        int totalPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;

        // 2. Create a "Seat" list. We will fill this.
        // You'll need a way to represent players and bots. 
        // Using Photon's Player object for humans is easy.
        // For bots, we can instantiate them.

        List<Player> allPlayers = new List<Player>(humanPlayers);

        // 3. Instantiate Bots
        // We use PhotonNetwork.Instantiate so they exist on the network.
        // We also "assign" them to the Master Client, who will control their AI.
        for (int i = 0; i < botCount; i++)
        {
            // Instantiate the bot prefab
            GameObject botGameObject = PhotonNetwork.Instantiate(botPrefab.name, Vector3.zero, Quaternion.identity);

            // This is an advanced but common pattern:
            // The bot's Player object on the network doesn't exist,
            // but we can get its PhotonView to identify it.
            // Or, for simplicity, you can just manage them locally on the Master Client.

            // Let's use a simpler approach for now:
            // The Master Client will just *simulate* the bots.
            // We need to tell all clients what the "full" player list is.
        }

        // 4. Determine final turn order. This is the most important part.
        // We'll create an array of Actor Numbers (or Nicknames) in seat order.

        List<string> seatNicknames = new List<string>();
        foreach (var player in humanPlayers)
        {
            seatNicknames.Add(player.NickName);
        }
        for (int i = 0; i < botCount; i++)
        {
            seatNicknames.Add($"Bot {i + 1}"); // Add bot names
        }

        // This is just a simple list. You'd want to shuffle this
        // or assign seats properly (e.g., Master is always East).
        // For example:
        // string[] seatOrder = new string[4] { "Player1", "Bot 1", "Player2", "Bot 2" };

        // 5. Save this new "seat order" to the room properties
        var roomProps = new ExitGames.Client.Photon.Hashtable
        {
            // You'll need to add SEAT_ORDER to SettingKeys
            { SettingKeys.SEAT_ORDER, seatNicknames.ToArray() }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

        // 6. Tell all clients (including ourselves) to officially start
        // We use an RPC (Remote Procedure Call)
        photonView.RPC("Rpc_InitializeGame", RpcTarget.All, seatNicknames.ToArray());
    }

    // This RPC is called on ALL clients (including Master)
    [PunRPC]
    public void Rpc_InitializeGame(string[] seatOrder)
    {
        Debug.Log("Game is starting! Seat order: " + string.Join(", ", seatOrder));

        // Here, every client (Player, Player, Bot, Bot)
        // will build its UI and game logic based on this seatOrder.
        //
        // For example:
        // - Find your own nickname in the list to know your seat.
        // - Set up the UI for the other players and bots.
        // - The Master Client will also know it is responsible for "Bot 1" and "Bot 2".

        // This is where you would call your game's "Start" logic
        // e.g., GameController.Instance.Initialize(seatOrder);
    }

   
}