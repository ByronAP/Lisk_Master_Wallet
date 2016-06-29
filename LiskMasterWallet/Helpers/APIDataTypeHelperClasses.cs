using Lisk.API.Responses;

namespace LiskMasterWallet.Helpers
{
    //these classes act as type converters since WPF only understands properties not fields
    public class Delegate_Class
    {
        public Delegate_Class()
        {
        }

        public Delegate_Class(Delegate_Object delegate_object)
        {
            Address = delegate_object.address;
            Approval = delegate_object.approval;
            MissedBlocks = delegate_object.missedBlocks;
            ProducedBlocks = delegate_object.producedBlocks;
            Productivity = delegate_object.productivity;
            PublicKey = delegate_object.publicKey;
            Rate = delegate_object.rate;
            Username = delegate_object.username;
            Vote = delegate_object.vote;
        }

        public string Address { get; set; }
        public decimal Approval { get; set; }
        public long MissedBlocks { get; set; }
        public long ProducedBlocks { get; set; }
        public decimal Productivity { get; set; }
        public string PublicKey { get; set; }
        public int Rate { get; set; }
        public string Username { get; set; }
        public string Vote { get; set; }
    }
}