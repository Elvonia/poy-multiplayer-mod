namespace Multiplayer.Steam
{
    public enum PacketType : byte
    {
        None,
        PlayerColorUpdate,
        PlayerPositionUpdate,
        PlayerSceneRequest,
        PlayerSceneUpdate,
        PlayerSitUpdate,
        PlayerSummitedUpdate
    }
}