using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core.Exceptions;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Utility;
using System.Web.Caching;

namespace LotteryVoteMVC.Core.Application
{
    public class LotterySystem
    {
        #region Const Strings
        private const string M_GAMEPLAYWAY = "CacheGamePlayWay";
        private const string M_DEFAULTUPPERLIMIT = "DefaultUpperLimit";
        private const string M_ADDITIONALLASTBETMINUTES = "AdditionalLastBetMinutes";
        private const string M_CANCANCLEAFTERBETMINUTES = "CanCancleAfterBetMinutes";
        private const string M_AUTOELECTIONCODECOMMISSION = "AutoElectionCodeCommission";
        private const string M_PRECLOSETIME = "PreCloseTime";
        private const string M_TWELVEZODIAC = "TwelveZodiac";
        private const string M_EVENODD = "EvenOdd";
        private const string M_ODDEVEN = "OddEven";
        private const string M_EVENEVEN = "EVENEVEN";
        private const string M_ODDODD = "OddOdd";
        private const string M_SMALL = "Small";
        private const string M_BIG = "Big";
        private const string M_CENTER = "Center";
        private const string M_ODDHEAD = "OddHead";
        private const string M_EVENHEAD = "EvenHead";
        private const string M_ODDLAST = "OddLast";
        private const string M_EVENLAST = "EvenLast";
        private const string M_ALLCOMPANYLIST = "AllCompanylist";
        private const string M_QUICKADDDROPAMOUNT = "QuckAddDropAmount";
        private const string M_STATEMENTORDERCOUNT = "StatementOrderCount";
        private const string M_ENABLESHARERATEEDIT = "EnableShareRateEdit";
        private const string M_BIGSHEETORDERCOUNT = "BigSheetOrderCount";
        public const string M_ONLINEUSERCOUNT = "OnLineUserCount";
        public const string M_FAILEDPWDLOCKOFCOUNT = "FiledPWDLockOfCount";
        public const string M_PAGESIZE = "PageSize";
        public const string M_MEMBERCOUNT = "MemberCount";
        public const string M_CHANGEPASSWORDCYCLE = "ChangePasswordCycle";
        public const string M_CAPTCHAFONTFAMILY = "CaptchaFontFamily";
        #endregion

        private static object _lockHelper = new object();
        private static LotterySystem _system;
        private LotterySystem() { }

        public static LotterySystem Current
        {
            get
            {
                if (_system == null)
                {
                    lock (_lockHelper)
                    {
                        if (_system == null)
                            _system = new LotterySystem();
                    }
                }
                return _system;
            }
        }

        private IEnumerable<GamePlayWay> _gamePlayWays;
        private IDictionary<int, GamePlayWay> _gpwDic;
        private IDictionary<string, GamePlayWay> _mixKeyGPWDic;
        private IDictionary<CompanyType, IList<CompanyTypeSupportNumLen>> _numLenSupportDic;
        private IDictionary<int, LotteryCompany> _comDic;
        public IEnumerable<GamePlayWay> GamePlayWays
        {
            get
            {
                if (_gamePlayWays == null)
                {
                    GamePlayWayDataAccess daGPW = new GamePlayWayDataAccess();
                    _gamePlayWays = daGPW.GetAll();
                }
                return _gamePlayWays;
            }
        }
        internal IDictionary<int, GamePlayWay> GpwDic
        {
            get
            {
                if (_gpwDic == null)
                    _gpwDic = new Dictionary<int, GamePlayWay>();
                return _gpwDic;
            }
        }
        internal IDictionary<string, GamePlayWay> MixKeyGWPDic
        {
            get
            {
                if (_mixKeyGPWDic == null)
                    _mixKeyGPWDic = new Dictionary<string, GamePlayWay>();
                return _mixKeyGPWDic;
            }
        }
        public IDictionary<CompanyType, IList<CompanyTypeSupportNumLen>> NumLenSupportDic
        {
            get
            {
                if (_numLenSupportDic == null)
                    _numLenSupportDic = new Dictionary<CompanyType, IList<CompanyTypeSupportNumLen>>();
                return _numLenSupportDic;
            }
        }
        public IDictionary<int, LotteryCompany> ComDic
        {
            get
            {
                if (_comDic == null)
                    _comDic = new Dictionary<int, LotteryCompany>();
                return _comDic;
            }
        }

