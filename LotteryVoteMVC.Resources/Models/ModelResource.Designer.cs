﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.225
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace LotteryVoteMVC.Resources.Models {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ModelResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ModelResource() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LotteryVoteMVC.Resources.Models.ModelResource", typeof(ModelResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   使用此强类型资源类，为所有资源查找
        ///   重写当前线程的 CurrentUICulture 属性。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 超出范围{0}~{1} 的本地化字符串。
        /// </summary>
        public static string DynamicRange {
            get {
                return ResourceManager.GetString("DynamicRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 请输入正确的Email地址 的本地化字符串。
        /// </summary>
        public static string EmailError {
            get {
                return ResourceManager.GetString("EmailError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 ^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$ 的本地化字符串。
        /// </summary>
        public static string EmailRule {
            get {
                return ResourceManager.GetString("EmailRule", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 ^\d+(\.\d+)?$ 的本地化字符串。
        /// </summary>
        public static string FloatNumRule {
            get {
                return ResourceManager.GetString("FloatNumRule", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 ^\d+$ 的本地化字符串。
        /// </summary>
        public static string IntegerNumRule {
            get {
                return ResourceManager.GetString("IntegerNumRule", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 字段{0}必须是纯数字 的本地化字符串。
        /// </summary>
        public static string MustBeNum {
            get {
                return ResourceManager.GetString("MustBeNum", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 {0}必须大于{1} 的本地化字符串。
        /// </summary>
        public static string MustGreatThan {
            get {
                return ResourceManager.GetString("MustGreatThan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 {0}必须小于{1} 的本地化字符串。
        /// </summary>
        public static string MustLessThan {
            get {
                return ResourceManager.GetString("MustLessThan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 {0}超过范围{1}~{2} 的本地化字符串。
        /// </summary>
        public static string OverOutRange {
            get {
                return ResourceManager.GetString("OverOutRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 请选择一个{0} 的本地化字符串。
        /// </summary>
        public static string PleaseSelected {
            get {
                return ResourceManager.GetString("PleaseSelected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 已输入错误密码{0}次！ 的本地化字符串。
        /// </summary>
        public static string PwdErrorTimes {
            get {
                return ResourceManager.GetString("PwdErrorTimes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 字段{0}必填 的本地化字符串。
        /// </summary>
        public static string Required {
            get {
                return ResourceManager.GetString("Required", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 该字段长度必须最短{2},最长{1} 的本地化字符串。
        /// </summary>
        public static string StringLengthRange {
            get {
                return ResourceManager.GetString("StringLengthRange", resourceCulture);
            }
        }
    }
}
