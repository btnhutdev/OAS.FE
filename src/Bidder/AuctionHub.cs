using Bidder.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Constant;

namespace Bidder
{
    public class AuctionHub : Hub
    {
        #region constructor
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly string _methodInvoke = "ReceiveMessage";
        private readonly string _hubName = "AuctionHub";

        public AuctionHub(IConnectionMultiplexer connectionMultiplexer)
        {
            _redisConnection = connectionMultiplexer;
        }
        #endregion

        #region SendMessage
        public async Task SendMessage(Guid auctionId, Guid productId, Guid bidderId, float currentPrice,
            float myPrice, int auctionType, float myMaxPriceAuto, float stepPrice)
        {
            var message = new Message
            {
                AuctionId = auctionId,
                ProductId = productId,
                BidderId = bidderId,
                CurrentPrice = currentPrice,
                MyPrice = myPrice,
                AuctionType = auctionType,
                MyMaxPriceAuto = myMaxPriceAuto,
                StepPrice = stepPrice
            };

            // connect db redis
            var redisDb = _redisConnection.GetDatabase();
            var groupKey = $"{KeyRedisNameConst.GroupMessageHistory}:{auctionId}";

            // push item to db redis
            await redisDb.ListLeftPushAsync(groupKey, JsonConvert.SerializeObject(message)).ConfigureAwait(false);

            // push to signalR hub
            await redisDb.PublishAsync(_hubName, JsonConvert.SerializeObject(message)).ConfigureAwait(false);

            await Clients.Group(auctionId.ToString()).SendAsync(_methodInvoke, auctionId, productId,
                bidderId, currentPrice, myPrice, auctionType, myMaxPriceAuto, stepPrice).ConfigureAwait(false);
        }
        #endregion

        #region JoinGroup
        public async Task JoinGroup(Guid auctionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, auctionId.ToString()).ConfigureAwait(false);
            var historyMessages = GetHistoryMessages(auctionId);

            foreach (var message in historyMessages)
            {
                await Clients.Caller.SendAsync(_methodInvoke, message.AuctionId, message.ProductId, message.BidderId, 
                    message.CurrentPrice, message.MyPrice, message.AuctionType, message.MyMaxPriceAuto, message.StepPrice).ConfigureAwait(false);
            }
        }
        #endregion

        #region LeaveGroup
        public async Task LeaveGroup(Guid auctionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, auctionId.ToString()).ConfigureAwait(false);
        }
        #endregion

        #region GetHistoryMessages
        public List<Message> GetHistoryMessages(Guid auctionId)
        {
            var redisDb = _redisConnection.GetDatabase();

            var groupKey = $"{KeyRedisNameConst.GroupMessageHistory}:{auctionId}";
            var messages = redisDb.ListRange(groupKey, start: 0, stop: -1);

            // Đảo ngược thứ tự các phần tử để lấy ra price cao nhất
            messages = messages.Reverse().ToArray();

            return messages.Select(m => JsonConvert.DeserializeObject<Message>(m)).ToList();
        }
        #endregion
    }

    #region Message
    public class Message
    {
        public Guid AuctionId { get; set; }
        public Guid ProductId { get; set; }
        public Guid BidderId { get; set; }
        public float CurrentPrice { get; set; }
        public float MyPrice { get; set; }
        public int AuctionType { get; set; }
        public float MyMaxPriceAuto { get; set; }
        public float StepPrice { get; set; }
    }
    #endregion
}
