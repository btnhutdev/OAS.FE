namespace Utilities.Constant
{
    public static class PermissionNameConst
    {
        public const string Auctioneer = "Auctioneer";
        public const string Bidder = "Bidder";
        public const string Admin = "Admin";

        public const string AuctioneerOrBidder = Auctioneer + "," + Bidder;
        public const string AuctioneerOrAdmin = Auctioneer + "," + Admin;
        public const string AllRole = Auctioneer + "," + Bidder + "," + Admin;
    }

    public static class PermissionTypeConst
    {
        public const int Auctioneer = 1;
        public const int Bidder = 2;
        public const int Admin = 3;
    }
}
