namespace TestingCommons.RabbitMq
{
    public class EncryptionSettings
    {
        public bool IsActive { get; set; }
        public bool SkipEncryptOnSend { get; set; }
        public string EncryptionKeyName { get; set; }
        public string KeyVaultClientId { get; set; }
        public string KeyVaultClientSecret { get; set; }
        public string KeyVaultUrl { get; set; }
    }
}
