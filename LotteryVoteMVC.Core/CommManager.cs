using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Data.Commission;
using LotteryVoteMVC.Core.Exceptions;
using System.Web;

namespace LotteryVoteMVC.Core
{
    /// <summary>
    /// 佣金管理器
    /// </summary>
    public class CommManager : ManagerBase
    {
        private IList<GameType> _gameTypeList;
        private IList<CompanyType> _companyTypeList;
        private CommissionGroupDataAccess _daCommGroup;
        private ConcreteCommissionDataAccess _daConcreteComm;
        private UserCommissionDataAccess _daUserComm;
        private CommissionValueDataAccess _daCommValue;
        private MemberPackageDataAccess _daMemPackage;

        public IList<GameType> GameTypeList
        {
            get
            {
                if (_gameTypeList == null || _gameTypeList.Count == 0)
                    _gameTypeList = EnumHelper.GetAll<GameType>().ToList();
                return _gameTypeList;
            }
        }
        public IList<CompanyType> CompanyTypeList
        {
            get
            {
                if (_companyTypeList == null || _companyTypeList.Count == 0)
                    _companyTypeList = EnumHelper.GetAll<CompanyType>().ToList();
                return _companyTypeList;
            }
        }
        public CommissionGroupDataAccess DaCommGroup
        {
            get
            {
                if (_daCommGroup == null)
                    _daCommGroup = new CommissionGroupDataAccess();
                return _daCommGroup;
            }
        }
        public ConcreteCommissionDataAccess DaConcreteComm
        {
            get
            {
                if (_daConcreteComm == null)
                    _daConcreteComm = new ConcreteCommissionDataAccess();
                return _daConcreteComm;
            }
        }
        public UserCommissionDataAccess DaUserComm
        {
            get
            {
                if (_daUserComm == null)
                    _daUserComm = new UserCommissionDataAccess();
                return _daUserComm;
            }
        }
        public CommissionValueDataAccess DaCommValue
        {
            get
            {
                if (_daCommValue == null)
                    _daCommValue = new CommissionValueDataAccess();
                return _daCommValue;
            }
        }
        public MemberPackageDataAccess DaMemPackage
        {
            get
            {
                if (_daMemPackage == null)
                    _daMemPackage = new MemberPackageDataAccess();
                return _daMemPackage;
            }
        }

        #region 佣金组
        /// <summary>
        /// 添加新佣金组
        /// </summary>
        /// <param name="specie">The specie.</param>
        /// <param name="name">The name.</param>
        /// <param name="comms">The comms.</param>
        public void AddCommGroup(LotterySpecies specie, string name, IEnumerable<ConcreteCommission> comms)
        {
            var group = new CommissionGroup
            {
                Specie = specie,
                GroupName = name
            };
            DaCommGroup.ExecuteWithTransaction(() =>
            {
                DaCommGroup.Insert(group);
                comms = comms.ForEach(it => { it.GroupId = group.GroupId; return it; });
                DaConcreteComm.InsertComms(comms);
            });
        }
        /// <summary>
        ///修改指定佣金组佣金信息
        /// </summary>
        /// <param name="groupId">The group id.</param>
        /// <param name="comms">The comms.</param>
        public void UpdateCommGroup(int groupId, IEnumerable<ConcreteCommission> comms)
        {
            comms = comms.ForEach(it => { it.GroupId = groupId; return it; });
            DaCommGroup.ExecuteWithTransaction(() =>
            {
                DaConcreteComm.DeleteComms(groupId);
                DaConcreteComm.InsertComms(comms);
            });
        }
        public void RemoveCommGroup(int groupId)
        {
            DaCommGroup.Delete(groupId);
        }
        /// <summary>
        /// 获取越南彩佣金组
        /// </summary>
        /// <param name="specie">The specie.</param>
        /// <returns></returns>
        public IEnumerable<CommissionGroup> GetVNVoteCommGroup()
        {
            return DaCommGroup.GetCommissionGroupBySpecie(LotterySpecies.VietnamLottery);
        }
        public IEnumerable<CommissionGroup> GetCommGroups(params int[] groups)
        {
            return DaCommGroup.GetCommissionGroupByGroupId(groups);
        }
        /// <summary>
        /// 根据GroupId获取分组的佣金信息
        /// </summary>
        /// <param name="groupId">The group id.</param>
        /// <returns></returns>
        public Pair<CommissionGroup, IEnumerable<ConcreteCommission>> GetGroupComms(int groupId)
        {
            var group = DaCommGroup.GetCommissionGroup(groupId);
            if (group == null) return null;
            var comms = DaConcreteComm.GetConcreteCommission(group);
            return new Pair<CommissionGroup, IEnumerable<ConcreteCommission>>(group, comms);
        }
        #endregion

