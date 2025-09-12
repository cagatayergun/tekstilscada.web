// Models/PlcOperator.cs
namespace TekstilScada.Models
{
    public class PlcOperator
    {
        public int SlotIndex { get; set; } // 0-4 arası yuva numarası
        public string Name { get; set; }
        public short UserId { get; set; }
        public short Password { get; set; }
    }
}
