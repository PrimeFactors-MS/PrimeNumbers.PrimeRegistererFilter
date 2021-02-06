using System.Threading.Tasks;

namespace PrimeNumbers.PrimeRegistererFilter.Core
{
    public interface IPrimeSubmitter
    {
        Task SubmitPrimeRecord(PrimeRecord record);
    }
}
