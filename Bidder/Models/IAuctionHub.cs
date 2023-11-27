using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bidder.Models
{
    public interface IAuctionHub
    {
        Task SendMessage(int auctionId, int productId, int bidderId, float currentPrice,
            float myPrice, int auctionType, float myMaxPriceAuto, float stepPrice);
        Task JoinGroup(int auctionId);
        List<Message> GetHistoryMessages(int auctionId);
    }
}