        #region 用户佣金
        /// <summary>
        /// 获取指定用户特定市场的具体佣金组佣金
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="specie">The specie.</param>
        /// <returns></returns>
        public IDictionary<CommissionGroup, IEnumerable<ConcreteCommission>> GetCommissionGroupByUser(User user, LotterySpecies specie)
        {
            //1.获取指定市场的佣金分组
            var groups = DaCommGroup.GetCommissionGroupBySpecie(specie);
            var concreteComms = DaConcreteComm.GetConcreteCommission(specie).GroupBy(it => it.GroupId);
            //2.获取各父级的佣金差
            var familyCommVals = DaCommValue.GetFamilyUserCommissionValue(user, specie);
            //3.累加各级的佣金差 
            Dictionary<string, CommissionValue> exaCommValDic = new Dictionary<string, CommissionValue>();
            foreach (var commVal in familyCommVals)
            {
                var key = string.Format("{0}_{1}", commVal.GameId, commVal.CompanyTypeId);
                CommissionValue exaCommVal = null;
                if (exaCommValDic.TryGetValue(key, out exaCommVal))
                    exaCommVal.Comm += commVal.Comm;
                else
                    exaCommValDic.Add(key, commVal);
            }
            //4.计算该用户不同佣金组的实际佣金
            Dictionary<CommissionGroup, IEnumerable<ConcreteCommission>> result = new Dictionary<CommissionGroup, IEnumerable<ConcreteCommission>>();
            foreach (var gp in concreteComms)
            {
                var group = groups.Find(it => it.GroupId == gp.Key);
                List<ConcreteCommission> commList = new List<ConcreteCommission>();
                foreach (var cm in gp)
                {
                    var key = string.Format("{0}_{1}", (int)cm.GameType, (int)cm.CompanyType);

                    CommissionValue exaCommVal;
                    if (exaCommValDic.TryGetValue(key, out exaCommVal))
                    {
                        cm.Commission -= exaCommVal.Comm;
                        cm.Odds -= exaCommVal.Odds;
                    }
                    commList.Add(cm);
                }
                result.Add(group, commList);
            }
            return result;
        }
        /// <summary>
        /// 获取会员各父级佣金信息
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="specie">The specie.</param>
        /// <returns></returns>
        public IDictionary<Role, IEnumerable<ConcreteCommission>> GetParentsCommission(User member, LotterySpecies specie)
        {
            if (member.Role != Role.Guest) throw new InvalidDataException("Role", string.Format("用户必须是Guest,User:{0}", member));
            //1.获取会员所在分组
            var commGroup = DaCommGroup.GetMemebersCommGroup(member, specie);
            if (commGroup == null)
                throw new ApplicationException(string.Format("找不到会员{0}的佣金组信息!", member));
            //2.获取分组的信息
            var comms = DaConcreteComm.GetConcreteCommission(commGroup);
            //3.获取各父级佣金差
            var userManager = ManagerHelper.Instance.GetManager<UserManager>();
            var parents = userManager.GetFamily(member.ParentId);   //从父级开始查找

            Dictionary<Role, IEnumerable<ConcreteCommission>> resultComm = new Dictionary<Role, IEnumerable<ConcreteCommission>>(); //各级真实佣金信息
            Dictionary<Role, IEnumerable<CommissionValue>> roleComm = new Dictionary<Role, IEnumerable<CommissionValue>>();   //各级佣金差
            foreach (var parent in parents.OrderBy(it => it.UserId))
            {
                if (parent.Role == Role.Company) continue;  //公司特殊计算

                var userComms = GetCommissionValue(parent, specie); //获取父级的佣金查
                //a.计算佣金差
                Dictionary<string, CommissionValue> exaCommValDic = new Dictionary<string, CommissionValue>();
                foreach (var comm in userComms)
                {
                    var key = string.Format("{0}_{1}", comm.GameId, comm.CompanyTypeId);
                    CommissionValue exaCommVal = null;
                    if (exaCommValDic.TryGetValue(key, out exaCommVal))
                        exaCommVal.Comm += comm.Comm;
                    else
                        exaCommValDic.Add(key, comm);
                }
                roleComm.Add(parent.Role, exaCommValDic.Values);
                //b.累加上级+本级佣金差，获得本级真实佣金
                List<ConcreteCommission> commList = new List<ConcreteCommission>();
                foreach (var cm in comms)
                {
                    var key = string.Format("{0}_{1}", (int)cm.GameType, (int)cm.CompanyType);
                    //计算出上级的佣金差
                    var parentDiff = roleComm.Where(it => it.Key <= parent.Role)
                        .Select(it => it.Value.Where(x => x.GameType == cm.GameType && x.CompanyType == cm.CompanyType).Sum(x => x.Comm))
                        .Sum(it => it);
                    cm.Commission -= parentDiff;
                    //cm.Odds -= exaCommVal.Odds;
                    commList.Add(cm);
                }
                resultComm.Add(parent.Role, commList);
            }

            //计算公司佣金=100-super佣金
            var superComms = resultComm[Role.Super];
            List<ConcreteCommission> companyCommList = new List<ConcreteCommission>();
            foreach (var comm in superComms)
                companyCommList.Add(new ConcreteCommission
                {
                    GameType = comm.GameType,
                    CompanyType = comm.CompanyType,
                    Commission = 100 - comm.Commission,
                    Odds = comm.Odds
                });
            resultComm.Add(Role.Company, companyCommList);

            return resultComm;
        }
        /// <summary>
        /// 获取会员的佣金信息
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="specie">The specie.</param>
        /// <returns></returns>
        public Pair<CommissionGroup, IEnumerable<ConcreteCommission>> GetMemberCommission(User member, LotterySpecies specie)
        {
            if (member.Role != Role.Guest) throw new InvalidDataException("Role", string.Format("用户必须是Guest,User:{0}", member));
            //1.获取会员所在分组
            var commGroup = DaCommGroup.GetMemebersCommGroup(member, specie);
            if (commGroup == null)
                throw new ApplicationException(string.Format("找不到会员{0}的佣金组信息!", member));
            //2.获取分组的信息
            var comms = DaConcreteComm.GetConcreteCommission(commGroup);
            //3.获取父级佣金差
            var familyCommVals = DaCommValue.GetFamilyUserCommissionValue(member, specie);
            //4.累加各级的佣金差 
            Dictionary<string, CommissionValue> exaCommValDic = new Dictionary<string, CommissionValue>();
            foreach (var commVal in familyCommVals)
            {
                var key = string.Format("{0}_{1}", commVal.GameId, commVal.CompanyTypeId);
                CommissionValue exaCommVal = null;
                if (exaCommValDic.TryGetValue(key, out exaCommVal))
                    exaCommVal.Comm += commVal.Comm;
                else
                    exaCommValDic.Add(key, commVal);
            }
            var memberComms = comms.ForEach(it =>
            {
                var key = string.Format("{0}_{1}", it.GameId, it.CompanyTypeId);
                CommissionValue exaCommVal = null;
                if (exaCommValDic.TryGetValue(key, out exaCommVal))
                    it.Commission -= exaCommVal.Comm;
                return it;
            });
            return new Pair<CommissionGroup, IEnumerable<ConcreteCommission>>(commGroup, memberComms);
        }
        /// <summary>
        /// 在Session中获取会员的佣金信息，若佣金信息不存在，则读取数据库中的佣金信息
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="specie">The specie.</param>
        /// <returns></returns>
        public Pair<CommissionGroup, IEnumerable<ConcreteCommission>> GetMemberCommissionInSession(User member, LotterySpecies specie)
        {
            string key = string.Format("{0}_{1}_COMM", member.UserId, (int)specie);
            var comm = HttpContext.Current.Session[key] as Pair<CommissionGroup, IEnumerable<ConcreteCommission>>;
            if (comm == null)
            {
                comm = GetMemberCommission(member, specie);
                HttpContext.Current.Session.Add(key, comm);
            }
            return comm;
        }
        #endregion

