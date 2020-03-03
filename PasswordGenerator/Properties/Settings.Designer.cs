﻿namespace PasswordGenerator.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.0.1.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("16")]
        public int PasswordGeneratorLength {
            get {
                return ((int)(this["PasswordGeneratorLength"]));
            }
            set {
                this["PasswordGeneratorLength"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int PasswordGeneratorMinSymbols {
            get {
                return ((int)(this["PasswordGeneratorMinSymbols"]));
            }
            set {
                this["PasswordGeneratorMinSymbols"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int PasswordGeneratorMinUpperCharacters {
            get {
                return ((int)(this["PasswordGeneratorMinUpperCharacters"]));
            }
            set {
                this["PasswordGeneratorMinUpperCharacters"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int PasswordGeneratorMinLowerCharacters {
            get {
                return ((int)(this["PasswordGeneratorMinLowerCharacters"]));
            }
            set {
                this["PasswordGeneratorMinLowerCharacters"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int PasswordGeneratorMinDigits {
            get {
                return ((int)(this["PasswordGeneratorMinDigits"]));
            }
            set {
                this["PasswordGeneratorMinDigits"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("abcdefghijklmnopqrstuvwxyz")]
        public string PasswordGeneratorLowerCharacters {
            get {
                return ((string)(this["PasswordGeneratorLowerCharacters"]));
            }
            set {
                this["PasswordGeneratorLowerCharacters"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ABCDEFGHIJKLMNOPQRSTUVWXYZ")]
        public string PasswordGeneratorUpperCharacters {
            get {
                return ((string)(this["PasswordGeneratorUpperCharacters"]));
            }
            set {
                this["PasswordGeneratorUpperCharacters"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("!@$()=+-,:.")]
        public string PasswordGeneratorSymbols {
            get {
                return ((string)(this["PasswordGeneratorSymbols"]));
            }
            set {
                this["PasswordGeneratorSymbols"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0123456789")]
        public string PasswordGeneratorDigits {
            get {
                return ((string)(this["PasswordGeneratorDigits"]));
            }
            set {
                this["PasswordGeneratorDigits"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("30")]
        public int AutoClearClipboard {
            get {
                return ((int)(this["AutoClearClipboard"]));
            }
            set {
                this["AutoClearClipboard"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string Language {
            get {
                return ((string)(this["Language"]));
            }
            set {
                this["Language"] = value;
            }
        }
    }
}
