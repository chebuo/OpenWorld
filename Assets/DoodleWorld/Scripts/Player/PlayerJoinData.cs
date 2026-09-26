using UnityEngine.InputSystem;

public class PlayerJoinData
{
    public InputDevice Device{get;private set;}
    public PlayerJoinState JoinState{get; set;}
    public PlayerJoinData(InputDevice device)
    {
        this.Device=device;
        this.JoinState=PlayerJoinState.Joined;
    }
}