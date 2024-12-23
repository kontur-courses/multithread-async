namespace AdvancedSynchronization.Examples
{
    public class BankAccount
    {
        public BankAccount(string ownerName) => OwnerName = ownerName;
        
        public readonly string OwnerName;
        public long Rubles;

        public override string ToString() => $"{OwnerName} has {Rubles} RUB";
    }
}