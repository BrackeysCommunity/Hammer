﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Hammer.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class LoggerMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal LoggerMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Hammer.Resources.LoggerMessages", typeof(LoggerMessages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {user} is not in {guild} - this mute will take effect if they rejoin the server..
        /// </summary>
        internal static string CantMuteNonMember {
            get {
                return ResourceManager.GetString("CantMuteNonMember", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {user} is not in {guild} - the Muted role will not be reapplied if they rejoin the server..
        /// </summary>
        internal static string CantUnmuteNonMember {
            get {
                return ResourceManager.GetString("CantUnmuteNonMember", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {user} attempted to create a duplicate report on {message} by {message.Author} in {message.Channel} - this report will not be logged in Discord..
        /// </summary>
        internal static string DuplicateMessageReport {
            get {
                return ResourceManager.GetString("DuplicateMessageReport", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {user} was muted by {staffMember} in {guild}.
        /// </summary>
        internal static string MemberMuted {
            get {
                return ResourceManager.GetString("MemberMuted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {message} in channel {message.Channel} was deleted by {staffMember}.
        /// </summary>
        internal static string MessageDeleted {
            get {
                return ResourceManager.GetString("MessageDeleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {user} reported a message, but is blocked from doing so.
        /// </summary>
        internal static string MessageReportBlocked {
            get {
                return ResourceManager.GetString("MessageReportBlocked", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {user} reported {message} by {message.Author} in {message.Channel}.
        /// </summary>
        internal static string MessageReported {
            get {
                return ResourceManager.GetString("MessageReported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to grant role! Muted role not found for {guild}.
        /// </summary>
        internal static string NoMutedRoleToGrant {
            get {
                return ResourceManager.GetString("NoMutedRoleToGrant", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to revoke role! Muted role not found for {guild}.
        /// </summary>
        internal static string NoMutedRoleToRevoke {
            get {
                return ResourceManager.GetString("NoMutedRoleToRevoke", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {staffMember} sent a message to {recipient} from {guild}. Contents: {message}.
        /// </summary>
        internal static string StaffMessagedMember {
            get {
                return ResourceManager.GetString("StaffMessagedMember", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Temporary mute for {user} expired in {guild}.
        /// </summary>
        internal static string TemporaryMuteExpired {
            get {
                return ResourceManager.GetString("TemporaryMuteExpired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tracking disabled for {user} in {guild}.
        /// </summary>
        internal static string TrackingDisabledForUser {
            get {
                return ResourceManager.GetString("TrackingDisabledForUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tracking enabled for {user} in {guild}. Duration: {duration}.
        /// </summary>
        internal static string TrackingEnabledForUser {
            get {
                return ResourceManager.GetString("TrackingEnabledForUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {reporter} attempted to report {message} but the message is deleted!.
        /// </summary>
        internal static string UserReportedDeletedMessage {
            get {
                return ResourceManager.GetString("UserReportedDeletedMessage", resourceCulture);
            }
        }
    }
}