        /// <summary>
        ///分页每页呈现元素数量.
        /// </summary>
        public int PageSize
        {
            get
            {
                string countStr = ConfigurationManager.AppSettings[M_PAGESIZE];
                int countValue;

                return int.TryParse(countStr, out countValue) ? countValue : 20;
            }
        }
        /// <summary>
        ///分成修改时间段，默认是星期天20点之后
        /// </summary>
        public int[] EnableShareRateEdit
        {
            get
            {
                string enableStr = ConfigurationManager.AppSettings[M_ENABLESHARERATEEDIT] ?? "0,20";
                var arr = enableStr.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length != 2)
                    throw new InvalidDataException("分成修改时间设置错误!");
                int[] intArr = new int[2];
                intArr[0] = int.Parse(arr[0]);
                intArr[1] = int.Parse(arr[1]);
                return intArr;
            }
        }
        /// <summary>
        /// 结单条数（默认10）
        /// </summary>
        public int StatementOrderCount
        {
            get
            {
                string countStr = ConfigurationManager.AppSettings[M_STATEMENTORDERCOUNT];
                int countValue;

                return int.TryParse(countStr, out countValue) ? countValue : 10;
            }
        }
        /// <summary>
        /// 当前在线人数
        /// </summary>
        public int OnLineUserCount
        {
            get
            {
                return (HttpContext.Current.Application[M_ONLINEUSERCOUNT] as IDictionary<int, Pair<string, Role>>)
                    .Where(it => it.Value.Value == Role.Guest).Distinct().Count();
            }
        }
        /// <summary>
        /// 投注上限默认金额.
        /// </summary>
        public decimal DefaultUpperLimit
        {
            get
            {
                string limitStr = ConfigurationManager.AppSettings[M_DEFAULTUPPERLIMIT];
                decimal limitValue;

                return decimal.TryParse(limitStr, out limitValue) ? limitValue : 10000000;
            }
        }
        /// <summary>
        /// 走地时间，关盘之后，尾还能投注的时间(默认5分钟).
        /// </summary>
        public double AdditionalLastBetMinutes
        {
            get
            {
                string minuteStr = ConfigurationManager.AppSettings[M_ADDITIONALLASTBETMINUTES];
                double minute;
                return double.TryParse(minuteStr, out minute) ? minute : 5;
            }
        }
        /// <summary>
        /// 提前关盘时间，默认5秒
        /// </summary>
        public double PreCloseTime
        {
            get
            {
                string minuteStr = ConfigurationManager.AppSettings[M_PRECLOSETIME];
                double minute;
                return double.TryParse(minuteStr, out minute) ? minute : 5;
            }
        }
        /// <summary>
        /// 下注后，多长时间内能取消注单（默认5分钟）
        /// </summary>
        public double CanCancleAfterBetMinutes
        {
            get
            {
                string minuteStr = ConfigurationManager.AppSettings[M_CANCANCLEAFTERBETMINUTES];
                double minute;
                return double.TryParse(minuteStr, out minute) ? minute : 5;
            }
        }
        /// <summary>
        ///自动选码佣金(默认0.5)，用于十二生肖，双单，双双，单双，单单
        /// </summary>
        public double AutoElectionCodeCommission
        {
            get
            {
                string commStr = ConfigurationManager.AppSettings[M_AUTOELECTIONCODECOMMISSION];
                double comm;
                return double.TryParse(commStr, out comm) ? comm : 0.5;
            }
        }
        /// <summary>
        /// 快速添加跌水时在已投注金额上累加的金额(默认10).
        /// </summary>
        public decimal QuickAddDropAmount
        {
            get
            {
                string amountStr = ConfigurationManager.AppSettings[M_QUICKADDDROPAMOUNT];
                decimal amount;
                return decimal.TryParse(amountStr, out amount) ? amount : 10;
            }
        }
        /// <summary>
        /// 大注单范畴的注单数，如果超过这个数，则该注单则标注为大注单(默认是50)
        /// </summary>
        public int BigSheetOrderCount
        {
            get
            {
                string countStr = ConfigurationManager.AppSettings[M_BIGSHEETORDERCOUNT];
                int countValue;

                return int.TryParse(countStr, out countValue) ? countValue : 50;
            }
        }
        /// <summary>
        /// 用户输入错误帐号密码次数（锁定）默认5次
        /// </summary>
        public int LockUserOfFailedPWDCount
        {
            get
            {
                string countStr = ConfigurationManager.AppSettings[M_FAILEDPWDLOCKOFCOUNT];
                int countValue;

                return int.TryParse(countStr, out countValue) ? countValue : 5;
            }
        }
        /// <summary>
        /// 十二生肖号码.
        /// </summary>
        public int[] TwelveZodiac
        {
            get
            {
                string zodiacStr = ConfigurationManager.AppSettings[M_TWELVEZODIAC];
                int[] zodiacArr;
                if (string.IsNullOrEmpty(zodiacStr))
                    zodiacArr = new[] { 
                        06, 07, 09, 10, 11, 12, 14, 15,
                        18, 46, 47, 49, 50, 51, 52, 54,
                        55, 58, 86, 87, 89, 90, 91, 92,
                        94, 95, 98, 23, 26, 28, 32, 35,
                        63, 66, 68, 72, 75
                    };
                else
                {
                    var strArr = zodiacStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    zodiacArr = new int[strArr.Length];
                    for (int i = 0; i < strArr.Length; i++)
                        zodiacArr[i] = int.Parse(strArr[i]);
                }
                return zodiacArr;
            }
        }
        /// <summary>
        /// 双单号码.
        /// </summary>
        public int[] EvenOdd
        {
            get
            {
                string appSettingStr = ConfigurationManager.AppSettings[M_EVENODD];
                int[] returnVal;
                if (string.IsNullOrEmpty(appSettingStr))
                    returnVal = new[] { 
                        01, 21, 41, 61, 81, 
                        03, 23, 43, 63, 83, 
                        05, 25, 45, 65, 85,
                        07, 27, 47, 67, 87, 
                        09, 29, 49, 69, 89 
                    };
                else
                {
                    var strArr = appSettingStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    returnVal = new int[strArr.Length];
                    for (int i = 0; i < strArr.Length; i++)
                        returnVal[i] = int.Parse(strArr[i]);
                }
                return returnVal;
            }
        }
        /// <summary>
        /// 单双号码.
        /// </summary>
        public int[] OddEven
        {
            get
            {
                string appSettingStr = ConfigurationManager.AppSettings[M_ODDEVEN];
                int[] returnVal;
                if (string.IsNullOrEmpty(appSettingStr))
                    returnVal = new[] { 
                        10,30,50,70,90,
                        12,32,52,72,92,
                        14,34,54,74,94,
                        16,36,56,76,96,
                        18,38,58,78,98
                    };
                else
                {
                    var strArr = appSettingStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    returnVal = new int[strArr.Length];
                    for (int i = 0; i < strArr.Length; i++)
                        returnVal[i] = int.Parse(strArr[i]);
                }
                return returnVal;
            }
        }
        /// <summary>
        /// 双双号码.
        /// </summary>
        public int[] EvenEven
        {
            get
            {
                string appSettingStr = ConfigurationManager.AppSettings[M_EVENEVEN];
                int[] returnVal;
                if (string.IsNullOrEmpty(appSettingStr))
                    returnVal = new[] { 
                        00,20,40,60,80,
                        02,22,42,62,82,
                        04,24,44,64,84,
                        06,26,46,66,86,
                        08,28,48,68,88
                    };
                else
                {
                    var strArr = appSettingStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    returnVal = new int[strArr.Length];
                    for (int i = 0; i < strArr.Length; i++)
                        returnVal[i] = int.Parse(strArr[i]);
                }
                return returnVal;
            }
        }
        /// <summary>
        /// 单单号码.
        /// </summary>
        public int[] OddOdd
        {
            get
            {
                string appSettingStr = ConfigurationManager.AppSettings[M_ODDODD];
                int[] returnVal;
                if (string.IsNullOrEmpty(appSettingStr))
                    returnVal = new[] { 
                        11,31,51,71,91,
                        13,33,53,73,93,
                        15,35,55,75,95,
                        17,37,57,77,97,
                        19,39,59,79,99
                    };
                else
                {
                    var strArr = appSettingStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    returnVal = new int[strArr.Length];
                    for (int i = 0; i < strArr.Length; i++)
                        returnVal[i] = int.Parse(strArr[i]);
                }
                return returnVal;
            }
        }
        /// <summary>
        /// 单头（十位是奇数）.
        /// </summary>
        public int[] OddHead
        {
            get
            {
                string appSettingStr = ConfigurationManager.AppSettings[M_ODDHEAD];
                int[] returnVal;
                if (string.IsNullOrEmpty(appSettingStr))
                {
                    List<int> vals = new List<int>();
                    for (var i = 0; i < 9; i = i + 2)
                        for (int j = 0; j < 10; j++)
                            vals.Add(int.Parse(string.Format("{0}{1}", i, j)));

                    returnVal = vals.ToArray();
                }
                else
                {
                    var strArr = appSettingStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    returnVal = new int[strArr.Length];
                    for (int i = 0; i < strArr.Length; i++)
                        returnVal[i] = int.Parse(strArr[i]);
                }
                return returnVal;
            }
        }

