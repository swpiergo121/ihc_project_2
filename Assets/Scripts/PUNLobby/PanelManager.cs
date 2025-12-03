using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace PUNLobby
{
    public class PanelManager : MonoBehaviour
    {
        public RectTransform LoginPanel;
        public RectTransform LobbyPanel;
        [SerializeField] private RoomListPanel roomListPanel;
        [SerializeField] private CreateRoomPanel createPanel;
        public WarningPanel warningPanel;
        public WarningPanel infoPanel;
        private RectTransform currentPanel;

        public void ChangeTo(RectTransform newPanel)
        {
            if (currentPanel != null)
            {
                currentPanel.gameObject.SetActive(false);
            }
            if (newPanel != null)
            {
                newPanel.gameObject.SetActive(true);
            }
            currentPanel = newPanel;
        }

       /* public void ShowCreateRoomPanel()
        {
            createPanel.gameObject.SetActive(true);
        }
    */
            

        public void SetRoomList(IList<RoomInfo> rooms)
        {
            roomListPanel.SetRoomList(rooms);
        }
        public void ShowCreateRoomPanel()
        {
            // ESTA ES LA LÍNEA QUE DEBES CAMBIAR O AÑADIR:
            // Llama a 'ChangeTo' para ocultar el panel actual y mostrar el de crear sala
            ChangeTo(createPanel.GetComponent<RectTransform>());

            // La línea original probablemente era esta, la cual está mal porque no oculta el panel anterior:
            // createPanel.gameObject.SetActive(true); 
        }
    }
}
