using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 注单
    /// </summary>
    public class BetSheet
    {
        public const string TABLENAME = "tb_BetSheet";
        public const string TEMP_TABLENAME = "tb_BetSheetTemp";
        public const string SHEETID = "SheetId";
        public const string USERID = "UserId";
        public const string NUM = "Num";
        public const string BETCOMPANY = "BetCompany";
        public const string BETAMOUNT = "BetAmount";
        public const string STATUS = "Status";
        public const string IPADDRESS = "IPAddress";
        public const string CREATETIME = "CreateTime";

        public BetSheet() { }
        public BetSheet(DataRow row)
        {
            this.SheetId = row.Field<int>(SHEETID);
            this.UserId = row.Field<int>(USERID);
            this.Num = row.Field<string>(NUM);
            this.BetCompany = row.Field<string>(BETCOMPANY);
            this.BetAmount = row.Field<string>(BETAMOUNT);
            this.Status = (BetStatus)row.Field<int>(STATUS);
            this.IPAddress = row.Field<string>(IPADDRESS);
            this.CreateTime = row.Field<DateTime>(CREATETIME);
        }

        public DataRow DataRow { get; set; }
        public int SheetId { get; set; }
        public int UserId { get; set; }
        public string Num { get; set; }
        public string BetCompany { get; set; }
        /// <summary>
        ///各種玩法的下注金額
        /// </summary>
        /// <value>
        /// The bet amount.
        /// </value>
        public string BetAmount { get; set; }
        public BetStatus Status { get; set; }
        /// <summary>
        /// 总下注额（根据BetOrder计算）.
        /// </summary>
        /// <value>
        /// The turnover.
        /// </value>
        public decimal Turnover { get; set; }
        /// <summary>
        /// 总佣金（根据BetOrder计算）
        /// </summary>
        /// <value>
        /// The commission.
        /// </value>
        public decimal Commission { get; set; }
        /// <summary>
        /// 净金额（根据BetOrder计算）.
        /// </summary>
        /// <value>
        /// The net amount.
        /// </value>
        public decimal NetAmount { get; set; }
        public string IPAddress { get; set; }
        public DateTime CreateTime { get; set; }
        private User _user;
        public User User
        {
            get
            {
                if (_user == null && DataRow != null)
                    _user = ModelParser<User>.ParseModel(DataRow);
                return _user;
            }
            set
            {
                _user = value;
            }
        }
        private IList<WagerItem> _wagerList;
        public IList<WagerItem> WagerList
        {
            get
            {
                if (_wagerList == null)
                {
                    if (string.IsNullOrEmpty(BetAmount))
                        throw new ApplicationException("找不到玩法下注額,SheetId:" + SheetId);
                    _wagerList = new List<WagerItem>();
                    var wagers = BetAmount.Split(new[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var wager in wagers)
                    {
                        var wagerInfo = wager.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                        if (wagerInfo.Length != 3)
                            throw new ApplicationException(string.Format("玩法下注額格式不正確,SheetId:{0},Wager:{1}", SheetId, wager));
                        _wagerList.Add(new WagerItem
                        {
                            GamePlayTypeId = int.Parse(wagerInfo[0]),
                            Wager = decimal.Parse(wagerInfo[1]),
                            IsFullPermutation = wagerInfo[2] == "1" ? true : false
                        });
                    }
                }
                return _wagerList;
            }
        }
    }
}
