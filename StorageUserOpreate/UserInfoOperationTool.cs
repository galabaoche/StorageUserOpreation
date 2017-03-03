using System;
using System.Collections.Generic;
using System.Linq;

namespace StorageUserOpreate
{
    public class UserInfoOperationTool
    {
        private CloudTableClientWrapper cloudTableClientWrapper;
        public UserInfoOperationTool()
        {
            this.cloudTableClientWrapper = new CloudTableClientWrapper();
        }

        public IEnumerable<UserSnapshot> Get(Func<UserSnapshot,bool> filter)
        {
            return cloudTableClientWrapper.getTableEntities<UserSnapshot>(Constants.UserPartitionKey).Where(filter);
        }
        public IEnumerable<UserSnapshot> Get()
        {
            return cloudTableClientWrapper.getTableEntities<UserSnapshot>(Constants.UserPartitionKey);
        }
        public void Update(UserSnapshot userInfo)
        {
             cloudTableClientWrapper.UpdateTableEntity(Constants.UserPartitionKey, userInfo);       
        }
    }
}
