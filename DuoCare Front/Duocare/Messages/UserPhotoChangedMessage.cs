using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Duocare.Messages;

public class UserPhotoChangedMessage : ValueChangedMessage<string>
{
    public UserPhotoChangedMessage(string value) : base(value) { }
}
