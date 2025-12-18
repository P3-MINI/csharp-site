namespace Chat.Common.MessageHandlers;


public class InvalidMessageException(string message) : Exception(message) {}


public class TooLongMessageException(string message) : InvalidMessageException(message) {}


public abstract class MessageHandler
{
    protected const int HeaderLen = 4;
    protected const int MaxMessageLen = 1024 * 10;
}
