using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Utilities.Constant;

namespace Bidder
{
    public class SignalRAndRedisClient
    {
        #region constructor
        private readonly HubConnection _connection;
        private readonly ConnectionMultiplexer _redisConnection;
        private readonly string _hubName = "AuctionHub";

        public SignalRAndRedisClient(string hubUrl, string redisConnectionString)
        {
            // connect signalR
            _connection = new HubConnectionBuilder().WithUrl(hubUrl).Build();

            // connect Redis
            _redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);

            // Receive Message
            _connection.On("ReceiveMessage", (Guid auctionId, Guid productId, Guid bidderId, float currentPrice, float myPrice,
            int auctionType, float myMaxPriceAuto, float stepPrice) =>
            {
                // Handle received message, temporarily not in use
            });

            // start connect
            _connection.StartAsync().Wait();
        }
        #endregion

        #region SendMessageAsync
        public async Task SendMessageAsync(Guid auctionId, Guid productId, Guid bidderId, float currentPrice,
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

            var redisDb = _redisConnection.GetDatabase();
            var groupKey = $"{KeyRedisNameConst.GroupMessageHistory}:{auctionId}";

            await redisDb.ListLeftPushAsync(groupKey, JsonConvert.SerializeObject(message));
            await redisDb.PublishAsync(_hubName, JsonConvert.SerializeObject(message));

            await _connection.InvokeAsync("SendMessage", auctionId, productId, bidderId, currentPrice,
                myPrice, auctionType, myMaxPriceAuto, stepPrice);
        }
        #endregion
    }
}
