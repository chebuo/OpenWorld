using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class DeviceManager : MonoBehaviour
{
    [SerializeField] private int minPlayer = 2;
    [SerializeField] private int maxPlayer = 4;

    private Dictionary<int, PlayerJoinData> playerDevices = new();

    private IDisposable buttonPressSubscription;

    private readonly Subject<int> playerStateChangedSubject = new();
    private readonly Subject<int> allReadySubject = new();

    public Observable<int> OnPlayerStateChanged =>playerStateChangedSubject;
    public Observable<int> OnAllReady=>allReadySubject;

    private void OnEnable()
    {
        buttonPressSubscription =InputSystem.onAnyButtonPress.Call(OnButtonPressed);
    }

    private void OnDisable()
    {
        buttonPressSubscription?.Dispose();
        buttonPressSubscription = null;
    }

    private void OnButtonPressed(InputControl control)
    {
        Debug.Log($"Button pressed: {control.name} on device: {control.device.displayName}");
        InputDevice device = control.device;

        if (device is not Keyboard && device is not Gamepad)return;

        HandleInput(device, control);
    }

    private void HandleInput(InputDevice device,InputControl control)
    {
        // まだ参加していない
        if (!IsJoined(device))
        {
            if(IsReadyInput(device, control))
            JoinPlayer(device);
            return;
        }

        int playerIndex = GetPlayerIndex(device);

        if (playerIndex < 0)return;

        // Cancel / Leave
        if (IsCancelInput(device, control))
        {
            LeavePlayer(playerIndex);
        }

        // Ready
        if (IsReadyInput(device, control))
        {
            ToggleReady(playerIndex);
        }
    }

    private bool IsReadyInput(InputDevice device,InputControl control)
    {
        if (device is Gamepad gamepad)
        {
            return control == gamepad.buttonSouth;
        }

        if (device is Keyboard keyboard)
        {
            Debug.Log($"Keyboard input detected: {control.name}");
            return control == keyboard.enterKey;
        }
        return false;
    }

    private bool IsCancelInput(InputDevice device,InputControl control)
    {
        if (device is Gamepad gamepad)
        {
            return control == gamepad.buttonEast;
        }

        if (device is Keyboard keyboard)
        {
            return control == keyboard.escapeKey;
        }
        return false;
    }

    private void JoinPlayer(InputDevice device)
    {
        if (playerDevices.Count >= maxPlayer)return;

        int playerIndex = GetEmptyPlayerIndex();

        if (playerIndex < 0)return;

        playerDevices.Add(playerIndex,new PlayerJoinData(device));

        playerStateChangedSubject.OnNext(playerIndex);

        Debug.Log($"{playerIndex + 1}P joined : {device.displayName}");
    }

    private void ToggleReady(int playerIndex)
    {
        PlayerJoinData player = playerDevices[playerIndex];

        if (player.JoinState == PlayerJoinState.Joined)
        {
            player.JoinState = PlayerJoinState.Ready;
        }
        else if (player.JoinState == PlayerJoinState.Ready)
        {
            player.JoinState = PlayerJoinState.Joined;
        }
        playerStateChangedSubject.OnNext(playerIndex);

        Debug.Log($"{playerIndex + 1}P:{player.JoinState}");

        CheckAllReady();
    }

    private void LeavePlayer(int playerIndex)
    {
        Debug.Log($"Player {playerIndex + 1} is leaving.");
        if (!playerDevices.ContainsKey(playerIndex))return;

        playerDevices.Remove(playerIndex);

        playerStateChangedSubject.OnNext(playerIndex);

        Debug.Log($"{playerIndex + 1}P left");
        CheckAllReady();
    }

    private bool IsJoined(InputDevice device)
    {
        foreach (PlayerJoinData player in playerDevices.Values)
        {
            if (player.Device == device)
                return true;
        }
        return false;
    }

    private void CheckAllReady()
    {
        if(playerDevices.Count<minPlayer)return;
        if(playerDevices.Count==0)return;
        foreach(PlayerJoinData player in playerDevices.Values)
        {
            if(player.JoinState!=PlayerJoinState.Ready)return;
        }
        Debug.Log("All players are ready!");
        allReadySubject.OnNext(0);
    }

    public bool IsAllReady()
    {
        if(playerDevices.Count<minPlayer)return false;

        foreach(PlayerJoinData player in playerDevices.Values)
        {
            if(player.JoinState!=PlayerJoinState.Ready)return false;
        }
        return true;
    }

    private int GetPlayerIndex(InputDevice device)
    {
        foreach (var pair in playerDevices)
        {
            if (pair.Value.Device == device)
                return pair.Key;
        }
        return -1;
    }

    private int GetEmptyPlayerIndex()
    {
        for (int i = 0; i < maxPlayer; i++)
        {
            if (!playerDevices.ContainsKey(i))
                return i;
        }
        return -1;
    }

    public InputDevice GetDevice(int playerIndex)
    {
        if (playerDevices.TryGetValue(
            playerIndex,
            out PlayerJoinData playerData))
        {
            return playerData.Device;
        }
        return null;
    }

    public int GetPlayerCount()
    {
        return playerDevices.Count;
    }

    public PlayerJoinState GetPlayerState(int playerIndex)
    {
        if (playerDevices.TryGetValue(playerIndex,out PlayerJoinData playerData))
        {
            return playerData.JoinState;
        }
        return PlayerJoinState.NoJoined;
    }

    public Dictionary<int,PlayerJoinData> GetPlayerDevices()
    {
        return new Dictionary<int, PlayerJoinData>(playerDevices);
    }
}