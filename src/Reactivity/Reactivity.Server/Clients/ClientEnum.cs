using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivity.Server.Clients
{
    [Serializable]
    public enum UserLoginResult { Success, InvalidArgument, Mismatch, InvalidUsername, UserAlreadyLoggedIn, Error, InvalidSession };
}