        /// <summary>
        /// 双头（十位是偶数）.
        /// </summary>
        public int[] EvenHead
        {
            get
            {
                string appSettingStr = ConfigurationManager.AppSettings[M_EVENHEAD];
                int[] returnVal;
                if (string.IsNullOrEmpty(appSettingStr))
                {
                    List<int> vals = new List<int>();
                    for (var i = 1; i < 10; i = i + 2)
                        for (int j = 0; j < 10; j++)
                            vals.Add(int.Parse(string.Format("{0}{1}", i, j)));

                    returnVal = vals.ToArray();
                }
                else
                {
                    var strArr = appSettingStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    returnVal = new int[strArr.Length];
                    for (int i = 0; i < strArr.Length; i++)
                        returnVal[i] = int.Parse(strArr[i]);
                }
                return returnVal;
            }
        }

        /// <summary>
        /// 单尾（奇数）.
        /// </summary>
        public int[] OddLast
        {
            get
            {
                string appSettingStr = ConfigurationManager.AppSettings[M_ODDLAST];
                int[] returnVal;
                if (string.IsNullOrEmpty(appSettingStr))
                {
                    List<int> vals = new List<int>();
                    for (int i = 1; i < 100; i = i + 2)
                        vals.Add(i);

                    returnVal = vals.ToArray();
                }
                else
                {
                    var strArr = appSettingStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    returnVal = new int[strArr.Length];
                    for (int i = 0; i < strArr.Length; i++)
                        returnVal[i] = int.Parse(strArr[i]);
                }
                return returnVal;
            }
        }

