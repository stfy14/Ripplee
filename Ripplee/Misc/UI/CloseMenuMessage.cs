using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Ripplee.Misc.UI
{
    class CloseMenuMessage : ValueChangedMessage<bool>
    {
        public CloseMenuMessage() : base(true) { }
    }
}
