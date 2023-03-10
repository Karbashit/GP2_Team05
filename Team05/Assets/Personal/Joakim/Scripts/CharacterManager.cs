using System;
using System.Collections.Generic;
using Andreas.Scripts;
using FlowFieldSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterManager : MonoBehaviour
{
    public bool rangedLockedIn, meleeLockedIn;
    public GameObject characterSelectionScreen;
    public GameObject mainGameUI, playerHUDui;
    public List<Player> Players = new List<Player>();


    [SerializeField] private Transform _spawnPoint1;
    [SerializeField] private Transform _spawnPoint2;

    public GameObject selectionObj;
    public GameObject camerToDisable;


    // public static CharacterManager Instance = null;

    private void Awake()
    {
        // if (Instance == null)
        // Instance = this;
        // else if (Instance != this)
        // Destroy(gameObject);
    }

    public bool IsAllLockedIn()
    {
        return rangedLockedIn && meleeLockedIn;
    }
    
    public bool CheckIfAllAreLockedIn() {
        bool allLockedIn = rangedLockedIn && meleeLockedIn;
        if(rangedLockedIn && meleeLockedIn)
        {
            Destroy(camerToDisable);
            Destroy(selectionObj);
            characterSelectionScreen.SetActive(false);
            mainGameUI.SetActive(true);
            playerHUDui.SetActive(true);

            foreach(var player in Players)
            {
                player.GetComponent<PlayerInput>().actions.Enable();
                player.GetComponent<PlayerInput>().SwitchCurrentActionMap("Player");
                player.AssignPlayerToRole(player.cType);
                player.HealthMaterial = Resources.Load<Material>("Player" + player._playerNumber + "Health");
                player.Health.ResetHealth(player);

                switch (player.cType) {
                    case Player.CharacterType.Melee:
                        player.HealthMaterial.SetColor("_CharacterDependentColor", new Color(101/255f, 183/255f, 19/255f));
                        break;
                    case Player.CharacterType.Ranged:
                        player.HealthMaterial.SetColor("_CharacterDependentColor", new Color(53/255f, 93/255f, 123/255f));
                        break;
                }

                if(player._playerNumber == 1)
                {
                    player.otherPlayer = GameObject.FindWithTag("Player2");
                    player.transform.position = _spawnPoint1.position;
                }
                else if(player._playerNumber == 2)
                {
                    player.otherPlayer = GameObject.FindWithTag("Player1");
                    player.transform.position = _spawnPoint2.position;
                }

                GameManager.Instance.CameraController.SetTransforms(player.transform);
                var spawner = GameManager.Instance.EnemyManager.Spawner;
                spawner.EnableSpawning(false);
                // spawner.AssignFlowField(player.GetComponentInChildren<FlowFieldManager>());
            }

            var player1 = Players[0];
            var player2 = Players[1];

            player1.GetComponent<PlayerBounds>().LinkWithOther(player2.GetComponent<Rigidbody>());
            player2.GetComponent<PlayerBounds>().LinkWithOther(player1.GetComponent<Rigidbody>());

            var p1Con = Players[0].transform.Find("TetherConnector").gameObject;
            var hinge = p1Con.GetComponent<HingeJoint>();
            Destroy(hinge);
            var p2Con = Players[1].transform.Find("TetherConnector").gameObject;

            var dollyManager = GameManager.Instance.DollyManager;
            if(dollyManager != null)
            {
                dollyManager.AssignTargets(Players[0].gameObject, Players[1].gameObject);
            }

            var ropeManager = GameManager.Instance.RopeManager;
            if(ropeManager != null)
            {
                ropeManager.SetRopeEnds(p1Con, p2Con);
            }
        }
        GameManager.Instance.PlayerHudUi.SetUi();

        return allLockedIn;
    }


    public void GetPlayerType(int player) {
        
    }

    public void RespawnPlayers()
    {
        var cm = GameManager.Instance.CheckpointManager;
        var respawnPoints = cm.GetCurrentSpawnPositions();
        for(int i = 0; i < Players.Count; i++)
        {
            var respawnPoint = respawnPoints[i];
            var player = Players[i];
            player.transform.position = respawnPoint;
            player.Health.ResetHealth(player);
        }
    }
}