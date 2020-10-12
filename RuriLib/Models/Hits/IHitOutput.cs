using System.Threading.Tasks;

namespace RuriLib.Models.Hits
{
    public interface IHitOutput
    {
        Task Store(Hit hit);
    }
}
