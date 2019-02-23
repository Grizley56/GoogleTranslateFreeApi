using System;

namespace GoogleTranslateFreeApi
{
  public class GoogleTranslateIPBannedException: Exception
  {
    public enum Operation { TokenGeneration, Translation }

    public Operation OperationBanned { get; }
    
    public GoogleTranslateIPBannedException(string message, Operation operation)
      :base("Google translate banned this IP for some time (about a few hours). " + message)
    {
      OperationBanned = operation;
    }

    public GoogleTranslateIPBannedException(Operation operation)
      :this(String.Empty, operation) { }
  }
}