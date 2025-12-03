/*
using System.Collections;
using System.Collections.Generic;
using Mahjong.Model;
using Managers;
using Photon.Pun;
using UI.DataBinding;
using UnityEngine;
using UnityEngine.UI;

namespace PUNLobby
{
    public class CreateRoomPanel : MonoBehaviour
    {
        public InputField roomNameInputField;
        public RectTransform baseSettingPanel;
        public RectTransform yakuSettingPanel;
        private readonly List<UIBinder> binders = new List<UIBinder>();
        private ResourceManager manager;
        private GameSetting gameSettings;
        private string roomName;
        private void OnEnable()
        {
            binders.Clear();
            binders.AddRange(GetComponentsInChildren<UIBinder>(true));
            // load settings or load preset
            manager = ResourceManager.Instance;
            LoadSettings();
            binders.ForEach(b => b.Target = gameSettings);
            binders.ForEach(b => b?.ApplyBinds());
            roomName = $"{PhotonNetwork.NickName}'s Room";
            roomNameInputField.text = roomName;
        }

        private void LoadSettings()
        {
            Debug.Log("Loading last settings...");
            manager.LoadSettings(out gameSettings);
        }

        public void ResetSettings()
        {
            Debug.Log("Reset to corresponding default settings");
            manager.ResetSettings(gameSettings);
            binders.ForEach(binder => binder?.ApplyBinds());
        }

        public void OpenYakuSettingPanel()
        {
            baseSettingPanel.gameObject.SetActive(false);
            yakuSettingPanel.gameObject.SetActive(true);
            binders.ForEach(b => b.Target = gameSettings);
            binders.ForEach(b => b?.ApplyBinds());
        }

        public void CloseYakuSettingPanel()
        {
            baseSettingPanel.gameObject.SetActive(true);
            yakuSettingPanel.gameObject.SetActive(false);
            binders.ForEach(b => b.Target = gameSettings);
            binders.ForEach(b => b?.ApplyBinds());
        }

        public void SetRoomName(string value)
        {
            roomName = value;
        }

        public void CreateRoom()
        {
            Launcher.Instance.CreateRoom(roomName, gameSettings);
            gameObject.SetActive(false);
        }

        public void BackToLobby()
        {
            Debug.Log("Back to lobby");
            gameObject.SetActive(false);
        }

        public void OnTotalPlayerChanged(int value)
        {
            var players = (GamePlayers)value;
            ResetSettings();
        }
    }
}
*/

using System.Collections;
using System.Collections.Generic;
using Mahjong.Model;
using Managers;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace PUNLobby
{
    public class CreateRoomPanel : MonoBehaviour
    {
        [Header("UI References")]
        public InputField roomNameInputField;

        [Header("Navegación")]
        public GameObject panelSiguiente; // <--- ARRASTRA AQUÍ TU PANEL DE LOBBY/SALA

        [Header("Nuevos Selectores")]
        public ControllerPlayer  selectorLargo;  // row_length
        public ControllerPlayer  selectorTiempo; // row_time

        // Variables internas
        private GameSetting gameSettings;
        private string roomName;

        private void OnEnable()
        {
            roomName = $"{PhotonNetwork.NickName}'s Room";
            if (roomNameInputField != null) roomNameInputField.text = roomName;
            gameSettings = new GameSetting();
        }

        // ESTA ES LA FUNCIÓN QUE DEBE LLAMAR TU BOTÓN NARANJA
        public void CreateRoom()
        {
            // 1. Resetear configuración
            gameSettings = new GameSetting();
            gameSettings.GamePlayers = GamePlayers.Four; // Fijo a 4 jugadores

            // 2. Leer botones visuales
            ConfigurarJuegoDesdeUI();

            Debug.Log($"Creando sala... Rondas: {gameSettings.RoundCount}, Tiempo Base: {gameSettings.BaseTurnTime}");

            // 3. Lógica de Photon (Crear la sala online)
            Launcher.Instance.CreateRoom(roomName, gameSettings);

            // 4. CAMBIO DE PANTALLA VISUAL
            if (panelSiguiente != null)
            {
                panelSiguiente.SetActive(true); // Abre el lobby
                this.gameObject.SetActive(false); // Cierra este panel
            }
            else
            {
                Debug.LogWarning("¡Ojo! No has asignado el 'Panel Siguiente' en el inspector.");
            }
        }

        private void ConfigurarJuegoDesdeUI()
        {
            // --- DURACIÓN ---
            if (selectorLargo != null)
            {
                string seleccion = selectorLargo.opcionElegida;
                switch (seleccion)
                {
                    case "GLength": gameSettings.RoundCount = RoundCount.E; break;
                    case "GLengthSecond": gameSettings.RoundCount = RoundCount.ES; break;
                    case "GLengthThird": gameSettings.RoundCount = RoundCount.FULL; break;
                    default: gameSettings.RoundCount = RoundCount.E; break;
                }
            }

            // --- TIEMPOS ---
            if (selectorTiempo != null)
            {
                string seleccion = selectorTiempo.opcionElegida;
                switch (seleccion)
                {
                    case "TTFirst": // 5+20s
                        gameSettings.BaseTurnTime = 5;
                        gameSettings.BonusTurnTime = 20;
                        break;
                    case "TTSecond": // 20+60s
                        gameSettings.BaseTurnTime = 20;
                        gameSettings.BonusTurnTime = 60;
                        break;
                    case "TTThird": // 280+20s
                        gameSettings.BaseTurnTime = 280;
                        gameSettings.BonusTurnTime = 20;
                        break;
                }
            }
        }

        public void BackToLobby()
        {
            gameObject.SetActive(false);
        }
    }
}