using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace StorageUserOpreate
{
    class Program
    {
        static void Main(string[] args)
        {

            #region get userinfo by filter
            UserInfoOperationTool userInfoOperation = new UserInfoOperationTool();
            string pattern = @"[A-Za-z]+\.[A-Za-z]+\d{4}@hotmail.com";
            IEnumerable<UserSnapshot> users = userInfoOperation.Get(u =>
                        (u.Timestamp.Ticks - (new DateTime(2017, 2, 23)).Ticks > 0) /*&& Regex.IsMatch(u.RowKey, pattern)*/);

            string infoHeader = string.Format("{0,-80},{1,-50} \r\n","Email", "Timestamp");
            File.AppendAllText("usersimpleinfo.csv", infoHeader);
            users = users.OrderBy(u=>u.Timestamp);

            foreach (var user in users)
            {
                string info = string.Format("{0,-80},{1,-50} \r\n", user.RowKey, user.Timestamp.ToLocalTime());
                File.AppendAllText("usersimpleinfo.csv", info);
            }
            #endregion

            #region update user IsVerified
            //users = users.Select(u => { u.IsVerified = false; return u; });
            //foreach (var user in users)
            //{
            //    userInfoOperation.Update(user);
            //} 
            #endregion
        }
    }
}
