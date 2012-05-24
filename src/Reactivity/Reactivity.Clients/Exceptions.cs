using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivity.Clients
{
    public class InvalidSessionException : System.Exception { }
    public class InvalidUsernameException : System.Exception { }
    public class UserAlreadyLoggedInException : System.Exception { }
    public class PermissionDeniedException : System.Exception { }
    public class DuplicateUsernameException : System.Exception { }

}
