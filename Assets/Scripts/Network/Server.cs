﻿using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour, INetEventListener
{
    [SerializeField] public UserConfiguration myUserConfig;
    [SerializeField] public ServerDataStorage serverData;
    [SerializeField] public GameObject lobbyPanel;
    
    NetManager netManager;

    /* Initializes the Server
     * 
     */
    public void startServer()
    {
        this.netManager = new NetManager(this);

        this.netManager.Start(this.serverData.serverPort);
        this.netManager.BroadcastReceiveEnabled = true;
        this.netManager.UnconnectedMessagesEnabled = true;
        this.netManager.UpdateTime = 15;
        this.netManager.ReuseAddress = true;

        this.serverData.activeUsers.Add(myUserConfig);
        this.serverData.MyNetworkId = this.myUserConfig.networkId;

        Debug.Log("SERVER STARTED");
        Debug.Log("[SERVER] MY NETWORKID: " + this.myUserConfig.networkId);

        //Mean Light Dir initialized with Server Light direction
        this.serverData.meanLightDir = myUserConfig.getLightDir();
    }

    /* Stops the Server
     * 
     */
    public void StopServer()
    {
        if(this.myUserConfig.role == UserRole.LOBBY_CREATOR)
        {
            if (this.netManager != null)
            {
                this.netManager.DisconnectAll();
                this.netManager.Stop();
            }
            Debug.Log("SERVER Stopped");
        }
    }

    /* Update is called once per frame
     * 
     */
    void Update()
    {
        if (this.netManager != null && this.netManager.IsRunning)
        {
            this.netManager.PollEvents();
            Thread.Sleep(15);
        }
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        if(request.Data != null)
        {
            string reqDataJson = request.Data.GetString();
            if (reqDataJson != null)
            {
                //Deserialize json into UserConfigModel class
                UserConfigModel userConfigModel = JsonUtility.FromJson<UserConfigModel>(reqDataJson);

                //Check if User is already registered -- basically just check wether user is in activeUsers list or passiveUsers list
                if (!NetworkUtils.userAlreadyExists(userConfigModel, this.serverData.activeUsers) && !NetworkUtils.userAlreadyExists(userConfigModel, this.serverData.passiveUsers))
                {
                    if (userConfigModel.isActive && this.serverData.activeUsers.Count <= this.serverData.maxActiveClients)
                    {
                        //Accept Connection and trigger OnPeerConnected for the Client
                        request.Accept();
                        Debug.Log("[SERVER] Client ConnectionRequest ACCEPTED");
                    }
                    else if (!userConfigModel.isActive)
                    {
                        //Accept Connection and trigger OnPeerConnected for the Client
                        request.Accept();
                        Debug.Log("[SERVER] Client ConnectionRequest ACCEPTED");
                    }
                    else
                    {
                        request.Reject();
                        Debug.Log("[SERVER] Client ConnectionRequest REJECTED");
                    }
                }
                else
                {
                    request.Reject();
                    Debug.Log("[SERVER] Client ConnectionRequest REJECTED");
                }
            }
        }
        
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("[Server] Peer has been connected with endpoint : " + peer.EndPoint.ToString());
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        string json = reader.GetString();
        TransMissionContainerModel container = JsonUtility.FromJson<TransMissionContainerModel>(json);

        switch (container.action)
        {
            //Action to add the User
            case Action.REGISTER_USER_CONFIGURATION:
                if(container.dataModel == DataModel.USER_CONFIG_MODEL)
                {
                    UserConfigModel userConfigModel = container.configModel;
                    NetworkUtils.addUser(userConfigModel, this.serverData.activeUsers, this.serverData.passiveUsers); //wird bei ConnectionReq bereits geprüft, ob der User bereits existiert
                    UpdateLobbyAfterUserAdded();
                    Debug.Log("SERVER : Amount Of Passive Users: " + this.serverData.passiveUsers.Count);
                    InformAllClientsUserAdded();

                    if (userConfigModel.isActive)
                    {
                        this.serverData.meanLightDir = NetworkUtils.averageLightDirections(this.serverData.activeUsers);
                        InformAllClientsAverageLightDir();
                    }
                }
                break;
            default:
                Debug.Log("[Server] Action not detected!");
                break;
        }
    }

    /** Informs all clients about the new Averaged LightDirection
     * 
     */
    void InformAllClientsAverageLightDir()
    {
        TransMissionContainerModel transMissionContainerModel = new TransMissionContainerModel(
            Action.INFORM_CLIENTS_ABOUT_MEAN_LIGHT_AVERAGE,
            DataModel.LIGHT_DIRECTION
            );

        transMissionContainerModel.MeanLightDir = this.serverData.meanLightDir;

        string json = JsonUtility.ToJson(transMissionContainerModel);
        NetDataWriter writer = new NetDataWriter();
        writer.Put(json);

        this.netManager.SendToAll(writer, DeliveryMethod.ReliableUnordered);
    }

    /** Informs all clients about about the number of active and passive Users
     * 
     */
    void InformAllClientsUserAdded()
    {
        TransMissionContainerModel transMissionContainerModel = new TransMissionContainerModel(
            Action.INFORM_CLIENTS_ABOUT_AMOUNT_OF_USERS,
            DataModel.NUM_ACTIVE_AND_NUM_PASSIVE_USERS);

        transMissionContainerModel.NumActiveUsers = this.serverData.activeUsers.Count;
        transMissionContainerModel.NumPassiveUsers = this.serverData.passiveUsers.Count;

        string json = JsonUtility.ToJson(transMissionContainerModel);
        NetDataWriter writer = new NetDataWriter();
        writer.Put(json);

        this.netManager.SendToAll(writer, DeliveryMethod.ReliableUnordered);
    }

    /** Just Activates any new Users (Index of these activated Users is the same as the index of the activeUser list)
     * 
     */
    void UpdateLobbyAfterUserAdded()
    {
        for(int i = 0; i < this.serverData.activeUsers.Count; i++)
        {
            Transform t = this.lobbyPanel.transform.GetChild(i);
            if(!t.gameObject.activeInHierarchy)
            {
                this.lobbyPanel.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.Broadcast)
        {
            NetDataWriter resp = new NetDataWriter();
            resp.Put(1);
            this.netManager.SendUnconnectedMessage(resp, remoteEndPoint);
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
}