        /// <summary>
        /// 双尾（偶数）
        /// </summary>
        public int[] EvenLast
        {
            get
            {
                string appSettingStr = ConfigurationManager.AppSettings[M_EVENLAST];
                int[] returnVal;
                if (string.IsNullOrEmpty(appSettingStr))
                {
                    List<int> vals = new List<int>();
                    for (int i = 0; i < 100; i = i + 2)
                        vals.Add(i);

                    returnVal = vals.ToArray();
                }
                else
                {
                    var strArr = appSettingStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    returnVal = new int[strArr.Length];
                    for (int i = 0; i < strArr.Length; i++)
                        returnVal[i] = int.Parse(strArr[i]);
                }
                return returnVal;
            }
        }

        public int[] Small
        {
            get
            {
                string appSettingStr = ConfigurationManager.AppSettings[M_SMALL];
                int[] returnVal;
                if (string.IsNullOrEmpty(appSettingStr))
                    returnVal = Enumerable.Range(0, 50).ToArray();
                else
                {
                    var strArr = appSettingStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    returnVal = new int[strArr.Length];
                    for (int i = 0; i < strArr.Length; i++)
                        returnVal[i] = int.Parse(strArr[i]);
                }
                return returnVal;
            }
        }
        public int[] Big
        {
            get
            {
                string appSettingStr = ConfigurationManager.AppSettings[M_BIG];
                int[] returnVal;
                if (string.IsNullOrEmpty(appSettingStr))
                    returnVal = Enumerable.Range(50, 50).ToArray();
                else
                {
                    var strArr = appSettingStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    returnVal = new int[strArr.Length];
                    for (int i = 0; i < strArr.Length; i++)
                        returnVal[i] = int.Parse(strArr[i]);
                }
                return returnVal;
            }
        }
        public int[] Center
        {
            get
            {
                string appSettingStr = ConfigurationManager.AppSettings[M_CENTER];
                int[] returnVal;
                if (string.IsNullOrEmpty(appSettingStr))
                    returnVal = Enumerable.Range(30, 50).ToArray();
                else
                {
                    var strArr = appSettingStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    returnVal = new int[strArr.Length];
                    for (int i = 0; i < strArr.Length; i++)
                        returnVal[i] = int.Parse(strArr[i]);
                }
                return returnVal;
            }
        }
        /// <summary>
        /// 系统有效的会员 总数
        /// </summary>
        public int MemberCount
        {
            get
            {
                var count = HttpRuntime.Cache.Get(M_MEMBERCOUNT);
                if (count == null)
                {
                    var userManager = ManagerHelper.Instance.GetManager<UserManager>();
                    count = userManager.DaUser.CountMember();
                    //缓存2小时
                    HttpRuntime.Cache.Add(M_MEMBERCOUNT, count, null, Cache.NoAbsoluteExpiration, TimeSpan.FromHours(2), CacheItemPriority.Normal, null);
                }
                return Convert.ToInt32(count);
            }
        }
        /// <summary>
        /// 修改密码周期（单位:月，默认3）
        /// </summary>
        public int ChangePasswordCycle
        {
            get
            {
                string countStr = ConfigurationManager.AppSettings[M_CHANGEPASSWORDCYCLE];
                int countValue;

                return int.TryParse(countStr, out countValue) ? countValue : 3;
            }
        }
        /// <summary>
        /// 验证码字体
        /// </summary>
        public string CaptchaFontFamily
        {
            get
            {
                return ConfigurationManager.AppSettings[M_CAPTCHAFONTFAMILY] ?? "Consolas";
            }
        }

