using SmartSync.Domain.Entities;

namespace SmartSync.Domain.Events
{
    public class AcaoCommand
    {
        public string? Acao { get; set; }
        public Guid? DispositivoId { get; set; }
        public Guid? ComodoId { get; set; }
        public Guid? ResidenciaId { get; set; }
    }

}