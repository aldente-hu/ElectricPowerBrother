﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.34209
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B:\\test.xml")]
        public string DailyXmlDestination {
            get {
                return ((string)(this["DailyXmlDestination"]));
            }
            set {
                this["DailyXmlDestination"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B:\\ep.sqlite3")]
        public string DatabaseFile {
            get {
                return ((string)(this["DatabaseFile"]));
            }
            set {
                this["DatabaseFile"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B:\\4hours.xml")]
        public string DetailXmlDestination {
            get {
                return ((string)(this["DetailXmlDestination"]));
            }
            set {
                this["DetailXmlDestination"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B:\\plot_riko_template.plt")]
        public string PltTemplatePath {
            get {
                return ((string)(this["PltTemplatePath"]));
            }
            set {
                this["PltTemplatePath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B:\\plot_riko.plt")]
        public string PltOutputPath {
            get {
                return ((string)(this["PltOutputPath"]));
            }
            set {
                this["PltOutputPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string GnuplotBinaryPath {
            get {
                return ((string)(this["GnuplotBinaryPath"]));
            }
            set {
                this["GnuplotBinaryPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B:\\test.csv")]
        public string TrinityCsvDestination {
            get {
                return ((string)(this["TrinityCsvDestination"]));
            }
            set {
                this["TrinityCsvDestination"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B:\\trinity.svg")]
        public string TrinityChartDestination {
            get {
                return ((string)(this["TrinityChartDestination"]));
            }
            set {
                this["TrinityChartDestination"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B:\\ElectricPower")]
        public string TrinityDataRootPath {
            get {
                return ((string)(this["TrinityDataRootPath"]));
            }
            set {
                this["TrinityDataRootPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("public\\himichu\\trinity.svg")]
        public string TrinitySvgOutputPath {
            get {
                return ((string)(this["TrinitySvgOutputPath"]));
            }
            set {
                this["TrinitySvgOutputPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B:\\24hours.xml")]
        public string LatestXmlDestination {
            get {
                return ((string)(this["LatestXmlDestination"]));
            }
            set {
                this["LatestXmlDestination"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("public\\himichu\\today_temperature.csv")]
        public string TemperatureCsvPath {
            get {
                return ((string)(this["TemperatureCsvPath"]));
            }
            set {
                this["TemperatureCsvPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B:\\latest.atom")]
        public string AtomDestination {
            get {
                return ((string)(this["AtomDestination"]));
            }
            set {
                this["AtomDestination"] = value;
            }
        }
    }
}
