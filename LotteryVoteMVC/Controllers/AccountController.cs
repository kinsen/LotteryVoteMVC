using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Core.Authorizes;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Utility;
using System.Text;
using LotteryVoteMVC.Core.Exceptions;
using LotteryVoteMVC.Core.Application;

namespace LotteryVoteMVC.Controllers
{
    public class AccountController : BaseController
    {
        private CommManager _commManager;
        private ShadowAuthManager _authManager;
        private UserNameBuilder _unBuilder;
        public CommManager CommManager
        {
            get
            {
                if (_commManager == null)
                    _commManager = new CommManager();
                return _commManager;
            }
        }
        public ShadowAuthManager AuthManager
        {
            get
            {
                if (_authManager == null)
                    _authManager = new ShadowAuthManager();
                return _authManager;
            }
        }
        public UserNameBuilder UNameBuilder
        {
            get
            {
                if (_unBuilder == null)
                    _unBuilder = new UserNameBuilder();
                return _unBuilder;
            }
        }
        public ShareRateGroupManager RateGroupManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<ShareRateGroupManager>();
            }
        }

        [AgentAuthorize(UserState.Active)]
        public ActionResult Index()
        {
            var childs = UserManager.GetChilds(MatrixUser).Where(it => it.UserInfo.State == UserState.Active);
            ViewBag.Matrix = MatrixUser;
            ViewBag.User = MatrixUser;
            ViewBag.Family = new[] { MatrixUser };
            return View(childs);
        }
        [AgentAuthorize(UserState.Active)]
        public ActionResult Child(int? Id)
        {
            User parent = MatrixUser;
            IEnumerable<User> family = new[] { MatrixUser };
            if (Id.HasValue)
            {
                var familys = UserManager.GetFamily(Id.Value);
                if (familys.Contains(it => it.UserId == Id.Value))
                {
                    var user = familys.Find(it => it.UserId == Id.Value);
                    if (user != null) parent = user;
                    family = familys.Where(it => it.UserId >= MatrixUser.UserId).OrderBy(it => it.UserId);
                }
            }
            var childs = UserManager.GetChilds(parent).Where(it => it.UserInfo.State == UserState.Active);
            ViewBag.Matrix = MatrixUser;
            ViewBag.User = parent;
            ViewBag.CurrentUser = CurrentUser;
            ViewBag.Family = family;
            return View("Index", childs);
        }

        [AgentAuthorize(UserState.Active)]
        public ActionResult Shadow(int? Id)
        {
            User target;
            if (!(Id.HasValue && UserManager.IsParent(MatrixUser.UserId, Id.Value, out target)))
                target = CurrentUser;
            var shadows = UserManager.GetShadows(target);
            ViewBag.User = target;
            ViewBag.Matrix = MatrixUser;
            ViewBag.IsShadow = true;
            return View("Index", shadows);
        }
        [AgentAuthorize(UserState.Active)]
        public ActionResult Search(int? Id, FormCollection collect, bool shadow = false)
        {
            User parent;
            if (!Id.HasValue || !UserManager.IsParent(MatrixUser.UserId, Id.Value, out parent))
                parent = MatrixUser;
            string name = collect[UserInfo.NAME];
            string username = collect[LotteryVoteMVC.Models.User.USERNAME];
            var state = EnumHelper.GetEnum<UserState>(collect[UserInfo.STATE]);
            string sortField = collect["LastLoginTime"];
            ViewBag.User = parent;
            ViewBag.Matrix = MatrixUser;
            var result = shadow ?
                UserManager.GetShadowByCondition(username, name, state, sortField, MatrixUser) :
                UserManager.GetUserByCondition(username, name, state, sortField, parent);
            return View("Index", result);
        }
        #region Add
        [AgentAuthorize(UserState.Active)]
        public ActionResult Add()
        {
            ViewBag.User = MatrixUser;
            ViewBag.NextName = UNameBuilder.GetNextUserName(MatrixUser);
            ViewBag.UserState = EnumHelper.ToSelectList<UserState>(
                it => it == UserState.Active,
                it => Resource.ResourceManager.GetString(it.ToString()),
                it => it.ToString());
            ViewBag.Species = EnumHelper.ToMultiSelectList<LotterySpecies>(
                it => true,
                it => Resource.ResourceManager.GetString(it.ToString()),
                it => it.ToString());
            if (MatrixUser.Role == Role.Agent)
                ViewBag.CommGroup = CommManager.GetVNVoteCommGroup().Select(it => new SelectListItem
                {
                    Text = it.GroupName,
                    Value = it.GroupId.ToString()
                });
            ViewBag.RateGroups = RateGroupManager.ListChildGroup(MatrixUser.UserInfo.RateGroup.ShareRate).Select(it => new SelectListItem { Text = it.Name, Value = it.Id.ToString() });
            var model = new RegisterModel
            {
                MinShareRate = 0,
                MinGivenCredit = 0,
                MaxShareRate = MatrixUser.UserInfo.RateGroupId * 100,
                MaxGivenCredit = MatrixUser.UserInfo.AvailableGivenCredit
            };
            return View(model);
        }
        [HttpPost]
        [AgentAuthorize(UserState.Active)]
        public ActionResult Add(RegisterModel model)
        {
            if (model.RateGroupId == 0)
                model.RateGroupId = CurrentUser.UserInfo.RateGroupId;
            JsonResultModel result = new JsonResultModel();
            if (!(result.IsSuccess = ModelState.IsValid))
            {
                result.Message = ModelState.ToErrorString();
                return Json(result);
            }
            if (!EncryptHelper.Equal(model.YourPassword, CurrentUser.UserInfo.Password))
            {
                result.IsSuccess = false;
                result.Message = Resource.PasswordError;
                return Json(result);
            }
            try
            {
                AddUser(model);
                result.Message = Resource.Success;
                result.Model = model;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = ex.Message;
                //TODO:筛选各种异常，并记录异常信息
            }
            return Json(result);
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Super, Role.Master, Role.Agent)]
        public ActionResult AddShadow()
        {
            ViewBag.NextName = UNameBuilder.GetNextUserName(MatrixUser, true);
            ViewBag.CurrentUser = CurrentUser;
            ViewBag.AuthActions = AuthManager.GetAuthByUser(CurrentUser);
            return View();
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Super, Role.Master, Role.Agent), HttpPost]
        public ActionResult AddShadow(ShadowModel model)
        {
            JsonResultModel result = new JsonResultModel();
            if (!(result.IsSuccess = ModelState.IsValid))
            {
                result.Message = ModelState.ToErrorString();
            }
            else
            {
                if (result.IsSuccess = EncryptHelper.Equal(model.YourPassword, CurrentUser.UserInfo.Password))
                {
                    var user = model.ToUser();
                    user.ParentId = CurrentUser.UserId;
                    user.UserInfo.RateGroupId = MatrixUser.UserInfo.RateGroupId;
                    UserManager.AddShadow(user, model.AuthActions);
                    ActionLogger.Log(CurrentUser, user, LogResources.AddShadow, LogResources.GetAddShadow(model.UserName));
                    result.Message = Resource.Success;
                    result.Model = model;
                }
                else
                    result.Message = Resource.PasswordError;
            }
            return Json(result);
        }
        private void AddUser(RegisterModel model)
        {
            var user = model.ToUser();
            user.ParentId = MatrixUser.UserId;
            switch (MatrixUser.Role)
            {
                case Role.Company: UserManager.AddSuper(user, model.Species); break;
                case Role.Super: UserManager.AddMaster(user, model.Species); break;
                case Role.Master: UserManager.AddAgent(user, model.Species); break;
                case Role.Agent: UserManager.AddMember(user, model.Species, model.CommGroup); break;
                default: throw new NoPermissionException("Add User", string.Format("User:{0},Role:{1}", CurrentUser.UserName, CurrentUser.Role));
            }
            ActionLogger.Log(CurrentUser, user, LogResources.AddUser, LogResources.GetAddUser(user.UserName));
        }
        #endregion
        #region Edit
        [AgentAuthorize(UserState.Active)]
        public ActionResult Edit(int Id = 0)
        {
            if (Id == 0) PageNotFound();
            User user = null;
            if (!UserManager.IsParent(MatrixUser.UserId, Id, out user)) PageNotFound();
            ViewBag.CurrentUser = MatrixUser;
            ViewBag.Species = EnumHelper.ToMultiSelectList<LotterySpecies>(
                it => true,
                it => Resource.ResourceManager.GetString(it.ToString()),
                it => it.ToString());
            ViewBag.UserState = EnumHelper.ToSelectList<UserState>(
                it => it == user.UserInfo.State,
                it => Resource.ResourceManager.GetString(it.ToString()),
                it => it.ToString()).ToList();
            var parent = user.ParentId != 0 ? UserManager.GetUser(user.ParentId) : MatrixUser;
            var rateGroups = RateGroupManager.ListChildGroup(parent.UserInfo.RateGroup.ShareRate == 0 ? 1 : parent.UserInfo.RateGroup.ShareRate);
            if (parent.UserInfo.RateGroup.ShareRate != 0 && parent.UserInfo.RateGroup.ShareRate != 1)
                rateGroups = rateGroups.Where(it => it.ShareRate != 0);
            ViewBag.RateGroups = rateGroups.Select(it =>
                new SelectListItem { Text = it.Name, Value = it.Id.ToString(), Selected = it.Id == user.UserInfo.RateGroupId });
            ViewBag.CanEditShareRate = CanEditShareRate();
            if (user.Role == Role.Guest)
            {
                var pack = CommManager.GetMemberPackage(user, LotterySpecies.VietnamLottery);//当前只有越南彩有
                ViewBag.CommGroup = CommManager.GetVNVoteCommGroup().Select(it => new SelectListItem
                {
                    Selected = it.GroupId == pack.GroupId,
                    Text = it.GroupName,
                    Value = it.GroupId.ToString()
                });
            }
            return View(user);
        }
        [HttpPost]
        [AgentAuthorize(UserState.Active)]
        public ActionResult Edit(int Id, User model, FormCollection form)
        {
            JsonResultModel result = new JsonResultModel();
            if (!(result.IsSuccess = ModelState.IsValid))
            {
                result.Message = ModelState.ToErrorString();
            }
            else
            {
                var user = UserManager.GetUser(Id);
                var shareRate = user.UserInfo.RateGroupId;
                if (user.ParentId != MatrixUser.UserId && CurrentUser.Role != Role.Company) throw new NoPermissionException("Update User");
                if (result.IsSuccess = EncryptHelper.Equal(model.UserInfo.Password, CurrentUser.UserInfo.Password))
                {
                    UpdateModel(user);
                    user.UserInfo.RateGroup = RateGroupManager.GetGroup(model.UserInfo.RateGroupId);
                    Dictionary<LotterySpecies, int> groups = null;
                    if (user.Role == Role.Guest)
                    {
                        groups = new Dictionary<LotterySpecies, int>();
                        groups.Add(LotterySpecies.VietnamLottery, int.Parse(form["MemberPack"]));
                    }
                    bool changeShareRate;
                    var update_user = UserManager.UpdateUser(user, groups, out changeShareRate);
                    var change_rate_str = changeShareRate ? string.Format(", Change Share Rate : from {0}", shareRate) : string.Empty;
                    ActionLogger.Log(CurrentUser, user, LogResources.EditUser, LogResources.GetEditUser(user.UserName) + change_rate_str);
                    result.Message = Resource.Success;
                    if (user.UserInfo.RateGroupId > update_user.UserInfo.RateGroupId)
                    {
                        result.Message = Resource.ShareRate + " " + Resource.Error;
                        return Json(result);
                    }
                }
                else
                    result.Message = Resource.PasswordError;
            }
            return Json(result);
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Super, Role.Master, Role.Agent)]
        public ActionResult EditShadow(int Id = 0)
        {
            if (Id == 0) PageNotFound();
            User user = null;
            if (!UserManager.IsParent(CurrentUser.UserId, Id, out user))
                PageNotFound();
            ViewBag.CurrentUser = CurrentUser;
            ViewBag.UserState = EnumHelper.ToSelectList<UserState>(it => it == user.UserInfo.State, it => Resource.ResourceManager.GetString(it.ToString()), it => it.ToString());
            ViewBag.AuthActions = AuthManager.GetAuthByUser(CurrentUser);
            ViewBag.SelectedAction = AuthManager.GetShadowAuths(user).Select(it => it.AuthorizeId.ToString()).ToArray();
            return View(new ShadowModel(user));
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Super, Role.Master, Role.Agent), HttpPost]
        public ActionResult EditShadow(ShadowModel model, int Id = 0)
        {
            if (Id == 0) PageNotFound();
            ModelState.Remove("UserName");
            ModelState.Remove("Password");
            JsonResultModel result = new JsonResultModel();
            if (!(result.IsSuccess = ModelState.IsValid))
            {
                result.Message = ModelState.ToErrorString();
            }
            else
            {
                var user = UserManager.GetUser(Id);
                if (user == null || user.ParentId != CurrentUser.UserId)
                    throw new NoPermissionException("Update Shadow");
                if (result.IsSuccess = EncryptHelper.Equal(model.YourPassword, CurrentUser.UserInfo.Password))
                {
                    user.UserInfo.Name = model.Name;
                    user.UserInfo.State = model.State;
                    user.UserInfo.Email = model.Email;
                    UserManager.UpdateShadow(user, model.AuthActions);
                    ActionLogger.Log(CurrentUser, user, LogResources.EditShadow, LogResources.GetEditShadow(user.UserName));
                    result.Message = Resource.Success;
                }
                else
                    result.Message = Resource.PasswordError;
            }
            return Json(result);
        }

        #endregion

        [UserAuthorize(UserState.Active, Role.Company, Role.Super, Role.Master, Role.Agent, Role.Guest, Role.Shadow, Role.Master, IsNormal = true)]
        public ActionResult ChangePassword()
        {
            return View();
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Super, Role.Master, Role.Agent, Role.Guest, Role.Shadow, Role.Master, IsNormal = true), HttpPost]
        public ActionResult ChangePassword(int? Id, PwdModel model)
        {
            if (!ModelState.IsValid)
                if (Request.IsAjaxRequest()) throw new BusinessException(ModelState.ToErrorString());
                else return View(model);
            User target = CurrentUser;
            if (Id.HasValue && (!UserManager.IsParent(MatrixUser.UserId, Id.Value, out target)))
                target = CurrentUser;
            UserManager.UpdatePwd(target, model.Password);
            ActionLogger.Log(CurrentUser, target, LogResources.UpdatePassword, LogResources.GetUpdatePassword(target.UserName));
            if (Request.IsAjaxRequest())
                return Json(new JsonResultModel
                {
                    IsSuccess = true,
                    Message = Resource.Success
                });
            return SuccessAction();
        }
        [AgentAuthorize(UserState.Active)]
        public ActionResult Start(int Id = 0)
        {
            if (Id == 0) PageNotFound();

            User user = null;
            if (!UserManager.IsParent(MatrixUser.UserId, Id, out user)) PageNotFound();

            UserManager.ChangeState(user, UserState.Active);
            ActionLogger.Log(CurrentUser, user, LogResources.StartUser, LogResources.GetStartUser(user.UserName));
            return Redirect(Request.UrlReferrer.ToString());
        }
        [AgentAuthorize(UserState.Active)]
        public ActionResult Stop(int Id = 0)
        {
            if (Id == 0) PageNotFound();

            User user = null;
            if (!UserManager.IsParent(MatrixUser.UserId, Id, out user)) PageNotFound();

            UserManager.ChangeFamilyState(user, UserState.Suspended);
            ActionLogger.Log(CurrentUser, user, LogResources.StopUser, LogResources.GetStopUser(user.UserName));
            return Redirect(Request.UrlReferrer.ToString());
        }

        /// <summary>
        /// 能否修改分成信息
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can edit share rate]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanEditShareRate()
        {
            //只有在周日晚上8点之后才可以修改
            var dateInfo = LotterySystem.Current.EnableShareRateEdit;
            //return DateTime.Now > DateTime.Today.AddHours(dateInfo[1]);
            var isToday = DateTime.Today.DayOfWeek == (DayOfWeek)dateInfo[0] && DateTime.Now > DateTime.Today.AddHours(dateInfo[1]);
            var isTomorrow = DateTime.Today.DayOfWeek == (DayOfWeek)dateInfo[0] + 1 && DateTime.Now.Hour < 6;
            return isToday || isTomorrow;
        }
    }
}
