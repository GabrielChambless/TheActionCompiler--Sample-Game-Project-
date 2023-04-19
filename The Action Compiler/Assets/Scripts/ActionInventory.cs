using System;

public static class ActionInventory     // testing pulling actions from a separate non-MonoBehaviour script
{
    public static Action<string> Left;      
    public static Action<string> Right;     
    public static Action<string> Jump;

    public static Action<string> Shoot;     
    public static Action<string> Grenade;
    public static Action<string> Reload;
    public static Action<string> Pickup;
}
