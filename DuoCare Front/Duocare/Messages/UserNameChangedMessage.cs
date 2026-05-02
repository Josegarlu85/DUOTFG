using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Duocare.Messages;

public class UserNameChangedMessage : ValueChangedMessage<string>
{
    public UserNameChangedMessage(string value) : base(value) { }
}
