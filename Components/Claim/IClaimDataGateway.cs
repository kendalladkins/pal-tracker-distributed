using System.Collections.Generic;

namespace Claim
{
    public interface IClaimDataGateway
    {
        ClaimRecord Create(long userId, long projectId, string expenseType, decimal expenseTotal, DateTime expenseDate);

        List<ClaimRecord> FindBy(long projectId);
    }
}