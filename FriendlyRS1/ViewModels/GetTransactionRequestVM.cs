namespace FriendlyRS1.ViewModels
{
    public class GetTransactionRequestVM
    {
        public string search { get; set; } = "";
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 10;
    }
}
