using Discord;
using Discord.Commands;
using Discord.Modules;
using System;
using System.Linq;

namespace MidnightBot.Modules.Permissions.Classes
{
    internal static class PermissionHelper
    {
        public static bool ValidateBool ( string passedArg )
        {
            if (string.IsNullOrWhiteSpace (passedArg))
            {
                throw new ArgumentException ("Falsche Eingabe! Argument fehlt");
            }
            switch (passedArg.ToLower ())
            {
                case "1":
                case "t":
                case "true":
                case "enable":
                case "enabled":
                case "allow":
                case "unban":
                    return true;
                case "0":
                case "f":
                case "false":
                case "disable":
                case "disabled":
                case "disallow":
                case "ban":
                    return false;
                default:
                    throw new ArgumentException ("Keinen gültiges Boolean Wert erhalten.");
            }
        }

        internal static string ValidateModule ( string mod )
        {
            if (string.IsNullOrWhiteSpace (mod))
                throw new ArgumentNullException (nameof (mod));

            foreach (var m in MidnightBot.Client.GetService<ModuleService> ().Modules)
            {
                if (m.Name.ToLower ().Equals (mod.Trim ().ToLower ()))
                    return m.Name;
            }
            throw new ArgumentException ("Dieses Modul existiert nicht.");
        }

        internal static string ValidateCommand ( string commandText )
        {
            if (string.IsNullOrWhiteSpace (commandText))
                throw new ArgumentNullException (nameof (commandText));

            foreach (var com in MidnightBot.Client.GetService<CommandService> ().AllCommands)
            {
                if (com.Text.ToLower ().Equals (commandText.Trim ().ToLower ()))
                    return com.Text;
            }
            throw new NullReferenceException ("Dieser Befehl exisitert nicht.");
        }

        internal static Role ValidateRole ( Server server,string roleName )
        {
            if (string.IsNullOrWhiteSpace (roleName))
                throw new ArgumentNullException (nameof (roleName));

            if (roleName.Trim () == "everyone")
                roleName = "@everyone";
            var role = server.FindRoles (roleName.Trim ()).FirstOrDefault ();
            if (role == null)
                throw new NullReferenceException ("Diese Rolle existiert nicht.");
            return role;
        }

        internal static Channel ValidateChannel ( Server server,string channelName )
        {
            if (string.IsNullOrWhiteSpace (channelName))
                throw new ArgumentNullException (nameof (channelName));
            var channel = server.FindChannels (channelName.Trim (),ChannelType.Text).FirstOrDefault ();
            if (channel == null)
                throw new NullReferenceException ("Dieser Channel existiert nicht.");
            return channel;
        }

        internal static User ValidateUser ( Server server,string userName )
        {
            if (string.IsNullOrWhiteSpace (userName))
                throw new ArgumentNullException (nameof (userName));
            var user = server.FindUsers (userName.Trim ()).FirstOrDefault ();
            if (user == null)
                throw new NullReferenceException ("Dieser Benutzer existiert nicht.");
            return user;
        }
    }
}