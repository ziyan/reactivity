using System;
using System.ServiceModel;

using Reactivity.Objects;

namespace Reactivity.Server.Nodes
{
    [ServiceContract(Namespace = "http://service.reac.tivity.org")]
    public interface INodeService
    {
        [OperationContract]
        Guid SessionNew();

        [OperationContract]
        bool SessionExists(Guid session);

        [OperationContract(IsOneWay = true)]
        void SessionAbandon(Guid session);

        [OperationContract(IsOneWay = true)]
        void SessionKeepAlive(Guid session);

        [OperationContract]
        NodeEvent[] NodeEventGet(int timeout, Guid session);

        [OperationContract]
        bool DeviceRegister(Guid device, Guid session);

        [OperationContract]
        Device DeviceGet(Guid device, Guid session);

        [OperationContract(IsOneWay = true)]
        void DeviceDeregister(Guid device, Guid session);

        [OperationContract(IsOneWay = true)]
        void DeviceDeregisterAll(Guid session);

        [OperationContract]
        bool DeviceIsRegisterred(Guid device, Guid session);

        [OperationContract(IsOneWay = true)]
        void DataUpload(Objects.Data[] data, Guid session);
    }
}
