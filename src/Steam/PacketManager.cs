﻿using Steamworks;
using System.IO;
using System.Numerics;
using UnityEngine;

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
                            break;

                        case (byte)PacketType.PlayerPositionUpdate:
                            ReceivePositionUpdate(instance, sender, buffer);
                            break;

                        case (byte)PacketType.PlayerSceneUpdate:
                            ReceiveSceneUpdate(instance, sender, buffer);
                            break;

                        case (byte)PacketType.PlayerSitUpdate:
                            ReceiveSitUpdate(instance, sender, buffer);
                            break;

                        case (byte)PacketType.PlayerSummitedUpdate:
                            ReceiveSummitedUpdate(instance, sender, buffer);
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
                if (instance.player == null) 
                    return;

                if (packet.Length < 5) 
                    return;

                byte packetType = reader.ReadByte();
                int sceneIndex = reader.ReadInt32();

                if (sceneIndex == instance.player.GetSceneIndex())
                {
                    // player is in cabin
                    if (sceneIndex == -1 && playerClone == null)
                    {
                        playerClone = new PlayerClone(senderID, instance.playerShadow);
                        playerClone.SetSceneIndex(sceneIndex);
                        playerClone.DestroyPlayerGameObject();

                        instance.remotePlayers.Add(playerClone);
                        return;
                    }

                    // player is in the same scene
                    if (playerClone == null)
                    {
                        playerClone = new PlayerClone(senderID, instance.playerShadow);
                        playerClone.SetSceneIndex(sceneIndex);

                        instance.remotePlayers.Add(playerClone);
                        return;
                    }
                    else
                    {
                        playerClone.CreatePlayerGameObject(instance.playerShadow);
                        playerClone.SetSceneIndex(sceneIndex);
                        return;
                    }
                }
                else
                {
                    // player is in a different scene
                    if (playerClone != null)
                    {
                        playerClone.DestroyPlayerGameObject();
                        playerClone.SetSceneIndex(sceneIndex);
                        return;
                    }
                    else
                    {
                        playerClone = new PlayerClone(senderID, instance.playerShadow);
                        playerClone.SetSceneIndex(sceneIndex);
                        playerClone.DestroyPlayerGameObject();

                        instance.remotePlayers.Add(playerClone);
                        return;
                    }
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