        public IList<LotteryCompany> AllCompanyList
        {
            get
            {
                var companyList = HttpRuntime.Cache[M_ALLCOMPANYLIST] as IList<LotteryCompany>;
                if (companyList == null || companyList.Count == 0)
                {
                    CompanyManager cm = ManagerHelper.Instance.GetManager<CompanyManager>();
                    companyList = cm.GetAllCompany().ToList();
                    HttpRuntime.Cache.Add(M_ALLCOMPANYLIST, companyList, null, Cache.NoAbsoluteExpiration, TimeSpan.FromDays(1), CacheItemPriority.Default, null);
                }
                return companyList;
            }
        }
        public GamePlayWay FindGamePlayWay(int gpwId)
        {
            GamePlayWay gpw;
            if (!GpwDic.TryGetValue(gpwId, out gpw))
            {
                lock (_lockHelper)
                {
                    if (!GpwDic.TryGetValue(gpwId, out gpw))
                    {
                        gpw = GamePlayWays.Find(it => it.Id == gpwId);
                        GpwDic.Add(gpwId, gpw);
                    }
                }
            }
            return gpw;
        }
        public GamePlayWay FindGamePlayWay(GameType gameType, PlayWay playWay)
        {
            string key = string.Format("{0}_{1}", (int)gameType, (int)playWay);
            GamePlayWay gpw;
            if (!MixKeyGWPDic.TryGetValue(key, out gpw))
            {
                lock (_lockHelper)
                {
                    if (!MixKeyGWPDic.TryGetValue(key, out gpw))
                    {
                        gpw = GamePlayWays.Find(it => it.GameType == gameType && it.PlayWay == playWay);
                        MixKeyGWPDic.Add(key, gpw);
                    }
                }
            }
            return gpw;
        }
        public IList<CompanyTypeSupportNumLen> GetNumLenSupport(CompanyType companyType)
        {
            if (!NumLenSupportDic.ContainsKey(companyType))
            {
                lock (_lockHelper)
                {
                    if (!NumLenSupportDic.ContainsKey(companyType))
                    {
                        var comManager = ManagerHelper.Instance.GetManager<CompanyManager>();
                        var numLenSupport = comManager.GetNumLenthByCompanyType(companyType).ToList();
                        NumLenSupportDic.Add(companyType, numLenSupport);
                    }
                }
            }
            return NumLenSupportDic[companyType];
        }
        public LotteryCompany FindCompany(int companyId)
        {
            LotteryCompany company;
            if (!ComDic.TryGetValue(companyId, out company))
            {
                company = AllCompanyList.Find(it => it.CompanyId == companyId);
                if (company == null)
                    throw new InvalidDataException("Company", string.Format("找不到公司:{0}", companyId));
                ComDic.Add(companyId, company);
            }
            return company;
        }
        /// <summary>
        ///清空所有公司列表缓存
        /// </summary>
        public static void ClearAllCompany()
        {
            HttpRuntime.Cache.Remove(M_ALLCOMPANYLIST);
            HttpRuntime.Cache[M_ALLCOMPANYLIST] = new object();//清空
            LotterySystem.Current.ComDic.Clear();
        }
    }
}
