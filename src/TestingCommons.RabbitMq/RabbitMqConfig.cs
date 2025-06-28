namespace TestingCommons.RabbitMq
{
    public class RabbitMqConfig
    {
        public string Hostname { get; set; }
        public ushort Port => EnvironmentConnectionPort;
        public string ClusterNodes { get; set; }
        public string[] ClusterNodesList => string.IsNullOrEmpty(ClusterNodes) ? new string[0] : GetClusterNodes();
        public string VirtualHost { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseTlsPolicy { get; set; }
        public bool IsTlsConnection { get; set; }
        public int HeartbeatInterval { get; set; }
        public string SchedulerQueue { get; set; }
        public RedeliverySettings Redelivery { get; set; }

        private string[] GetClusterNodes()
            => ClusterNodes.Split(",")
                           .Where(x => !string.IsNullOrWhiteSpace(x))
                           .Select(x => x.ToLower().Trim())
                           .Distinct()
                           .ToArray();

        private ushort EnvironmentConnectionPort => IsTlsConnection ? (ushort)5671 : (ushort)5672;
    }
}
