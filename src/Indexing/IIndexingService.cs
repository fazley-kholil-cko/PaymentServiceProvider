using PaymentServiceProvider.Models;
using Refit;
using System.Threading.Tasks;

namespace PaymentServiceProvider.Indexing
{
    public interface IIndexingService
    {
        [Post("/api/indexing/transaction")]
        Task<object> Index(Transaction t);
    }
}
