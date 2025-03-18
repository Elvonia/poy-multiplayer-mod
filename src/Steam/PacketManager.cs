using Multiplayer.Logger;
using Steamworks;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Multiplayer.Steam
{
    public class PacketManager
    {
        private void BroadcastPacket(CSteamID lobbyID, byte[] packet, EP2PSend sendType)
        {
            int memberCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);

            for (int i = 0; i < memberCount; i++)
            {
                CSteamID playerSteamID = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);

                if (playerSteamID == SteamUser.GetSteamID())
                    continue;

                SteamNetworking.SendP2PPacket(playerSteamID, packet, (uint)packet.Length, sendType);

                //LogManager.Debug($"Sent packet of size {packet.Length} to {playerSteamID}");
            }
        }

        public byte[] CreateColorUpdatePacket(Player player)
        {
            byte[] colorData = player.GetColorBytes();

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)PacketType.PlayerColorUpdate);
                writer.Write(colorData);

                return stream.ToArray();
            }
        }

        public byte[] CreatePositionUpdatePacket(Player player)
        {
            byte[] positionData = player.GetPositionBytes();

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)PacketType.PlayerPositionUpdate);
                writer.Write(positionData);

                return stream.ToArray();
            }
        }

        public byte[] CreateSceneUpdatePacket(Player player)
        {
            byte[] sceneData = player.GetSceneBytes();

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)PacketType.PlayerSceneUpdate);
                writer.Write(sceneData);

                return stream.ToArray();
            }
        }

        public byte[] CreateSitUpdatePacket(Player player)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)PacketType.PlayerSitUpdate);
                // something, something....do butt stuff o.O
                return stream.ToArray();
            }
        }

        public byte[] CreateSummitedUpdatePacket(Player player)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)PacketType.PlayerSummitedUpdate);
                // @@@
                return stream.ToArray();
            }
        }

        public byte[] CreateNullSceneUpdatePacket()
        {
            int i = -1;

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)PacketType.PlayerSceneUpdate);
                writer.Write(i);

                return stream.ToArray();
            }
        }

        public void ReceivePackets(MultiplayerMod instance)
        {
            uint msgSize;
            while (SteamNetworking.IsP2PPacketAvailable(out msgSize))
            {
                byte[] buffer = new byte[msgSize];
                CSteamID sender;

                if (SteamNetworking.ReadP2PPacket(buffer, msgSize, out uint _bytesRead, out sender))
                {
                    // avoids any potential malformed packets
                    if (buffer.Length == 0) 
                        return;

                    switch (buffer[0])
                    {
                        case (byte)PacketType.PlayerColorUpdate:
                            ReceiveColorUpdate(instance, sender, buffer);

                            LogManager.Debug($"PlayerColorUpdate of size {buffer.Length} received from {sender}");
                            break;

                        case (byte)PacketType.PlayerPositionUpdate:
                            ReceivePositionUpdate(instance, sender, buffer);

                            //LogManager.Debug($"PlayerPositionUpdate of size {buffer.Length} received from {sender}");
                            break;

                        case (byte)PacketType.PlayerSceneUpdate:
                            ReceiveSceneUpdate(instance, sender, buffer);

                            LogManager.Debug($"PlayerSceneUpdate of size {buffer.Length} received from {sender}");
                            break;

                        case (byte)PacketType.PlayerSitUpdate:
                            ReceiveSitUpdate(instance, sender, buffer);

                            LogManager.Debug($"PlayerSitUpdate of size {buffer.Length} received from {sender}");
                            break;

                        case (byte)PacketType.PlayerSummitedUpdate:
                            ReceiveSummitedUpdate(instance, sender, buffer);

                            LogManager.Debug($"PlayerSummitedUpdate of size {buffer.Length} received from {sender}");
                            break;

                    }
                }
            }
        }

        public void ReceiveColorUpdate(MultiplayerMod instance, CSteamID senderID, byte[] packet)
        {
            PlayerClone playerClone = instance.remotePlayers.Find(s => s.GetSteamID() == senderID);

            using (MemoryStream stream = new MemoryStream(packet))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                byte packetType = reader.ReadByte();
                Color color = new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

                if (playerClone != null)
                {
                    playerClone.SetMaterialColor(color);

                    LogManager.Debug($"Applied new clone color from packet to {senderID}");
                }
            }
        }

        public void ReceivePositionUpdate(MultiplayerMod instance, CSteamID senderID, byte[] packet)
        {
            PlayerClone playerClone = instance.remotePlayers.Find(s => s.GetSteamID() == senderID);

            using (MemoryStream stream = new MemoryStream(packet))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                if (packet.Length < 2) 
                    return;

                byte packetType = reader.ReadByte();
                byte[] positionData = reader.ReadBytes(packet.Length - 1);

                if (playerClone.GetPlayer() != null)
                {
                    if (positionData.Length > 0)
                    {
                        playerClone.SetPositionDataFromBytes(positionData);
                        playerClone.UpdateTransforms();

                        //LogManager.Debug($"Applied new clone position from packet to {senderID}");
                    }
                }
            }
        }

        public void ReceiveSceneUpdate(MultiplayerMod instance, CSteamID senderID, byte[] packet)
        {
            PlayerClone playerClone = instance.remotePlayers.Find(s => s.GetSteamID() == senderID);

            using (MemoryStream stream = new MemoryStream(packet))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                if (packet.Length < 5) 
                    return;

                byte packetType = reader.ReadByte();
                int sceneIndex = reader.ReadInt32();

                LogManager.Debug($"{senderID}:{sceneIndex}");

                if (sceneIndex == SceneManager.GetActiveScene().buildIndex)
                {
                    if (sceneIndex == -1)
                        return;

                    playerClone.CreatePlayerGameObject(instance.shadowClone);
                    playerClone.SetSceneIndex(sceneIndex);

                    LogManager.Debug($"Same scene detected for player {senderID}");
                }
                else
                {
                    playerClone.DestroyPlayerGameObject();
                    playerClone.SetSceneIndex(sceneIndex);

                    LogManager.Debug($"Different scene detected for player {senderID}");
                }
            }
        }

        public void ReceiveSitUpdate(MultiplayerMod instance, CSteamID senderID, byte[] packet)
        {
            using (MemoryStream stream = new MemoryStream(packet))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                byte packetType = reader.ReadByte();
                byte[] sitData = reader.ReadBytes(packet.Length - 1);

                // todo
            }
        }

        public void ReceiveSummitedUpdate(MultiplayerMod instance, CSteamID senderID, byte[] packet)
        {
            using (MemoryStream stream = new MemoryStream(packet))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                byte packetType = reader.ReadByte();
                byte[] summitedData = reader.ReadBytes(packet.Length - 1);

                // todo
            }
        }

        // ------------------------------------------------------------------------ \\
        // SteamNetworking sendFlags    |                                           \\
        // -----------------------------|                                           \\
        //                                                                          \\
        // k_EP2PSendReliable               - ensures delivery                      \\
        // k_EP2PSendReliableWithBuffering  - ensures delivery + potential delay    \\
        //                                                                          \\
        // k_EP2PSendUnreliable             - no ensured delivery + potential delay \\
        // k_EP2PSendUnreliableNoDelay      - no ensured delivery + low latency     \\
        // ------------------------------------------------------------------------ \\

        public void SendReliablePacket(CSteamID lobbyID, byte[] packet)
        {
            BroadcastPacket(lobbyID, packet, EP2PSend.k_EP2PSendReliable);
        }

        public void SendUnreliableNoDelayPacket(CSteamID lobbyID, byte[] packet)
        {
            BroadcastPacket(lobbyID, packet, EP2PSend.k_EP2PSendUnreliableNoDelay);
        }
    }
}
