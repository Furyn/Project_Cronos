using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpdatesPlayersData : MonoBehaviour
{
    public GameObject playerPrefab;

    [SerializeField]
    private GameObject PlayersNode;

    [SerializeField]
    private List<Player> players = new List<Player>();
    private List<DataPlayer> _tempPlayers;
    private List<int> _playerToDelete = new List<int>();

    private void Update()
    {
        if (NetworkCore.instance.allPlayersUpdated)
        {
            _tempPlayers = new List<DataPlayer>(NetworkCore.instance.allPlayers);
            NetworkCore.instance.allPlayersUpdated = false;

            //If player is not in the current list of player so it's a new player 
            foreach (DataPlayer dataPlayer in _tempPlayers)
            {
                if ( !players.Any(x => x.GetId() == dataPlayer.id) )
                {
                    //Instanciate player
                    GameObject go = Instantiate(playerPrefab, new Vector3(0,0,0) , Quaternion.identity, PlayersNode.transform);
                    Player playerGo = go.GetComponent<Player>();
                    playerGo.name = dataPlayer.name + " #" + dataPlayer.id;
                    playerGo.InitData(dataPlayer);

                    if (NetworkCore.instance.currentPlayer.id == playerGo.GetId())
                    {
                        playerGo.isCurrentPlayer = true;
                        playerGo.SetActiveCamera(true);
                    }

                    players.Add(playerGo);
                }
                //ELSE PLAYER IS IN THE LISTE SO WE UPDATE INFO
                else
                {
                    Player player = players.Find(x => x.GetId() == dataPlayer.id);
                    player.SetPos(dataPlayer.position);
                }
            }

            
            //If player is not in the network list of player so it's a player to delete
            foreach (Player player in players)
            {
                if (!_tempPlayers.Any(x => x.id == player.GetId()))
                {
                    _playerToDelete.Add(player.GetId());
                }
            }

            foreach (int idPlayer in _playerToDelete)
            {
                //Delete player
                Player player = players.Find(x => x.GetId() == idPlayer);
                players.Remove(player);
                Destroy(player.gameObject);
            }

            _tempPlayers.Clear();
            _playerToDelete.Clear();

        }
    }
}
