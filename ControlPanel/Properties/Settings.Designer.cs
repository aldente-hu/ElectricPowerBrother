﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.34209
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace HirosakiUniversity.Aldente.ElectricPowerBrother.ControlPanel.Properties {
    
    
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
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\Progra~1\\gnuplot\\bin\\gnuplot.exe")]
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
        [global::System.Configuration.DefaultSettingValueAttribute("B:\\data\\")]
        public string DataRoot {
            get {
                return ((string)(this["DataRoot"]));
            }
            set {
                this["DataRoot"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B:\\detail\\")]
        public string DetailRoot {
            get {
                return ((string)(this["DetailRoot"]));
            }
            set {
                this["DetailRoot"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B:\\index_template.html")]
        public string IndexPageTemplate {
            get {
                return ((string)(this["IndexPageTemplate"]));
            }
            set {
                this["IndexPageTemplate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B:\\index.html")]
        public string IndexPageDestination {
            get {
                return ((string)(this["IndexPageDestination"]));
            }
            set {
                this["IndexPageDestination"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("B:\\loggers_config.xml")]
        public string DataLoggersConfig {
            get {
                return ((string)(this["DataLoggersConfig"]));
            }
            set {
                this["DataLoggersConfig"] = value;
            }
        }
    }
}
