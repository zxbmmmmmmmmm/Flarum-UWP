using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Web.Http.Filters;
using FlarumUWP.Core.Models;
using FlarumUWP.Services;
using FlarumUWP.Views;

namespace FlarumUWP.Helpers
{
    public class wvbProxy
    {
        public static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static async Task<MainPagePosts> GetMainPagePostsInfo(string htmlData,bool offline,string page)
        {
            var website = localSettings.Values["website"] as string;

            if (offline)//离线，读取API内容
            {
                var serializer = new DataContractJsonSerializer(typeof(MainPagePosts));
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(htmlData));
                var data = (MainPagePosts)serializer.ReadObject(ms);           

                return data;

            }
            else//在线
            {
                string result;
                if ((bool)localSettings.Values["CloudflareUnderAttackMode"] == false)
                {
                    var _filter = new HttpBaseProtocolFilter();
                    var http = new Windows.Web.Http.HttpClient(_filter);
                    var response = await http.GetAsync(new Uri(page));
                    result = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var dialog = new CloudFlareHelper(new Uri(page));
                    await dialog.ShowAsync();
                    result = dialog.HTMLData;
                }

                var serializer = new DataContractJsonSerializer(typeof(MainPagePosts));
                var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
                var data = (MainPagePosts)serializer.ReadObject(ms);
                return data;
            }           
        }
        public static async Task<UserInfo> GetUserInfo(string link)
        {
            string result;
            if ((bool)localSettings.Values["CloudflareUnderAttackMode"] == false)
            {
                var _filter = new HttpBaseProtocolFilter();
                var http = new Windows.Web.Http.HttpClient(_filter);
                var response = await http.GetAsync(new Uri(link));
                result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                var dialog = new CloudFlareHelper(new Uri(link));
                await dialog.ShowAsync();
                result = dialog.HTMLData;
            }

            var serializer = new DataContractJsonSerializer(typeof(UserInfo));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (UserInfo)serializer.ReadObject(ms);
            return data;
        }
        public static async Task<MainPagePosts> GetPostsView(Uri link)
        {
            string result = string.Empty;

            if ((bool)localSettings.Values["CloudflareUnderAttackMode"] == false)
            {
                var _filter = new HttpBaseProtocolFilter();
                var http = new Windows.Web.Http.HttpClient(_filter);
                var response = await http.GetAsync(link);
                result = await response.Content.ReadAsStringAsync();
            }
            else
            {
                var dialog = new CloudFlareHelper(link);
                await dialog.ShowAsync();
                result = dialog.HTMLData;
            }

            //var http = new HttpClient();
            //var response = await http.GetAsync(link);

            //var result = await response.Content.ReadAsStringAsync();
            var serializer = new DataContractJsonSerializer(typeof(MainPagePosts));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (MainPagePosts)serializer.ReadObject(ms);

            for (int i = 0; i < data.data.Count(); i++)

            {


                data.data[i].attributes.poster = data.included[i].attributes.displayName;
                if (data.included[i].attributes.avatarUrl != null)
                    data.data[i].attributes.poster_picture = data.included[i].attributes.avatarUrl;
                else
                    data.data[i].attributes.poster_picture = "https://docs.microsoft.com/zh-cn/windows/apps/design/style/images/segoe-mdl/ea84.png";
                data.data[i].attributes.poster_display_name = data.data[i].attributes.poster;

            }

            return data;

        }
        public static string StringDateTimeFriendFormat(string str)
        {
            var stringDt = FriendFormat(StringToDateTime(str));
            return stringDt;
        }
        public static DateTime StringToDateTime(string str)
        {
            DateTime dateTime;

            str = str.Replace('T', ' ');
            str = str.Remove(str.Length - 6, 6);
            dateTime = Convert.ToDateTime(str);
            dateTime = dateTime.AddHours(8);//GMT +8:00时间转换

            return dateTime;
        }

        public static string FriendFormat(long dateTime)
        {
            String fTime = String.Empty;
            try
            {
                DateTime dt = new DateTime(dateTime);
                return FriendFormat(dt);
            }
            catch
            {
                return dateTime.ToString();
            }
        }
        public static string StringFriendFormat(string dateTime)
        {
            string fTime = string.Empty;
            try
            {
                DateTime dt = DateTime.Parse(dateTime);
                return FriendFormat(dt);
            }
            catch
            {
                return dateTime;
            }
        }
        public static string FriendFormat(DateTime dateTime)
        {
            if (dateTime == null)
            {
                return string.Empty;
            }
            int nowYear = DateTime.Now.Year;
            int year = dateTime.Year;
            if (nowYear - year == 0)
            {
                //本年内
                int nowMounth = DateTime.Now.Month;
                int mounth = dateTime.Month;
                if (nowMounth - mounth == 0)
                {
                    //本月内
                    int nowDay = DateTime.Now.Day;
                    int day = dateTime.Day;
                    int diffDay = nowDay - day;

                    if (nowDay - day == 0)
                    {
                        //当天
                        int nowHour = DateTime.Now.Hour;
                        int hour = dateTime.Hour;
                        int diffHour = nowHour - hour;

                        if (nowHour - hour == 0)
                        {
                            int nowMinute = DateTime.Now.Minute;
                            int minute = dateTime.Minute;
                            int diff = nowMinute - minute;
                            if (diff < 2)
                            {
                                return "刚刚";
                            }
                            else
                            {
                                return $"{diff} 分钟前";
                            }
                        }
                        else
                        {
                          
                            return $"{diffHour} 小时前";
                        }
                    }
                    else
                    {
                        return $"{diffDay} 天前";
                    }
                }
                else
                {
                    //跨月
                    return dateTime.ToString("yyyy-MM-dd");
                }
            }
            else
            {
                //跨年
                return dateTime.ToString("yyyy-MM-dd"); // "yyyy-MM-dd HH:mm:ss"
            }
        }
    }

}
