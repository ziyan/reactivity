using System;
using System.ServiceModel;

using Reactivity.Objects;

namespace Reactivity.Server.Clients
{
    [ServiceContract(Namespace="http://service.reac.tivity.org")]
    public interface IClientService
    {
        [OperationContract]
        Guid SessionNew();

        [OperationContract]
        bool SessionExists(Guid session);

        [OperationContract(IsOneWay=true)]
        void SessionAbandon(Guid session);

        [OperationContract(IsOneWay=true)]
        void SessionKeepAlive(Guid session);




        [OperationContract]
        ClientEvent[] ClientEventGet(int timeout, Guid session);



        [OperationContract]
        Subscription Subscribe(Guid device, short service, Guid session);

        [OperationContract(IsOneWay=true)]
        void Unsubscribe(Guid subscription, Guid session);

        [OperationContract(IsOneWay = true)]
        void UnsubscribeAll(Guid session);




        [OperationContract]
        string ResourceGetIndex(Guid session);

        [OperationContract]
        byte[] ResourceGet(Guid guid, Guid session);

        [OperationContract]
        System.IO.Stream ResourceGetStream(Guid guid, Guid session);





        [OperationContract]
        UserLoginResult UserLogin(string username, string hashpassword, Guid session);

        [OperationContract(IsOneWay=true)]
        void UserLogout(Guid session);

        [OperationContract]
        User UserCurrent(Guid session);

        [OperationContract]
        bool UserIsLoggedIn(Guid session);

        [OperationContract]
        bool UserChangePassword(string hashpassword, string hashnewpassword, Guid session);

        [OperationContract]
        int UserCreate(User user, Guid session);

        [OperationContract]
        bool UserRemove(int user, Guid session);

        [OperationContract]
        User[] UserList(Guid session);

        [OperationContract]
        bool UserUpdate(User user, Guid session);

        [OperationContract]
        bool UserSetPassword(int user, string hashpassword, Guid session);

        [OperationContract]
        User UserGet(int id, Guid session);

        [OperationContract]
        User UserGetByUsername(string username, Guid session);






        [OperationContract]
        Rule[] RuleList(Guid session);

        [OperationContract]
        Rule RuleGet(int id, Guid session);

        [OperationContract]
        int RuleCreate(Rule rule, Guid session);

        [OperationContract]
        bool RuleRemove(int id, Guid session);

        [OperationContract]
        bool RuleUpdate(Rule rule, Guid session);

        [OperationContract(IsOneWay=true)]
        void RuleChainReload(Guid session);

        [OperationContract(IsOneWay = true)]
        void RuleChainReloadFromDatabase(Guid session);

        [OperationContract]
        Device[] DeviceList(Guid session);

        [OperationContract]
        Device DeviceGet(Guid guid, Guid session);

        [OperationContract]
        bool DeviceCreate(Device device, Guid session);

        [OperationContract]
        bool DeviceRemove(Guid guid, Guid session);

        [OperationContract]
        bool DeviceUpdate(Device device, Guid session);

        [OperationContract(IsOneWay = true)]
        void DataSend(Objects.Data[] data, Guid session);

        [OperationContract]
        Statistics[] StatisticsQuery(Guid device, short service, DateTime start_date, DateTime end_date, StatisticsType type, Guid session);
    }
}
