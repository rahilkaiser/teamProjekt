﻿using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour, INetEventListener
{
    [SerializeField] public UserConfiguration myUserConfig;
    [SerializeField] int portTpBroadcast = 9999;
    [SerializeField] public List<UserConfiguration> activeUsers;

    NetManager netManager;

    public void StartClient()
    {
        this.netManager = new NetManager(this);
        this.netManager.Start();
        this.netManager.UnconnectedMessagesEnabled = true;
        this.netManager.BroadcastReceiveEnabled = true;
        this.netManager.UpdateTime = 15;

        Debug.Log("CLIENT Started");
        Debug.Log("[Client] MY USER CONFIG: " + this.myUserConfig.networkId);
    }

    public void StopClient()
    {
        if(this.myUserConfig.role != UserRole.LOBBY_CREATOR)
        {
            if (netManager != null)
            {
                this.netManager.Stop();
            }
            Debug.Log("CLIENT Stopped ");
        }
    }

    private void Update()
    {
        if (this.netManager != null && this.netManager.IsRunning)
        {
            this.netManager.PollEvents();
            if(this.netManager.ConnectedPeersCount == 0)
            {
                this.senDiscoveryRequest();
            }
        }
    }

    public void senDiscoveryRequest()
    {
        var peer = this.netManager.FirstPeer;

        if(peer != null && peer.ConnectionState == ConnectionState.Connected)
        {

        } 
        else
        {
            UserConfigModel userConfigModel = NetworkUtils.toUserConfigurationModel(this.myUserConfig);
            string json = JsonUtility.ToJson(userConfigModel);

            NetDataWriter writer = new NetDataWriter();
            writer.Put(json);
            this.netManager.SendBroadcast(writer, this.portTpBroadcast);
        }
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("ON PEER COnnected");
        //Registriere meine UserConfig zum Server
        UserConfigModel userConfigModel = NetworkUtils.toUserConfigurationModel(this.myUserConfig);
        TransMissionContainerModel transMissionContainerModel = new TransMissionContainerModel(
            Action.REGISTER_USER_CONFIGURATION, 
            DataModel.USER_CONFIGURATION, 
            userConfigModel);

        string json = JsonUtility.ToJson(transMissionContainerModel);

        NetDataWriter writer = new NetDataWriter();
        writer.Put(json);

        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        if (this.netManager.ConnectedPeersCount == 0 && reader.GetInt() == 1)
        {
            reader.Clear();
            Debug.Log("[CLIENT] Received discovery response. Connecting to: " + remoteEndPoint);

            Debug.Log("[Client] MY USER CONFIG NET ID: " + this.myUserConfig.networkId);
            Debug.Log("[Client] MY USER CONFIG IS ACTIVE: " + this.myUserConfig.isActive);
            Debug.Log("[Client] MY USER CONFIG Light DIr: " + this.myUserConfig.lightDir);
            Debug.Log("[Client] MY USER CONFIG ROLE: " + this.myUserConfig.role);

            UserConfigModel userConfigModel = NetworkUtils.toUserConfigurationModel(this.myUserConfig);

            userConfigModel.networkId = "1234F";
            string json = JsonUtility.ToJson(userConfigModel);

            NetDataWriter writer = new NetDataWriter(true);
            writer.Reset();
            writer.Put(json);

            this.netManager.Connect(remoteEndPoint,writer);
        }
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {

    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {

    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {

    }

    public void OnConnectionRequest(ConnectionRequest request)
    {

    }
}
