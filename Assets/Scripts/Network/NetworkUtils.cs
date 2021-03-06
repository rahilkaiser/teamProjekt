using LiteNetLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NetworkUtils 
{
    public static UserConfiguration toUserConfiguration(UserConfigModel userConfigModel)
    {
        UserConfiguration userConfig = new UserConfiguration(
            userConfigModel.networkId,
            userConfigModel.isActive,
            userConfigModel.lightDir, 
            userConfigModel.role,
            userConfigModel.userPeerInfo,
            userConfigModel.lightColor
            );
        return userConfig;
    }

    public static UserConfigModel toUserConfigurationModel(UserConfiguration userConfig)
    {
        UserConfigModel userConfigModel = new UserConfigModel(
            userConfig.networkId,
            userConfig.isActive,
            userConfig.lightDir,
            userConfig.role,
            userConfig.userPeerInfo,
            userConfig.lightCol
            );

        return userConfigModel;
    }

    public static bool userAlreadyExists(UserConfigModel userConfigModel, List<UserConfiguration> userConfigurations)
    {
        foreach(UserConfiguration uc in userConfigurations)
        {
            if(uc.networkId == userConfigModel.networkId)
            {
                return true;
            }
        }

        return false;
    }

    public static void addUser(UserConfigModel userConfigModel, List<UserConfiguration> activeUsers, List<UserConfiguration> passiveUsers)
    {
        UserConfiguration uc = NetworkUtils.toUserConfiguration(userConfigModel);

        if (uc.isActive)
        {
            activeUsers.Add(uc);
        }
        else
        {
            passiveUsers.Add(uc);
        }
    }

    public static void removeUser(UserConfigModel userConfigModel, List<UserConfiguration> activeUsers, List<UserConfiguration> passiveUsers)
    {
        UserConfiguration uc = NetworkUtils.toUserConfiguration(userConfigModel);

        if (uc.isActive)
        {
            activeUsers.Add(uc);
        }
        else
        {
            passiveUsers.Add(uc);
        }
    }

    /** Returns the mean of all light directions ---> basically 1/n * (v1 + v2 + ... + vn)
     * 
     */
    public static Vector3 averageLightDirections(List<UserConfiguration> activeUsers)
    {
        Vector3 res = Vector3.zero;
        for(int i = 0; i < activeUsers.Count; i++)
        {
            res += activeUsers[i].getLightDir();
        }

        res /= activeUsers.Count;

        return res;
    }
    
    /** Returns the mean of all light colors ---> basically 1/n * (v1 + v2 + ... + vn)
     * 
     */
    public static Color averageLightColors(List<UserConfiguration> activeUsers)
    {
        Color res = Color.black;
        for(int i = 0; i < activeUsers.Count; i++)
        {
            res.r += activeUsers[i].getLightCol().r;
            res.g += activeUsers[i].getLightCol().g;
            res.b += activeUsers[i].getLightCol().b;
        }

        res.r /= activeUsers.Count;
        res.g /= activeUsers.Count;
        res.b /= activeUsers.Count;

        return res;
    }


    
}

public enum Action
{
    REGISTER_USER_CONFIGURATION,
    INFORM_CLIENTS_ABOUT_AMOUNT_OF_USERS,
    INFORM_CLIENTS_ABOUT_MEAN_LIGHT_AVERAGE,
    START_GAME,
    MAKE_MOVE,
    RESET_GAME, 
}

public enum DataModel
{
    USER_CONFIG_MODEL,
    NUM_ACTIVE_AND_NUM_PASSIVE_USERS,
    LIGHT_DIRECTION,
    TEAM,
    MOVE,
}

