using System.Threading.Tasks;

namespace PrimeNumbers.PrimeRegistererFilter.Core
{
    public class PrimeFilter
    {
        private readonly IPrimeSubmitter _primeSubmitter;
        private readonly long _minimumTimeMilliseconds;

        public PrimeFilter(IPrimeSubmitter primeSubmitter, long minimumTimeMilliseconds = 400)
        {
            _primeSubmitter = primeSubmitter;
            _minimumTimeMilliseconds = minimumTimeMilliseconds;
        }
        public Task FilterPrimeRecord(PrimeRecordReport recordReport)
        {
            if (recordReport.CalculationTimeMilliseconds > _minimumTimeMilliseconds)
            {
                return _primeSubmitter.SubmitPrimeRecord(recordReport.PrimeRecord);
            }
            return Task.CompletedTask;
        }
    }
}
