using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;

namespace StorageUserOpreate
{
    public class UserSnapshot : TableEntity
    {
        public string UserJson { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool ContactByEmail { get; set; }
        public bool IsVerified { get; set; }

        public DateTime RegistrationDate { get; set; }
        public UserSnapshot() { }

        public UserSnapshot(User user)
        {
            PartitionKey = Constants.UserPartitionKey;
            RowKey = SenitizeEmail(user.Email);
            Email = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            RegistrationDate = user.RegistrationDate;
            // Richard. Updated the registration date to be null for ignoring it from the json serializer.
            user.RegistrationDate = DateTime.MinValue;
            user.Id = null;
            UserJson = JsonConvert.SerializeObject(user);

            try
            {
                if (user.Note.ToLower().Contains("contactbyemail") || user.Note.ToLower().Contains("isverified"))
                {
                    UserNoteInfo noteInfo = JsonConvert.DeserializeObject<UserNoteInfo>(user.Note);
                    this.ContactByEmail = noteInfo == null ? false : noteInfo.ContactByEmail;
                    this.IsVerified = noteInfo == null ? false : noteInfo.IsVerified;
                }
            }
            catch (Exception)
            {
                this.ContactByEmail = false;
                this.IsVerified = false;
            }
        }

        private string SenitizeEmail(string email)
        {
            return email.Replace("#", "").Replace("\\", "").Replace("/", "").Replace("?", "");
        }

        public override bool Equals(object obj)
        {
            UserSnapshot us = obj as UserSnapshot;
            if (us == null)
            {
                return false;
            }

            return this.UserJson == us.UserJson || us.IsVerified && !this.IsVerified;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class UserNoteInfo
    {
        public bool ContactByEmail { get; set; }

        public bool IsVerified { get; set; }
    }
}
