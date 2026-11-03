using System;

namespace ZhadniyatProgramist.Models
{
    /// <summary>Един ред в тефтера – какво, кой, за колко и кога е поръчал.</summary>
    public class TabItem
    {
        public int Id { get; set; }
        public int ProgrammerId { get; set; }
        public string ItemName { get; set; }
        public double Price { get; set; }
        public DateTime Date { get; set; }
    }
}