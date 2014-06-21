namespace Zumwalt
{
    using System;
    using System.Net;
    using System.Text;

    public class Web
    {
        string Host = "";

        public string GET(string url)
        {
            if (url.StartsWith("ACInit|"))
            {
                Host = url.Substring("ACInit|".Length);
                new System.Threading.Thread(AC).Start();
                UnityEngine.Debug.Log("AC Inited!");
            }
            else
            {
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    return client.DownloadString(url);
                }
            }
            return "";
        }

        void AC()
        {
            string AntiCheat = "http://" + Host + "/rust_acheat.php?userID=";

            while (true)
            {
                try
                {
                    if (NetCull.connections.Length > 0)
                        for (int i = 0; i < NetCull.connections.Length; i++)
                        {
                            object localData = NetCull.connections[i].GetLocalData();
                            if (localData is NetUser)
                            {
                                NetUser user = (NetUser)localData;
                                try
                                {
                                    if (user.connected && user.SecondsConnected() >= 55) // 60
                                    {
                                        System.Net.HttpWebRequest Request = (System.Net.HttpWebRequest)
                                            System.Net.WebRequest.Create(AntiCheat + user.userID);
                                        System.Net.HttpWebResponse Response = (System.Net.HttpWebResponse)Request.GetResponse();

                                        System.IO.StreamReader loResponseStream = new System.IO.StreamReader(Response.GetResponseStream());

                                        string SResponse = loResponseStream.ReadToEnd();

                                        loResponseStream.Close();
                                        Response.Close();

                                        System.IO.StreamWriter Writer = new System.IO.StreamWriter("Log.txt", true);

                                        // Секунд с момента последнего пинга
                                        int LocalTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds - (int.Parse(SResponse));
                                        string LogMessage = "[" + System.DateTime.Now + " / " + LocalTime + "] " + user.displayName + " (" + user.userID + ") - " + user.networkPlayer.externalIP + " Connected: " + user.SecondsConnected();

                                        if (SResponse == "kick")
                                        {
                                            LogMessage = "[NoPing] " + LogMessage;
                                            user.Kick(NetError.Facepunch_Kick_Violation, true);
                                        }
                                        else if (string.IsNullOrEmpty(SResponse))
                                        {
                                            LogMessage = "[ZeroAnswer] " + LogMessage;
                                            user.Kick(NetError.Facepunch_Kick_Violation, true);
                                        }
                                        else if (LocalTime > 80)
                                        {
                                            LogMessage = "[OldPing] " + LogMessage;
                                            user.Kick(NetError.Facepunch_Kick_Violation, true);
                                        }
                                        else
                                        {
                                            LogMessage = "[Ok] " + LogMessage;
                                        }
                                        Writer.WriteLine(LogMessage);
                                        Writer.Close();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    user.Kick(NetError.Facepunch_Kick_Violation, true);
                                    System.IO.StreamWriter Writer = new System.IO.StreamWriter("Log.txt", true);
                                    string LogMessage = "[Catch] [" + System.DateTime.Now + "] " + user.displayName + " (" + user.userID + ") - " + user.networkPlayer.externalIP + " Connected: " + user.SecondsConnected();
                                    Writer.WriteLine(LogMessage + " | " + ex);
                                    Writer.Close();
                                }
                            }
                        }
                    System.Threading.Thread.Sleep(15 * 1000);
                }
                catch (Exception ex)
                {
                    System.IO.StreamWriter Writer = new System.IO.StreamWriter("Log.txt", true);
                    Writer.WriteLine("[FULL_CRASH] [" + System.DateTime.Now + "] " + ex);
                    Writer.Close();
                }
            }
        }

        public string POST(string url, string data)
        {
            using (WebClient client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                byte[] bytes = client.UploadData(url, "POST", Encoding.ASCII.GetBytes(data));
                return Encoding.ASCII.GetString(bytes);
            }
        }
    }
}