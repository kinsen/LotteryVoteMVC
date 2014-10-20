using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Core.Authorizes;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core;
using LotteryVoteMVC.Models.VietnamLottery;
using System.Text;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Core.Web;

namespace LotteryVoteMVC.Controllers
{
    public class BetController : BaseController
    {
        public CompanyManager ComManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<CompanyManager>();
            }
        }
        public FreezeFundsManager FreezeManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<FreezeFundsManager>();
            }
        }
        public BetManager BetManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<BetManager>();
            }
        }


        [UserAuthorize(Models.UserState.Active, Role.Guest)]
        public ActionResult Index(CompanyType? type)
        {

            var companys = TodayLotteryCompany.Instance.GetOpenCompany();
            if (Theme == "Red")
            {
                if (type == null)
                {
                    companys = companys.Where(it => it.CompanyType == CompanyType.EighteenA || it.CompanyType == CompanyType.EighteenB).ToList();
                    ViewBag.CompanyType = "18A,B";
                }
                else
                {
                    companys = companys.Where(it => it.CompanyType == type.Value).ToList();
                    ViewBag.CompanyType = EnumDescriptionAttribute.GetEnumDesc(type).Description;
                }
            }
            ViewBag.Regions = new[] { Region.South.ToString(), Region.Middle.ToString(), Region.North.ToString() };
            ViewBag.GamePlayWays = LotterySystem.Current.GamePlayWays.ToList();
            WriteCompanyNumLenToClient(companys);
            return View(companys);
        }
        [RequestCostTimeFilter]
        [UserAuthorize(Models.UserState.Active, Role.Guest), HttpPost]
        public ActionResult Index([ModelBinder(typeof(JsonBinder<BetItem[]>))]BetItem[] betList)
        {
            var betResult = BetManager.AddBet(CurrentUser, LotterySpecies.VietnamLottery, betList);
            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = true;
            result.Model = BuildBetResult(betResult);
            return Json(result);
        }

        [UserAuthorize(Models.UserState.Active, Role.Guest)]
        public ActionResult MultiD(CompanyType? type)
        {
            if (Theme != "Red")
                return Redirect("/Bet");
            return this.Index(type);
        }

        [UserAuthorize(UserState.Active, Role.Guest)]
        public ActionResult FastBet2D()
        {
            if (Theme != "Default")
                return Redirect("/Bet");
            var companys = TodayLotteryCompany.Instance.GetOpenCompany();
            ViewBag.GamePlayWays = LotterySystem.Current.GamePlayWays.Where(it => it.GameType == GameType.TwoDigital && it.PlayWay != PlayWay.Roll7);
            WriteCompanyNumLenToClient(companys);
            return View(companys);
        }
        [UserAuthorize(UserState.Active, Role.Guest), HttpPost]
        public ActionResult FastBet2D([ModelBinder(typeof(JsonBinder<FastBetItem>))]FastBetItem fastBet)
        {
            var betResult = BetManager.AddBet(CurrentUser, LotterySpecies.VietnamLottery, fastBet);
            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = true;
            result.Model = BuildBetResult(betResult);
            return Json(result);
        }

        [UserAuthorize(UserState.Active, Role.Guest)]
        public ActionResult FastBet3D()
        {
            if (Theme != "Default")
                return Redirect("/Bet");
            var companys = TodayLotteryCompany.Instance.GetOpenCompany();
            ViewBag.GamePlayWays = LotterySystem.Current.GamePlayWays.Where(it => it.GameType == GameType.ThreeDigital && it.PlayWay != PlayWay.Roll7);
            WriteCompanyNumLenToClient(companys);
            return View(companys);
        }

        [UserAuthorize(UserState.Active, Role.Guest), HttpPost]
        public ActionResult FastBet3D([ModelBinder(typeof(JsonBinder<FastBetItem>))]FastBetItem fastBet)
        {
            var betResult = BetManager.AddBet(CurrentUser, LotterySpecies.VietnamLottery, fastBet, GameType.ThreeDigital);
            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = true;
            result.Model = BuildBetResult(betResult);
            return Json(result);
        }

        [UserAuthorize(Models.UserState.Active, Role.Guest)]
        public ActionResult RollParlay(CompanyType? type)
        {
            var companys = TodayLotteryCompany.Instance.GetOpenCompany();
            if (Theme == "Red")
            {
                if (type == null)
                {
                    companys = companys.Where(it => it.CompanyType == CompanyType.EighteenA || it.CompanyType == CompanyType.EighteenB).ToList();
                    ViewBag.CompanyType = "18A,B";
                }
                else
                {
                    companys = companys.Where(it => it.CompanyType == type.Value).ToList();
                    ViewBag.CompanyType = EnumDescriptionAttribute.GetEnumDesc(type).Description;
                }
            }
            var pl2 = LotterySystem.Current.GamePlayWays.Find(it => it.GameType == GameType.PL2);
            var pl3 = LotterySystem.Current.GamePlayWays.Find(it => it.GameType == GameType.PL3);
            ViewBag.Regions = new[] { Region.South.ToString(), Region.Middle.ToString(), Region.North.ToString() };
            ViewBag.PL2 = pl2.Id;
            ViewBag.PL3 = pl3.Id;
            WriteCompanyNumLenToClient(companys);
            return View(companys);
        }
        [RequestCostTimeFilter]
        [UserAuthorize(Models.UserState.Active, Role.Guest), HttpPost]
        public ActionResult RollParlay([ModelBinder(typeof(JsonBinder<BetItem[]>))]BetItem[] betList)
        {
            var betResult = BetManager.AddBet(CurrentUser, LotterySpecies.VietnamLottery, betList);
            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = true;
            result.Model = BuildBetResult(betResult);
            return Json(result);
        }

        [UserAuthorize(Models.UserState.Active, Role.Guest)]
        public ActionResult Roll7()
        {
            if (Theme != "Default")
                return Redirect("/Bet");
            var companys = TodayLotteryCompany.Instance.GetOpenCompany().Where(it => it.CompanyType != CompanyType.Hanoi);
            ViewBag.Regions = new[] { Region.South.ToString(), Region.Middle.ToString(), Region.North.ToString() };
            ViewBag.GamePlayWays = LotterySystem.Current.GamePlayWays.ToList();
            WriteCompanyNumLenToClient(companys);
            return View(companys);
        }
        [UserAuthorize(Models.UserState.Active, Role.Guest), HttpPost]
        public ActionResult Roll7([ModelBinder(typeof(JsonBinder<BetItem[]>))]BetItem[] betList)
        {
            var betResult = BetManager.AddBet(CurrentUser, LotterySpecies.VietnamLottery, betList);
            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = true;
            result.Model = BuildBetResult(betResult);
            return Json(result);
        }

        [UserAuthorize(Models.UserState.Active, Role.Guest)]
        public ActionResult ABRollParlay()
        {
            if (Theme != "Default")
                return Redirect("/Bet");
            var openCompanys = TodayLotteryCompany.Instance.GetOpenCompany();
            var companys = TodayLotteryCompany.Instance.GetOpenCompany()
                .Where(it => it.CompanyType == CompanyType.EighteenA).ToList();
            var companyBs = TodayLotteryCompany.Instance.GetOpenCompany()
                .Where(it => it.CompanyType == CompanyType.EighteenB).ToList();
            IDictionary<Tuple<CompanyType, CompanyType>, GameType> union_PL2_dic = new Dictionary<Tuple<CompanyType, CompanyType>, GameType>();
            union_PL2_dic.Add(new Tuple<CompanyType, CompanyType>(CompanyType.EighteenA, CompanyType.EighteenB), GameType.A_B_PL2);
            union_PL2_dic.Add(new Tuple<CompanyType, CompanyType>(CompanyType.EighteenC, CompanyType.EighteenA), GameType.C_A_PL2);
            union_PL2_dic.Add(new Tuple<CompanyType, CompanyType>(CompanyType.EighteenB, CompanyType.EighteenC), GameType.B_C_PL2);

            List<Tuple<LotteryCompany, LotteryCompany, int>> companyTuple = new List<Tuple<LotteryCompany, LotteryCompany, int>>();
            foreach (var x in openCompanys)
            {
                foreach (var y in openCompanys)
                {
                    var key = new Tuple<CompanyType, CompanyType>(x.CompanyType, y.CompanyType);
                    if (union_PL2_dic.ContainsKey(key))
                    {
                        var gameType = LotterySystem.Current.GamePlayWays.Find(it => it.GameType == union_PL2_dic[key]).Id;
                        companyTuple.Add(new Tuple<LotteryCompany, LotteryCompany, int>(x, y, gameType));
                    }

                }
            }
            //for (int i = 0, j = 0; i < companys.Count && j < companyBs.Count; )
            //{
            //    companyTuple.Add(new[] { companys[i], companyBs[j] });

            //    i++;
            //    if (j < companyBs.Count - 1) j++;
            //}


            ViewBag.Regions = new[] { Region.South.ToString(), Region.Middle.ToString(), Region.North.ToString() };
            ViewBag.PL2 = LotterySystem.Current.GamePlayWays.Find(it => it.GameType == GameType.A_B_PL2).Id;

            WriteCompanyNumLenToClient(openCompanys);
            return View(companyTuple);
        }
        [RequestCostTimeFilter]
        [UserAuthorize(Models.UserState.Active, Role.Guest), HttpPost]
        public ActionResult ABRollParlay([ModelBinder(typeof(JsonBinder<BetItem[]>))]BetItem[] betList)
        {
            var betResult = BetManager.AddUnionPL2Bet(CurrentUser, LotterySpecies.VietnamLottery, betList);
            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = true;
            result.Model = BuildBetResult(betResult);
            return Json(result);
        }

        [UserAuthorize(UserState.Active, Role.Guest)]
        public ActionResult Zodiac(CompanyType? type)
        {
            var companys = TodayLotteryCompany.Instance.GetOpenCompany();
            if (Theme == "Red")
            {
                if (type == null)
                {
                    companys = companys.Where(it => it.CompanyType == CompanyType.EighteenA || it.CompanyType == CompanyType.EighteenB).ToList();
                    ViewBag.CompanyType = "18A,B";
                }
                else
                {
                    companys = companys.Where(it => it.CompanyType == type.Value).ToList();
                    ViewBag.CompanyType = EnumDescriptionAttribute.GetEnumDesc(type).Description;
                }
            }
            ViewBag.GamePlayWays = LotterySystem.Current.GamePlayWays.ToList();
            WriteCompanyNumLenToClient(companys);
            return View(companys);
        }
        [RequestCostTimeFilter]
        [UserAuthorize(UserState.Active, Role.Guest), HttpPost]
        public ActionResult Zodiac([ModelBinder(typeof(JsonBinder<AutoBetItem[]>))]AutoBetItem[] betList)
        {
            var betResult = BetManager.AddBet(CurrentUser, LotterySpecies.VietnamLottery, betList);
            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = true;
            result.Model = BuildBetResult(betResult);
            return Json(result);
        }


        [UserAuthorize(UserState.Active, Role.Guest)]
        public ActionResult AmountBoard()
        {
            var freeze = FreezeManager.GetFreezeFund(CurrentUser);
            if (Request.IsAjaxRequest())
            {
                JsonResultModel result = new JsonResultModel
                {
                    IsSuccess = true,
                    Model = new
                    {
                        BetCredit = CurrentUser.UserInfo.AvailableGivenCredit.ToString(),
                        Outstanding = freeze.Amount.ToString()
                    }
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                ViewBag.BetCredit = CurrentUser.UserInfo.AvailableGivenCredit;
                ViewBag.Outstanding = freeze == null ? 0 : freeze.Amount;
                return View();
            }
        }

        [UserAuthorize(UserState.Active, Role.Guest)]
        public ActionResult Rule()
        {
            return View();
        }

        public object BuildBetResult(BetResult result)
        {
            var companys = TodayLotteryCompany.Instance.GetTodayCompany();
            return new
            {
                ActualTurnover = result.ActualTurnover,
                ExceptTurnover = result.ExceptTurnover,
                Drops = result.DropOrderList.Select(it => new
                {
                    Num = it.Num,
                    Company = companys.Find(x => x.CompanyId == it.CompanyId).Abbreviation,
                    GameType = Extended.GetGPWDescription(it.GamePlayWayId),
                    Drop = it.DropWater.ToString("0.00"),
                    Net = it.Net.ToString("N")
                }),
                NotAccept = result.NotAcceptOrderList.Select(it => new
                {
                    Num = it.Num,
                    Company = companys.Find(x => x.CompanyId == it.CompanyId).Abbreviation,
                    GameType = Extended.GetGPWDescription(it.GamePlayWayId)
                })
            };
        }
        private void WriteCompanyNumLenToClient(IEnumerable<LotteryCompany> betCompanys)
        {
            List<CompanyNumLen> numLens = new List<CompanyNumLen>();
            foreach (var company in betCompanys)
            {
                var numLenSupport = ComManager.GetNumLengthByCompany(company);
                var lenCount = numLenSupport.Select(it => new LotteryVoteMVC.Models.VietnamLottery.CompanyNumLen.NumLenItem { Length = it.NumLen.Length, Count = it.Count });
                numLens.Add(new CompanyNumLen
                {
                    CompanyId = company.CompanyId,
                    NumLenList = lenCount.ToList()
                });
            }
            var baseStr = Convert.ToBase64String(Encoding.UTF8.GetBytes(JSONHelper.ToJson(numLens)));
            CookieSerializer cs = new CookieSerializer("CompanyNumLen", baseStr);
            cs.SaveCookie();
        }
    }
}
