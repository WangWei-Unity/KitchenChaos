using System;
using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjectArray;

    void Start()
    {
        if (Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedCounterChange += Player_OnSelectedCounterChanged;
        }
        else
        {
            Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        }
    }

    private void Player_OnAnyPlayerSpawned(object sender, EventArgs e)
    {
        if (Player.LocalInstance != null)
        {
            //避免重复
            Player.LocalInstance.OnSelectedCounterChange -= Player_OnSelectedCounterChanged;
            Player.LocalInstance.OnSelectedCounterChange += Player_OnSelectedCounterChanged;
        }
    }

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangeEventArgs e)
    {
        if (e.selectedCounter == baseCounter)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        foreach(GameObject visualGameObject in visualGameObjectArray)
        {
            if (visualGameObject != null)
            {
                visualGameObject.SetActive(true);
            }
        }
    }

    private void Hide()
    {
        foreach (GameObject visualGameObject in visualGameObjectArray)
        {
            if (visualGameObject != null)
            {
                visualGameObject.SetActive(false);
            }
        }
    }

    void OnDestroy()
    {
        if (Player.LocalInstance != null)
        {
            Player.LocalInstance.OnSelectedCounterChange -= Player_OnSelectedCounterChanged;
        }

        Player.OnAnyPlayerSpawned -= Player_OnAnyPlayerSpawned;
    }
}
