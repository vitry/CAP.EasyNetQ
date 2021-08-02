namespace DotNetCore.CAP.EasyNetQ
{
    public static class MessagePriority
    {
        public const byte LOW = 1;
        public const byte HIGH = 2;

        public static byte ConvertToPriority(string str)
        {
            return str switch
            {
                "1" => LOW,
                "2" => HIGH,
                _ => throw new System.Exception($"'{str}' is not a valid priority")
            };
        }
    }
}