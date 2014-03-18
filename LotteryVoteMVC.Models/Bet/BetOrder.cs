using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 下注明细单
    /// </summary>
    public class BetOrder
    {
        public const string TABLENAME = "tb_BetOrder";
        public const string TEMP_TABLENAME = "tb_BetOrderTemp";
        public const string ORDERID = "OrderId";
        public const string SHEETID = "SheetId";
        public const string NUM = "Num";
        public const string GAMEPLAYWAYID = "GamePlayWayId";
        public const string COMPANYID = "CompanyId";
        public const string AMOUNT = "Amount";
        public const string TURNOVER = "Turnover";
        public const string ODDS = "Odds";
        public const string COMMISSION = "Commission";
        public const string NET = "Net";
        public const string NETAMOUNT = "NetAmount";
        public const string STATUS = "Status";
        public const string DRAWRESULT = "DrawResult";
        public const string DROPWATER = "DropWater";
        public const string CREATETIME = "CreateTime";
        public const string CANCELTIME = "CancelTime";
        public const string CANCELAMOUNT = "CancelAmount";
        public const string EXT1 = "Ext1";

        public BetOrder() { }
        public BetOrder(DataRow row)
        {
            this.OrderId = row.Field<int>(ORDERID);
            this.SheetId = row.Field<int>(SHEETID);
            this.Num = row.Field<string>(NUM);
            this.GamePlayWayId = row.Field<int>(GAMEPLAYWAYID);
            this.CompanyId = row.Field<int>(COMPANYID);
            this.Amount = row.Field<decimal>(AMOUNT);
            this.Turnover = row.Field<decimal>(TURNOVER);
            this.Odds = row.Field<double>(ODDS);
            this.Commission = row.Field<decimal>(COMMISSION);
            this.Net = row.Field<double>(NET);
            this.NetAmount = row.Field<decimal>(NETAMOUNT);
            this.Status = (BetStatus)row.Field<int>(STATUS);
            this.DropWater = row.Field<double>(DROPWATER);
            this.DrawResult = row[DRAWRESULT] == DBNull.Value ? 0 : row.Field<decimal>(DRAWRESULT);
            this.CreateTime = row.Field<DateTime>(CREATETIME);
            this.Ext1 = row.Field<string>(EXT1);
            if (row.Table.Columns["USERID"] != null)
                this.UserId = row.Field<int>("USERID");
        }

        public int OrderId { get; set; }
        public int SheetId { get; set; }
        public string Num { get; set; }
        public int GamePlayWayId { get; set; }
        public int CompanyId { get; set; }
        /// <summary>
        /// 投注金额.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal Amount { get; set; }
        /// <summary>
        /// 下注额.
        /// </summary>
        /// <value>
        /// The turnover.
        /// </value>
        public decimal Turnover { get; set; }
        /// <summary>
        /// 赔率.
        /// </summary>
        /// <value>
        /// The odds.
        /// </value>
        public double Odds { get; set; }
        /// <summary>
        /// 佣金.
        /// </summary>
        /// <value>
        /// The commission.
        /// </value>
        public decimal Commission { get; set; }
        /// <summary>
        /// 净值.
        /// </summary>
        /// <value>
        /// The net.
        /// </value>
        public double Net { get; set; }
        /// <summary>
        /// 净金额.
        /// </summary>
        /// <value>
        /// The net amount.
        /// </value>
        public decimal NetAmount { get; set; }
        public BetStatus Status { get; set; }
        /// <summary>
        /// 输赢结果.
        /// </summary>
        /// <value>
        /// The draw result.
        /// </value>
        public decimal DrawResult { get; set; }
        /// <summary>
        /// 跌水值
        /// </summary>
        /// <value>
        /// The drop water.
        /// </value>
        public double DropWater { get; set; }
        public string IPAddress { get; set; }
        /// <summary>
        /// 非必要项，需联合查询BetSheet才存在
        /// </summary>
        /// <value>
        /// The user id.
        /// </value>
        public int UserId { get; set; }
        public string UserName { get; set; }
        /// <summary>
        /// 取消金额，仅在结单查询（Statement）中出现
        /// </summary>
        /// <value>
        /// The cancel amount.
        /// </value>
        public decimal CancelAmount { get; set; }
        /// <summary>
        /// 纯赢（仅包含赢）
        /// </summary>
        /// <value>
        /// The net win.
        /// </value>
        public decimal NetWin { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? CancelTime { get; set; }
        public IList<OrderAncestorCommInfo> AncestorCommission { get; set; }
        public bool CanCancel
        {
            get
            {
                //TODO:应该使用配置
                return Status == BetStatus.Valid && CreateTime.AddMinutes(5) > DateTime.Now;
            }
        }
        /// <summary>
        /// 扩展字段1.
        /// </summary>
        /// <value>
        /// The ext1.
        /// </value>
        public string Ext1 { get; set; }

        public override string ToString()
        {
            return string.Format(@"OrderId:{0},SheetId:{1},Num:{2},GamePlayWayId:{3},CompanyId:{4},Amount:{5},Turnover:{6},
Odds:{7},Commission:{8},Net:{9},NetAmount:{10},Status:{11},DropWater:{12},DrawResult:{13},UserId:{14}", this.OrderId, this.SheetId, this.Num,
                    this.GamePlayWayId, this.CompanyId, this.Amount, this.Turnover, this.Odds, this.Commission, this.Net, this.NetAmount,
                    this.Status, this.DropWater, this.DrawResult, this.UserId);
        }
    }
}