        #region 佣金差
        /// <summary>
        /// 为用户添加默认的佣金差
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="species">The species.</param>
        public void AddDefaultUserCommission(User user, IEnumerable<LotterySpecies> species)
        {
            List<CommissionValue> commValueList = new List<CommissionValue>();
            foreach (var specie in species)
            {
                UserCommission userComm = new UserCommission
                {
                    Specie = specie,
                    UserId = user.UserId
                };
                DaUserComm.InsertUserCommission(userComm);
                commValueList.AddRange(BuildDefaultCommValueList(userComm));
            }
            //批量插入佣金差
            DaCommValue.InsertCommValues(commValueList);
        }
        /// <summary>
        /// 更新用户特定市场的佣金差（先删除，再插入）
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="specie">The specie.</param>
        /// <param name="commValues">The comm values.</param>
        public void UpdateUserCommission(User user, LotterySpecies specie, IEnumerable<CommissionValue> commValues)
        {
            var userComm = DaUserComm.GetUserCommission(user, specie);
            commValues = commValues.ForEach(it => { it.CommissionId = userComm.CommissionId; return it; });
            DaCommValue.ExecuteWithTransaction(() =>
            {
                DaCommValue.DeleteCommValues(user, specie);
                DaCommValue.InsertCommValues(commValues);
            });
        }
        /// <summary>
        ///获取用户特定市场的佣金差
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="specie">The specie.</param>
        /// <returns></returns>
        public IEnumerable<CommissionValue> GetCommissionValue(User user, LotterySpecies specie)
        {
            return DaCommValue.GetCommissionValue(user, specie);
        }
        /// <summary>
        /// 根据用户佣金创建佣金差集合
        /// </summary>
        /// <param name="userComm">The user comm.</param>
        /// <returns></returns>
        private IEnumerable<CommissionValue> BuildDefaultCommValueList(UserCommission userComm)
        {
            foreach (var companyType in CompanyTypeList)
                foreach (var gameType in GameTypeList)
                    yield return new CommissionValue
                    {
                        CommissionId = userComm.CommissionId,
                        CompanyType = companyType,
                        GameType = gameType
                    };
        }
        #endregion

        #region MemberPackage
        public void AddMemberPackage(User user, CommissionGroup commGroup)
        {
            DaMemPackage.InsertMemberPackage(user, commGroup);
        }
        public void UpdateMemberPackage(User user, CommissionGroup group)
        {
            DaMemPackage.ExecuteWithTransaction(() =>
            {
                DaMemPackage.DeleteMemeberPackage(user, group.Specie);
                DaMemPackage.InsertMemberPackage(user, group);
            });
        }
        public MemberPackage GetMemberPackage(User member, LotterySpecies specie)
        {
            if (member.Role != Role.Guest) throw new InvalidDataException("MemberPackage", string.Format("用户{0}没有佣金组信息", member.UserId));
            return DaMemPackage.GetMemberPackageBySpecie(member, specie);
        }
        #endregion
    }
}
