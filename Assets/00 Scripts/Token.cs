using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Token : MonoBehaviour
{
    PlayerData player;
    PlayerTokensManager manager;

    [Header("Components")]
    [SerializeField] SpriteRenderer tokenVisual;
    [SerializeField] SpriteRenderer shadowVisual;

    //Properties
    public PlayerData Player => player;

    public void Setup(PlayerData playerData, PlayerTokensManager tokenManager)
    {
        player = playerData;
        tokenVisual.color = player.Color;
        manager = tokenManager;
    }

    public void DestroyToken()
    {
        manager.HandleTokenDestroyed(this);

        Destroy(gameObject);
        //Do animation;
    }
}