namespace Multiplayer.Steam
{
    public enum PacketType : byte
    {
        None,
        PlayerColorUpdate,
        PlayerPositionUpdate,
        PlayerSceneUpdate,
        PlayerSitUpdate,
        PlayerSummitedUpdate
    }
}