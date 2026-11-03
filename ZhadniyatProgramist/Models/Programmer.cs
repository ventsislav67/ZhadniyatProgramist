namespace ZhadniyatProgramist.Models
{
    /// <summary>
    /// Програмист (клиент на кръчмата).
    /// ToString() връща името, защото обектите се слагат директно
    /// в ComboBox-ите и те показват точно този текст.
    /// </summary>
    public class Programmer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FavoriteLanguage { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}